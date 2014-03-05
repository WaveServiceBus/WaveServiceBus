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
using Topshelf;
using Topshelf.Logging;
using Wave.ServiceHosting.TopShelf.Logging;
using Wave.Utility;

namespace Wave.ServiceHosting.TopShelf
{
    /// <summary>
    /// BusHost implementation that hosts the wave service host inside of a topshelf host.
    /// This allows for running the service as both a console application or a windows service
    /// </summary>
    public class TopShelfHost : BusHost
    {        
        public TopShelfHost(IConfigurationContext configuration) : base(configuration)
        {
            this.ConfigurationContext = MergeConfiguration(configuration);
        }

        private IConfigurationContext MergeConfiguration(IConfigurationContext configuration)
        {
            var defaultSettings = new Configuration.ConfigurationSettings();
            var configSection = ConfigurationHelper.GetConfigSection<Configuration.ConfigurationSection>();

            if (configSection != null)
            {
                if (!String.IsNullOrWhiteSpace(configSection.RunAs.UserType))
                {
                    switch (configSection.RunAs.UserType.ToUpper())
                    {
                        case "LOCALSERVICE":
                            configuration.SetUserType(Configuration.RunAs.LocalService);
                            break;
                        case "LOCALSYSTEM":
                            configuration.SetUserType(Configuration.RunAs.LocalSystem);
                            break;
                        case "NETWORKSERVICE":
                            configuration.SetUserType(Configuration.RunAs.NetworkService);
                            break;
                        case "USER":
                            configuration.SetUserType(Configuration.RunAs.User);
                            break;
                    }

                    if (!String.IsNullOrWhiteSpace(configSection.RunAs.Username))
                    {
                        configuration.SetUsername(configSection.RunAs.Username);
                    }

                    if (!String.IsNullOrWhiteSpace(configSection.RunAs.Password))
                    {
                        configuration.SetPassword(configSection.RunAs.Password);
                    }
                }

                if (!String.IsNullOrWhiteSpace(configSection.Service.Description))
                {
                    configuration.SetDescription(configSection.Service.Description);
                }

                if (!String.IsNullOrWhiteSpace(configSection.Service.DisplayName))
                {
                    configuration.SetDisplayName(configSection.Service.DisplayName);
                }

                if (!String.IsNullOrWhiteSpace(configSection.Service.Name))
                {
                    configuration.SetServiceName(configSection.Service.Name);
                }
            }

            // Set Defaults
            if (String.IsNullOrWhiteSpace(configuration.GetServiceName()))
            {
                configuration.SetServiceName(configuration.AssemblyLocator.GetEntryAssembly().GetName().Name);
            }

            if (String.IsNullOrWhiteSpace(configuration.GetDescription()))
            {
                configuration.SetDescription("Wave Service Host");
            }

            if (String.IsNullOrWhiteSpace(configuration.GetDisplayName()))
            {
                configuration.SetDisplayName(configuration.AssemblyLocator.GetEntryAssembly().GetName().Name);
            }

            return configuration;
        }

        public override void Start()
        {
            // Wires up the internal topshelf logger to the same logger that wave is configured to use
            HostLogger.UseLogger(new WaveLogWriterConfigurator(this.ConfigurationContext.Logger));

            HostFactory.Run(x =>
            {
                switch (this.ConfigurationContext.GetUserType())
                {
                    case Configuration.RunAs.LocalService:
                        x.RunAsLocalService();
                        break;
                    case Configuration.RunAs.LocalSystem:
                        x.RunAsLocalSystem();
                        break;
                    case Configuration.RunAs.NetworkService:
                        x.RunAsNetworkService();
                        break;
                    case Configuration.RunAs.User:
                        x.RunAs(this.ConfigurationContext.GetUsername(), this.ConfigurationContext.GetPassword());
                        break;
                }

                x.SetDescription(this.ConfigurationContext.GetDescription());
                x.SetDisplayName(this.ConfigurationContext.GetDisplayName());
                x.SetServiceName(this.ConfigurationContext.GetServiceName());

                x.Service<BusHost>(sc =>
                {
                    sc.ConstructUsing(() => this);
                    sc.WhenStarted(s => base.Start());
                    sc.WhenStopped(s => base.Stop());
                });
            });
        }
    }
}
