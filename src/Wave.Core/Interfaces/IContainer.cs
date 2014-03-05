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

using System;

namespace Wave
{
    /// <summary>
    /// Simple IoC interface - Consumers / Publishers should provide an adapter to this
    /// using their preferred IOC container (Unity, Ninject, Structuremap etc).
    ///
    /// Wave will use this interface to resolve and register types necessary for internal purposes
    /// </summary>
    public interface IContainer
    {
        /// <summary>
        /// Construct an instance of T and all of its dependencies
        /// </summary>
        /// <typeparam name="T">The type to construct</typeparam>
        /// <returns>An instance of T</returns>
        T Resolve<T>();

        /// <summary>
        /// Construct an instance of the specified type and all of its dependencies
        /// </summary>
        /// <param name="type">The type to construct</param>
        /// <returns>An instance of type</returns>
        object Resolve(Type type);

        /// <summary>
        /// Maps TSource to TDest. Is used by Resolve to return the desired concrete types when resolving dependencies
        /// </summary>
        /// <typeparam name="TSource">Source Type</typeparam>
        /// <typeparam name="TDest">Destination Type</typeparam>
        void Register<TFrom, TTo>(InstanceScope scope) where TTo : TFrom;

        /// <summary>
        /// Maps Source to Dest. Is used by Resolve to return the desired concrete types when resolving dependencies
        /// </summary>
        /// <param name="source">Source Type</param>
        /// <param name="Dest">Destination Type</param>
        void Register(Type from, Type to, InstanceScope scope);
    }
}