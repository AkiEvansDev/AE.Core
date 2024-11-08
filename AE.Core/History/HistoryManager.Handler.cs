using AE.Core.Log;
using AE.Dal;
using System.Collections.Generic;

namespace AE.Core.History
{
    /// <summary>
    /// Cancel undo actions and return redo record
    /// </summary>
    /// <param name="record"></param>
    /// <returns></returns>
    public delegate (IHistoryRecord Redo, bool UseFile) HistoryHandler(IHistoryRecord undo);

    public static partial class HistoryManager
    {
        private static Dictionary<string, HistoryHandler> Handlers = new Dictionary<string, HistoryHandler>();

        public static bool AddHistoryHandler(string type, HistoryHandler historyHandler)
        {
            if (Handlers.ContainsKey(type))
            {
                AELogger.DefaultLogger?.Log($"Duplicate {type} handler!", LogLevel.Error);
                return false;
            }

            Handlers.Add(type, historyHandler);

            return true;
        }
    }
}
