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
using Common.Logging;

namespace Wave.Logging.CommingsLogging
{
    public class CommonsLogger : ILogger
    {
        private readonly ILog internalLogger;

        public CommonsLogger(IConfigurationContext configuration)
        {
            internalLogger = LogManager.GetLogger(configuration.AssemblyLocator.GetEntryAssembly().GetName().Name);
        }

        public bool IsDebugEnabled
        {
            get { return internalLogger.IsDebugEnabled; }
        }

        public bool IsErrorEnabled
        {
            get { return internalLogger.IsErrorEnabled; }
        }

        public bool IsFatalEnabled
        {
            get { return internalLogger.IsFatalEnabled; }
        }

        public bool IsInfoEnabled
        {
            get { return internalLogger.IsInfoEnabled; }
        }

        public bool IsWarnEnabled
        {
            get { return internalLogger.IsWarnEnabled; }
        }

        public void Debug(string message)
        {
            internalLogger.Debug(message);
        }

        public void DebugFormat(string format, params object[] args)
        {
            internalLogger.DebugFormat(format, args);
        }

        public void Error(string message)
        {
            internalLogger.Error(message);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            internalLogger.ErrorFormat(format, args);
        }

        public void Fatal(string message)
        {
            internalLogger.Fatal(message);
        }

        public void FatalFormat(string format, params object[] args)
        {
            internalLogger.FatalFormat(format, args);
        }

        public void Info(string message)
        {
            internalLogger.Info(message);
        }

        public void InfoFormat(string format, params object[] args)
        {
            internalLogger.InfoFormat(format, args);
        }

        public void Warn(string message)
        {
            internalLogger.Warn(message);
        }

        public void WarnFormat(string format, params object[] args)
        {
            internalLogger.WarnFormat(format, args);
        }
    }
}
