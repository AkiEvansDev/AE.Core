using AE.Dal;

namespace AE.Core.History
{
    public interface IHistoryRecord
    {
        string Type { get; }
    }

    [AESerializable]
    public abstract class HistoryRecord : IHistoryRecord
    {
        public string Type { get; }

        public HistoryRecord(string type)
        {
            Type = type;
        }
    }
}
