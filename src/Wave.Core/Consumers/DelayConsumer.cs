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
using System.Threading;
using System.Threading.Tasks;
using Wave.Utility;

namespace Wave.Consumers
{
    internal class DelayConsumer : IConsumer, IDisposable
    {
        private readonly IConfigurationContext configuration;
        private readonly AutoResetEvent resetEvent;

        public DelayConsumer(IConfigurationContext configuration)
        {
            this.configuration = configuration;

            // Flipped whenever a new message arrives from the transport to short circuit
            // the 15 second timeout intervals
            this.resetEvent = new AutoResetEvent(false);
        }

        public void Cancel()
        {
            // Will cause the delay consumer to stop waiting for message and immediatly look to see if it is cancelled
            this.resetEvent.Set();
        }

        public void ConsumeQueue()
        {
            // To cut down the noise
            var cancelToken = this.configuration.TokenSource.Token;
            var transport = this.configuration.Transport;

            // PriorityQueue is not threadsafe, lock around accessing it
            var @lock = new object();

            // Min-Oriented Priority Queue using DelayUntil as the comparison property
            var messages = new PriorityQueue<MessageWrapper>(
                 (x, y) => x.RawMessage.DelayUntil.GetValueOrDefault(DateTime.MinValue)
                            .CompareTo(y.RawMessage.DelayUntil.GetValueOrDefault(DateTime.MinValue)));

            // Background task that is constantly checking the in memory priority queue
            // for expired delay messages and publishing them into the primary queue
            // as they expire.
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        cancelToken.ThrowIfCancellationRequested();
                    }

                    var maxWait = TimeSpan.FromSeconds(15);

                    lock (@lock)
                    {
                        if (!messages.IsEmpty)
                        {
                            var current = DateTime.UtcNow;
                            var minDelay = messages.Peek().RawMessage.DelayUntil.GetValueOrDefault(current);
                            maxWait = minDelay - current < maxWait
                                ? minDelay - current
                                : maxWait;
                        }
                    }

                    // Wait for maxWait, unless the message expired by the time we got here.
                    // If it did, Maxwait will be negative and cause the resetEvent to throw
                    // so pass TimeSpan.Zero in that condition
                    resetEvent.WaitOne((maxWait > TimeSpan.Zero) ? maxWait : TimeSpan.Zero);
                    resetEvent.Reset();

                    lock (@lock)
                    {
                        if (messages.IsEmpty || cancelToken.IsCancellationRequested)
                        {
                            continue;
                        }

                        // Scan for expired messages.
                        while (!messages.IsEmpty)
                        {
                            var message = messages.Peek();

                            // Short circuit, any messages beyond this will be unexpired due to the
                            // queue ordering
                            if (message.RawMessage.DelayUntil.HasValue && message.RawMessage.DelayUntil > DateTime.UtcNow)
                            {
                                break;
                            }

                            message = messages.Dequeue();
                            transport.SendToPrimary(message.RawMessage);
                            message.Acknowledge();
                        }
                    }
                }
            });

            // Ask the transport to start sending messages from the delay queue and push them into
            // the internal priority queue
            transport.GetDelayMessages(
                this.configuration.TokenSource.Token,
                (message, ack, reject) =>
                {
                    lock (@lock)
                    {
                        messages.Enqueue(new MessageWrapper
                        {
                            RawMessage = message,
                            Acknowledge = ack,
                            Reject = reject
                        });
                    }

                    // Flag for immediate processing
                    resetEvent.Set();
                });
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.resetEvent.Dispose();
        }

        // Because we need to pass a reference to the transport ack/reject
        // callbacks with the raw message
        private class MessageWrapper
        {
            public Action Acknowledge { get; set; }

            public RawMessage RawMessage { get; set; }

            public Action Reject { get; set; }
        }
    }
}