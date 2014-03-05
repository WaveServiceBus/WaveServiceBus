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

namespace Wave
{
    /// <summary>
    /// Message Filters are additional code units that can be placed in the inboundpipeline to perform work outside
    /// of the message handler function.
    /// </summary>
    public interface IInboundMessageFilter
    {
        /// <summary>
        /// Invoked after dispatching to the handler method
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        IHandlerResult OnHandlerExecuted(IHandlerResult handlerResult, Type messageType, object message);

        /// <summary>
        /// Invoked before dispatching to the handler method
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        IHandlerResult OnHandlerExecuting(Type messageType, object message);
    }
}