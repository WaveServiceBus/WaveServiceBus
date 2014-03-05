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

namespace Wave.Defaults
{
    /// <summary>
    /// Default Queue Name Resolver - Sets Primary Queue name to match the name of the hosting process
    /// </summary>
    public class DefaultQueueNameResolver : IQueueNameResolver
    {
        private readonly IAssemblyLocator assemblyLocator;

        public DefaultQueueNameResolver(IAssemblyLocator assemblyLocator)
        {
            this.assemblyLocator = assemblyLocator;
        }

        public virtual string GetPrimaryQueueName()
        {
            return this.assemblyLocator.GetEntryAssembly().GetName().Name;
        }     
    }
}
