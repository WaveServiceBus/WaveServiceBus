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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Hosting;

namespace Wave.ServiceHosting.IIS
{
    /// <summary>
    /// Configured to force a request to the host app so that it wakes up and restarts the Wave host when a application pool cycles.
    /// </summary>
    class AutoStartProvider : IProcessHostPreloadClient
    {
        /// <summary>
        /// Makes a web request to the host application so that it wakes up and starts
        /// </summary>
        /// <param name="parameters"></param>
        public void Preload(string[] parameters)
        {
            var binding = GetBindings().First();

            // Ping the site directly to activate it.     
            try
            {
                new WebClient().DownloadStringAsync(new Uri(String.Format("{0}://localhost:{1}", binding.Key, binding.Value)));
            }
            catch
            {
                // NOP
            }
        }

        /// <summary>
        /// Retrieves IIS bindings, used to figure out the scheme and port the host is running on
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<KeyValuePair<string, string>> GetBindings()
        {
            // Get the sites section from the AppPool.config 
            var sitesSection = WebConfigurationManager.GetSection(HostingEnvironment.SiteName, HostingEnvironment.ApplicationVirtualPath, "system.applicationHost/sites");

            return (from s in sitesSection.GetCollection()
                    where (string)s["name"] == HostingEnvironment.SiteName
                    from b in s.GetCollection("bindings")
                    select new
                    {
                        Protocol = (string)b["protocol"],
                        BindingInfo = (string)b["bindingInformation"]
                    })
                    .Where(b => b.Protocol.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    .Select(b => new KeyValuePair<string, string>(b.Protocol, b.BindingInfo.Split(':')[1]));

        }
    }
}
