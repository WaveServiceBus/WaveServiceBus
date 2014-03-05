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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Wave.Defaults
{
    internal class DefaultTransport : ITransport, IDisposable
    {
        private readonly string primaryQueueName;
        private readonly string delayQueueName;
        private readonly string errorQueueName;

        private readonly ManualResetEvent delayResetEvent;
        private readonly ManualResetEvent primaryResetEvent;

        private readonly InMemoryQueue delayQueue;
        private readonly InMemoryQueue primaryQueue;
        private readonly InMemoryQueue errorQueue;

        private readonly List<Object> publishedMessages;

        public DefaultTransport(IAssemblyLocator assemblyLocator)
        {
            this.primaryResetEvent = new ManualResetEvent(false);
            this.delayResetEvent = new ManualResetEvent(false);

            this.primaryQueueName = assemblyLocator.GetEntryAssembly().GetName().Name;
            this.delayQueueName = string.Format("{0}_Delay", this.primaryQueueName);
            this.errorQueueName = string.Format("{0}_Error", this.primaryQueueName);
            this.primaryQueue = new InMemoryQueue();
            this.delayQueue = new InMemoryQueue();
            this.errorQueue = new InMemoryQueue();
            this.publishedMessages = new List<Object>();
        }

        public void Dispose()
        {
            this.primaryResetEvent.Dispose();
            this.delayResetEvent.Dispose();
        }
    
        /// <summary>
        /// Blocks current thread until a message is available
        /// </summary>        
        public void GetDelayMessages(CancellationToken token, Action<RawMessage, Action, Action> onMessageReceived)
        {
            this.GetMessages(this.delayQueue, token, this.delayResetEvent, onMessageReceived);
        }

        /// <summary>
        /// Blocks current thread until a message is available
        /// </summary>        
        public void GetMessages(CancellationToken token, Action<RawMessage, Action, Action> onMessageReceived)
        {
            this.GetMessages(this.primaryQueue, token, this.primaryResetEvent, onMessageReceived);
        }

        public void InitializeForConsuming()
        {
            this.primaryQueue.Subscribe(this.primaryQueueName);
            this.delayQueue.Subscribe(this.delayQueueName);
            this.errorQueue.Subscribe(this.errorQueueName);
        }

        public void InitializeForPublishing()
        {
            // No Op for InMemory
        }

        /// <summary>
        /// Registers a subscription with the work queue. Messages of this type will be delivered
        /// </summary>
        /// <param name="subscription"></param>
        public void RegisterSubscription(string subscription)
        {
            this.primaryQueue.Subscribe(subscription);
        }

        /// <summary>
        /// Sends a message to all queues that subscribe to the subscription
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="message"></param>
        public void Send(string subscription, object message)
        {          
            this.Send(subscription, RawMessage.Create(message));
        }

        /// <summary>
        /// Sends a message to all queues that subscribe to the subscription
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="message"></param>
        public void Send(string subscription, RawMessage message)
        {
            if (this.primaryQueue.Enqueue(subscription, message))
            {
                this.primaryResetEvent.Set();
            }

            if (this.delayQueue.Enqueue(subscription, message))
            {
                this.delayResetEvent.Set();
            }

            this.errorQueue.Enqueue(subscription, message);
        }

        public void SendToDelay(RawMessage message)
        {
            if (this.delayQueue.Enqueue(this.delayQueueName, message))
            {
                this.delayResetEvent.Set();
            }
        }

        public void SendToError(RawMessage message)
        {
            this.errorQueue.Enqueue(this.errorQueueName, message);
        }

        public void SendToPrimary(RawMessage message)
        {
            if (this.primaryQueue.Enqueue(this.primaryQueueName, message))
            {
                this.primaryResetEvent.Set();
            }
        }

        public void Shutdown()
        {
            // No Op
        }

        /// <summary>
        /// Waits on the specified queue for messages
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="token"></param>
        /// <param name="onMessageReceived"></param>
        private void GetMessages(InMemoryQueue queue, CancellationToken token, ManualResetEvent resetEvent, Action<RawMessage, Action, Action> onMessageReceived)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }

                resetEvent.WaitOne(TimeSpan.FromSeconds(15));

                RawMessage message = null;
                while (queue.Dequeue(ref message))
                {
                    onMessageReceived(
                        message,
                     () => { /* Ack No Op for InMemory */ },
                     () =>
                     {
                         // Reject puts message back in queue
                         queue.Nack(message);
                     });
                }

                resetEvent.Reset();
            }
        }

        internal class InMemoryQueue
        {
            private readonly ConcurrentQueue<RawMessage> messageQueue;
            private readonly List<string> subscriptions;

            public InMemoryQueue()
            {
                this.messageQueue = new ConcurrentQueue<RawMessage>();
                this.subscriptions = new List<string>();
            }

            public bool Dequeue(ref RawMessage message)
            {
                return this.messageQueue.TryDequeue(out message);
            }

            public bool Enqueue(string subscription, RawMessage message)
            {
                if (this.subscriptions.Contains(subscription))
                {
                    this.messageQueue.Enqueue(message);

                    return true;
                }

                return false;
            }

            public void Nack(RawMessage message)
            {
                // Assumes that the message actually came from this queue
                // Ok for this purpose
                this.messageQueue.Enqueue(message);
            }

            public void Subscribe(string subscription)
            {
                this.subscriptions.Add(subscription);
            }
        }
    }
}