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
using Wave.Configuration;

namespace Wave
{
    public class ServiceBus
    {
        private static Lazy<IBusClient> busClient = new Lazy<IBusClient>(() => ConfigurationContext.Current.Container.Resolve<IBusClient>());

        public static IBusHost ConfigureForHosting()
        {
            return ConfigureForHosting(null);
        }

        public static IBusHost ConfigureForHosting(Action<FluentConfigurationSource> configureFunction)
        {
            var config = new ConfigurationBuilder();
            return config.ConfigureForHosting(configureFunction);
        }

        public static IBusClient ConfigureForPublishing()
        {
            return ConfigureForPublishing(null);
        }

        public static IBusClient ConfigureForPublishing(Action<FluentConfigurationSource> configureFunction)
        {
            var config = new ConfigurationBuilder();
            return config.ConfigureForPublishing(configureFunction);
        }

        public static void Publish(object message)
        {
            busClient.Value.Publish(message);
        }

        public static void Send(string route, object message)
        {
            busClient.Value.Send(route, message);
        }
    }
}