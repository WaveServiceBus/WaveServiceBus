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
using System.Collections.Generic;
using System.Linq;

namespace Wave.Utility
{
    /// <summary>
    /// Priority Queue implementation.
    ///     - Not threadsafe
    ///     - Uses a set internally, so duplicates will fail
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PriorityQueue<T>
    {
        private readonly SortedSet<T> internalCollection;

        public PriorityQueue(Func<T, T, int> compareTo)
        {
            this.internalCollection = new SortedSet<T>(new FunctionBasedComparer(compareTo));
        }

        public int Count
        {
            get
            {
                return this.internalCollection.Count;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return this.Count == 0;
            }
        }

        public T Dequeue()
        {
            if (this.IsEmpty)
            {
                return default(T);
            }

            var item = this.internalCollection.First();
            this.internalCollection.Remove(item);
            return item;
        }

        public void Enqueue(T item)
        {
            this.internalCollection.Add(item);
        }

        public T Peek()
        {
            if (this.IsEmpty)
            {
                return default(T);
            }

            return this.internalCollection.First();
        }

        /// <summary>
        /// Because nobody wants to create IComparer instance.
        /// Allows for a lambda to specify the compare function
        /// </summary>
        private class FunctionBasedComparer : IComparer<T>
        {
            private readonly Func<T, T, int> compareTo;

            public FunctionBasedComparer(Func<T, T, int> compareTo)
            {
                this.compareTo = compareTo;
            }

            public int Compare(T x, T y)
            {
                return this.compareTo(x, y);
            }
        }
    }
}