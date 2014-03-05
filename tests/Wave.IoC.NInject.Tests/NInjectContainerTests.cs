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
using Ninject;
using NUnit.Framework;
using Wave.Configuration;
using Wave.Tests.Internal;

namespace Wave.IoC.NInject.Tests
{
    [Category("Container: NInject")]
    public class NInjectContainerTests : ContainerTestBase
    {
        public override IContainer GetNewContainer()
        {
            return new NInjectContainerAdapter(new StandardKernel());
        }

        [Test]
        public void NInject_Extension_Method_Makes_Builder_Use_NInject()
        {
            var config = new ConfigurationBuilder();                
            config.ConfigureAndCreateContext(x =>
                {
                    x.UsingAssemblyLocator<TestAssemblyLocator>();
                    x.UseNInject(new StandardKernel());
                });

            Assert.IsInstanceOf<NInjectContainerAdapter>(config.ConfigurationContext.Container);
        }
    }
}