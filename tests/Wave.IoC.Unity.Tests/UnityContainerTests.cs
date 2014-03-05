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

using Microsoft.Practices.Unity;
using NUnit.Framework;
using Wave.Configuration;
using Wave.Tests.Internal;

namespace Wave.IoC.Unity.Tests
{
    [Category("Container: Unity")]
    public class UnityContainerTests : ContainerTestBase
    {
        public override IContainer GetNewContainer()
        {
            return new UnityContainerAdapter(new UnityContainer());
        }

        [Test]
        public void Unity_Extension_Method_Makes_Builder_Use_Unity()
        {
            var config = new ConfigurationBuilder();
            config.ConfigureAndCreateContext(x =>
                {
                    x.UsingAssemblyLocator<TestAssemblyLocator>();
                    x.UseUnity(new UnityContainer());
                });

            Assert.IsInstanceOf<UnityContainerAdapter>(config.ConfigurationContext.Container);
        }
    }
}