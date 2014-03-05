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
using System.Runtime.Serialization.Json;
using System.Text;

namespace Wave.Defaults
{
    /// <summary>
    /// Default serializer used if an external one is not provided.
    ///
    /// Utilizes the WCF DataContractJsonSerializer to generate JSON data without any external assemblies
    /// </summary>
    public class DefaultSerializer : ISerializer
    {
        public string ContentType
        {
            get
            {
                return "application/json";
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
                var serializer = new DataContractJsonSerializer(type);
                return serializer.ReadObject(stream);
            }
        }

        public string Serialize(object input)
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(input.GetType());
                serializer.WriteObject(stream, input);
                return Encoding.GetString(stream.ToArray());
            }
        }
    }
}