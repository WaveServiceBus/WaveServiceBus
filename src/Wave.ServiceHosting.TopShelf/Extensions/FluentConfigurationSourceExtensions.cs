
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
using Wave.ServiceHosting.TopShelf;
using Wave.ServiceHosting.TopShelf.Configuration;

namespace Wave
{
    public static class FluentConfigurationSourceExtensions
    {
        public static FluentConfigurationSource UseTopShelf(this FluentConfigurationSource builder)        
        {
            return UseTopShelf(builder, settings => { });
        }

        public static FluentConfigurationSource UseTopShelf(this FluentConfigurationSource builder, Action<ConfigurationSettings> settingsFactory)
        {
            var settings = new ConfigurationSettings();
            settingsFactory(settings);

            return builder.UsingServiceHost<TopShelfHost>(context =>
            {
                context.SetDescription(settings.Description);
                context.SetDisplayName(settings.DisplayName);
                context.SetPassword(settings.Password);
                context.SetServiceName(settings.ServiceName);
                context.SetUsername(settings.Username);
                context.SetUserType(settings.UserType);
            });            
        }
    }
}
