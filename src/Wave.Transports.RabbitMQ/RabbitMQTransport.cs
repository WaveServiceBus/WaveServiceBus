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

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using Wave.Transports.RabbitMQ.Extensions;
using Wave.Utility;

namespace Wave.Transports.RabbitMQ
{
    public class RabbitMQTransport : ITransport
    {
        private readonly RabbitConnectionManager connectionManager;        
        private readonly String delayQueueName;
        private readonly String errorQueueName;
        private readonly String primaryQueueName;
        private readonly IConfigurationContext configuration;

        private readonly Lazy<string> encodingName;

        private readonly ThreadLocal<IModel> sendChannel;
 
        public RabbitMQTransport(IConfigurationContext configuration)
            : this(configuration.QueueNameResolver.GetPrimaryQueueName(), configuration)
        {            
        }

        internal RabbitMQTransport(string baseQueueName, IConfigurationContext configuration)
        {
            this.configuration = MergeConfiguration(configuration);

            // Cache encoding name as looking it from the Encoding class is expensive.
            this.encodingName = new Lazy<string>(() => configuration.Serializer.Encoding.EncodingName);

            this.connectionManager = new RabbitConnectionManager(new Uri(configuration.GetConnectionString()));
            this.primaryQueueName = baseQueueName;
            this.delayQueueName = String.Format("{0}_Delay", this.primaryQueueName);
            this.errorQueueName = String.Format("{0}_Error", this.primaryQueueName);

            using (var channel = this.connectionManager.GetChannel())
            {
                // Create exchange if it doesn't already exist
                channel.ExchangeDeclare(this.configuration.GetExchange(), "direct", true);
            }

            this.sendChannel = new ThreadLocal<IModel>(() => this.connectionManager.GetChannel(), true);
        }

        public void GetDelayMessages(CancellationToken token, Action<RawMessage, Action, Action> onMessageReceived)
        {
            this.GetMessages(this.delayQueueName, token, onMessageReceived);
        }

        public void GetMessages(CancellationToken token, Action<RawMessage, Action, Action> onMessageReceived)
        {
            this.GetMessages(this.primaryQueueName, token, onMessageReceived);
        }

        public void InitializeForConsuming()
        {
            using (var channel = this.connectionManager.GetChannel())
            {
                var autoDelete = this.configuration.GetAutoDeleteQueues();
                var workQueue = channel.QueueDeclare(this.primaryQueueName, true, autoDelete, autoDelete, null);
                var delayQueue = channel.QueueDeclare(this.delayQueueName, true, autoDelete, autoDelete, null);
                var errorQueue = channel.QueueDeclare(this.errorQueueName, true, autoDelete, autoDelete, null);

                // Create a routing key for the work queue name for direct sends
                channel.QueueBind(workQueue, this.configuration.GetExchange(), this.primaryQueueName);

                // Create a routing key for the delay queue
                channel.QueueBind(delayQueue, this.configuration.GetExchange(), this.delayQueueName);

                // Create a routing key for the error queue
                channel.QueueBind(errorQueue, this.configuration.GetExchange(), this.errorQueueName);
            }
        }

        public void InitializeForPublishing()
        {
            // No Op for RabbitMQ Transport
        }

        public void RegisterSubscription(string subscription)
        {
            // Create a binding on work queue, set the routing key to the susbcription
            using (var channel = this.connectionManager.GetChannel())
            {
                channel.QueueBind(this.primaryQueueName, this.configuration.GetExchange(), subscription);
            }
        }

        public void Send(string subscription, object message)
        {
            this.Send(subscription, RawMessage.Create(message));
        }

        public void Send(string subscription, RawMessage message)
        {
            if (this.sendChannel.Value.IsClosed)
            {
                this.sendChannel.Value.Dispose();
                this.sendChannel.Value = this.connectionManager.GetChannel();
            }

            this.sendChannel.Value.BasicPublish(
                this.configuration.GetExchange(),
                subscription,
                this.CreateProperties(message, this.sendChannel.Value),
                this.configuration.Serializer.Encoding.GetBytes(message.Data));
        }

