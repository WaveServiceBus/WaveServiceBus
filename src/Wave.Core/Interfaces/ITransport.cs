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
using System.Threading;

namespace Wave
{
    public interface ITransport
    {
        void GetDelayMessages(CancellationToken token, Action<RawMessage, Action, Action> onMessageReceived);

        void GetMessages(CancellationToken token, Action<RawMessage, Action, Action> onMessageReceived);

        void InitializeForConsuming();

        void InitializeForPublishing();

        void RegisterSubscription(string subscription);

        void Send(string subscription, object message);

        void Send(string subscription, RawMessage message);

        void SendToDelay(RawMessage message);

        void SendToError(RawMessage message);

        void SendToPrimary(RawMessage message);

        void Shutdown();
    }
}