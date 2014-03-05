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

namespace Wave
{
    /// <summary>
    /// This is the core logging interface used interally by Wave.
    /// The implementation should be provided by an adapter in a consumer or publisher allowing use
    /// of a preferred logging platform.
    /// </summary>
    public interface ILogger
    {
        bool IsDebugEnabled
        {
            get;
        }

        bool IsErrorEnabled
        {
            get;
        }

        bool IsFatalEnabled
        {
            get;
        }

        bool IsInfoEnabled
        {
            get;
        }

        bool IsWarnEnabled
        {
            get;
        }

        void Debug(string message);

        void DebugFormat(string format, params object[] args);

        void Error(string message);

        void ErrorFormat(string format, params object[] args);

        void Fatal(string message);

        void FatalFormat(string format, params object[] args);

        void Info(string message);

        void InfoFormat(string format, params object[] args);

        void Warn(string message);

        void WarnFormat(string format, params object[] args);
    }
}