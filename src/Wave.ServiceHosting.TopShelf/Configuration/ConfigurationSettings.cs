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

namespace Wave.ServiceHosting.TopShelf.Configuration
{
    public enum RunAs
    {
        LocalSystem = 0,
        LocalService = 1,
        NetworkService = 2,
        User = 3,
    }

    public class ConfigurationSettings
    {
        public string Description
        {
            get;
            private set;
        }

        public string DisplayName
        {
            get;
            private set;
        }

        public string Password
        {
            get;
            private set;
        }

        public string ServiceName
        {
            get;
            private set;
        }

        public string Username
        {
            get;
            private set;
        }

        public RunAs UserType
        {
            get;
            private set;
        }

        public ConfigurationSettings RunAsLocalService()
        {
            this.UserType = RunAs.LocalService;
            return this;
        }

        public ConfigurationSettings RunAsLocalSystem()
        {
            this.UserType = RunAs.LocalSystem;
            return this;
        }

        public ConfigurationSettings RunAsNetworkService()
        {
            this.UserType = RunAs.NetworkService;
            return this;
        }

        public ConfigurationSettings RunAsUser(string username, string password)
        {
            return this.RunAsUser()
                    .WithUsername(username)
                    .WithPassword(password);
        }

        public ConfigurationSettings RunAsUser()
        {
            this.UserType = RunAs.User;
            return this;
        }

        public ConfigurationSettings WithDescription(string description)
        {
            this.Description = description;
            return this;
        }

        public ConfigurationSettings WithDisplayName(string displayName)
        {
            this.DisplayName = displayName;
            return this;
        }

        public ConfigurationSettings WithPassword(string password)
        {
            this.Password = password;
            return this;
        }

        public ConfigurationSettings WithServiceName(string serviceName)
        {
            this.ServiceName = serviceName;
            return this;
        }

        public ConfigurationSettings WithUsername(string username)
        {
            this.Username = username;
            return this;
        }
    }
}
