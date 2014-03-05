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
using System.Collections.Generic;
using Wave.HandlerResults;

namespace Wave
{
    public class MessageEnvelope<T>
    {
        public MessageEnvelope(
            Guid id,
            T content,
            DateTime? delayUntil,
            int retryCount,
            string replyTopic,
            Dictionary<string, string> headers)
        {
            this.Id = id;
            this.Content = content;
            this.RetryCount = retryCount;
            this.Headers = headers;
            this.DelayUntil = delayUntil;
            this.ReplyTopic = replyTopic;
        }

        public T Content { get; private set; }

        public DateTime? DelayUntil { get; private set; }

        public Dictionary<string, string> Headers { get; private set; }

        public Guid Id { get; private set; }

        public String ReplyTopic { get; private set; }

        public int RetryCount { get; private set; }

        public bool WasDelayed
        {
            get { return this.DelayUntil.HasValue && this.DelayUntil.Value < DateTime.UtcNow; }
        }

        /// <summary>
        /// Requests that the message be redelivered at a future date.
        /// </summary>
        /// <param name="delayUntil"></param>
        /// <returns></returns>
        public IHandlerResult Delay(DateTime delayUntil)
        {
            return new DelayResult(delayUntil);
        }

        /// <summary>
        /// Requests that the message be redelivered at a future date.
        /// </summary>
        /// <param name="delayUntil"></param>
        /// <returns></returns>
        public IHandlerResult Delay(TimeSpan delayUntil)
        {
            return new DelayResult(DateTime.UtcNow.Add(delayUntil));
        }

        /// <summary>
        /// Explicitly fails a message. The message will not be retried.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public IHandlerResult Fail(string message)
        {
            return new FailResult(message);
        }

        /// <summary>
        /// Sends a reply message directly back to the original publisher
        /// </summary>
        /// <param name="replyMessage"></param>
        /// <returns></returns>
        public IHandlerResult Reply(object replyMessage)
        {
            return new ReplyResult(replyMessage);
        }

        /// <summary>
        /// Requests that incoming message be retried unless the message has hit the configured max retries value.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public IHandlerResult Retry(string message)
        {
            return new RetryResult(message);
        }

        /// <summary>
        /// Signals that the message handle processed the incoming message with no issues.
        /// </summary>
        /// <returns></returns>
        public IHandlerResult Success()
        {
            return new SuccessResult();
        }
    }
}