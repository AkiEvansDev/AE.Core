using AE.Dal;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AE.Core.Log
{
	/// <summary>
	/// Implement <see cref="ILogger"/>
	/// </summary>
	public class AELogger : ILogger
	{
		#region Static

		/// <summary>
		/// Default logs
		/// </summary>
		public static ILogger DefaultLogger { get; set; }

		/// <summary>
		/// Event on log message
		/// </summary>
		public static event Action<string> OnLog;

		/// <summary>
		/// Create new logger
		/// </summary>
		/// <param name="providers"></param>
		/// <param name="tag"></param>
		/// <returns></returns>
		public static ILogger New(IEnumerable<ILoggerProvider> providers, string tag = null)
		{
			return new AELogger(providers, tag);
		}

		#endregion

		/// <inheritdoc/>
		public string Tag { get; set; }

		private readonly IEnumerable<ILoggerProvider> Providers;

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="providers"></param>
		/// <param name="tag"></param>
		protected AELogger(IEnumerable<ILoggerProvider> providers, string tag)
		{
			Tag = tag;
			Providers = providers;
		}

		/// <inheritdoc/>
		public void Log(string message, LogLevel level = LogLevel.Message, bool ignoreEvent = false)
			=> Log(null, message, level, "", ignoreEvent);

		/// <inheritdoc/>
		public void Log(Exception ex, string message = null, LogLevel level = LogLevel.Error, [CallerMemberName] string method = null, bool ignoreEvent = false)
		{
			if (ex != null)
			{
				message = $"{(!method.IsNull() ? $"{method}() - " : "")}{(message.IsNull() ? "" : $"{message}:{Environment.NewLine}")}";
				var tab = "";

				while (ex != null)
				{
					message += $"{tab}{ex.Message.Replace("\n", $"\n{tab}")}{Environment.NewLine}{tab}{ex.StackTrace.Replace("\n", $"\n{tab}")}{Environment.NewLine}";
					ex = ex.InnerException;
					tab = $"{tab}    ";
				}
			}

			Log($"[{DateTime.Now:hh:mm:ss}]{(!Tag.IsNull() ? $"[{Tag.ToUpper()}]" : "")}[{level.GetDescription()}] {message}", ignoreEvent);
		}

		private void Log(string message, bool ignoreEvent)
		{
			lock (this)
			{
				if (!ignoreEvent)
					OnLog?.Invoke(message);

				foreach (var provider in Providers)
					provider.Log(message);
			}
		}
	}
}
