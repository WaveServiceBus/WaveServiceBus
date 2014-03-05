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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Text;

namespace Wave.Serialization.JsonDotNet
{
    public class JsonDotNetSerializer : ISerializer
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
            return JsonConvert.DeserializeObject(input, type);
        }

        public string Serialize(object input)
        {           
            return JsonConvert.SerializeObject(input, new IsoDateTimeConverter());
        }      
    }
}
