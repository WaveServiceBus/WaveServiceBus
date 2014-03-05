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
using System.Web.Mvc;
using Wave.Examples.ECommerce.Messages;
using Wave.Examples.ECommerce.Models;

namespace Wave.Examples.ECommerce.Web.Controllers
{
    public class OrdersController : Controller
    {
        private readonly OrderContext dbContext = new OrderContext();

        public ActionResult Index()
        {
            return View(this.dbContext.Orders.ToList());
        }

        [HttpPost]
        public ActionResult Create(Order order)
        {
            // Return with validation errors if needed
            if (!this.ModelState.IsValid)
            {
                return View("Index", this.dbContext.Orders.ToList());
            }

            // Set remaining fields
            order.Status = OrderStatus.Created;
            order.OrderDate = DateTime.UtcNow;

            // Publish the OrderPlaced event to the bus containing the order
            ServiceBus.Publish(new OrderPlaced
            {
                OrderData = order
            });

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            this.dbContext.Dispose();
            base.Dispose(disposing);
        }
    }
}
