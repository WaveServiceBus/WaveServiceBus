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

using System.Configuration;

namespace Wave.ServiceHosting.IIS.Configuration
{
    public class ConfigurationSection : System.Configuration.ConfigurationSection
    {
        [ConfigurationProperty("useAutomaticConfiguration", IsRequired = false)]
        public bool UseAutomaticConfiguration
        {
            get { return (bool)base["useAutomaticConfiguration"]; }
            set { base["useAutomaticConfiguration"] = value; }
        }

        [ConfigurationProperty("useQueuePerWorker", IsRequired = false)]
        public bool UseQueuePerWorker
        {
            get { return (bool)base["useQueuePerWorker"]; }
            set { base["useQueuePerWorker"] = value; }
        }
    }
       
}
