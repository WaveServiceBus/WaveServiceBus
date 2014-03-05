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

using Wave.Examples.ECommerce.Models;
using Wave.Examples.ECommerce.Messages;

namespace Wave.Examples.ECommerce.OrderAgent.Subscriptions
{
    public class OrderPlacedSubscription : ISubscription<OrderPlaced>
    {
        private readonly OrderContext dbContext = new OrderContext();
        private readonly ILogger log;
        private readonly IBusClient busClient;

        public OrderPlacedSubscription(ILogger log, IBusClient busClient)
        {
            this.log = log;
            this.busClient = busClient;
        }

        public IHandlerResult Handle(MessageEnvelope<OrderPlaced> message)
        {            
            this.dbContext.Orders.Add(message.Content.OrderData);
            this.dbContext.SaveChanges();
            this.log.DebugFormat("Order {0} Created", message.Content.OrderData.Number);

            this.busClient.Publish(new OrderCreated
            {
                OrderNumber = message.Content.OrderData.Number
            });

            return message.Success();
        }
    }
}
