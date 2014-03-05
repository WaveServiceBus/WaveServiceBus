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

using System.Collections.Generic;
using System.Configuration;

namespace Wave.Configuration
{
    public class ConfigurationSection : System.Configuration.ConfigurationSection
    {
        [ConfigurationProperty("inboundMessageFilters", IsRequired = false)]
        public MessageFiltersCollection InboundMessageFilters
        {
            get { return (MessageFiltersCollection)base["inboundMessageFilters"]; }
            set { base["inboundMessageFilters"] = value; }
        }

        [ConfigurationProperty("logger", IsRequired = false)]
        public ExtensionElement Logger
        {
            get { return (ExtensionElement)base["logger"]; }
            set { base["logger"] = value; }
        }

        [ConfigurationProperty("outboundMessageFilters", IsRequired = false)]
        public MessageFiltersCollection OutboundMessageFilters
        {
            get { return (MessageFiltersCollection)base["outboundMessageFilters"]; }
            set { base["outboundMessageFilters"] = value; }
        }

        [ConfigurationProperty("queueNameResolver", IsRequired = false)]
        public ExtensionElement QueueNameResolver
        {
            get { return (ExtensionElement)base["queueNameResolver"]; }
            set { base["queueNameResolver"] = value; }
        }

        [ConfigurationProperty("serializer", IsRequired = false)]
        public ExtensionElement Serializer
        {
            get { return (ExtensionElement)base["serializer"]; }
            set { base["serializer"] = value; }
        }

        [ConfigurationProperty("serviceHost", IsRequired = false)]
        public ExtensionElement ServiceHost
        {
            get { return (ExtensionElement)base["serviceHost"]; }
            set { base["serviceHost"] = value; }
        }

        [ConfigurationProperty("settings", IsRequired = false)]
        public SettingsElement Settings
        {
            get { return (SettingsElement)base["settings"]; }
            set { base["settings"] = value; }
        }

        [ConfigurationProperty("subscriptionKeyResolver", IsRequired = false)]
        public ExtensionElement SubscriptionKeyResolver
        {
            get { return (ExtensionElement)base["subscriptionKeyResolver"]; }
            set { base["subscriptionKeyResolver"] = value; }
        }

        [ConfigurationProperty("transport", IsRequired = false)]
        public ExtensionElement Transport
        {
            get { return (ExtensionElement)base["transport"]; }
            set { base["transport"] = value; }
        }

        public class ExtensionElement : ConfigurationElement
        {
            [ConfigurationProperty("type", IsRequired = true)]
            public string Type
            {
                get { return (string)this["type"]; }
                set { this["type"] = value; }
            }
        }

        public class MessageFilterConfigElement : ConfigurationElement
        {
            public MessageFilterConfigElement()
            {
            }

            public MessageFilterConfigElement(string message, string type)
            {
                this.Message = message;
                this.Type = type;
            }

            public Dictionary<string, string> AdditionalAttributes
            {
                get;
                set;
            }

            [ConfigurationProperty("message", IsRequired = true, IsKey = true)]
            public string Message
            {
                get { return (string)this["message"]; }
                set { this["message"] = value; }
            }

            [ConfigurationProperty("type", IsRequired = true, IsKey = true)]
            public string Type
            {
                get { return (string)this["type"]; }
                set { this["type"] = value; }
            }

            protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
            {
                // Any unrecognized attribute is tossed into the AdditionalProperties collection
                // This is used to set additional properties on the filter instance during filter
                // creation
                if (this.AdditionalAttributes == null)
                {
                    this.AdditionalAttributes = new Dictionary<string, string>();
                }

                this.AdditionalAttributes[name] = value;
                return true;
            }
        }

        [ConfigurationCollection(typeof(MessageFilterConfigElement), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
        public class MessageFiltersCollection : ConfigurationElementCollection
        {
            public override ConfigurationElementCollectionType CollectionType
            {
                get
                {
                    return ConfigurationElementCollectionType.AddRemoveClearMap;
                }
            }

            public MessageFilterConfigElement this[int index]
            {
                get
                {
                    return (MessageFilterConfigElement)BaseGet(index);
                }

                set
                {
                    if (this.BaseGet(index) != null)
                    {
                        this.BaseRemoveAt(index);
                    }

                    this.BaseAdd(index, value);
                }
            }

            public void Add(string message, string type)
            {
                this.BaseAdd(new MessageFilterConfigElement(message, type));
            }

            protected override ConfigurationElement CreateNewElement()
            {
                return new MessageFilterConfigElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((MessageFilterConfigElement)element).Type + ((MessageFilterConfigElement)element).Message;
            }
        }

        public class SettingsElement : ConfigurationElement
        {
            [ConfigurationProperty("maxWorkers", IsRequired = false)]
            public int MaxWorkers
            {
                get { return (int)base["maxWorkers"]; }
                set { base["maxWorkers"] = value; }
            }

            [ConfigurationProperty("messageRetryLimit", IsRequired = false)]
            public int MessageRetryLimit
            {
                get { return (int)base["messageRetryLimit"]; }
                set { base["messageRetryLimit"] = value; }
            }
        }
    }
}