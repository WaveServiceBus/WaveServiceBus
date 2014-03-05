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

using Autofac;
using Autofac.Core;
using System;
using System.Linq;

namespace Wave.IoC.Autofac
{
    /// <summary>
    /// Integrates AutoFac into Wave    
    /// </summary>
    public class AutofacContainerAdapter : Wave.IContainer
    {
        private global::Autofac.IContainer container;

        public AutofacContainerAdapter(global::Autofac.IContainer container)
        {
            this.container = container;
        }

        public T Resolve<T>()
        {
            return (T)this.Resolve(typeof(T));
        }

        public object Resolve(Type type)
        {            
            return container.Resolve(type);
        }

        public void Register<TFrom, TTo>(InstanceScope scope)
            where TTo : TFrom
        {
            this.Register(typeof(TFrom), typeof(TTo), scope);
        }

        public void Register(Type from, Type to, InstanceScope scope)
        {
            var builder = new ContainerBuilder();

            if (scope == InstanceScope.Singleton)
            {
                builder.RegisterType(to).As(from).SingleInstance();
            }
            else
            {
                builder.RegisterType(to).As(from);
            }

            builder.Update(container);
        }       
    }
}
