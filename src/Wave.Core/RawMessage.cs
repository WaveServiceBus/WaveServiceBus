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
using Wave.Configuration;

namespace Wave
{
    /// <summary>
    /// Represents a serialized message - Used as a convienance object since C# doesn't allow for multiple return types
    /// </summary>
    [Serializable]
    public class RawMessage
    {
        private Dictionary<string, string> headers;

        public string Data { get; set; }

        public DateTime? DelayUntil
        {
            get
            {
                if (this.Headers.ContainsKey("DelayUntil") && !string.IsNullOrEmpty(this.Headers["DelayUntil"]))
                {
                    return new DateTime(DateTime.Parse(this.Headers["DelayUntil"]).Ticks, DateTimeKind.Utc);
                }

                return null;
            }

            set
            {
                this.Headers["DelayUntil"] = value.ToString();
            }
        }

        public Dictionary<string, string> Headers
        {
            get { return this.headers ?? (this.headers = new Dictionary<string, string>()); }
            set { this.headers = value; }
        }

        public Guid Id { get; set; }

        public string ReplyTopic
        {
            get
            {
                if (this.Headers.ContainsKey("ReplyTopic"))
                {
                    return this.Headers["ReplyTopic"];
                }

                return string.Empty;
            }

            set
            {
                this.Headers["ReplyTopic"] = value;
            }
        }

        public int RetryCount
        {
            get
            {
                if (this.Headers.ContainsKey("RetryCount") && !string.IsNullOrEmpty(this.headers["RetryCount"]))
                {
                    return int.Parse(this.Headers["RetryCount"]);
                }

                return 0;
            }

            set
            {
                this.Headers["RetryCount"] = value.ToString();
            }
        }
        public string Type
        {
            get
            {
                if (this.Headers.ContainsKey("Type"))
                {
                    return this.Headers["Type"];
                }

                return string.Empty;
            }

            set
            {
                this.Headers["Type"] = value;
            }
        }

        public static RawMessage Create(object message)
        {
            return new RawMessage
            {
                Data = ConfigurationContext.Current.Serializer.Serialize(message),
                Id = Guid.NewGuid(),
                Type = ConfigurationContext.Current.SubscriptionKeyResolver.GetKey(message.GetType()),
                ReplyTopic = ConfigurationContext.Current.QueueNameResolver.GetPrimaryQueueName()
            };
        }

        public override string ToString()
        {
            return this.Id.ToString();
        }
    }
}