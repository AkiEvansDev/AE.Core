using AE.Core.Log;
using System;
using System.IO;

namespace AE.Core
{
    public static class EnvironmentHelper
    {
        public static string AppFolder { get; set; } = "data";

        public static bool AppDataExist(string name)
        {
            if (!Directory.Exists(AppFolder))
                return false;

            return File.Exists(Path.Combine(AppFolder, name));
        }

        public static void SaveAppData(string name, object data)
        {
            try
            {
                if (!Directory.Exists(AppFolder))
                {
                    var di = Directory.CreateDirectory(AppFolder);
                    di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                }
            }
            catch (Exception ex)
            {
                AELogger.DefaultLogger?.Log(ex);
                return;
            }

            SaveData(Path.Combine(AppFolder, name), data, true);
        }

        public static void SaveData(string path, object data, bool hidden = false)
        {
            try
            {
                if (hidden && File.Exists(path))
                    File.SetAttributes(path, File.GetAttributes(path) & ~FileAttributes.Hidden);

                File.WriteAllText(path, data.Serialize());

                if (hidden)
                    File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.Hidden);
            }
            catch (Exception ex)
            {
                AELogger.DefaultLogger?.Log(ex);
            }
        }

        public static T LoadAppData<T>(string name) where T : class
        {
            if (!Directory.Exists(AppFolder))
                return default;

            return LoadData<T>(Path.Combine(AppFolder, name));
        }

        public static T LoadData<T>(string path) where T : class
        {
            try
            {
                if (!File.Exists(path))
                    return null;

                var data = File.ReadAllText(path);
                return data.Deserialize<T>();
            }
            catch (Exception ex)
            {
                AELogger.DefaultLogger?.Log(ex);
            }

            return null;
        }

        public static void ClearAppData(string name)
        {
            var path = Path.Combine(AppFolder, name);

            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                AELogger.DefaultLogger?.Log(ex);
            }
        }
    }
}
