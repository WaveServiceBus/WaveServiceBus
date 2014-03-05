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
using Wave.HandlerResults;

namespace Wave.Testing
{
    public class WaveServiceTest
    {
        private WaveServiceTest()
        {
        }

        public static WaveServiceTest Create()
        {
            return new WaveServiceTest();
        }

        // Extension Methods from Test Adapters are hung on this type
    }

    public class TestScenarioBuilder<TMessage>
    {
        private readonly TestScenario<TMessage> scenario;
        private readonly ITestFrameworkAdapter testAdapter;

        public TestScenarioBuilder(TMessage message, ITestFrameworkAdapter testAdapter)
        {
            this.scenario = new TestScenario<TMessage>(message);
            this.testAdapter = testAdapter;
        }

        internal TestScenario<TMessage> Scenario
        {
            get
            {
                return this.scenario;
            }
        }

        public TestScenarioBuilder<TMessage> AndItShouldNotPublishAnything()
        {
            this.scenario.ShouldPublishNothing = true;
            return this;
        }

        public TestScenarioBuilder<TMessage> AndItShouldPublish<TPublishedMessage>()
        {
            return AndItShouldPublish<TMessage>(null);
        }

        public TestScenarioBuilder<TMessage> AndItShouldPublish<TPublishedMessage>(Action<TPublishedMessage> messageCallback)
        {
            if (messageCallback != null)
            {
                this.scenario.ExpectedMessages[typeof(TPublishedMessage)] = c => messageCallback((TPublishedMessage)c);
            }
            else
            {
                this.scenario.ExpectedMessages[typeof(TPublishedMessage)] = null;
            }

            return this;
        }

        public TestScenarioBuilder<TMessage> ExpectADelayResult()
        {
            return this.ExpectADelayResult(null);
        }

        public TestScenarioBuilder<TMessage> ExpectADelayResult(Action<DelayResult> resultCallback)
        {
            return this.TheExpectedResultIs<DelayResult>(resultCallback);
        }

        public TestScenarioBuilder<TMessage> ExpectAFailResult()
        {
            return this.ExpectAFailResult(null);
        }

        public TestScenarioBuilder<TMessage> ExpectAFailResult(Action<FailResult> resultCallback)
        {
            return this.TheExpectedResultIs<FailResult>(resultCallback);
        }

        public TestScenarioBuilder<TMessage> ExpectAIgnoreResult()
        {
            return this.ExpectAIgnoreResult(null);
        }

        public TestScenarioBuilder<TMessage> ExpectAIgnoreResult(Action<IgnoreResult> resultCallback)
        {
            return this.TheExpectedResultIs<IgnoreResult>(resultCallback);
        }

        public TestScenarioBuilder<TMessage> ExpectAReplyResult()
        {
            return this.ExpectAReplyResult(null);
        }

        public TestScenarioBuilder<TMessage> ExpectAReplyResult(Action<ReplyResult> resultCallback)
        {
            return this.TheExpectedResultIs<ReplyResult>(resultCallback);
        }

        public TestScenarioBuilder<TMessage> ExpectARetryResult()
        {
            return this.TheExpectedResultIs<RetryResult>();
        }

        public TestScenarioBuilder<TMessage> ExpectASuccessResult()
        {
            return this.TheExpectedResultIs<SuccessResult>(null);
        }

        public TestScenarioBuilder<TMessage> ExpectASuccessResult(Action<SuccessResult> resultCallback)
        {
            return this.TheExpectedResultIs<SuccessResult>(resultCallback);
        }

        public TestScenarioBuilder<TMessage> TheExpectedResultIs<TResult>(Action<TResult> resultCallback)
            where TResult : IHandlerResult
        {
            this.scenario.ExpectedResultType = typeof(TResult);

            if (resultCallback != null)
            {
                this.scenario.ResultValidationCallback = c => resultCallback((TResult)c);
            }

            return this;
        }

        public TestScenarioBuilder<TMessage> TheExpectedResultIs<TResult>()
            where TResult : IHandlerResult
        {
            return TheExpectedResultIs<TResult>(null);
        }
      
        public void Execute()
        {
            new TestScenarioExecutor<TMessage>(this.scenario, testAdapter).ExecuteTest();
        }
    }
}
