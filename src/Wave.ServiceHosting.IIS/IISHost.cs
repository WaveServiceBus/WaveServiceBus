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

using Microsoft.Web.Administration;
using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System.Web.Hosting;
using Wave.Utility;

namespace Wave.ServiceHosting.IIS
{
    public class IISHost : BusHost
    {
        private readonly ILogger log;

        public IISHost(IConfigurationContext configuration)
            : base(configuration)
        {
            this.ConfigurationContext = MergeConfiguration(configuration);
            this.log = this.ConfigurationContext.Logger;
        }

        /// <summary>
        /// Initializes and start the service host on a seperate thread from the main IIS
        /// application
        /// </summary>
        public override void Start()
        {
            if (this.ConfigurationContext.UseAutomaticConfiguration())
            {
                this.ConfigureIISForAutoStart();
            }

            // IIS will call the ShutdownHook when it is recycling the app pool
            HostingEnvironment.RegisterObject(new ShutdownHook(this));

            // Start the host on a background task
            Task.Factory.StartNew(
                () => base.Start(),
                this.ConfigurationContext.TokenSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }
        
        /// <summary>
        /// Merges in configuration from external files from the passed in configuration
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private IConfigurationContext MergeConfiguration(IConfigurationContext configuration)
        {
            var configSection = ConfigurationHelper.GetConfigSection<Configuration.ConfigurationSection>();
            if (configSection != null)
            {
                configuration.SetAutomaticConfigurationFlag(configSection.UseAutomaticConfiguration);
            }

            return configuration;
        }

        private void ConfigureIISForAutoStart()
        {           
            // Helper to find elements from the application host config
            Func<ConfigurationElementCollection, String, Predicate<ConfigurationAttribute>, ConfigurationElement> findElement = 
                (collection, elementTagName, comparer) =>
                {
                    foreach (ConfigurationElement element in collection)
                    {
                        if (element.ElementTagName == elementTagName)
                        {
                            foreach (var attribute in element.Attributes)
                            {                        
                                if (comparer(attribute))
                                {
                                    return element;
                                }
                            }                   
                        }
                    }

                    return null;
                };

            try
            {
                bool changedConfiguration = false;
                var siteName = HostingEnvironment.ApplicationHost.GetSiteName();
                var appPoolName = String.Empty;

                using (var serverManager = new ServerManager())
                {                    
                    var config = serverManager.GetApplicationHostConfiguration();

                    // Create AutoStart Node
                    var autoStartSection = config.GetSection("system.applicationHost/serviceAutoStartProviders");
                    if (autoStartSection != null)
                    {
                        var autoStartCollection = autoStartSection.GetCollection();

                        var addElement = findElement(autoStartCollection, "add", a => a.Name == "name" && (string)a.Value == "Wave.ServiceHosting.IIS");
                        if (addElement == null)
                        {
                            changedConfiguration = true;
                            addElement = autoStartCollection.CreateElement("add");
                            addElement["name"] = @"Wave.ServiceHosting.IIS";
                            addElement["type"] = @"Wave.ServiceHosting.IIS.AutoStartProvider, Wave.ServiceHosting.IIS";
                            autoStartCollection.Add(addElement);
                        }
                    }

                    // Configure Site to use AutoStartProvider
                    var siteSection = config.GetSection("system.applicationHost/sites");
                    var siteCollection = siteSection.GetCollection();
                    var siteElement = findElement(siteCollection, "site", a => a.Name == "name" && (string)a.Value == siteName);
                    if (siteElement != null)
                    {
                        var applicationElement = findElement(siteElement.GetCollection(), "application", a => a.Name == "path" && (string)a.Value == @"/");
                        if (applicationElement != null)
                        {
                            changedConfiguration = true;
                            appPoolName = (string)applicationElement["applicationPool"];
                            applicationElement["serviceAutoStartEnabled"] = true;
                            applicationElement["serviceAutoStartProvider"] = @"Wave.ServiceHosting.IIS";
                        }
                    }

                    if (appPoolName != String.Empty)
                    {
                        // Set Application Pool to AlwaysRunning
                        var appPoolSection = config.GetSection("system.applicationHost/applicationPools");
                        var appPoolCollection = appPoolSection.GetCollection();
                        if (appPoolCollection != null)
                        {
                            var addElement = findElement(appPoolCollection, "add", a => a.Name == "name" && (string)a.Value == appPoolName);
                            if (addElement != null)
                            {
                                changedConfiguration = true;
                                addElement["startMode"] = "AlwaysRunning";
                            }
                        }
                    }

                    // Only commit changes if actual updates were made
                    // to avoid a infinite loop of server restarts
                    if (changedConfiguration)
                    {
                        serverManager.CommitChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                this.log.ErrorFormat("Exception attempting to auto-configure IIS", ex.ToString());
            }
        }

        /// <summary>
        /// Allows for a graceful shutdown of the application pool
        /// </summary>
        private class ShutdownHook : IRegisteredObject
        {
            private readonly IBusHost host;

            public ShutdownHook(IBusHost host)
            {
                this.host = host;
            }

            public void Stop(bool immediate)
            {
                if (!immediate)
                {
                    // The base class Stop cancels the token used for all Wave threads, allowing
                    // wave to shutdown gracefully.
                    host.Stop();

                    // Prevent IIS from calling this again until the app pool is restarted
                    HostingEnvironment.UnregisterObject(this);
                }
            }
        }
    }
}