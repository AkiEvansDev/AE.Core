using System;
using System.Collections.Generic;
namespace AE.Core.Serializer
{
    /// <summary>
    /// Serializer
    /// </summary>
    public partial class AESerializer : IDisposable
    {
        private const string DATETIME_FORMAT = "MM.dd.yyyy HH:mm:ss.f";
        private const string TIMESPAN_FORMAT = "c";

		private const char FIRST_ST = '~';
        private const string STRING_T = "~[";

        private List<string> TypeTabel { get; set; }

        private Type GetSaveType(string name)
        {
            if (name.TryInt(out int index) && index < TypeTabel?.Count)
                name = TypeTabel[index];

            return name.Type();
        }

        private int GetTypeToSave(object obj)
            => GetTypeToSave(obj.GetType());

        private int GetTypeToSave(Type t)
        {
            if (!TypeTabel.Contains(t.FullName))
                TypeTabel.Add(t.FullName);

            return TypeTabel.IndexOf(t.FullName);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Builder?.Clear();
            TypeTabel?.Clear();

            Sources?.Clear();
        }
    }
}
