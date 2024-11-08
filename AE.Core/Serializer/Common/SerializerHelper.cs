using AE.Core.Log;
using AE.Dal;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AE.Core
{
    /// <summary>
    /// Help methods for serializer
    /// </summary>
    public static class SerializerHelper
    {
        static SerializerHelper()
        {
            var mainAsm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly() ?? Assembly.GetCallingAssembly();
            foreach (var refAsmName in mainAsm.GetReferencedAssemblies())
            {
                try
                {
                    Assembly.Load(refAsmName);
                }
                catch (Exception ex)
                {
                    AELogger.DefaultLogger?.Log(ex, $"Assembly {refAsmName.FullName}");
                }
            }

            var folder = new FileInfo(mainAsm.Location).Directory.FullName;
            foreach (var path in Directory.GetFiles(folder).Where(p => p.EndsWith(".dll")))
            {
                try
                {
                    Assembly.LoadFrom(path);
                }
                catch (Exception ex)
                {
                    AELogger.DefaultLogger?.Log(ex, $"File {path}");
                }
            }
        }

        /// <summary>
        /// Save text to file
        /// </summary>
        /// <param name="data"></param>
        /// <param name="path"></param>
        public static void SaveText(string data, string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);

                using var stream = new FileStream(path, FileMode.CreateNew);
                var info = new UTF8Encoding(true).GetBytes(data);

                stream.Write(info, 0, info.Length);
            }
            catch (Exception ex)
            {
                AELogger.DefaultLogger?.Log(ex);
            }
        }

        /// <summary>
        /// Load text from file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string LoadText(string path)
        {
            try
            {
                if (File.Exists(path))
                    return File.ReadAllText(path);

                return default;
            }
            catch (Exception ex)
            {
                AELogger.DefaultLogger?.Log(ex);
            }

            return default;
        }

        /// <summary>
        /// Get object from type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="params"></param>
        /// <returns></returns>
        internal static object GetObject(Type type, object[] @params = null)
        {
            if (type.IsArray)
                return Activator.CreateInstance(type, 0);

            var constructors = type.GetConstructors();

            if (constructors.Length == 0 || @params == null)
                try
                {
                    return Activator.CreateInstance(type);
                }
                catch (Exception ex)
                {
                    AELogger.DefaultLogger?.Log(ex);
                }
            else
                foreach (var constructor in constructors)
                {
                    if (IsParameters(constructor.GetParameters(), @params))
                    {
                        try
                        {
                            return constructor.Invoke(@params);
                        }
                        catch (Exception ex)
                        {
                            AELogger.DefaultLogger?.Log(ex);
                        }
                    }
                }

            AELogger.DefaultLogger?.Log($"Null control [type {type.Name}]", LogLevel.Error);

            return null;
        }

        private static bool IsParameters(ParameterInfo[] parameters, object[] @params)
        {
            if (parameters.Length == @params.Length)
            {
                for (var i = 0; i < parameters.Length; ++i)
                {
                    var parameterType = parameters[i].ParameterType;

                    if (@params[i] != null)
                    {
                        var paramType = @params[i].GetType();

                        if (parameterType != paramType)
                            if (parameterType.IsInterface)
                            {
                                if (!paramType.GetInterfaces().Contains(parameterType))
                                    return false;
                            }
                            else
                                return false;
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Get type by string name
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        internal static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);

            if (type != null)
                return type;

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);

                if (type != null)
                    return type;
            }

            throw new ArgumentNullException(typeName);
        }
    }
}
