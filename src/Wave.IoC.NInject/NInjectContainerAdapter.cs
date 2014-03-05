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
using System;
using System.Linq;

namespace Wave.IoC.NInject
{
    /// <summary>
    /// Integates NInject into Wave    
    /// </summary>
    public class NInjectContainerAdapter : Wave.IContainer
    {
        private readonly IKernel kernel;

        public NInjectContainerAdapter(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public T Resolve<T>()
        {
            return (T)this.Resolve(typeof(T));
        }

        public object Resolve(Type type)
        {                        
            return this.kernel.Get(type);
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

            if (!this.kernel.GetBindings(from).Any())
            {
                if (scope == InstanceScope.Singleton)
                {
                    this.kernel.Bind(from).To(to).InSingletonScope();
                }
                else
                {
                    this.kernel.Bind(from).To(to).InTransientScope();
                }
            }
        }
    }
}
