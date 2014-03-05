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

using System.Reflection;
using Wave.Configuration;

namespace Wave.Tests
{
    public class TestHelpers
    {
        internal static T CallPrivateMethod<T, TObject>(string methodName, TObject instance, object[] methodParams)
        {
            var method = typeof(TObject).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)method.Invoke(instance, methodParams);
        }

        internal static T GetPrivateField<T, TObject>(string fieldName, TObject instance)
        {
            var field = typeof(TObject).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)field.GetValue(instance);
        }

        internal static T GetPrivateProperty<T, TObject>(string propName, TObject instance)
        {
            var prop = typeof(TObject).GetProperty(propName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)prop.GetValue(instance, null);
        }

        internal static RawMessage GetRawMessage<T>(T input, ISerializer serializer, ISubscriptionKeyResolver keyResolver)
        {
            return new RawMessage
            {
                Type = keyResolver.GetKey(typeof(T)),
                Data = serializer.Serialize(input)                
            };
        }

        internal static RawMessage GetRawMessage<T>(T input, ConfigurationSource config)
        {
            return GetRawMessage<T>(input, config.ConfigurationContext.Serializer, config.ConfigurationContext.SubscriptionKeyResolver);
        }
    }
}
