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
using System.Linq;
using System.Threading;
using Wave.Configuration;

namespace Wave.Filters
{
    /// <summary>
    /// Enforces that messages are delivered to a handler no faster than rate specified in RatePerSecond
    /// Implemented using the Token Bucket Algorithm.
    /// </summary>
    public class ThrottleMessages : Attribute, IInboundMessageFilter, IDisposable
    {
        private readonly Lazy<ILogger> log = new Lazy<ILogger>(() => ConfigurationContext.Current.Container.Resolve<ILogger>());
        private TimeSpan delayInterval;
        private Timer refillTimer;
        private ConcurrentQueue<byte> tokenBucket;

        public int RatePerSecond
        {
            get;
            set;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public IHandlerResult OnHandlerExecuted(IHandlerResult handlerResult, Type messageType, object message)
        {
            return handlerResult;
        }

        public IHandlerResult OnHandlerExecuting(Type messageType, object message)
        {
            // First message? Init the bucket and refill timer
            if (this.tokenBucket == null)
            {
                this.tokenBucket = new ConcurrentQueue<byte>(Enumerable.Range(0, this.RatePerSecond).Select(i => (byte)1));
                this.delayInterval = TimeSpan.FromMilliseconds(1000 / this.RatePerSecond);

                this.refillTimer = new Timer((s) =>
                {
                    if (this.tokenBucket.Count < this.RatePerSecond)
                    {
                        this.tokenBucket.Enqueue((byte)1);
                    }
                }, null, 0, (long)this.delayInterval.TotalMilliseconds);
            }

            // Attempt to retrieve a token, if there are none available, delay the message
            byte token;
            if (this.tokenBucket.TryDequeue(out token))
            {
                return this.Success();
            }
            else
            {
                this.log.Value.DebugFormat("Delivery rate limit exceeded - Redelivering message {0} in {1}ms", message.ToString(), this.delayInterval.TotalMilliseconds);
                return this.Delay(this.delayInterval);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            this.refillTimer.Dispose();
        }
    }
}