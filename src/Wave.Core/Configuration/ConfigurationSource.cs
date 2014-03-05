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
    public class ConfigurationSource
    {
        private IConfigurationContext configContext;
        private IContainer container;

        private int maxWorkers = 1;
        private Dictionary<Type, List<IInboundMessageFilter>> inboundMessageFilters = new Dictionary<Type, List<IInboundMessageFilter>>();
        private Dictionary<Type, List<IOutboundMessageFilter>> outboundMessageFilters = new Dictionary<Type, List<IOutboundMessageFilter>>();
        private int messageRetryLimit = 5;
        private Dictionary<Type, Func<object, IHandlerResult>> subscriptions = new Dictionary<Type, Func<object, IHandlerResult>>();
        private Dictionary<Type, MapHelper> typeMap = new Dictionary<Type, MapHelper>();

        internal IConfigurationContext ConfigurationContext
        {
            get { return this.configContext; }
            set { this.configContext = value; }
        }

        internal IContainer Container
        {
            get { return this.container; }
            set { this.container = value; }
        }
      
        internal int MaxWorkers
        {
            get { return this.maxWorkers; }
            set { this.maxWorkers = value; }
        }

        internal Dictionary<Type, List<IInboundMessageFilter>> InboundMessageFilters
        {
            get { return this.inboundMessageFilters; }
            set { this.inboundMessageFilters = value; }
        }

        internal Dictionary<Type, List<IOutboundMessageFilter>> OutboundMessageFilters
        {
            get { return this.outboundMessageFilters; }
            set { this.outboundMessageFilters = value; }
        }

        internal int MessageRetryLimit
        {
            get { return this.messageRetryLimit; }
            set { this.messageRetryLimit = value; }
        }

        internal Dictionary<Type, Func<object, IHandlerResult>> Subscriptions
        {
            get { return this.subscriptions; }
            set { this.subscriptions = value; }
        }

        internal Dictionary<Type, MapHelper> TypeMap
        {
            get { return this.typeMap; }
            set { this.typeMap = value; }
        }

        internal virtual ConfigurationSource BuildUpConfiguration(ConfigurationSource previousSource)
        {
            throw new NotImplementedException();
        }

        internal ConfigurationSource Populate(ConfigurationSource source)
        {
            if (source != null)
            {
                this.ConfigurationContext = source.ConfigurationContext;
                this.Container = source.Container;
                this.Subscriptions = source.Subscriptions;
                this.MaxWorkers = source.MaxWorkers;
                this.InboundMessageFilters = source.InboundMessageFilters;
                this.OutboundMessageFilters = source.OutboundMessageFilters;
                this.MessageRetryLimit = source.MessageRetryLimit;
                this.TypeMap = source.TypeMap;
            }

            return this;
        }

        protected virtual void MapType<T, T1>(Action<IConfigurationContext> configCallback)
        {
            if (this.typeMap.ContainsKey(typeof(T)))
            {
                this.typeMap[typeof(T)] = new MapHelper
                {
                    DestinationType = typeof(T1),
                    Callback = configCallback
                };
            }
            else
            {
                this.typeMap.Add(typeof(T), new MapHelper
                {
                    DestinationType = typeof(T1),
                    Callback = configCallback
                });
            }
        }

        protected void RegisterTypeOrDefault<TInterface, TDefault>()
        {
            if (this.typeMap.ContainsKey(typeof(TInterface)))
            {
                this.container.Register(typeof(TInterface), this.typeMap[typeof(TInterface)].DestinationType, InstanceScope.Singleton);

                // This allows implementations to access the config context at creation time
                // to allow them to push additional settings into it.
                if (this.typeMap[typeof(TInterface)].Callback != null)
                {
                    // Pass the config context backed to the registed type
                    this.typeMap[typeof(TInterface)].Callback(this.configContext);
                }
            }
            else
            {
                this.container.Register(typeof(TInterface), typeof(TDefault), InstanceScope.Singleton);
            }
        }

        internal class MapHelper
        {
            public Action<IConfigurationContext> Callback { get; set; }

            public Type DestinationType { get; set; }
        }
    }
}