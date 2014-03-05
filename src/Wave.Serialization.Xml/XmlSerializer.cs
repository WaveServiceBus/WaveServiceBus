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
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Wave.Serialization.Xml
{
    public class XmlSerializer : ISerializer
    {
        public string ContentType
        {
            get
            {
                return "application/xml";
            }
        }

        public Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }


        public object Deserialize(string input, Type type)
        {           
            using (var stream = new MemoryStream(Encoding.GetBytes(input)))
            {
                var serializer = new DataContractSerializer(type);
                return serializer.ReadObject(stream);
            }
        }

        public string Serialize(object input)
        {           
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractSerializer(input.GetType());
                serializer.WriteObject(stream, input);
                return Encoding.GetString(stream.ToArray());
            }
        }                     
    }
}
