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
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.Windsor;
using System;

namespace Wave.IoC.CastleWindsor
{
    /// <summary>
    /// Integrates Castle Windsor and Wave
    /// </summary>
    public sealed class CastleWindsorContainerAdapter : Wave.IContainer, IDisposable
    {
        private readonly WindsorContainer container;

        public CastleWindsorContainerAdapter(WindsorContainer container)
        {
            this.container = new WindsorContainer();            
        }

        public void Dispose()
        {
            this.container.Dispose();
        }

        public void Register<TFrom, TTo>(InstanceScope scope)
            where TTo : TFrom
        {
            this.Register(typeof(TFrom), typeof(TTo), scope);
        }

        public void Register(Type from, Type to, InstanceScope scope)
        {
            if (!from.IsAssignableFrom(to))
            {
                throw new ArgumentException("To must be assignable to From");
            }

            try
            {
                if (scope == InstanceScope.Singleton)
                {
                    this.container.Register(Component.For(from).ImplementedBy(to).LifestyleSingleton());
                }
                else
                {
                    this.container.Register(Component.For(from).ImplementedBy(to).LifestyleTransient());
                }
            }
            catch (ComponentRegistrationException)
            {
                // Castle Windsor sucks and doesn't allow a easy way to determine if a registration already exists
            }
        }

        public T Resolve<T>()
        {
            return this.container.Resolve<T>();
        }

        public object Resolve(Type type)
        {
            return this.container.Resolve(type);
        }
    }
}