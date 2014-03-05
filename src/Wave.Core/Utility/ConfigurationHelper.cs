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

namespace Wave.Utility
{
    public class ConfigurationHelper
    {
        /// <summary>
        /// Returns the correct configuration section based on if the bus code is hosted in
        /// a web application (web.config) or a console app / service (foo.exe.config)
        /// </summary>
        /// <returns></returns>
        public static T GetConfigSection<T>()
        {
            System.Configuration.Configuration config = null;
            if (System.Web.HttpContext.Current != null)
            {
                config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
            }
            else
            {
                config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            }

            foreach (var section in config.Sections)
            {
                if (section.GetType() == typeof(T))
                {
                    return (T)section;
                }
            }

            return default(T);
        }
    }
}