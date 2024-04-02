using System;
using System.Collections.Generic;

namespace AE.Core.Log
{
	/// <summary>
	/// Implement <see cref="ILoggerProvider"/> for console log
	/// </summary>
	public class ConsoleLoggerProvider : ILoggerProvider
	{
		/// <inheritdoc/>
		public ILogger GetLogger(string tag = null)
		{
			return AELogger.New(new List<ILoggerProvider>
			{
				this
			}, tag);
		}

		/// <inheritdoc/>
		public void Log(string message)
		{
			lock (typeof(Console))
			{
				var save = Console.ForegroundColor;

				if (message.Contains("[ERROR]"))
					Console.ForegroundColor = ConsoleColor.Red;

				Console.WriteLine(message);

				Console.ForegroundColor = save;
			}
		}
	}
}
