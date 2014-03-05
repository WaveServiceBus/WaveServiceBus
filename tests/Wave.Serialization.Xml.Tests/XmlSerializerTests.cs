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
using Wave.Tests;
using Wave.Tests.Internal;

namespace Wave.Serialization.Xml.Tests
{
    [TestFixture]
    [Category("Serializer: XML")]
    public class XMLSerializerTests : SerializerTestBase
    {
        public override ISerializer GetNewSerializer()
        {
            return new XmlSerializer();
        }

        [Test]
        public void Xml_Extension_Method_Makes_Builder_Use_Xml()
        {
            var config = new ConfigurationBuilder();
            config.ConfigureForHosting(x =>
                {
                    x.UsingAssemblyLocator<TestAssemblyLocator>();
                    x.UseXMLSerialization();
                });

            Assert.IsInstanceOf<XmlSerializer>(config.ConfigurationContext.Serializer);
        }
    }
}
