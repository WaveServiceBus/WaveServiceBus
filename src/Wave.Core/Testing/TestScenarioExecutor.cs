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
using System.Linq;
using System.Reflection;
using Wave.Configuration;
using Wave.Consumers;
using Wave.Defaults;

namespace Wave.Testing
{
    internal class TestScenarioExecutor<TMessage>
    {
        private class InternalTestAssemblyLocator : IAssemblyLocator
        {
            public Assembly GetEntryAssembly()
            {                
                return Assembly.GetAssembly(typeof(InternalTestAssemblyLocator));
            }

            public Assembly[] GetSubscriberAssemblies()
            {
                return AppDomain.CurrentDomain.GetAssemblies();
            }
        }

        private readonly TestScenario<TMessage> scenario;
        private readonly PublishedMessageInterceptor interceptor;
        private readonly ITestFrameworkAdapter testAdapter;

        internal TestScenarioExecutor(TestScenario<TMessage> scenario, ITestFrameworkAdapter testAdapter)
        {
            this.scenario = scenario;
            this.testAdapter = testAdapter;
            this.interceptor = new PublishedMessageInterceptor();
        }
      
        internal void ExecuteTest()
        {
            var config = PrepareConfiguration();
            var consumer = config.Container.Resolve<PrimaryConsumer>();
            var transport = (DefaultTransport)config.Transport;

            var result = consumer.ProcessMessage(RawMessage.Create(this.scenario.Message));

            // Check Result Type
            if (this.scenario.ExpectedResultType != null)
            {
                this.testAdapter.Assert(result.GetType() == this.scenario.ExpectedResultType, String.Format("Expected a {0}, actual was {1}", scenario.ExpectedResultType, result));
                if (this.scenario.ResultValidationCallback != null)
                {
                    this.scenario.ResultValidationCallback(result);
                }
            }

            // Check Published Message Counts
            if (this.scenario.ShouldPublishNothing)
            {
                this.testAdapter.Assert(this.interceptor.Messages.Count == 0, "Messages were published when not expected");
            }
            
            // Check for expected messages and values
            foreach (var expectedMessage in this.scenario.ExpectedMessages)
            {
                var actualMessage = this.interceptor.Messages.FirstOrDefault(m => m.GetType() == expectedMessage.Key);
                this.testAdapter.Assert(actualMessage != null, String.Format("Expected message {0} was not published", expectedMessage.Key));

                if (expectedMessage.Value != null)
                {
                    expectedMessage.Value(actualMessage);
                }
            }
        }
       
        private IConfigurationContext PrepareConfiguration()
        {
            var config = new ConfigurationBuilder();
            config.ConfigureAndCreateContext(x =>
            {
                x.UsingAssemblyLocator<InternalTestAssemblyLocator>();
                x.WithGlobalOutboundMessageFilter(this.interceptor);
            });
            
            return config.ConfigurationContext;
        }
    }
}
