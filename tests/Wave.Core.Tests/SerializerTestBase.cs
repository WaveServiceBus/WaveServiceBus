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

namespace Wave.Tests
{
    [TestFixture]
    [Category("Serializer: All")]    
    [Ignore("Ignore Tests on Serializer Base")]
    public class SerializerTestBase
    {
        [Serializable]
        public class TestClass
        {
            public string Name { get; set; }
            public DateTime Date { get; set; }
        }
      
        protected ISerializer serializer = null;

        [SetUp]
        public void Init()
        {
            serializer = this.GetNewSerializer();
        }

        /// <summary>
        /// Inherit from SerializerTestBase and override GetNewSerializer to run the same test suite against
        /// each serializer facade implementation.
        /// </summary>
        /// <returns></returns>
        public virtual ISerializer GetNewSerializer()
        {
            return new DefaultSerializer();
        }
     
        [Test]
        public void Serializer_Should_Keep_DateTime_Kind()
        {
            var testInstance = new TestClass
            {
                Name = "Test"
            };

            // DateTimeKind.Utc
            testInstance.Date = new DateTime(2014, 1, 1, 0, 0, 1, DateTimeKind.Utc);
            Assert.AreEqual(testInstance.Date.Kind, ((TestClass)this.serializer.Deserialize(serializer.Serialize(testInstance), typeof(TestClass))).Date.Kind);

            // DateTimeKind.Local
            testInstance.Date = new DateTime(2014, 1, 1, 0, 0, 1, DateTimeKind.Local);
            Assert.AreEqual(testInstance.Date.Kind, ((TestClass)this.serializer.Deserialize(serializer.Serialize(testInstance), typeof(TestClass))).Date.Kind);                                                           
        }
    }
}