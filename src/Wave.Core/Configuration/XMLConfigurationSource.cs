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
using System.Configuration;
using System.Linq;
using System.Reflection;
using Wave.Utility;

namespace Wave.Configuration
{
    public class XMLConfigurationSource : ConfigurationSource
    {
        internal override ConfigurationSource BuildUpConfiguration(ConfigurationSource previousSource)
        {
            this.Populate(previousSource);

            var configSection = ConfigurationHelper.GetConfigSection<ConfigurationSection>();
            if (configSection == null)
            {
                return this;
            }

            // Basic Settings
            this.ConfigurationContext.MaxWorkers = configSection.Settings.MaxWorkers != default(int)
                ? configSection.Settings.MaxWorkers
                : ConfigurationContext.MaxWorkers;

            this.ConfigurationContext.MessageRetryLimit = configSection.Settings.MessageRetryLimit != default(int)
                ? configSection.Settings.MessageRetryLimit
                : ConfigurationContext.MessageRetryLimit;

            // Type mappings
            this.MergeTypeFromConfig<ILogger>(configSection.Logger);
            this.MergeTypeFromConfig<ISerializer>(configSection.Serializer);
            this.MergeTypeFromConfig<ISubscriptionKeyResolver>(configSection.SubscriptionKeyResolver);
            this.MergeTypeFromConfig<IQueueNameResolver>(configSection.QueueNameResolver);
            this.MergeTypeFromConfig<IBusHost>(configSection.ServiceHost);
            this.MergeTypeFromConfig<ITransport>(configSection.Transport);

            // Message Filters
            this.ConfigureFilters<IInboundMessageFilter>(configSection.InboundMessageFilters, this.InboundMessageFilters);
            this.ConfigureFilters<IOutboundMessageFilter>(configSection.OutboundMessageFilters, this.OutboundMessageFilters);

            return this;
        }

        private static object CreateFilterInstance(ConfigurationSection.MessageFilterConfigElement filter)
        {
            var filterType = Type.GetType(filter.Type);
            if (filterType == null)
            {
                throw new ConfigurationErrorsException(string.Format("Unable to locate filter: {0}", filter.Type));
            }

            // Use Activator.CreateInstance here, as Message Filters should not be singletons, and need a
            // unique instance per message handler.
            return Activator.CreateInstance(filterType);
        }

        /// <summary>
        /// Uses reflection to marshal XML attributes into public properties on the filter
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="filterInstance"></param>
        private static void SetFilterProperties(ConfigurationSection.MessageFilterConfigElement filter, object filterInstance)
        {
            // Filters configured via XML can have their properties set in XML, these show up in the AdditionalAttributes collection
            // If there is data here, stuff it into the filter using reflection to find matching properties
            if (filter.AdditionalAttributes != null)
            {
                var filterProps = filterInstance.GetType().GetProperties();
                foreach (var kvp in filter.AdditionalAttributes)
                {
                    var prop = filterProps.FirstOrDefault(p => p.Name.ToUpper() == kvp.Key.ToUpper());
                    if (prop != default(PropertyInfo))
                    {
                        prop.SetValue(filterInstance, Convert.ChangeType(kvp.Value, prop.PropertyType), null);
                    }
                }
            }
        }

        private void ConfigureFilters<T>(
            ConfigurationSection.MessageFiltersCollection filters,
            Dictionary<Type, List<T>> filterDictionary)
        {
            foreach (ConfigurationSection.MessageFilterConfigElement filter in filters)
            {                              
                var messageType = Type.GetType(filter.Message);

                if (filter.Message == "*")
                {
                    messageType = typeof(object);
                }
              
                if (messageType == null)
                {
                    throw new ConfigurationErrorsException(string.Format("Unable to locate message: {0}", filter.Message));
                }

                var filterInstance = CreateFilterInstance(filter);
                SetFilterProperties(filter, filterInstance);

                if (filterDictionary.ContainsKey(messageType))
                {
                    filterDictionary[messageType].Add((T)filterInstance);
                }
                else
                {
                    filterDictionary.Add(messageType, new List<T>(new[] { (T)filterInstance }));
                }
            }
        }

        /// <summary>
        /// Updates the type mappings for type T to the configured type
        /// </summary>
        /// <typeparam name="T">The type to configure from the config</typeparam>
        /// <param name="config">The configuration section element</param>
        private void MergeTypeFromConfig<T>(ConfigurationSection.ExtensionElement config)
        {
            if (config == null || config.Type == string.Empty)
            {
                return;
            }

            var type = Type.GetType(config.Type);
            if (type == null)
            {
                throw new ConfigurationErrorsException(string.Format("Unable to load {0} - Check config file and also ensure the type is in a referenced assembly.", config.Type));
            }

            // Check if there is no mapping at all
            if (!this.TypeMap.ContainsKey(typeof(T)))
            {
                this.TypeMap.Add(typeof(T), new MapHelper
                {
                    DestinationType = type
                });

                return;
            }

            // Check if the mapping is being changed
            if (this.TypeMap[typeof(T)].DestinationType != type)
            {
                // Config file is overriding the type map
                this.TypeMap[typeof(T)] = new MapHelper
                {
                    DestinationType = type
                };
            }
        }
    }
}