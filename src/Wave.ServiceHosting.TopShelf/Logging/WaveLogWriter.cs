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
using System.Globalization;
using Topshelf.Logging;

namespace Wave.ServiceHosting.TopShelf.Logging
{
    /// <summary>
    /// Used to integrate TopShelf logs into whatever framework wave has been configured to log to
    /// </summary>
    public class WaveLogWriter : LogWriter
    {
        private readonly ILogger log;

        public WaveLogWriter(ILogger log)
        {
            this.log = log;
        }

        public void Debug(object message)
        {
            log.Debug(message.ToString());
        }

        public void Debug(object message, Exception exception)
        {
            log.Debug(String.Format("{0} - {1}", message.ToString(), exception.ToString()));
        }

        public void Debug(LogWriterOutputProvider messageProvider)
        {
            if (!IsDebugEnabled)
                return;

            log.Debug(messageProvider().ToString());
        }

        public void DebugFormat(string format, params object[] args)
        {
            log.DebugFormat(format, args);
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            log.DebugFormat(format, args);
        }

        public void Info(object message)
        {
            log.Info(message.ToString());
        }

        public void Info(object message, Exception exception)
        {
            log.Info(String.Format("{0} - {1}", message.ToString(), exception.ToString()));
        }

        public void Info(LogWriterOutputProvider messageProvider)
        {
            if (!IsInfoEnabled)
                return;

            log.Info(messageProvider().ToString());
        }

        public void InfoFormat(string format, params object[] args)
        {
            log.InfoFormat(format, args);
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            log.InfoFormat(format, args);
        }

        public void Warn(object message)
        {
            log.Warn(message.ToString());
        }

        public void Warn(object message, Exception exception)
        {
            log.Warn(String.Format("{0} - {1}", message.ToString(), exception.ToString()));
        }

        public void Warn(LogWriterOutputProvider messageProvider)
        {
            if (!IsWarnEnabled)
                return;

            log.Warn(messageProvider().ToString());
        }

        public void WarnFormat(string format, params object[] args)
        {
            log.WarnFormat(format, args);
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            log.WarnFormat(format, args);
        }

        public void Error(object message)
        {
            log.Error(message.ToString());
        }

        public void Error(object message, Exception exception)
        {
            log.Error(String.Format("{0} - {1}", message.ToString(), exception.ToString()));
        }

        public void Error(LogWriterOutputProvider messageProvider)
        {
            if (!IsErrorEnabled)
                return;

            log.Error(messageProvider().ToString());
        }

        public void ErrorFormat(string format, params object[] args)
        {
            log.ErrorFormat(format, args);
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            log.ErrorFormat(format, args);
        }

        public void Fatal(object message)
        {
            log.Fatal(message.ToString());
        }

        public void Fatal(object message, Exception exception)
        {
            log.Fatal(String.Format("{0} - {1}", message.ToString(), exception.ToString()));
        }

        public void Fatal(LogWriterOutputProvider messageProvider)
        {
            if (!IsFatalEnabled)
                return;

            log.Fatal(messageProvider().ToString());
        }

        public void FatalFormat(string format, params object[] args)
        {
            log.FatalFormat(format, args);
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            log.FatalFormat(format, args);
        }

        public bool IsDebugEnabled
        {
            get { return log.IsDebugEnabled; }
        }

        public bool IsInfoEnabled
        {
            get { return log.IsInfoEnabled; }
        }

        public bool IsWarnEnabled
        {
            get { return log.IsWarnEnabled; }
        }

        public bool IsErrorEnabled
        {
            get { return log.IsErrorEnabled; }
        }

        public bool IsFatalEnabled
        {
            get { return log.IsFatalEnabled; }
        }

        public void Log(LoggingLevel level, object obj)
        {
            if (level == LoggingLevel.Fatal)
            {
                Fatal(obj);
            }
            else if (level == LoggingLevel.Error)
            {
                Error(obj);
            }
            else if (level == LoggingLevel.Warn)
            {
                Warn(obj);
            }
            else if (level == LoggingLevel.Info)
            {
                Info(obj);
            }
            else if (level >= LoggingLevel.Debug)
            {
                Debug(obj);
            }
        }

        public void Log(LoggingLevel level, object obj, Exception exception)
        {
            if (level == LoggingLevel.Fatal)
            {
                Fatal(obj, exception);
            }
            else if (level == LoggingLevel.Error)
            {
                Error(obj, exception);
            }
            else if (level == LoggingLevel.Warn)
            {
                Warn(obj, exception);
            }
            else if (level == LoggingLevel.Info)
            {
                Info(obj, exception);
            }
            else if (level >= LoggingLevel.Debug)
            {
                Debug(obj, exception);
            }
        }

        public void Log(LoggingLevel level, LogWriterOutputProvider messageProvider)
        {
            if (level == LoggingLevel.Fatal)
                Fatal(messageProvider);
            else if (level == LoggingLevel.Error)
                Error(messageProvider);
            else if (level == LoggingLevel.Warn)
                Warn(messageProvider);
            else if (level == LoggingLevel.Info)
                Info(messageProvider);
            else if (level >= LoggingLevel.Debug)
                Debug(messageProvider);
        }

        public void LogFormat(LoggingLevel level, string format, params object[] args)
        {
            if (level == LoggingLevel.Fatal)
                FatalFormat(CultureInfo.InvariantCulture, format, args);
            else if (level == LoggingLevel.Error)
                ErrorFormat(CultureInfo.InvariantCulture, format, args);
            else if (level == LoggingLevel.Warn)
                WarnFormat(CultureInfo.InvariantCulture, format, args);
            else if (level == LoggingLevel.Info)
                InfoFormat(CultureInfo.InvariantCulture, format, args);
            else if (level >= LoggingLevel.Debug)
                DebugFormat(CultureInfo.InvariantCulture, format, args);
        }

        public void LogFormat(LoggingLevel level, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (level == LoggingLevel.Fatal)
                FatalFormat(formatProvider, format, args);
            else if (level == LoggingLevel.Error)
                ErrorFormat(formatProvider, format, args);
            else if (level == LoggingLevel.Warn)
                WarnFormat(formatProvider, format, args);
            else if (level == LoggingLevel.Info)
                InfoFormat(formatProvider, format, args);
            else if (level >= LoggingLevel.Debug)
                DebugFormat(formatProvider, format, args);
        }
    }
}
