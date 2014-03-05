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
using System.Linq;
using System.Threading;
using Wave.Consumers;
using Wave.Defaults;

namespace Wave.Configuration
{
    public class ConfigurationBuilder : ConfigurationSource
    {
        public IBusHost ConfigureForHosting()
        {
            return this.ConfigureForHosting(null);
        }

        public IBusHost ConfigureForHosting(Action<FluentConfigurationSource> configurationFunction)
        {
            this.ConfigureAndCreateContext(configurationFunction);
            return this.Container.Resolve<IBusHost>();
        }

        public IBusClient ConfigureForPublishing()
        {
            return this.ConfigureForPublishing(null);
        }

        public IBusClient ConfigureForPublishing(Action<FluentConfigurationSource> configurationFunction)
        {
            this.ConfigureAndCreateContext(configurationFunction);
            return this.Container.Resolve<IBusClient>();
        }

        /// <summary>
        /// Configures internal container and creates all needed instances to run the bus
        /// </summary>
        internal void ConfigureAndCreateContext(Action<FluentConfigurationSource> configurationFunction)
        {
            var fluentConfig = new FluentConfigurationSource();
            if (configurationFunction != null)
            {
                // Populate the FluentConfigurationSource
                configurationFunction(fluentConfig);
                this.Populate(fluentConfig);
            }

            if (this.Container == null)
            {
                // Create a DefaultContainer, register it to itself
                // and then request it to resolve a instance of itself.
                //
                // This looks funky, but it ensures that everything is using the same container instance.
                this.Container = new DefaultContainer();
                this.Container.Register<IContainer, DefaultContainer>(InstanceScope.Singleton);
                this.Container = this.Container.Resolve<IContainer>();
            }

            // Create the global configuration context and set default values
            this.Container.Register<IConfigurationContext, ConfigurationContext>(InstanceScope.Singleton);
            this.ConfigurationContext = this.Container.Resolve<IConfigurationContext>();
            this.ConfigurationContext.Container = this.Container;
            this.ConfigurationContext.TokenSource = new CancellationTokenSource();

            // Parse Fluent and XML configuration - This builds up TypeMap so
            // RegisterTypeOrDefault below can grab the configured type for each
            // component
            this.Populate(fluentConfig.BuildUpConfiguration(this));
            this.Populate(new XMLConfigurationSource().BuildUpConfiguration(this));

            // Register Concrete types that will be used
            this.Container.Register<PrimaryConsumer, PrimaryConsumer>(InstanceScope.Singleton);
            this.Container.Register<DelayConsumer, DelayConsumer>(InstanceScope.Singleton);

            // Register all configured objects to the container
            this.RegisterTypeOrDefault<IAssemblyLocator, DefaultAssemblyLocator>();
            this.RegisterTypeOrDefault<ISubscriptionKeyResolver, DefaultSubscriptionKeyResolver>();
            this.RegisterTypeOrDefault<IQueueNameResolver, DefaultQueueNameResolver>();
            this.RegisterTypeOrDefault<ILogger, DefaultLogger>();
            this.RegisterTypeOrDefault<IBusClient, BusClient>();
            this.RegisterTypeOrDefault<ISerializer, DefaultSerializer>();
            this.RegisterTypeOrDefault<IBusHost, DefaultHost>();
            this.RegisterTypeOrDefault<ITransport, DefaultTransport>();
           
            // With TypeMap built, we can resolve the global instances for these now.
            this.ConfigurationContext.AssemblyLocator = this.Container.Resolve<IAssemblyLocator>();
            this.ConfigurationContext.SubscriptionKeyResolver = this.Container.Resolve<ISubscriptionKeyResolver>();
            this.ConfigurationContext.QueueNameResolver = this.Container.Resolve<IQueueNameResolver>();
            this.ConfigurationContext.Logger = this.Container.Resolve<ILogger>();
            this.ConfigurationContext.Serializer = this.Container.Resolve<ISerializer>();
            this.ConfigurationContext.Transport = this.Container.Resolve<ITransport>();
            
            // Final config pass is to build up subscriptions and filters via reflection
            this.Populate(new ReflectionConfigurationSource().BuildUpConfiguration(this));

            // Set subscriptions and filters into the context
            this.ConfigurationContext.Subscriptions = this.Subscriptions.ToLookup(k => k.Key, v => v.Value);
            
            this.ConfigurationContext.InboundMessageFilters = this.InboundMessageFilters
                    .SelectMany(p => p.Value.Distinct().Select(x => new { p.Key, Value = x }))
                    .ToLookup(pair => pair.Key, pair => pair.Value);

            this.ConfigurationContext.OutboundMessageFilters = this.OutboundMessageFilters
                  .SelectMany(p => p.Value.Distinct().Select(x => new { p.Key, Value = x }))
                  .ToLookup(pair => pair.Key, pair => pair.Value);            
        }
    }
}