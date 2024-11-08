using AE.Core.Log;
using AE.Dal;
using System;
using System.Collections.Generic;

namespace AE.Core.History
{
    public static partial class HistoryManager
    {
        public const int DEFAULT_MAX_HISTORY_COUNT = 100000;

        private static LimitedStack<IHistoryRecord> UndoHistory = new LimitedStack<IHistoryRecord>(DEFAULT_MAX_HISTORY_COUNT);
        private static LimitedStack<IHistoryRecord> RedoHistory = new LimitedStack<IHistoryRecord>(DEFAULT_MAX_HISTORY_COUNT);

        public static event Action UndoChanged;
        public static event Action RedoChanged;

        public static bool CanUndo => UndoHistory.Count > 0;
        public static bool CanRedo => RedoHistory.Count > 0;

        private static bool clearRedo;
        private static bool ignoreClearRedo;

        public static void Init(int maxHistoryCount = DEFAULT_MAX_HISTORY_COUNT)
        {
            Handlers = new Dictionary<string, HistoryHandler>();

            UndoHistory = new LimitedStack<IHistoryRecord>(maxHistoryCount);
            RedoHistory = new LimitedStack<IHistoryRecord>(maxHistoryCount);
        }

        public static bool Undo()
        {
            if (UndoHistory.TryPop(out var history))
            {
                clearRedo = false;
                IFileHistoryRecord fileHistory = history as IFileHistoryRecord;

                if (fileHistory != null)
                    history = fileHistory.Load();

                if (Handlers.ContainsKey(history.Type))
                {
                    var record = Handlers[history.Type](history);

                    PushRedo(record.Redo, record.UseFile);

                    fileHistory?.Clear();
                    return true;
                }
                else
                    AELogger.DefaultLogger?.Log($"No {history.Type} handler!", LogLevel.Error);
            }

            return false;
        }

        public static bool Redo()
        {
            if (clearRedo)
            {
                RedoHistory.Clear();
                clearRedo = false;

                RedoChanged?.Invoke();
            }
            else if (RedoHistory.TryPop(out var history))
            {
                IFileHistoryRecord fileHistory = history as IFileHistoryRecord;

                if (fileHistory != null)
                    history = fileHistory.Load();

                if (Handlers.ContainsKey(history.Type))
                {
                    var record = Handlers[history.Type](history);

                    ignoreClearRedo = true;
                    Push(record.Redo, record.UseFile);
                    ignoreClearRedo = false;

                    fileHistory?.Clear();
                    return true;
                }
                else
                    AELogger.DefaultLogger?.Log($"No {history.Type} handler!", LogLevel.Error);
            }

            return false;
        }

        public static void Push(IHistoryRecord history, bool useFile = false)
        {
            if (ignoreClearRedo == false)
                clearRedo = true;

            if (useFile)
                history = new FileHistoryRecord(history);

            UndoHistory.Push(history);
            UndoChanged?.Invoke();
        }

        private static void PushRedo(IHistoryRecord history, bool useFile = false)
        {
            if (useFile)
                history = new FileHistoryRecord(history);

            RedoHistory.Push(history);
            RedoChanged?.Invoke();
        }

        public static void Clear()
        {
            UndoHistory.Clear();
            RedoHistory.Clear();

            UndoChanged?.Invoke();
            RedoChanged?.Invoke();
        }
    }
}
