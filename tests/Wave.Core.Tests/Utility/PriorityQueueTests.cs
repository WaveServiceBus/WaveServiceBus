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
using Wave.Utility;

namespace Wave.Tests.Utility
{
    [Category("Utility: Priority Queue")]
    public class PriorityQueueTests
    {
        readonly Func<int, int, int> intComparer = 
            (x, y) => x == y ? 0 : x > y ? 1 : -1;

        [Test]
        public void Count_Returns_Correct_Count()
        {
            var queue = new PriorityQueue<int>(intComparer);
            queue.Enqueue(1);
            queue.Enqueue(2);

            Assert.AreEqual(2, queue.Count);
        }

        [Test]
        public void IsEmpty_Should_Be_True_On_Empty_Queues()
        {
            var queue = new PriorityQueue<int>(intComparer);
            Assert.IsTrue(queue.IsEmpty);
        }

        [Test]
        public void IsEmpty_Should_Be_False_On_Populated_Queues()
        {
            var queue = new PriorityQueue<int>(intComparer);
            queue.Enqueue(1);
            Assert.IsFalse(queue.IsEmpty);
        }

        [Test]
        public void Peek_Returns_Default_Value_When_Empty()
        {
            var queue = new PriorityQueue<int>(intComparer);
            Assert.AreEqual(queue.Peek(), default(int));
        }

        [Test]
        public void Peek_Does_Not_Throw_When_Empty()
        {
            Assert.DoesNotThrow(() =>
            {
                var queue = new PriorityQueue<int>(intComparer);
                Assert.AreEqual(queue.Peek(), default(int));
            });
        }

        [Test]
        public void Dequeue_Returns_Default_Value_When_Empty()
        {
            var queue = new PriorityQueue<int>(intComparer);
            Assert.AreEqual(queue.Dequeue(), default(int));
        }

        [Test]
        public void Dequeue_Does_Not_Throw_When_Empty()
        {
            Assert.DoesNotThrow(() =>
            {
                var queue = new PriorityQueue<int>(intComparer);
                Assert.AreEqual(queue.Dequeue(), default(int));
            });
        }

        [Test]
        public void Priority_Order_Works_Based_On_Comparer()
        {                        
                var queue = new PriorityQueue<int>(intComparer);
                
                // Enqueue out of order
                queue.Enqueue(3);                    
                queue.Enqueue(9);                
                queue.Enqueue(7);
                queue.Enqueue(2);
                queue.Enqueue(5);
                queue.Enqueue(1);                
                queue.Enqueue(10);                                
                queue.Enqueue(4);
                queue.Enqueue(8);
                queue.Enqueue(6);
                
                // Should dequeue in ascending order
                Assert.AreEqual(1, queue.Dequeue());
                Assert.AreEqual(2, queue.Dequeue());
                Assert.AreEqual(3, queue.Dequeue());
                Assert.AreEqual(4, queue.Dequeue());
                Assert.AreEqual(5, queue.Dequeue());
                Assert.AreEqual(6, queue.Dequeue());
                Assert.AreEqual(7, queue.Dequeue());
                Assert.AreEqual(8, queue.Dequeue());
                Assert.AreEqual(9, queue.Dequeue());
                Assert.AreEqual(10, queue.Dequeue());
        }
    }
}
