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
using Wave.Configuration;
using Wave.Consumers;
using Wave.HandlerResults;
using Wave.Tests.Internal;

namespace Wave.Tests.Consumers
{   
    [Category("Consumers: Primary Consumer")]
    public class PrimaryConsumerTests
    {
        private IHandlerResult DispatchMessage<T>(T message, ConfigurationSource configSource)
        {
            var consumer = configSource.ConfigurationContext.Container.Resolve<PrimaryConsumer>();
            var rawMessage = TestHelpers.GetRawMessage(message, configSource);
            return TestHelpers.CallPrivateMethod<IHandlerResult, PrimaryConsumer>("ProcessMessage", consumer, new[] { rawMessage });
        }

        [Test]
        public void Messages_Without_A_Handler_Return_FailResult()
        {
            var builder = new ConfigurationBuilder();
            builder.ConfigureAndCreateContext(x =>
            {
                x.UsingAssemblyLocator<TestAssemblyLocator>();
            });

            var result = DispatchMessage(new NoHandlerMessage(), builder);            
            Assert.That(result is FailResult);
        }

        [Test]
        public void Message_With_A_Handler_Routes_To_Handler_And_Returns_Success()
        {
            var builder = new ConfigurationBuilder();
            builder.ConfigureAndCreateContext(x =>
            {
                x.UsingAssemblyLocator<TestAssemblyLocator>();
            });

            var result = DispatchMessage(new AlwaysSuccessMessage(), builder);            
            Assert.That(result is SuccessResult);
        }

        [Test]
        public void Unhandled_Exceptions_In_A_Handler_Become_RetryResults()
        {
            var builder = new ConfigurationBuilder();
            builder.ConfigureAndCreateContext(x =>
            {
                x.UsingAssemblyLocator<TestAssemblyLocator>();
            });

            var result = DispatchMessage(new AlwaysThrowsExceptionMessage(), builder);            
            Assert.That(result is RetryResult);
        }

        [Test]
        public void Message_Filter_Stops_The_Pipeline_And_Discards_The_Message_When_Ignore_Result_Is_Returned()
        {
            var builder = new ConfigurationBuilder();
            builder.ConfigureAndCreateContext(x =>
                {
                    x.UsingAssemblyLocator<TestAssemblyLocator>();
                    x.WithInboundMessageFilter<AlwaysThrowsExceptionMessage>(new StopMessageEarlyFilter());
                });
            
            var result = DispatchMessage(new AlwaysThrowsExceptionMessage(), builder);

            // The filter should have converted what would be a retry result to a ignore result.
            Assert.That(!(result is RetryResult));
            Assert.That(result is IgnoreResult);
        }

        [Test]
        public void Filters_Applied_By_Attribute_Are_Invoked()
        {
            var builder = new ConfigurationBuilder();
            builder.ConfigureAndCreateContext(x =>
            {
                x.UsingAssemblyLocator<TestAssemblyLocator>();
            });
            
            var result = DispatchMessage(new AlwaysThrowsExceptionMessage2(), builder);

            // The filter should have converted what would be a retry result to a ignore result.
            Assert.That(!(result is RetryResult));
            Assert.That(result is IgnoreResult);
        }

        [Test]
        public void Message_Filter_Forwards_Message_When_Success_Is_Returned()
        {
            var builder = new ConfigurationBuilder();
            builder.ConfigureAndCreateContext(x =>
            {
                x.UsingAssemblyLocator<TestAssemblyLocator>();
                x.WithInboundMessageFilter<AlwaysThrowsExceptionMessage>(new PassMessageFilter());
            });
            
            var result = DispatchMessage(new AlwaysThrowsExceptionMessage(), builder);

            // The filter should forward the message to the handler which will always throw.
            Assert.That(result is RetryResult);
            Assert.That(!(result is SuccessResult));
        }

        [Test]
        public void Message_Filters_Should_Not_Be_Invoked_On_Messages_Without_Handlers()
        {
            var builder = new ConfigurationBuilder();
            builder.ConfigureAndCreateContext(x =>
            {
                x.UsingAssemblyLocator<TestAssemblyLocator>();
                x.WithInboundMessageFilter<NoHandlerMessage>(new FilterWithoutHandlerFilter());
            });
                        
            var result = DispatchMessage(new NoHandlerMessage(), builder);

            // If the pipeline calls the filter, an exception will be raised
            // This test should pass if there is no exception and the result is a failresult.
            Assert.That(result is FailResult);
        }

        [Test]
        public void Message_Filters_Can_Be_Chained()
        {
            var builder = new ConfigurationBuilder();
            builder.ConfigureAndCreateContext(x =>
            {
                x.UsingAssemblyLocator<TestAssemblyLocator>();
                x.WithInboundMessageFilter<AlwaysSuccessMessage>(new ChainedFilterOne());
                x.WithInboundMessageFilter<AlwaysSuccessMessage>(new ChainedFilterTwo());
            });
            
            var result = DispatchMessage(new AlwaysSuccessMessage(), builder);

            // Two Asserts should be ran in the chained filters
            Assert.That(Assert.Counter == 2);
        }           
    }
}
