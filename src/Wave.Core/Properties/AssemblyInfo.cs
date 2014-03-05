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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Wave.Core")]
[assembly: AssemblyDescription("Wave Service Bus")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Jonathan  Holland")]
[assembly: AssemblyProduct("Wave.Core")]
[assembly: AssemblyCopyright("Copyright © 2014 Jonathan Holland")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: InternalsVisibleTo("Wave.Core.Tests")]
[assembly: InternalsVisibleTo("Wave.IoC.Autofac.Tests")]
[assembly: InternalsVisibleTo("Wave.IoC.Unity.Tests")]
[assembly: InternalsVisibleTo("Wave.IoC.NInject.Tests")]
[assembly: InternalsVisibleTo("Wave.IoC.StructureMap.Tests")]
[assembly: InternalsVisibleTo("Wave.IoC.CastleWindsor.Tests")]
[assembly: InternalsVisibleTo("Wave.Logging.Log4Net.Tests")]
[assembly: InternalsVisibleTo("Wave.Logging.NLog.Tests")]
[assembly: InternalsVisibleTo("Wave.Logging.CommonsLogging.Tests")]
[assembly: InternalsVisibleTo("Wave.Serialization.JsonDotNet.Tests")]
[assembly: InternalsVisibleTo("Wave.Serialization.ServiceStack.Tests")]
[assembly: InternalsVisibleTo("Wave.Serialization.Xml.Tests")]
[assembly: InternalsVisibleTo("Wave.ServiceHosting.TopShelf.Tests")]
[assembly: InternalsVisibleTo("Wave.Transports.RabbitMQ.Tests")]
[assembly: InternalsVisibleTo("Wave.Transports.MSMQ.Tests")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("c5e9d4d1-c194-4b3f-a6c4-d38f9e7370e8")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0.0")]