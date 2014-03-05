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
using System.Messaging;
using System.Threading;
using Wave.Transports.MSMQ.Extensions;
using Wave.Utility;

namespace Wave.Transports.MSMQ
{
    public sealed class MSMQTransport : ITransport, IDisposable
    {
        private readonly String delayQueueName;
        private readonly String errorQueueName;
        private readonly String primaryQueueName;

        private readonly ConcurrentDictionary<String, String> subscriptions;

        private IConfigurationContext configuration;

        private MessageQueue delayQueue;
        private MessageQueue errorQueue;
        private MessageQueue outboundQueue;
        private MessageQueue primaryQueue;
        
        public MSMQTransport(IConfigurationContext configuration)
            : this(configuration.QueueNameResolver.GetPrimaryQueueName(), configuration)
        {
        }

        internal MSMQTransport(string baseQueueName, IConfigurationContext configuration)
        {
            this.configuration = MergeConfiguration(configuration);
            this.subscriptions = new ConcurrentDictionary<String, String>();
            this.primaryQueueName = baseQueueName;
            this.delayQueueName = String.Format("{0}_Delay", this.primaryQueueName);
            this.errorQueueName = String.Format("{0}_Error", this.primaryQueueName);
        }

        public void Dispose()
        {
            this.delayQueue.Dispose();
            this.errorQueue.Dispose();
            this.outboundQueue.Dispose();
            this.primaryQueue.Dispose();
        }

        public void GetDelayMessages(System.Threading.CancellationToken token, Action<RawMessage, Action, Action> onMessageReceived)
        {
            this.GetMessages(delayQueue, token, onMessageReceived);
        }

        public void GetMessages(System.Threading.CancellationToken token, Action<RawMessage, Action, Action> onMessageReceived)
        {
            this.GetMessages(primaryQueue, token, (msg, ack, nack) =>
            {
                // MSMQ Multicast will deliver all published messages to
                // all queues, throw away any messages we are not explicitly
                // subscribed too.
                if (this.subscriptions.ContainsKey(msg.Type))
                {
                    onMessageReceived(
                        msg,
                        () => { /* Ack is a No-Op  */ },
                        () => { /* Nack is a No-Op */ });
                }
            });
        }

        public void InitializeForConsuming()
        {
            this.primaryQueue = GetQueue(this.primaryQueueName, false);
            this.delayQueue = GetQueue(this.delayQueueName, false);
            this.errorQueue = GetQueue(this.errorQueueName, false);

            this.primaryQueue.MulticastAddress = configuration.GetMulticastAddress();
        }

        public void InitializeForPublishing()
        {
            this.outboundQueue = new MessageQueue(String.Format("FormatName:MULTICAST={0}", this.configuration.GetMulticastAddress()));
            this.outboundQueue.Formatter = new DelegatedSerializerFormatter(this.configuration.Serializer);
        }

        public void RegisterSubscription(string subscription)
        {
            this.subscriptions.AddOrUpdate(subscription, String.Empty, (k, v) => String.Empty);
        }
       
        public void Send(string subscription, object message)
        {
            this.Send(subscription, RawMessage.Create(message));
        }

        public void Send(string subscription, RawMessage message)
        {
            this.outboundQueue.Send(message);
        }

        public void SendToDelay(RawMessage message)
        {
            this.delayQueue.Send(message);
        }

        public void SendToError(RawMessage message)
        {
            this.errorQueue.Send(message);
        }

        public void SendToPrimary(RawMessage message)
        {
            this.primaryQueue.Send(message);
        }

        public void Shutdown()
        {
            // No Op
        }

        private void GetMessages(MessageQueue queue, CancellationToken token, Action<RawMessage, Action, Action> onMessageReceived)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }

                try
                {
                    var message = queue.Receive(TimeSpan.FromSeconds(15));
                    if (message != null)
                    {
                        var rawMessage = (RawMessage)message.Body;
                        onMessageReceived(
                            rawMessage,
                            () => { /* Ack is a No-Op  */ },
                            () => { /* Nack is a No-Op */ });
                    }
                }
                catch (MessageQueueException ex)
                {
                    // MSMQ throws an exception when it times out, just ignore that.
                    if (ex.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout)
                    {
                        throw;
                    }
                }
            }
        }

        private MessageQueue GetQueue(string queueName, bool transactional)
        {
            var queuePath = String.Format("{0}\\Private$\\{1}", configuration.GetHostname(), queueName);
            if (!MessageQueue.Exists(queuePath))
            {
                MessageQueue.Create(queuePath, transactional);
            }

            var queue = new MessageQueue(queuePath);
            queue.Formatter = new DelegatedSerializerFormatter(this.configuration.Serializer);
            queue.SetPermissions("Everyone", MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow);
            queue.SetPermissions("ANONYMOUS LOGON", MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow);

            return queue;
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
                if (!String.IsNullOrWhiteSpace(configSection.Hostname))
                {
                    context.SetHostname(configSection.Hostname);
                }
                else if (context.GetHostname() == null)
                {
                    context.SetHostname(defaultSettings.Hostname);
                }

                if (!String.IsNullOrWhiteSpace(configSection.MulticastAddress))
                {
                    context.SetMulticastAddress(configSection.MulticastAddress);
                }
                else if (context.GetMulticastAddress() == null)
                {
                    context.SetMulticastAddress(defaultSettings.MulticastAddress);
                }
            }

            return context;
        }
    }
}
