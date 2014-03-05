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

namespace Wave.Configuration
{
    public class FluentConfigurationSource : ConfigurationSource
    {
        public FluentConfigurationSource UsingAssemblyLocator<T>()
            where T : IAssemblyLocator
        {
            return this.UsingAssemblyLocator<T>(null);
        }

        public FluentConfigurationSource UsingAssemblyLocator<T>(Action<IConfigurationContext> configCallback)
         where T : IAssemblyLocator
        {
            this.MapType<IAssemblyLocator, T>(configCallback);
            return this;
        }

        public FluentConfigurationSource UsingContainer(IContainer container)
        {
            this.Container = container;
            return this;
        }

        public FluentConfigurationSource UsingLogger<T>()
           where T : ILogger
        {
            return this.UsingLogger<T>(null);
        }

        public FluentConfigurationSource UsingLogger<T>(Action<IConfigurationContext> configCallback)
            where T : ILogger
        {
            this.MapType<ILogger, T>(configCallback);
            return this;
        }

        public FluentConfigurationSource UsingQueueNameResolver<T>()
            where T : IQueueNameResolver
        {
            this.MapType<IQueueNameResolver, T>(null);
            return this;
        }

        public FluentConfigurationSource UsingSerializer<T>()
           where T : ISerializer
        {
            return this.UsingSerializer<T>(null);
        }

        public FluentConfigurationSource UsingSerializer<T>(Action<IConfigurationContext> configCallback)
           where T : ISerializer
        {
            this.MapType<ISerializer, T>(configCallback);
            return this;
        }

        public FluentConfigurationSource UsingServiceHost<T>()
            where T : IBusHost 
        {
            return this.UsingServiceHost<T>(null);
        }

        public FluentConfigurationSource UsingServiceHost<T>(Action<IConfigurationContext> configCallback)
            where T : IBusHost
        {
            this.MapType<IBusHost, T>(configCallback);
            return this;
        }

        public FluentConfigurationSource UsingSubscriptionKeyResolver<T>()
            where T : ISubscriptionKeyResolver
        {
            this.MapType<ISubscriptionKeyResolver, T>(null);
            return this;
        }

        public FluentConfigurationSource UsingTransport<T>()
            where T : ITransport
        {
            return this.UsingTransport<T>(null);
        }

        public FluentConfigurationSource UsingTransport<T>(Action<IConfigurationContext> configCallback)
            where T : ITransport
        {
            this.MapType<ITransport, T>(configCallback);
            return this;
        }

        public FluentConfigurationSource WithGlobalInboundMessageFilter(IInboundMessageFilter filter)            
        {
            this.WithInboundMessageFilter(typeof(object), filter);
            return this;
        }

        public FluentConfigurationSource WithGlobalOutboundMessageFilter(IOutboundMessageFilter filter)
        {
            this.WithOutboundMessageFilter(typeof(object), filter);
            return this;
        }

        public FluentConfigurationSource WithInboundMessageFilter(Type messageType, IInboundMessageFilter filter)
        {
            if (this.InboundMessageFilters.ContainsKey(messageType))
            {
                this.InboundMessageFilters[messageType].Add(filter);
            }
            else
            {
                this.InboundMessageFilters.Add(messageType, new List<IInboundMessageFilter>(new[] { filter }));
            }

            return this;
        }

        public FluentConfigurationSource WithInboundMessageFilter<TMessage>(IInboundMessageFilter filter)
        {
            this.WithInboundMessageFilter(typeof(TMessage), filter);
            return this;
        }

        public FluentConfigurationSource WithMaxWorkers(int maxWorkers)
        {
            this.MaxWorkers = maxWorkers;
            return this;
        }

        public FluentConfigurationSource WithMessageRetryLimit(int retryLimit)
        {
            this.MessageRetryLimit = retryLimit;
            return this;
        }

        public FluentConfigurationSource WithOutboundMessageFilter(Type messageType, IOutboundMessageFilter filter)
        {
            if (this.OutboundMessageFilters.ContainsKey(messageType))
            {
                this.OutboundMessageFilters[messageType].Add(filter);
            }
            else
            {
                this.OutboundMessageFilters.Add(messageType, new List<IOutboundMessageFilter>(new[] { filter }));
            }

            return this;
        }

        public FluentConfigurationSource WithOutboundMessageFilter<TMessage>(IOutboundMessageFilter filter)
        {
            this.WithOutboundMessageFilter(typeof(TMessage), filter);
            return this;
        }

        public FluentConfigurationSource WithSubscription<TMessage>(Func<MessageEnvelope<TMessage>, IHandlerResult> handler)
        {
            this.Subscriptions.Add(typeof(TMessage), message => handler((MessageEnvelope<TMessage>)message));
            return this;
        }

        internal override ConfigurationSource BuildUpConfiguration(ConfigurationSource previousSource)
        {
            this.Populate(previousSource);
            this.ConfigurationContext.MaxWorkers = this.MaxWorkers;
            this.ConfigurationContext.MessageRetryLimit = this.MessageRetryLimit;

            return this;
        }
    }
}