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
using Wave.Configuration;

namespace Wave.HandlerResults
{
    public class RetryResult : IHandlerResult
    {
        public RetryResult(string message)
        {
            this.Message = message;
        }

        public string Message { get; private set; }

        public static bool ShouldRetry(RawMessage message)
        {
            return message.RetryCount < ConfigurationContext.Current.MessageRetryLimit;
        }

        public void ProcessResult(RawMessage message, ITransport transport, ILogger log)
        {
            if (ShouldRetry(message))
            {
                message.RetryCount++;
                log.InfoFormat("Message requested to be retried {0} - Retry Count {1}", message.ToString(), message.RetryCount);

                // Exponential backoff with a 90% - 110% jitter added to avoid synchronized backoffs on failures.
                var delayTime = TimeSpan.FromMilliseconds((Math.Pow(4, message.RetryCount) * 100) * (1.0 + (new Random().NextDouble() - 0.5) / 5));
                message.RetryCount = message.RetryCount;

                // Convert to a delay result and process
                var delayResult = new DelayResult(DateTime.UtcNow.Add(delayTime));
                delayResult.ProcessResult(message, transport, log);
            }
            else
            {
                // Out of retries, convert to a fail result
                var failResult = new FailResult(this.Message);
                failResult.ProcessResult(message, transport, log);
            }
        }
    }
}