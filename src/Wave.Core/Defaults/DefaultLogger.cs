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

namespace Wave.Defaults
{
    /// <summary>
    /// Default Internal Logger Implementation
    /// If a consumer / producer does not specify a logger for Wave to use, this is the fallback for logging internal messages from Wave.
    ///
    /// Logs to the console.
    /// </summary>
    public class DefaultLogger : ILogger
    {
        private enum LogLevel
        {
            Debug = 1,
            Info = 2,
            Warn = 3,
            Error = 4,
            Fatal = 5
        }

        public bool IsDebugEnabled
        {
            get { return true; }
        }

        public bool IsErrorEnabled
        {
            get { return true; }
        }

        public bool IsFatalEnabled
        {
            get { return true; }
        }

        public bool IsInfoEnabled
        {
            get { return true; }
        }

        public bool IsWarnEnabled
        {
            get { return true; }
        }

        public void Debug(string message)
        {
            this.Log(LogLevel.Debug, message);
        }

        public void DebugFormat(string format, params object[] args)
        {
            this.Log(LogLevel.Debug, string.Format(format, args));
        }

        public void Error(string message)
        {
            this.Log(LogLevel.Error, message);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            this.Log(LogLevel.Error, string.Format(format, args));
        }

        public void Fatal(string message)
        {
            this.Log(LogLevel.Fatal, message);
        }

        public void FatalFormat(string format, params object[] args)
        {
            this.Log(LogLevel.Fatal, string.Format(format, args));
        }

        public void Info(string message)
        {
            this.Log(LogLevel.Info, message);
        }

        public void InfoFormat(string format, params object[] args)
        {
            this.Log(LogLevel.Info, string.Format(format, args));
        }

        public void Warn(string message)
        {
            this.Log(LogLevel.Warn, message);
        }

        public void WarnFormat(string format, params object[] args)
        {
            this.Log(LogLevel.Warn, string.Format(format, args));
        }

        private void Log(LogLevel level, string message)
        {
            Console.WriteLine("{0} - {1}: {2}", DateTime.UtcNow, level.ToString().ToUpper(), message);
        }
    }
}