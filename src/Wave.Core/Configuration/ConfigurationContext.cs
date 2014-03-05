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
using System.Threading;

namespace Wave.Configuration
{
    public class ConfigurationContext : IConfigurationContext
    {
        private static IConfigurationContext instance;

        private readonly Dictionary<string, object> configValues;

        public ConfigurationContext()
        {
            this.configValues = new Dictionary<string, object>();
            instance = this;
        }

        public static IConfigurationContext Current
        {
            get
            {
                return instance;
            }
        }

        public IAssemblyLocator AssemblyLocator
        {
            get { return (IAssemblyLocator)this["assemblyLocator"]; }
            set { this["assemblyLocator"] = value; }
        }

        public IContainer Container
        {
            get { return (IContainer)this["container"]; }
            set { this["container"] = value; }
        }

        public ILogger Logger
        {
            get { return (ILogger)this["logger"]; }
            set { this["logger"] = value; }
        }

        public int MaxWorkers
        {
            get { return (int)this["maxWorkers"]; }
            set { this["maxWorkers"] = value; }
        }

        public ILookup<Type, IInboundMessageFilter> InboundMessageFilters
        {
            get { return (ILookup<Type, IInboundMessageFilter>)this["inboundMessageFilters"]; }
            set { this["inboundMessageFilters"] = value; }
        }

        public ILookup<Type, IOutboundMessageFilter> OutboundMessageFilters
        {
            get { return (ILookup<Type, IOutboundMessageFilter>)this["outboundMessageFilters"]; }
            set { this["outboundMessageFilters"] = value; }
        }

        public int MessageRetryLimit
        {
            get { return (int)this["messageRetryLimit"]; }
            set { this["messageRetryLimit"] = value; }
        }

        public ISerializer Serializer
        {
            get { return (ISerializer)this["serializer"]; }
            set { this["serializer"] = value; }
        }

        public ILookup<Type, Func<object, IHandlerResult>> Subscriptions
        {
            get { return (ILookup<Type, Func<object, IHandlerResult>>)this["subscriptions"]; }
            set { this["subscriptions"] = value; }
        }

        public ISubscriptionKeyResolver SubscriptionKeyResolver
        {
            get { return (ISubscriptionKeyResolver)this["subscriptionkeyresolver"]; }
            set { this["subscriptionkeyresolver"] = value; }
        }

        public IQueueNameResolver QueueNameResolver
        {            
            get { return (IQueueNameResolver)this["queuenameresolver"]; }
            set { this["queuenameresolver"] = value; }
        }

        public ITransport Transport
        {
            get { return (ITransport)this["transport"]; }
            set { this["transport"] = value; }
        }

        public CancellationTokenSource TokenSource
        {
            get { return (CancellationTokenSource)this["tokenSource"]; }
            set { this["tokenSource"] = value; }
        }

        public object this[string name]
        {
            get
            {
                if (this.configValues.ContainsKey(name))
                {
                    return this.configValues[name];
                }

                return null;
            }

            set
            {
                if (this.configValues.ContainsKey(name))
                {
                    this.configValues[name] = value;
                }
                else
                {
                    this.configValues.Add(name, value);
                }
            }
        }     
    }
}