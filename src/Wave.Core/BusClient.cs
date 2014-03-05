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

namespace Wave
{
    public class BusClient : IBusClient
    {
        private readonly IConfigurationContext configurationContext;
        private readonly ISubscriptionKeyResolver keyResolver;        
        private readonly ITransport transport;

        public BusClient(ITransport transport, ISubscriptionKeyResolver keyResolver, IConfigurationContext configurationContext)
        {
            this.transport = transport;            
            this.keyResolver = keyResolver;
            this.transport.InitializeForPublishing();
            this.configurationContext = configurationContext;
        }

        public IConfigurationContext Configuration
        {
            get { return this.configurationContext; }
        }

        public void Publish(object message)
        {
            this.Send(this.keyResolver.GetKey(message.GetType()), message);
        }

        public void Send(string route, object message)
        {
            if (this.PerformFilters(message.GetType(), route, message))
            {
                this.transport.Send(route, message);
            }
        }

        private bool PerformFilters(Type messageType, string route, object message)
        {
            Func<Type, bool> performFilters = (mType) =>
            {
                foreach (var filter in this.configurationContext.OutboundMessageFilters[mType].ToList())
                {
                    if (!filter.OnMessagePublished(route, message))
                    {
                        return false;
                    }
                }

                return true;
            };

            // Global Filters
            if (this.configurationContext.OutboundMessageFilters.Contains(typeof(Object)))
            {
                if (!performFilters(typeof(Object)))
                {
                    return false;
                }
            }

            // Scoped Filters
            if (this.configurationContext.OutboundMessageFilters.Contains(messageType))
            {
                if (!performFilters(messageType))
                {
                    return false;
                }                
            }
           
            return true;
        }       
    }
}