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
using System;
using Wave.Defaults;

namespace Wave.Tests.Internal
{
    [Category("Container: All")]
    [TestFixture]
    [Ignore("Ignore Tests on Container Base")]
    public class ContainerTestBase
    {
        // Types needed to test Register / Resolve
        private interface IDummy { };
        private interface IDummy2 : IDummy { };
        private class ConcreteClass : IDummy { }
        private class NonAssignableClass { }
        private abstract class AbstractClass : IDummy { }

        // Used Constructor Injection Tests and also Cycle detection testing
        private class ClassWithDependancy : IDummy
        {
            public IDummy Dependancy { get; set; }

            public ClassWithDependancy(IDummy dependancy)
            {
                this.Dependancy = dependancy;
            }
        }

        private class AmbiguousConstructorsTestClass
        {
            public AmbiguousConstructorsTestClass(IDummy dummy1, ConcreteClass class1)
            {
            }

            public AmbiguousConstructorsTestClass(ConcreteClass class1, IDummy dummy1)
            {
            }
        }

        protected IContainer container = null;

        [SetUp]
        public virtual void Init()
        {
            container = this.GetNewContainer();
        }

        /// <summary>
        /// Inherit from ContainerTestBase and override GetNewContainer to run the same test suite against
        /// each IOC facade implementation.
        /// </summary>
        /// <returns></returns>
        public virtual IContainer GetNewContainer()
        {
            return new DefaultContainer();
        }

        [Test]
        public void Register_Throws_Exception_When_TTo_Is_Not_Assignable_To_TFrom()
        {
            Assert.Catch(() =>
            {
                this.container.Register(typeof(IDummy), typeof(NonAssignableClass), InstanceScope.Singleton);
            });
        }
            
        [Test]
        public void Register_Allows_Mapping_Assignable_Concrete_Class_To_Interface()
        {
            Assert.DoesNotThrow(() =>
            {
                this.container.Register<IDummy, ConcreteClass>(InstanceScope.Singleton);
            });
        }

        [Test]
        public void Register_Is_Idempotent()
        {
            Assert.DoesNotThrow(() =>
            {
                // Registering twice should not throw
                this.container.Register<IDummy, ConcreteClass>(InstanceScope.Singleton);               
                var foo = this.container.Resolve<IDummy>();
                
                this.container.Register<IDummy, ConcreteClass>(InstanceScope.Singleton);
                var baz = this.container.Resolve<IDummy>();
            });
        }

        [Test]
        public void Resolve_Returns_Simple_Mapped_Type()
        {
            this.container.Register<IDummy, ConcreteClass>(InstanceScope.Singleton);
            Assert.DoesNotThrow(() =>
            {
                var resolvedInstance = this.container.Resolve<IDummy>();
                Assert.IsInstanceOf<ConcreteClass>(resolvedInstance);
            });
        }
       
        [Test]
        public void Instance_Scope_For_Singleton_Returns_Same_Instance()
        {
            this.container.Register<IDummy, ConcreteClass>(InstanceScope.Singleton);

            var instance1 = this.container.Resolve<IDummy>();
            var instance2 = this.container.Resolve<IDummy>();

            Assert.AreSame(instance1, instance2);
        }

        [Test]
        public void Instance_Scope_For_Transient_Returns_Different_Instances()
        {
            this.container.Register<IDummy, ConcreteClass>(InstanceScope.Transient);

            var instance1 = this.container.Resolve<IDummy>();
            var instance2 = this.container.Resolve<IDummy>();

            Assert.AreNotSame(instance1, instance2);
        }
     
        [Test]
        public void Resolve_Returns_Complex_Mapped_Type_With_Dependancies()
        {
            this.container.Register<IDummy, ConcreteClass>(InstanceScope.Singleton);
            this.container.Register<ClassWithDependancy, ClassWithDependancy>(InstanceScope.Singleton);

            Assert.DoesNotThrow(() =>
            {
                var resolvedInstance = this.container.Resolve<ClassWithDependancy>();

                Assert.IsInstanceOf<ClassWithDependancy>(resolvedInstance);
                Assert.IsInstanceOf<ConcreteClass>(resolvedInstance.Dependancy);
            });
        }    

        [Test]
        public void Resolve_Throws_On_Ambiguous_Constructors()
        {
            Assert.Catch(() =>            
            {
                this.container.Resolve<AmbiguousConstructorsTestClass>();
            });
        }
    }
}