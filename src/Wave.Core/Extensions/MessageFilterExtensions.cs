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
using Wave.HandlerResults;

namespace Wave
{
    public static class MessageFilterExtensions
    {
        public static IHandlerResult Delay(this IInboundMessageFilter @this, DateTime delayUntil)
        {
            return new DelayResult(delayUntil);
        }

        public static IHandlerResult Delay(this IInboundMessageFilter @this, TimeSpan delayUntil)
        {
            return Delay(@this, DateTime.UtcNow.Add(delayUntil));
        }

        public static IHandlerResult Fail(this IInboundMessageFilter @this, string message)
        {
            return new FailResult(message);
        }

        public static IHandlerResult Ignore(this IInboundMessageFilter @this, string reason)
        {
            return new IgnoreResult(reason);
        }

        public static IHandlerResult Retry(this IInboundMessageFilter @this, string message)
        {
            return new RetryResult(message);
        }

        public static IHandlerResult Success(this IInboundMessageFilter @this)
        {
            return new SuccessResult();
        }
    }
}