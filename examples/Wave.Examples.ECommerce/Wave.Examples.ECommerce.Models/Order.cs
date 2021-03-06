﻿/* Copyright 2014 Jonathan Holland.
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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wave.Examples.ECommerce.Models
{
    public enum OrderStatus
    {
        Created,
        Backordered,
        Shipped
    }

    public class Order
    {
        [EmailAddress]
        [Required]
        public string CustomerEmail { get; set; }

        [Required]
        public string Item { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Number { get; set; }

        public DateTime OrderDate { get; set; }

        [Required]
        public int Quantity { get; set; }

        public OrderStatus Status { get; set; }        
    }
}