        public void SendToDelay(RawMessage message)
        {
            this.Send(this.delayQueueName, message);
        }

        public void SendToError(RawMessage message)
        {
            this.Send(this.errorQueueName, message);
        }

        public void SendToPrimary(RawMessage message)
        {
            this.Send(this.primaryQueueName, message);
        }

        public void Shutdown()
        {
            // Dispose all of the channels 
            foreach (var channel in this.sendChannel.Values)
            {
                channel.Dispose();
            }
            
            // Dispose the thread local wrapper
            this.sendChannel.Dispose();

            // Force the RabbitMQ connection to shutdown.
            this.connectionManager.Shutdown();
        }

        private IBasicProperties CreateProperties(RawMessage message, IModel channel)
        {
            var properties = channel.CreateBasicProperties();
            properties.MessageId = message.Id.ToString();
            properties.AppId = this.primaryQueueName;
            properties.ContentType = this.configuration.Serializer.ContentType;
            properties.ContentEncoding = this.encodingName.Value;
            properties.SetPersistent(true);
            properties.Timestamp = new AmqpTimestamp((long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds);
            properties.Headers = new Dictionary<String,Object>();

            foreach (var pair in message.Headers)
            {
                properties.Headers[pair.Key] = pair.Value;
            }

            return properties;
        }

        private void GetMessages(string queueName, CancellationToken token, Action<RawMessage, Action, Action> onMessageReceived)
        {
            using (var channel = this.connectionManager.GetChannel())
            {
                var consumer = new QueueingBasicConsumer(channel);
                var prefetchCount = (this.configuration.MaxWorkers * 2) >= ushort.MaxValue
                                        ? ushort.MaxValue
                                        : (ushort)(this.configuration.MaxWorkers * 2);

                channel.BasicQos(0, prefetchCount, false);
                channel.BasicConsume(queueName, false, consumer);
                
                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        channel.Dispose();                        
                        token.ThrowIfCancellationRequested();
                    }

                    BasicDeliverEventArgs rabbitMessage;
                    if (consumer.Queue.Dequeue(1500, out rabbitMessage))
                    {
                        if (rabbitMessage == null)
                        {
                            continue;
                        }
                        
                        var rawMessage = new RawMessage
                        {
                            Data = this.configuration.Serializer.Encoding.GetString(rabbitMessage.Body),
                            Id = new Guid(rabbitMessage.BasicProperties.MessageId)
                        };

                        foreach (var header in rabbitMessage.BasicProperties.Headers)
                        {
                            rawMessage.Headers[header.Key] = this.configuration.Serializer.Encoding.GetString((Byte[])header.Value);
                        }

                        // Callback and provide an accept and reject callback to the consumer                        
                        onMessageReceived(
                            rawMessage,
                            () => channel.BasicAck(rabbitMessage.DeliveryTag, true),
                            () => channel.BasicNack(rabbitMessage.DeliveryTag, false, true));
                    }
                }
            }
        }
           
        private IConfigurationContext MergeConfiguration(IConfigurationContext context)
        {
            var defaultSettings = new Configuration.ConfigurationSettings();
            var configSection = ConfigurationHelper.GetConfigSection<Configuration.ConfigurationSection>();

            // If the value is in the config use that
            // Otherwise, if the value is already set via fluent config use that
            // Otherwise use the default value
            if (configSection != null)
            {
                if (!String.IsNullOrWhiteSpace(configSection.ConnectionStringName))
                {
                    context.SetConnectionString(ConfigurationManager.ConnectionStrings[configSection.ConnectionStringName].ConnectionString);
                }
                else if (context.GetConnectionString() == null)
                {
                    context.SetConnectionString(defaultSettings.ConnectionString);
                }

                if (!String.IsNullOrWhiteSpace(configSection.Exchange))
                {
                    context.SetExchange(configSection.Exchange);
                }
                else if (context.GetExchange() == null)
                {
                    context.SetExchange(defaultSettings.Exchange);
                }
            }

            return context;
        }
    }
}
