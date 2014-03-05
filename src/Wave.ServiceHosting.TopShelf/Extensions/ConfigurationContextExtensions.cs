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

using Wave.ServiceHosting.TopShelf.Configuration;

namespace Wave.ServiceHosting.TopShelf
{
    internal static class ConfigurationContextExtensions
    {
        internal static string GetDescription(this IConfigurationContext context)
        {
            return (string)context["topshelf.description"];
        }

        internal static string GetDisplayName(this IConfigurationContext context)
        {
            return (string)context["topshelf.displayname"];
        }

        internal static string GetPassword(this IConfigurationContext context)
        {
            return (string)context["topshelf.password"];
        }

        internal static string GetServiceName(this IConfigurationContext context)
        {
            return (string)context["topshelf.servicename"];
        }

        internal static string GetUsername(this IConfigurationContext context)
        {
            return (string)context["topshelf.username"];
        }

        internal static RunAs GetUserType(this IConfigurationContext context)
        {
            if (context["topshelf.usertype"] == null)
            {
                return RunAs.LocalService;
            }

            return (RunAs)context["topshelf.usertype"];
        }

        internal static void SetDescription(this IConfigurationContext context, string description)
        {
            context["topshelf.description"] = description;
        }
        internal static void SetDisplayName(this IConfigurationContext context, string displayName)
        {
            context["topshelf.displayname"] = displayName;
        }

        internal static void SetPassword(this IConfigurationContext context, string password)
        {
            context["topshelf.password"] = password;
        }

        internal static void SetServiceName(this IConfigurationContext context, string serviceName)
        {
            context["topshelf.servicename"] = serviceName;
        }

        internal static void SetUsername(this IConfigurationContext context, string username)
        {
            context["topshelf.username"] = username;
        }

        internal static void SetUserType(this IConfigurationContext context, RunAs userType)
        {
            context["topshelf.usertype"] = userType;
        }
    }
}
