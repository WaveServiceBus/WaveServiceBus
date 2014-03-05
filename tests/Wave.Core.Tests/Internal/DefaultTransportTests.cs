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
using Wave.Defaults;

namespace Wave.Tests.Internal
{
    [Category("Transport: InMemory")]
    [TestFixture]    
    public class InMemoryTransportTests : TransportTestBase
    {
        public override ITransport GetTransport()
        {
            var transport = new DefaultTransport(new TestAssemblyLocator());
            transport.InitializeForConsuming();
            transport.InitializeForPublishing();

            return transport;
        }

        [Test]
        public void Send_With_Delay_Queue_As_Subscription_Puts_Message_In_Delay_Queue()
        {
            var transport = this.GetTransport();
            var testMessage = TestHelpers.GetRawMessage(new TestMessage(), new DefaultSerializer(), new DefaultSubscriptionKeyResolver());
            var returnedMessage = (RawMessage)null;

            transport.RegisterSubscription(typeof(TestMessage).Name);
            transport.Send("Wave.Core.Tests_Delay", testMessage);
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
        public void Send_With_Error_Queue_As_Subscription_Puts_Message_In_Error_Queue()
        {
            var transport = this.GetTransport();
            var testMessage = TestHelpers.GetRawMessage(new TestMessage(), new DefaultSerializer(), new DefaultSubscriptionKeyResolver());
            var returnedMessage = (RawMessage)null;

            transport.RegisterSubscription(typeof(TestMessage).Name);
            transport.Send("Wave.Core.Tests_Error", testMessage);
            this.GetErrorQueue(transport).Dequeue(ref returnedMessage);
            
            Assert.IsNotNull(returnedMessage);
            Assert.AreEqual(testMessage.Id, returnedMessage.Id);
        }

        [Test]
        public void SendToError_Puts_Message_In_Error_Queue()
        {
            var transport = this.GetTransport();
            var testMessage = TestHelpers.GetRawMessage(new TestMessage(), new DefaultSerializer(), new DefaultSubscriptionKeyResolver());
            var returnedMessage = (RawMessage)null;
          
            transport.RegisterSubscription(typeof(TestMessage).Name);
            transport.SendToError(testMessage);
            this.GetErrorQueue(transport).Dequeue(ref returnedMessage);

            Assert.IsNotNull(returnedMessage);
            Assert.AreEqual(testMessage.Id, returnedMessage.Id);
        }
       
        private DefaultTransport.InMemoryQueue GetErrorQueue(ITransport transport)
        {
            // Uses reflection to grab "errorQueue" out of the transport. 
            return TestHelpers.GetPrivateField<DefaultTransport.InMemoryQueue, DefaultTransport>("errorQueue", (DefaultTransport)transport);
        }
    }
}
