/* Copyright 2014 Jonathan Holland.
*
*  Licensed under the Apache License, Version 2.0 (the "License");
*  you may not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
*  http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" BASIS,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*  See the License for the specific language governing permissions and
*  limitations under the License.
*/

using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using Wave.Defaults;

namespace Wave.Tests
{
    [Category("Transport: All")]
    [TestFixture]
    [Ignore("Ignore Tests on Transport Base")]
    public class TransportTestBase
    {
        [Test]
        public void GetDelayMessages_Throws_OperationCancelledException_When_Cancelled()
        {
            var transport = this.GetTransport();
            var source = new CancellationTokenSource();
            var task = Task.Factory.StartNew(() =>
            {
                Assert.Throws<OperationCanceledException>(() =>
                {
                    transport.GetDelayMessages(source.Token, (message, ack, reject) => { });
                });
            });

            source.Cancel();
        }

        [Test]
        public void GetMessages_Throws_OperationCancelledException_When_Cancelled()
        {
            var transport = this.GetTransport();
            var source = new CancellationTokenSource();
            var task = Task.Factory.StartNew(() =>
            {
                Assert.Throws<OperationCanceledException>(() =>
                {
                    transport.GetMessages(source.Token, (message, ack, reject) => { });
                });
            });

            source.Cancel();
        }

        public virtual ITransport GetTransport()
        {
            return null;
        }

        [Test]
        public void RegisterSubscription_Is_Idempotent()
        {
            var transport = this.GetTransport();
            Assert.DoesNotThrow(() =>
            {
                transport.RegisterSubscription(typeof(TestMessage).Name);
                transport.RegisterSubscription(typeof(TestMessage).Name);
            });
        }

        [Test]
        [Ignore("This test needs to be redone to avoid failing due to lock contention")]         
        public void Rejected_Messages_Return_To_Source_Queue()
        {
            var transport = this.GetTransport();
            var testMessage = TestHelpers.GetRawMessage(new TestMessage(), new DefaultSerializer(), new DefaultSubscriptionKeyResolver());
            var returnedFirstMessage = (RawMessage)null;
            var returnedSecondMessage = (RawMessage)null;

            // Send message
            transport.RegisterSubscription(typeof(TestMessage).Name);
            transport.Send(typeof(TestMessage).Name, testMessage);

            // Receive, log and reject the message
            this.RunBlocking((unblockEvent) =>
            {
                transport.GetMessages(new CancellationToken(),
                (message, ack, reject) =>
                {
                    returnedFirstMessage = message;
                    reject();
                    unblockEvent.Set();
                });
            }, TimeSpan.FromSeconds(15));

            // Receive it a 2nd time
            this.RunBlocking((unblockEvent) =>
            {
                transport.GetMessages(new CancellationToken(),
                (message, ack, reject) =>
                {
                    returnedSecondMessage = message;
                    ack();
                    unblockEvent.Set();
                });
            }, TimeSpan.FromSeconds(15));

            Assert.IsNotNull(returnedFirstMessage);
            Assert.IsNotNull(returnedSecondMessage);
            Assert.AreEqual(testMessage.Id, returnedFirstMessage.Id);
            Assert.AreEqual(testMessage.Id, returnedSecondMessage.Id);
        }

        [Test]
        public void Send_Puts_Message_In_Primary_Queue()
        {
            var transport = this.GetTransport();
            var testMessage = TestHelpers.GetRawMessage(new TestMessage(), new DefaultSerializer(), new DefaultSubscriptionKeyResolver());
            var returnedMessage = (RawMessage)null;

            transport.RegisterSubscription(typeof(TestMessage).Name);
            transport.Send(typeof(TestMessage).Name, testMessage);
            this.RunBlocking((unblockEvent) =>
            {
                transport.GetMessages(new CancellationToken(),
                (message, ack, reject) =>
                {
                    returnedMessage = message;
                    ack();
                    unblockEvent.Set();
                });
            }, TimeSpan.FromSeconds(15));

            Assert.IsNotNull(returnedMessage);
            Assert.AreEqual(testMessage.Id, returnedMessage.Id);
        }

        [Test]
        public void SendToDelay_Puts_Message_In_Delay_Queue()
        {
            var transport = this.GetTransport();
            var testMessage = TestHelpers.GetRawMessage(new TestMessage(), new DefaultSerializer(), new DefaultSubscriptionKeyResolver());
            var returnedMessage = (RawMessage)null;

            transport.RegisterSubscription(typeof(TestMessage).Name);
            transport.SendToDelay(testMessage);
            this.RunBlocking((unblockEvent) =>
            {
                transport.GetDelayMessages(new CancellationToken(),
                (message, ack, reject) =>
                {
                    returnedMessage = message;
                    ack();
                    unblockEvent.Set();
                });
            }, TimeSpan.FromSeconds(15));

            Assert.IsNotNull(returnedMessage);
            Assert.AreEqual(testMessage.Id, returnedMessage.Id);
        }

        [Test]
        public void SendToPrimary_Puts_Message_In_Primary_Queue()
        {
            var transport = this.GetTransport();
            var testMessage = TestHelpers.GetRawMessage(new TestMessage(), new DefaultSerializer(), new DefaultSubscriptionKeyResolver());
            var returnedMessage = (RawMessage)null;

            transport.RegisterSubscription(typeof(TestMessage).Name);
            transport.SendToPrimary(testMessage);
          
            this.RunBlocking((unblockEvent) =>
            {
                transport.GetMessages(new CancellationToken(),
                (message, ack, reject) =>
                {
                    returnedMessage = message;
                    ack();
                    unblockEvent.Set();
                });
            }, TimeSpan.FromSeconds(15));

            Assert.IsNotNull(returnedMessage);
            Assert.AreEqual(testMessage.Id, returnedMessage.Id);
        }

        [Test]
        public void Send_Without_Registration_Does_Not_Queue_Message()
        {
            var transport = this.GetTransport();
            var testMessage = TestHelpers.GetRawMessage(new TestMessage(), new DefaultSerializer(), new DefaultSubscriptionKeyResolver());
            var returnedMessage = (RawMessage)null;
            
            transport.Send(typeof(TestMessage).Name, testMessage);
            this.RunBlocking((unblockEvent) =>
            {
                transport.GetMessages(new CancellationToken(),
                (message, ack, reject) =>
                {
                    returnedMessage = message;
                    ack();
                    unblockEvent.Set();
                });
            }, TimeSpan.FromSeconds(2));

            Assert.IsNull(returnedMessage);            
        }

        protected void RunBlocking(Action<ManualResetEvent> action, TimeSpan timeout)
        {
            var source = new CancellationTokenSource();
            var resetEvent = new ManualResetEvent(false);
            Task.Factory.StartNew(() => action(resetEvent),            
                        source.Token,
                        TaskCreationOptions.LongRunning, 
                        TaskScheduler.Default);

            resetEvent.WaitOne(timeout);
            source.Cancel();
        }

        [Serializable]
        protected class TestMessage { }       
    }
}
