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
using System.Linq;
using System.Reflection;
using Wave.HandlerResults;

namespace Wave.Consumers
{
    internal class PrimaryConsumer : IConsumer
    {
        private readonly IConfigurationContext configuration;
        private readonly ISubscriptionKeyResolver keyResolver;
        private readonly Dictionary<Type, List<IInboundMessageFilter>> messageFilters;
        private readonly Dictionary<Type, Func<object, IHandlerResult>> subscriptions;

        public PrimaryConsumer(IConfigurationContext configuration)
        {
            this.configuration = configuration;
            this.keyResolver = configuration.SubscriptionKeyResolver;
            this.messageFilters = new Dictionary<Type, List<IInboundMessageFilter>>(configuration.InboundMessageFilters.ToDictionary(k => k.Key, v => new List<IInboundMessageFilter>(v)));
            this.subscriptions = new Dictionary<Type, Func<object, IHandlerResult>>(configuration.Subscriptions.ToDictionary(k => k.Key, v => v.First()));
        }

        /// <summary>
        /// Blocks waiting for the transport to deliver messages to it forever
        /// </summary>
        public void ConsumeQueue()
        {
            this.configuration.Transport.GetMessages(
                this.configuration.TokenSource.Token,
                (message, ack, reject) =>
                {
                    this.ProcessMessage(message)
                        .ProcessResult(message, this.configuration.Transport, this.configuration.Logger);

                    ack();
                });
        }

        /// <summary>
        /// Inbound message pipeline. Processes messages in this order:
        ///
        ///     Message -> Filters -> Handler
        ///
        /// Failure at any stage causes the message to be retried
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal IHandlerResult ProcessMessage(RawMessage message)
        {
            // Type mapping check, if a message arrives without a defined handler, fail it.
            var messageType = this.subscriptions.Keys.SingleOrDefault(t => this.keyResolver.GetKey(t) == message.Type);
            if (messageType == null)
            {
                return new FailResult("Unexpected message type - No Handlers defined");
            }

            // Create an instance of a MessageEnvelope from the raw message
            var messageEnvelope = Activator.CreateInstance(
                typeof(MessageEnvelope<>).MakeGenericType(messageType),
                new[]
                {
                    message.Id,
                    this.configuration.Serializer.Deserialize(message.Data, messageType),
                    message.DelayUntil,
                    message.RetryCount,
                    message.ReplyTopic,
                    message.Headers,
                });

            // Invoke OnHandlerExecuting method on filters
            var filterResult = this.PerformFilters(messageType, messageEnvelope, new SuccessResult(), filter => filter.OnHandlerExecuting(messageType, message));
            if (!(filterResult is SuccessResult))
            {
                // Filters returned something other than a success result, process that instead of invoking the handler
                return filterResult;
            }

            // Invoke handler and get handler result
            var handlerResult = this.PerformHandler(messageType, messageEnvelope, message);

            // Run OnHandlerExecuted on filters and return final result
            return this.PerformFilters(messageType, messageEnvelope, handlerResult, filter => filter.OnHandlerExecuted(handlerResult, messageType, message));
        }

        /// <summary>
        /// Invokes all chained filters in the order they were applied on the specified message
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="messageEnvelope"></param>
        /// <returns></returns>
        private IHandlerResult PerformFilters(Type messageType, object messageEnvelope, IHandlerResult handlerResult, Func<IInboundMessageFilter, IHandlerResult> filterFunc)
        {
            Func<Type, IHandlerResult, IHandlerResult> performFilters = (mType, result) =>
            {
                foreach (var filter in this.messageFilters[mType].ToList())
                {
                    try
                    {
                        result = filterFunc(filter);
                        if (!(result is SuccessResult))
                        {
                            return result;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.configuration.Logger.ErrorFormat("Unhandled exception in filter: {0}", ex.ToString());
                        return new RetryResult(ex.ToString());
                    }
                }

                return result;
            };

            // Global Filters
            if (this.messageFilters.ContainsKey(typeof(Object)))
            {
                handlerResult = performFilters(typeof(Object), handlerResult);
            }

            // Scoped Filters
            if (this.messageFilters.ContainsKey(messageType))
            {
                handlerResult = performFilters(messageType, handlerResult);
            }
           
            return handlerResult;
        }
       
        /// <summary>
        /// Invokes associated handler function for the specified message
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="messageEnvelope"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private IHandlerResult PerformHandler(Type messageType, object messageEnvelope, RawMessage message)
        {
            try
            {
                return this.subscriptions[messageType].Invoke(messageEnvelope);
            }
            catch (TargetInvocationException ex)
            {
                const string LogMessageFormat = "Unhandled exception in handler: {0}";
                if (RetryResult.ShouldRetry(message))
                {
                    // log a warning for retriable messages, since the error may just be transient
                    this.configuration.Logger.WarnFormat(LogMessageFormat, ex.InnerException.ToString());
                }
                else
                {
                    this.configuration.Logger.ErrorFormat(LogMessageFormat, ex.InnerException.ToString());                    
                }

                return new RetryResult(ex.InnerException.ToString());
            }
        }
    }
}