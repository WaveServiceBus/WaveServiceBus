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

namespace Wave.HandlerResults
{
    public class FailResult : IHandlerResult
    {
        public FailResult(string message)
        {
            this.ExceptionMessage = message;
        }

        public string ExceptionMessage { get; private set; }

        public void ProcessResult(RawMessage message, ITransport transport, ILogger log)
        {
            log.ErrorFormat("Message {0} moved to error queue", message.ToString());

            // If retry count is not reset to 0, replaying this message will immediatly fail it again
            // Store the original retry count in a header for diagnostic purposes
            if (message.RetryCount != 0)
            {
                message.Headers.Add("OriginalRetryCount", message.RetryCount.ToString());
                message.RetryCount = 0;
            }

            // Useful for diagnostics
            message.Headers.Add("ExceptionMessage", this.ExceptionMessage);

            transport.SendToError(message);
        }
    }
}