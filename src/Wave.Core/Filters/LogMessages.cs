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

namespace Wave.Filters
{
    /// <summary>
    /// Log Messages will emit a Informational level log entry before and after invoking each message handler
    /// </summary>
    public class LogMessages : Attribute, IInboundMessageFilter, IOutboundMessageFilter
    {
        private readonly Lazy<ILogger> log = new Lazy<ILogger>(() => ConfigurationContext.Current.Container.Resolve<ILogger>());

        public IHandlerResult OnHandlerExecuted(IHandlerResult handlerResult, Type messageType, object message)
        {
            this.log.Value.InfoFormat("Processed {0} message. Result {1}", messageType, handlerResult);
            return handlerResult;
        }

        public IHandlerResult OnHandlerExecuting(Type messageType, object message)
        {
            this.log.Value.InfoFormat("Received {0} message.", messageType);
            return this.Success();
        }

        public bool OnMessagePublished(string routeKey, object message)
        {
            this.log.Value.InfoFormat("Published {0} message to route {1}.", message.ToString(), routeKey);
            return true;
        }
    }
}