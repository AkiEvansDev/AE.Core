namespace AE.Core.Log
{
	/// <summary>
	/// Logger provider interface
	/// </summary>
	public interface ILoggerProvider
	{
		/// <summary>
		/// Return logger with provider
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		ILogger GetLogger(string tag = null);

		/// <summary>
		/// Log formatted message
		/// </summary>
		/// <param name="message"></param>
		void Log(string message);
	}
}
