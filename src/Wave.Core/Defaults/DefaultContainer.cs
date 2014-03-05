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
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Wave.Defaults
{   
    /// <summary>
    /// Lightweight ThreadSafe IoC Container - Used if Wave is not provided with a external IOC container
    /// Only supports constructor injection of new instances.
    /// </summary>
    public class DefaultContainer : IContainer
    {
        private delegate object ExpressionActivator(params object[] args);
        private ConcurrentDictionary<Type, object> instanceCache = new ConcurrentDictionary<Type, object>();
        private ConcurrentDictionary<Type, Registration> registrationMap = new ConcurrentDictionary<Type, Registration>();
        private ConcurrentDictionary<Type, ExpressionActivator> activatorCache = new ConcurrentDictionary<Type, ExpressionActivator>();
        
        /// <summary>
        /// Registers a mapping from TFrom to TTo
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTo"></typeparam>
        public void Register<TFrom, TTo>(InstanceScope scope)
            where TTo : TFrom
        {
            this.Register(typeof(TFrom), typeof(TTo), scope);
        }

        /// <summary>
        /// Registers a mapping from Source to Dest
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTo"></typeparam>
        public void Register(Type from, Type to, InstanceScope scope)
        {
            if (!from.IsAssignableFrom(to))
            {
                throw new ArgumentException(string.Format("{0} is not assignable from {1}", to.Name, from.Name));
            }

            if (to.IsInterface || to.IsAbstract)
            {
                throw new ArgumentException(string.Format("Cannot map {0} to {1}. You must specify a concrete type for TTo", to.Name, from.Name));
            }

            this.registrationMap.AddOrUpdate(from, new Registration(to, scope), (key, value) => new Registration(to, scope));
        }

        /// <summary>
        /// Creates a Instance of T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Resolve<T>()
        {
            return (T)this.ResolveObject(typeof(T));
        }

        /// <summary>
        /// Creates a instance of the specified type
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object Resolve(Type type)
        {
            return this.ResolveObject(type);
        }

        /// <summary>
        /// Handles creating an actual instance of the requested type, using the constructor parameters applied
        /// 
        /// Used to use Activator.CreateInstance, but switched to creating and caching expressions
        /// for a drastic performance increase.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ctor"></param>
        /// <param name="paramValues"></param>
        /// <returns></returns>
        private object CreateInstance(Type type, ConstructorInfo ctor, object[] paramValues)
        {
            if (!activatorCache.ContainsKey(type))
            {
                var activatorParam = Expression.Parameter(typeof(object[]), "args");
                var activator = (ExpressionActivator) Expression.Lambda(
                    typeof(ExpressionActivator),
                    Expression.New(ctor, ctor.GetParameters()
                                             .Select((p,i) => Expression.Convert(Expression.ArrayIndex(activatorParam, Expression.Constant(i)), p.ParameterType))),
                    activatorParam).Compile();
                
                activatorCache.AddOrUpdate(type, activator, (k, v) => activator);
            }
                                   
            return activatorCache[type](paramValues);
        }

        /// <summary>
        /// Resolves the correct constructor to use for a given type. Modelled after Microsoft Unitys behavior, except without use of
        /// the InjectionConstructor attribute.
        ///
        /// From Unity:
        /// When a target class contains more than one constructor, Unity will use the one that has the InjectionConstructor attribute applied.
        /// If there is more than one constructor, and none carries the InjectionConstructor attribute, Unity will use the constructor with the most parameters.
        /// If there is more than one such constructor (more than one of the “longest” with the same number of parameters), Unity will raise an exception.
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        private ConstructorInfo ResolveCorrectConstructor(Type objectType)
        {
            // Get Public / Static constructors and sort them by the number of
            // parameters. By default, when there is more than one constructor,
            // we use the one with the most parameters
            var constructors = objectType
                .GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .ToList();

            // No Public / Static constructors?
            if (constructors.Count == 0)
            {
                throw new ArgumentException(string.Format("Unable to construct {0} - No constructor", objectType.Name));
            }

            // Handle ambigious parameter count edge case
            if (constructors.Count > 1 && constructors[0].GetParameters().Length == constructors[1].GetParameters().Length)
            {
                throw new ArgumentException(string.Format("Unable to construct {0} - Ambigious constructors. Cannot have multiple constructors with the same number of parameters", objectType.Name));
            }

            // Otherwise return the first constructor
            return constructors.First();
        }

        /// <summary>
        /// Recursive helper function. Creates a instance of the specified type and its dependencies
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        private object ResolveObject(Type objectType)
        {
            if (!this.registrationMap.ContainsKey(objectType))
            {
                throw new ArgumentException(string.Format("Unable to map {0} to a concrete type. Interfaces must be registered to a concrete type", objectType.Name));
            }

            if (this.instanceCache.ContainsKey(objectType))
            {
                return this.instanceCache[objectType];
            }

            // Recursively build up all constructor dependencies
            var ctor = this.ResolveCorrectConstructor(this.registrationMap[objectType].RegisteredType);
            var parameters = ctor
                                .GetParameters()
                                .Select(p =>
                                    {
                                        if (p.ParameterType == objectType)
                                        {
                                            throw new Exception("Type mappings have created a cycle");
                                        }

                                        return this.ResolveObject(p.ParameterType);
                                    })
                                .ToArray();

            var instance = this.CreateInstance(this.registrationMap[objectType].RegisteredType, ctor, parameters);

            if (this.registrationMap[objectType].Scope == InstanceScope.Singleton)
            {
                this.instanceCache.AddOrUpdate(objectType, instance, (key, value) => value);
            }

            return instance;
        }

        private class Registration
        {
            public Registration(Type type, InstanceScope scope)
            {
                this.RegisteredType = type;
                this.Scope = scope;
            }

            public Type RegisteredType
            {
                get;
                private set;
            }

            public InstanceScope Scope
            {
                get;
                private set;
            }
        }
    }
}