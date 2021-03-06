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

using System;
using System.Diagnostics;
using Wave.Defaults;

namespace Wave.ServiceHosting.IIS
{
    /// <summary>
    /// This queue resolver tags queue names with the machine name (TEMP REMOVED UNTIL CLEANUP STRATEGY IN PLACE: and worker process Id) listening on this queue.
    /// </summary>
    public class IISQueueNameResolver : DefaultQueueNameResolver
    {
        public IISQueueNameResolver(IAssemblyLocator assemblyLocator) : base(assemblyLocator) { }

        public override string GetPrimaryQueueName()
        {
            return String.Format("{0}_{1}_{2}",
                base.GetPrimaryQueueName(),
                Environment.MachineName,
                Process.GetCurrentProcess().Id
                );
        }
    }
}
