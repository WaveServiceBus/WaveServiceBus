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
using System.Threading.Tasks;
using Wave.Configuration;
using Wave.Consumers;

namespace Wave
{
    using System.Web.Handlers;

    public class BusHost : IBusHost
    {
        private bool consumersFaulted;

        private IConfigurationContext configuration;

        public BusHost(IConfigurationContext configuration)
        {
            this.configuration = configuration;
        }

        protected IConfigurationContext ConfigurationContext
        {
            get { return this.configuration; }
            set { this.configuration = value; }
        }

        public static IBusHost Create()
        {
            return Create(config => { });
        }

        public static IBusHost Create(Action<ConfigurationBuilder> configureFunction)
        {
            var config = new ConfigurationBuilder();
            configureFunction(config);
            return config.ConfigureForHosting();
        }

        public static void Run(Action<ConfigurationBuilder> configureFunction)
        {
            Create(configureFunction).Start();
        }

        public static void Run()
        {
            Create().Start();
        }

        public virtual void Start()
        {
            var cancelToken = this.configuration.TokenSource.Token;
            var consumerTasks = new List<Task>();

            this.configuration.Logger.Info("Starting...");
            this.InitTransport();

            // Start the work queue consumers
            this.configuration.Logger.DebugFormat("Starting {0} work queue consumer{1}", this.configuration.MaxWorkers, this.configuration.MaxWorkers == 1 ? string.Empty : "s");
            for (var i = 0; i < this.configuration.MaxWorkers; i++)
            {
                var consumer = new PrimaryConsumer(this.configuration);
                consumerTasks.Add(this.CreateConsumerTask(consumer));
            }

            // Start the delay consumer
            var delayConsumer = new DelayConsumer(this.configuration);
            cancelToken.Register(() => delayConsumer.Cancel());
            consumerTasks.Add(this.CreateConsumerTask(delayConsumer));

            try
            {
                Task.WaitAll(consumerTasks.ToArray());
            }
            catch (AggregateException ex)
            {
                // Ignore if it is just OperationCancelledException
                // That will be thrown when cancellation is requested
                if (!ex.InnerExceptions.All(e => e is OperationCanceledException))
                {
                    this.configuration.Logger.FatalFormat("Unhandled exception inside of consumer: {0}", ex.ToString());
                    this.consumersFaulted = true;
                    throw;
                }
            }

            this.configuration.Transport.Shutdown();
        }

        public virtual bool IsHealthy
        {
            get
            {
                return !this.consumersFaulted;
            }
        }

        public virtual void Stop()
        {
            this.configuration.Logger.Info("Stopping");
            this.configuration.TokenSource.Cancel();           
        }

        private Task CreateConsumerTask(IConsumer consumer)
        {
            return Task.Factory.StartNew(
                    () => consumer.ConsumeQueue(),
                    this.configuration.TokenSource.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
        }

        private void InitTransport()
        {
            this.configuration.Transport.InitializeForConsuming();
            foreach (var subscription in this.configuration.Subscriptions)
            {
                // Tell the backend transport to subscribe to this message type
                this.configuration.Logger.InfoFormat("Subscribing to message type {0}", subscription.Key);
                this.configuration.Transport.RegisterSubscription(this.configuration.SubscriptionKeyResolver.GetKey(subscription.Key));
            }
        }
    }
}