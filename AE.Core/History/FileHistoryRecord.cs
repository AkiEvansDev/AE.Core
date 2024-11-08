using AE.Dal;
using System;

namespace AE.Core.History
{
    internal interface IFileHistoryRecord : IHistoryRecord
    {
        string File { get; }

        IHistoryRecord Load();
        void Clear();
    }

    [AESerializable]
    internal class FileHistoryRecord : HistoryRecord, IFileHistoryRecord
    {
        public string File { get; }

        public FileHistoryRecord(IHistoryRecord data) : base("FILE_HISTORY")
        {
            File = $"{DateTime.UtcNow.Ticks}-{Guid.NewGuid()}";
            EnvironmentHelper.SaveAppData(File, data);
        }

        public IHistoryRecord Load()
        {
            return EnvironmentHelper.LoadAppData<IHistoryRecord>(File);
        }

        public void Clear()
        {
            EnvironmentHelper.ClearAppData(File);
        }
    }
}
