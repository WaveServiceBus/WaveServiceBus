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

using System.Reflection;

namespace Wave.Defaults
{
    /// <summary>
    /// Finds the entry assembly regardless of the underlying application host (Web / Windows Service / Console App / Etc)
    /// </summary>
    public class DefaultAssemblyLocator : IAssemblyLocator
    {
        // Cache the value because dumb things are dumb when hosted under a web context
        private Assembly entryAssemblyCache = null;

        public Assembly GetEntryAssembly()
        {
            if (this.entryAssemblyCache == null)
            {
                // Hosted as a web application. As usual, there is no
                // decent way to make this work, so walk the type tree until you hit
                // the first non ASP (lol) class.
                if (System.Web.HttpContext.Current != null)
                {
                    var type = System.Web.HttpContext.Current.ApplicationInstance.GetType();
                    while (type != null && type.Namespace == "ASP")
                    {
                        type = type.BaseType;
                    }

                    // If for some reason this didn't find a type, default to ExecutingAssembly
                    this.entryAssemblyCache = type == null ? Assembly.GetExecutingAssembly() : type.Assembly;
                }
                else
                {
                    // Console or Windows Service
                    this.entryAssemblyCache = Assembly.GetEntryAssembly();
                }
            }

            return this.entryAssemblyCache;
        }


        public Assembly[] GetSubscriberAssemblies()
        {
            return new Assembly[] { this.GetEntryAssembly() };
        }
    }
}