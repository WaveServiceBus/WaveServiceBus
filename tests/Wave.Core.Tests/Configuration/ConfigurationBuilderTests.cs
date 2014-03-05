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
using NUnit.Framework;
using Wave.Configuration;
using Wave.Defaults;
using Wave.Tests.Internal;

namespace Wave.Tests.Configuration
{
    [Category("Internal: Configuration Builder")]
    public class ConfigurationBuilderTests
    {                
        [Test]
        public void Default_Value_For_Container_Is_DefaultContainer()
        {
            var builder = new ConfigurationBuilder();
            builder.ConfigureForHosting(x => x.UsingAssemblyLocator<TestAssemblyLocator>());
            Assert.That(builder.ConfigurationContext.Container is DefaultContainer);
        }

        [Test]
        public void Default_Value_For_Serializer_Is_DefaultSerializer()
        {
            var builder = new ConfigurationBuilder();
            builder.ConfigureForHosting(x => x.UsingAssemblyLocator<TestAssemblyLocator>());
            Assert.That(builder.ConfigurationContext.Serializer is DefaultSerializer);
        }

        [Test]
        public void Default_Value_For_Logger_Is_DefaultLogger()
        {
            var builder = new ConfigurationBuilder();
            builder.ConfigureForHosting(x => x.UsingAssemblyLocator<TestAssemblyLocator>());
            Assert.That(builder.ConfigurationContext.Logger is DefaultLogger);
        }

    }
}
