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

using RabbitMQ.Client;
using System;

namespace Wave.Transports.RabbitMQ
{
    internal class RabbitConnectionManager
    {
        private readonly ConnectionFactory connectionFactory;        
        private Lazy<IConnection> connection;

        internal RabbitConnectionManager(Uri connectionString)
        {            
            this.connectionFactory = new ConnectionFactory { Uri = connectionString.AbsoluteUri, RequestedHeartbeat = 30 };
            this.connection = new Lazy<IConnection>(CreateConnection);                        
        }

        internal IModel GetChannel()
        {
            return this.connection.Value.CreateModel();
        }

        internal void Shutdown()
        {
            this.connection.Value.ConnectionShutdown -= OnConnectionShutDown;
            this.connection.Value.Close();
        }

        private IConnection CreateConnection()
        {
            var conn = this.connectionFactory.CreateConnection();
            conn.ConnectionShutdown += OnConnectionShutDown;

            return conn;
        }

        private void OnConnectionShutDown(IConnection connection, ShutdownEventArgs reason)
        {
            // If the connection is aborted, reinit the lazy connection so that next access will reconnect.
            this.connection = new Lazy<IConnection>(CreateConnection);
        }
    }
}