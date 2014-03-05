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
using Wave.Examples.ECommerce.Messages;
using Wave.Examples.ECommerce.Models;

namespace Wave.Examples.ECommerce.InventoryAgent.Subscriptions
{
    public class OrderCreatedSubscription : ISubscription<OrderCreated>
    {
        private readonly IBusClient busClient;
        private readonly OrderContext dbContext = new OrderContext();
        private readonly ILogger log;
        public OrderCreatedSubscription(ILogger log, IBusClient busClient)
        {
            this.log = log;
            this.busClient = busClient;
        }

        public IHandlerResult Handle(MessageEnvelope<OrderCreated> message)
        {
            var order = this.dbContext.Orders.FirstOrDefault(o => o.Number == message.Content.OrderNumber);
           
            // In reality, fancy business logic would live here, but for this sample
            // if inventory is < 10, we update the order to shipped, if it is >= 10
            // we update the order to back ordered.
            if (order.Quantity < 10)
            {
                this.log.DebugFormat("Order {0} Shipped", order.Number);              
                this.busClient.Publish(new OrderShipped
                {
                    OrderNumber = order.Number
                });
            }
            else
            {
                this.log.DebugFormat("Order {0} Backordered", order.Number);                          
                this.busClient.Publish(new OrderBackOrdered
                {
                    OrderNumber = order.Number
                });
            }
            
            return message.Success();
        }
    }
}
