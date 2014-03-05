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
using System.Configuration;
using System.Linq;

namespace Wave.Configuration
{
    public class ReflectionConfigurationSource : ConfigurationSource
    {
        internal override ConfigurationSource BuildUpConfiguration(ConfigurationSource previousSource)
        {
            this.Populate(previousSource);
            this.FindSubscribers();
            this.FindMessageFilters();
            return this;
        }

        /// <summary>
        /// Discovers and maps subscribers based on scanning for ISubscription interfaces
        /// </summary>
        private void FindSubscribers()
        {
            // Helper function that identifies if a type is a message handler
            Func<Type, bool> isMessageHandler = type =>
            {
                return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISubscription<>));
            };

            // TODO: Cache the function itself, instead of calling GetMethod on each incoming message?
            Func<object, Type, Func<object, IHandlerResult>> getHandlerFunction = (handler, messageType) =>
            {
                return m => (IHandlerResult)handler.GetType()
                                .GetMethod("Handle", new[]
                                    {
                                        typeof(MessageEnvelope<>).MakeGenericType(messageType)
                                    })
                                .Invoke(handler, new[] { m });
            };

            // Find all handlers in the entry assembly
            var handlerTypes = this.ConfigurationContext.AssemblyLocator.GetSubscriberAssemblies()
                .SelectMany(a => a.GetTypes())                
                .Where(isMessageHandler)
                .ToList();

            // Build a distinct list of all the messages that handlers consume
            var handledMessageTypes = handlerTypes
                .SelectMany(h => h.GetInterfaces())
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISubscription<>))
                .Select(i => i.GetGenericArguments().First())
                .Distinct()
                .ToList();

            // Create a instance of each handler
            var handlerInstances = handlerTypes.Select(h => 
                {
                    this.ConfigurationContext.Container.Register(h, h, InstanceScope.Singleton);
                    return this.ConfigurationContext.Container.Resolve(h);
                }).ToList();

            // Create a direct mapping between each message and the Handle function in the handler that
            // will handle them
            foreach (var message in handledMessageTypes)
            {
                var handlerFunctions = handlerTypes
                    .Where(h => h.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericArguments().First() == message))
                    .Select(h => getHandlerFunction(handlerInstances.First(hi => hi.GetType() == h), message));

                if (this.Subscriptions.ContainsKey(message))
                {
                    throw new ConfigurationErrorsException(string.Format("Multiple subscriptions registered for message type {0}. Only one subscription per message type is allowed.", message.Name));
                }

                this.Subscriptions.Add(message, handlerFunctions.First());
            }
        }

        /// <summary>
        /// Discovers and maps message filters applied via attributes
        /// </summary>
        private void FindMessageFilters()
        {
            // Scan for additional filters applied via attribute
            var handlers = this.ConfigurationContext.AssemblyLocator.GetSubscriberAssemblies().SelectMany(a => a.GetTypes())
                // Match all types that implement ISubscription<T>
                .Where(type => type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISubscription<>)))
                .Select(h => new
                {
                    // Extract all handled messages from the handler
                    Messages = h.GetInterfaces()
                                  .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISubscription<>))
                                  .Select(i => i.GetGenericArguments().First())
                                  .Distinct()
                                  .ToList(),
                    // Extract all attribute based filters from the handler
                    Filters = h.GetCustomAttributes(true)
                                    .Where(a => typeof(IInboundMessageFilter).IsAssignableFrom(a.GetType()))
                                    .Cast<IInboundMessageFilter>()
                                    .ToList()
                })
                .ToList();

            foreach (var handler in handlers)
            {
                foreach (var handledMessage in handler.Messages)
                {
                    if (this.InboundMessageFilters.ContainsKey(handledMessage))
                    {
                        this.InboundMessageFilters[handledMessage] = this.InboundMessageFilters[handledMessage].Concat(handler.Filters).ToList();
                    }
                    else
                    {
                        this.InboundMessageFilters.Add(handledMessage, handler.Filters);
                    }
                }
            }

            // Merge global filters into each type
            if (this.InboundMessageFilters.ContainsKey(typeof(object)))
            {
                foreach (var filter in this.InboundMessageFilters.Keys)
                {
                    if (filter != typeof(object))
                    {
                        this.InboundMessageFilters[filter].AddRange(this.InboundMessageFilters[typeof(object)]);
                    }
                }
            }
        }
    }
}