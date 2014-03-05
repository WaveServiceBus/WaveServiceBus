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

namespace Wave.Tests
{

    /// <summary>
    /// Messages / Handlers / Filters etc used by the unit tests
    /// </summary>

    [Serializable]
    public class AlwaysSuccessMessage
    {
    }

    [Serializable]
    public class AlwaysThrowsExceptionMessage
    {
    }

    [Serializable]
    public class AlwaysThrowsExceptionMessage2
    {
    }

    [Serializable]
    public class NoHandlerMessage
    {
    }

    public class TestMessageHandler : ISubscription<AlwaysSuccessMessage>, ISubscription<AlwaysThrowsExceptionMessage>
    {
        public IHandlerResult Handle(MessageEnvelope<AlwaysSuccessMessage> message)
        {
            return message.Success();
        }

        public IHandlerResult Handle(MessageEnvelope<AlwaysThrowsExceptionMessage> message)
        {
            throw new Exception("I always throw an exception.");
        }
    }

    [StopMessageEarlyFilter]
    public class AttributeBasedFilterTestHandler : ISubscription<AlwaysThrowsExceptionMessage2>
    {
        public IHandlerResult Handle(MessageEnvelope<AlwaysThrowsExceptionMessage2> message)
        {
            throw new Exception("I always throw an exception.");
        }
    }

    public class StopMessageEarlyFilter : Attribute, IInboundMessageFilter
    {
        public IHandlerResult OnHandlerExecuting(Type messageType, object message)
        {
            return this.Ignore("Ignored for test");
        }

        public IHandlerResult OnHandlerExecuted(IHandlerResult handlerResult, Type messageType, object message)
        {
            return handlerResult;
        }
    }

    public class PassMessageFilter : Attribute, IInboundMessageFilter
    {
        public IHandlerResult OnHandlerExecuted(IHandlerResult handlerResult, Type messageType, object message)
        {
            return handlerResult;
        }

        public IHandlerResult OnHandlerExecuting(Type messageType, object message)
        {
            return this.Success();
        }
    }

    public class FilterWithoutHandlerFilter : Attribute, IInboundMessageFilter
    {
        public IHandlerResult OnHandlerExecuting(Type messageType, object message)
        {
            throw new Exception("The inbound pipeline should never call me");
        }

        public IHandlerResult OnHandlerExecuted(IHandlerResult handlerResult, Type messageType, object message)
        {
            return handlerResult;
        }
    }

    public class ChainedFilterOne : Attribute, IInboundMessageFilter
    {
        public IHandlerResult OnHandlerExecuting(Type messageType, object message)
        {
            Assert.True(true);
            return this.Success();
        }

        public IHandlerResult OnHandlerExecuted(IHandlerResult handlerResult, Type messageType, object message)
        {
            return handlerResult;
        }
    }

    public class ChainedFilterTwo : Attribute, IInboundMessageFilter
    {
        public IHandlerResult OnHandlerExecuting(Type messageType, object message)
        {
            Assert.True(true);
            return this.Success();
        }

        public IHandlerResult OnHandlerExecuted(IHandlerResult handlerResult, Type messageType, object message)
        {
            return handlerResult;
        }
    }

}
