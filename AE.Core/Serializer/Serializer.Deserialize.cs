using AE.Dal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace AE.Core.Serializer
{
    public partial class AESerializer
    {
        /// <summary>
        /// Deserialize object from string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="useBase64"></param>
        /// <returns></returns>
        public T Deserialize<T>(string data, bool useBase64 = true)
            where T : class
        {
            var result = Deserialize(data, useBase64);

            if (result == null)
                return (T)typeof(T).ToObject();

            return (T)result;
        }

        /// <summary>
        /// Deserialize object from string
        /// </summary>
        /// <param name="data"></param>
        /// <param name="useBase64"></param>
        /// <returns></returns>
        public object Deserialize(string data, bool useBase64 = true)
        {
            if (useBase64)
                data = data.FromBase64();

            Init();

            if (data.Length < 2)
                throw new Exception("Incorect data");

            var options = data[..2];
            data = data[2..];

            if (options.Contains("t"))
                data = BeforeDeserialize(data);

            if (options.Contains("r"))
            {
                var index = data.IndexOf("&");
                if (index < data.Length && data[..index].TryInt(out int len))
                {
                    var start = len.ToString().Length + 1;
                    var referencesData = data.Substring(start, len - 1);

                    data = data[(referencesData.Length + start)..];

                    var type = GetSaveType(data[1..data.IndexOf(')')]);
                    var obj = type.ToObject();

                    SetReferenceObject(0, obj);

                    if (!referencesData.IsNull())
                    {
                        while (referencesData.Length > 0)
                        {
                            index = referencesData.IndexOf("&");
                            if (index < referencesData.Length && referencesData[..index].TryInt(out len))
                            {
                                start = len.ToString().Length + 1;
                                var refData = referencesData.Substring(start, len - 1);

                                referencesData = referencesData[(refData.Length + start)..];

                                index = refData.IndexOf("&");
                                if (index < refData.Length && refData[..index].TryInt(out int id))
                                {
                                    refData = refData[(index + 1)..];
                                    DeserializeObject(id, refData);
                                }
                            }
                        }
                    }

                    return DeserializeObject(data, obj);
                }
            }

            return DeserializeObject(-1, data);
        }

        private string BeforeDeserialize(string data)
        {
            var index = data.IndexOf("&");
            if (index < data.Length && data[..index].TryInt(out int len))
            {
                var start = len.ToString().Length + 1;
                var typesData = data.Substring(start, len - 1);

                data = data[(typesData.Length + start)..];

                if (!typesData.IsNull())
                {
                    foreach (var type in typesData.Split('&'))
                        TypeTable.Add(type);
                }
            }

            return data;
        }

        private object DeserializeObject(int id, string data)
        {
            if (data.IsNull())
                return null;

            var type = GetSaveType(data[1..data.IndexOf(')')]);
            var obj = type.ToObject();

            if (id >= 0)
                SetReferenceObject(id, obj);

            return DeserializeObject(data, obj);
        }

        private object DeserializeObject(string data, object obj)
        {
            if (data.IsNull())
                return null;

            var type = obj.GetType();

            if (type.GetCustomAttribute<AESerializableAttribute>() != null)
            {
                foreach (var param in Parse(data))
                {
                    var property = type.GetProperty(param.Key);
                    property?.SetValue(obj, DeserializeValue(param.Value));
                }

                return obj;
            }

            return Convert.ChangeType(data, type);
        }

        private object DeserializeValue(string data)
        {
            if (data.IsNull())
                return null;

            Type type = null;

            if (data.StartsWith(STRING_T))
            {
                data = data.Remove(0, STRING_T.Length);

                var lenString = data[..data.IndexOf(']')];
                var len = lenString.ToInt();

                data = data.Substring(lenString.Length + 1, len);
                type = typeof(string);
            }
            else
            {
                data = data.Remove(0, 1);
                type = GetSaveType(data[..data.IndexOf(')')]);
                data = data.Remove(0, data.IndexOf(')') + 1);
            }

            if (type == null)
                throw new ArgumentNullException();

            if (data.IsNull())
                return Convert.ChangeType(null, type) ?? type.ToObject();

            if (type == typeof(string))
                return data;

            if (data.StartsWith("ref"))
                return GetReferenceObject(data[3..]);

            var value = type.ToObject();

            if (value is IDictionary dictionary)
            {
                var keyType = dictionary.GetType().GenericTypeArguments[0];
                var valueType = dictionary.GetType().GenericTypeArguments[1];

                foreach (var param in Parse(data))
                {
                    var item = param.Value;
                    string k, v;

                    if (item.StartsWith(STRING_T))
                    {
                        var lenString = item[STRING_T.Length..item.IndexOf(']')];
                        var len = lenString.ToInt();

                        k = item[..(len + $"{STRING_T}{len}]".Length)];
                        v = item[(k.Length + 1)..];
                    }
                    else
                    {
                        k = item[..item.IndexOf('|')];
                        v = item[(k.Length + 1)..];
                    }

                    dictionary.Add(DeserializeValue(k), DeserializeValue(v));
                }

                return dictionary;
            }
            else if (value is IEnumerable enumerable)
            {
                var itemType = type.IsArray ? enumerable.GetType().GetElementType() : enumerable.GetType().GenericTypeArguments[0];
                var items = new List<object>();

                foreach (var param in Parse(data))
                    items.Add(DeserializeValue(param.Value));

                var enumerableType = typeof(Enumerable);

                var castMethod = enumerableType.GetMethod(nameof(Enumerable.Cast)).MakeGenericMethod(itemType);
                var castedItems = castMethod.Invoke(null, new[] { items });

                if (type.IsArray)
                {
                    var toArrayMethod = enumerableType.GetMethod(nameof(Enumerable.ToArray)).MakeGenericMethod(itemType);
                    return toArrayMethod.Invoke(null, new[] { castedItems });
                }
                else
                {
                    var toListMethod = enumerableType.GetMethod(nameof(Enumerable.ToList)).MakeGenericMethod(itemType);
                    castedItems = toListMethod.Invoke(null, new[] { castedItems });

                    var observType = typeof(ObservableCollection<>).MakeGenericType(itemType);

                    if (type == observType)
                        return observType.ToObject(new object[] { castedItems });

                    return castedItems;
                }
            }

            if (type == typeof(float))
                return data.ToSingle();

            if (type == typeof(double))
                return data.ToDouble();

            if (type == typeof(decimal))
                return data.ToDecimal();

            if (type == typeof(DateTime))
                data.ToDate(Expansions.DATETIME_FORMAT);

            if (type == typeof(TimeSpan))
                return data.ToTime(Expansions.TIMESPAN_FORMAT);

            if (type.IsEnum)
                return Enum.Parse(value.GetType(), data);

            if (value is Guid)
                return new Guid(data);

            return DeserializeObject(data, value);
        }

        private Dictionary<string, string> Parse(string data)
        {
            var result = new Dictionary<string, string>();

            if (data.IsNull())
                return result;

            if (data[0] == '(')
                data = data.Remove(0, data.IndexOf(')') + 1);

            while (data.Length > 0)
            {
                string key, value;

                data = data.Remove(0, 2);
                key = data[..data.IndexOf(']')];
                data = data.Remove(0, key.Length + 2);

                var start = 1;
                var end = 0;
                var index = 0;

                for (var i = 0; i < data.Length; ++i)
                {
                    var c = data[i];

                    if (c == FIRST_ST && data[i..].StartsWith(STRING_T))
                    {
                        i += STRING_T.Length;

                        var str = data[i..];
                        var lenString = str[..str.IndexOf(']')];

                        i += lenString.Length + 1 + lenString.ToInt();

                        c = data[i];
                    }

                    if (c == '<')
                        start++;
                    else if (c == '>')
                        end++;

                    if (start == end)
                    {
                        index = i;
                        break;
                    }
                }

                value = data[..index];
                data = data.Remove(0, value.Length);

                result.Add(key, value);

                if (!data.IsNull() && data[0] == '>')
                    data = data.Remove(0, 1);
            }

            return result;
        }
    }
}
