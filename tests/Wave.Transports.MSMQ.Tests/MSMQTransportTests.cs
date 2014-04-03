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
using System.Collections.Concurrent;
using System.Messaging;
using Wave.Configuration;
using Wave.Tests;
using Wave.Tests.Internal;

namespace Wave.Transports.MSMQ.Tests
{
    /// <summary>
    /// These tests require a running MSMQ broker - These are integration test and should not be ran by the build agent    
    /// 
    /// To run these tests the INTEGRATION directive will need to be set on the build
    /// </summary>
    [Category("Transport: MSMQ")]
    [TestFixture]
    public class MSMQTransportTests : TransportTestBase
    {
        private readonly string hostname = ".";
        private ConcurrentBag<Guid> usedGuids = new ConcurrentBag<Guid>();

        public override ITransport GetTransport()
        {
            // Each Transport uses a unique Guid as the queue base to ensure the tests are isolated            
            var transportGuid = Guid.NewGuid();
            usedGuids.Add(transportGuid);

            var config = new ConfigurationBuilder();
            config.ConfigureAndCreateContext(x =>
                {
                    x.UsingAssemblyLocator<TestAssemblyLocator>();
                    x.UseMSMQ(m =>
                    {
                        m.UseHostname(hostname);
                    });
                });

            var transport = new MSMQTransport(transportGuid.ToString(), config.ConfigurationContext);
            transport.InitializeForConsuming();
            transport.InitializeForPublishing();

            return transport;
        }

        [SetUp]
        public void Setup()
        {
            #if !INTEGRATION
                Assert.Inconclusive("MSMQ test is only ran under integration profile");
            #endif
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            #if INTEGRATION
                // Removes all the queues used for the tests
                foreach (var guid in usedGuids)
                {
                    MessageQueue.Delete(String.Format("{0}\\Private$\\{1}", hostname, guid.ToString()));
                    MessageQueue.Delete(String.Format("{0}\\Private$\\{1}_Delay", hostname, guid.ToString()));
                    MessageQueue.Delete(String.Format("{0}\\Private$\\{1}_Error", hostname, guid.ToString()));
                }
            #endif
        }
    }
}