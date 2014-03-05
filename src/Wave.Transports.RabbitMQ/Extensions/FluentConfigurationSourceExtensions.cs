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
using Wave.Transports.RabbitMQ;
using Wave.Transports.RabbitMQ.Configuration;
using Wave.Transports.RabbitMQ.Extensions;

namespace Wave
{
    public static class FluentConfigurationSourceExtensions
    {
        public static FluentConfigurationSource UseRabbitMQ(this FluentConfigurationSource builder)
        {
            return UseRabbitMQ(builder, settings => { });
        }

        public static FluentConfigurationSource UseRabbitMQ(this FluentConfigurationSource builder, Action<ConfigurationSettings> settingsFactory)
        {
            var settings = new ConfigurationSettings();
            settingsFactory(settings);

            return builder.UsingTransport<RabbitMQTransport>(context =>
            {
                context.SetConnectionString(settings.ConnectionString);
                context.SetExchange(settings.Exchange);
            });
        }
    }
}