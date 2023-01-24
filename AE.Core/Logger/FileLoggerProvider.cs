using System;
using System.Collections.Generic;
using System.IO;

namespace AE.Core.Log
{
    /// <summary>
    /// Implement <see cref="ILoggerProvider"/> for file log
    /// </summary>
    public class FileLoggerProvider : ILoggerProvider
    {
        private string FilePath { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="name"></param>
        public FileLoggerProvider(string folder, string name)
        {
            name = name.Replace("{date}", DateTime.Now.ToString("dd.MM.yyyy"));
            FilePath = Path.Combine(folder, name);

            lock (this)
            {
                new DirectoryInfo(folder).Create();

                if (!File.Exists(FilePath))
                    File.Create(FilePath).Close();
            }
        }

        /// <inheritdoc/>
        public ILogger GetLogger(string tag = null)
        {
            return Logger.New(new List<ILoggerProvider>
            {
                this
            }, tag);
        }

        /// <inheritdoc/>
        public void Log(string message)
        {
            lock (this)
            {
                using var stream = new StreamWriter(FilePath, true);
                stream.WriteLine(message);
            }
        }
    }
}
