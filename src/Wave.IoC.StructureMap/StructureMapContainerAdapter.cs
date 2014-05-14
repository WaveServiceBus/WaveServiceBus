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
using StructureMap;
using System;
using System.Linq;

namespace Wave.IoC.StructureMap
{
    /// <summary>
    /// Integrates StructureMap and Wave
    /// </summary>
    public class StructureMapContainerAdapter : Wave.IContainer
    {
        private readonly Container container;

        public StructureMapContainerAdapter(Container container)
        {
            this.container = container;
        }

        public T Resolve<T>()
        {
            return (T)this.Resolve(typeof(T));
        }

        public object Resolve(Type type)
        {                       
            return this.container.GetInstance(type);
        }

        public void Register<TFrom, TTo>(Wave.InstanceScope scope) 
            where TTo : TFrom
        {
            this.Register(typeof(TFrom), typeof(TTo), scope);
        }

        public void Register(Type from, Type to, Wave.InstanceScope scope)
        {
            if (scope == Wave.InstanceScope.Singleton)
            {
                this.container.Configure(x => x.For(from).LifecycleIs(new global::StructureMap.Pipeline.SingletonLifecycle()).Use(to));
            }
            else
            {
                this.container.Configure(x => x.For(from).LifecycleIs(new global::StructureMap.Pipeline.TransientLifecycle()).Use(to));
            }
        }
    }
}
