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

using System.Linq;
using Wave.Examples.ECommerce.Messages;
using Wave.Examples.ECommerce.Models;

namespace Wave.Examples.ECommerce.OrderAgent.Subscriptions
{
    public class OrderBackorderedSubscription : ISubscription<OrderBackOrdered>
    {
        private readonly OrderContext dbContext = new OrderContext();
        private readonly ILogger log;

        public OrderBackorderedSubscription(ILogger log)
        {
            this.log = log;
        }

        public IHandlerResult Handle(MessageEnvelope<OrderBackOrdered> message)
        {
            this.log.DebugFormat("Updating Order {0} to Backordered", message.Content.OrderNumber);
            var order = this.dbContext.Orders.FirstOrDefault(o => o.Number == message.Content.OrderNumber);
            order.Status = OrderStatus.Backordered;

            this.dbContext.SaveChanges();

            return message.Success();
        }
    }
}
