using AE.Dal;
using System;

namespace AE.Core.Log
{
    /// <summary>
    /// Logger interface
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Tag for log
        /// </summary>
        string Tag { get; set; }

        /// <summary>
        /// Format message and log
        /// </summary>
        /// <param name="message"></param>
        /// <param name="level"></param>
        /// <param name="ignoreEvent"></param>
        void Log(string message, LogLevel level = LogLevel.Message, bool ignoreEvent = false);

        /// <summary>
        /// Format exception and log
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        /// <param name="level"></param>
        /// <param name="method"></param>
        /// <param name="ignoreEvent"></param>
        void Log(Exception ex, string message = null, LogLevel level = LogLevel.Error, string method = null, bool ignoreEvent = false);
    }
}
