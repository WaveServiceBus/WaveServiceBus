﻿/* Copyright 2014 Jonathan Holland.
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

namespace Wave.ServiceHosting.TopShelf.Configuration
{
    public class ConfigurationSection : System.Configuration.ConfigurationSection
    {
        [ConfigurationProperty("runAs", IsRequired = false)]
        public RunAsElement RunAs
        {
            get { return (RunAsElement)base["runAs"]; }
            set { base["runAs"] = value; }
        }

        [ConfigurationProperty("service", IsRequired = false)]
        public ServiceElement Service
        {
            get { return (ServiceElement)base["service"]; }
            set { base["service"] = value; }
        }
    }

    public class RunAsElement : ConfigurationElement
    {
        [ConfigurationProperty("password", IsRequired = false)]
        public string Password
        {
            get { return (string)base["password"]; }
            set { base["password"] = value; }
        }

        [ConfigurationProperty("username", IsRequired = false)]
        public string Username
        {
            get { return (string)base["username"]; }
            set { base["username"] = value; }
        }

        [ConfigurationProperty("userType", IsRequired = true)]
        public string UserType
        {
            get { return (string)base["userType"]; }
            set { base["userType"] = value; }
        }
    }

    public class ServiceElement : ConfigurationElement
    {
        [ConfigurationProperty("description", IsRequired = false)]
        public string Description
        {
            get { return (string)base["description"]; }
            set { base["description"] = value; }
        }

        [ConfigurationProperty("displayName", IsRequired = false)]
        public string DisplayName
        {
            get { return (string)base["displayName"]; }
            set { base["displayName"] = value; }
        }

        [ConfigurationProperty("name", IsRequired = false)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }
    }
}
