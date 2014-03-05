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
using System.Text;

namespace Wave
{
    /// <summary>
    /// Core serialization interface used internally by Wave.
    ///
    /// Publishers / Consumers should plugin in Json.Net or ServiceStack or whatever they prefer.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Gets the Mime type of the serialized content
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Gets the encoding used to serialize content
        /// </summary>
        Encoding Encoding { get; }

        /// <summary>
        /// Converts serialized content into a constructed instance
        /// </summary>
        /// <param name="input"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        object Deserialize(string input, Type type);

        /// <summary>
        /// Serializes a instance into a string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        string Serialize(object input);
    }
}