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

using System.IO;
using System.Messaging;

namespace Wave.Transports.MSMQ
{
    public class DelegatedSerializerFormatter : IMessageFormatter
    {
        private ISerializer serializer;

        public DelegatedSerializerFormatter(ISerializer serializer)
        {
            this.serializer = serializer;
        }

        public bool CanRead(Message message)
        {
            return true;
        }

        public object Clone()
        {
            return new DelegatedSerializerFormatter(this.serializer);
        }

        public object Read(Message message)
        {
            using (var reader = new StreamReader(message.BodyStream))
            {
                return this.serializer.Deserialize(reader.ReadToEnd(), typeof(RawMessage));
            }
        }

        public void Write(Message message, object obj)
        {            
            message.BodyStream = new MemoryStream(
                this.serializer.Encoding.GetBytes(
                    this.serializer.Serialize(obj)));            
        }
    }
}
