using AE.Core.Log;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace AE.Core.Serializer
{
    public partial class AESerializer
    {
        private List<IReference> Sources { get; set; }

        /// <summary>
        /// Deserialize object from string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public T Deserialize<T>(string data)
        {
            if (data.IsNull())
                return typeof(T).Object<T>();

            try
            {
                var index = data.IndexOf("&");
                if (index < data.Length && data.Substring(0, index).TryInt(out int len))
                {
                    var start = len.ToString().Length + 1;
                    var types = data.Substring(start, len - 1);

                    data = data.Substring(types.Length + start);

                    if (!types.IsNull())
                    {
                        TypeTabel = new List<string>();

                        foreach (var t in types.Split('&'))
                            TypeTabel.Add(t);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.DefaultLogger?.Log(ex);
            }

            T result = (T)DeserializeObject(typeof(T).Object(), data);
            AfterDeserialize(result);

            return result;
        }

        private void AfterDeserialize(object obj)
        {
            Sources = new List<IReference>();

            GetSource(obj);
            SetReference(obj);
        }

        private void GetSource(object obj)
        {
            var type = obj?.GetType();

            if (type?.GetCustomAttribute<AESerializableAttribute>() != null)
            {
                if (obj is IReference reference)
                    Sources.Add(reference);

                foreach (var property in type.GetProperties())
                {
                    if (property.SetMethod == null || property.GetCustomAttribute<AEIgnoreAttribute>() != null)
                        continue;

                    var value = property.GetValue(obj);
                    if (property.GetCustomAttribute<AEReferenceAttribute>() != null)
                        continue;

                    if (value is IDictionary dictionary)
                    {
                        var keys = dictionary.Keys.GetEnumerator();
                        var values = dictionary.Values.GetEnumerator();

                        for (var i = 0; i < dictionary.Count; ++i)
                        {
                            keys.MoveNext();
                            values.MoveNext();

                            GetSource(keys.Current);
                            GetSource(values.Current);

                        }

                        continue;
                    }
                    else if (!(value is string) && value is IEnumerable enumerable)
                    {
                        foreach (var item in enumerable)
                            GetSource(item);

                        continue;
                    }

                    GetSource(value);
                }
            }
        }

        private void SetReference(object obj)
        {
            var type = obj?.GetType();

            if (type?.GetCustomAttribute<AESerializableAttribute>() != null)
            {
                foreach (var property in type.GetProperties())
                {
                    if (property.SetMethod == null || property.GetCustomAttribute<AEIgnoreAttribute>() != null)
                        continue;

                    var value = property.GetValue(obj);
                    var isReference = property.GetCustomAttribute<AEReferenceAttribute>() != null;

                    if (value is string)
                        continue;

                    var valueType = value?.GetType();

                    if (value is IDictionary dictionary)
                    {
                        var rDictionary = valueType.Object<IDictionary>();

                        var keys = dictionary.Keys.GetEnumerator();
                        var values = dictionary.Values.GetEnumerator();

                        for (var i = 0; i < dictionary.Count; ++i)
                        {
                            keys.MoveNext();
                            values.MoveNext();

                            var key = keys.Current;
                            var item = values.Current;

                            if (isReference)
                            {
                                var sourceKey = key;
                                var sourceItem = item;

                                if (sourceKey is IReference referenceKey)
                                    sourceKey = Sources.First(s => s.ReferenceId == referenceKey.ReferenceId);
                                if (sourceItem is IReference referenceItem)
                                    sourceItem = Sources.First(s => s.ReferenceId == referenceItem.ReferenceId);

                                rDictionary.Add(sourceKey, sourceItem);

                                continue;
                            }

                            SetReference(key);
                            SetReference(item);

                            rDictionary.Add(key, item);
                        }

                        property.SetValue(obj, rDictionary);
                    }
                    else if (value is IEnumerable enumerable)
                    {
                        var itemType = valueType.IsArray ? enumerable.GetType().GetElementType() : enumerable.GetType().GenericTypeArguments[0];
                        var items = new List<object>();

                        foreach (var item in enumerable)
                        {
                            if (isReference)
                            {
                                var sourceItem = item;

                                if (sourceItem is IReference referenceItem)
                                    sourceItem = Sources.First(s => s.ReferenceId == referenceItem.ReferenceId);

                                items.Add(sourceItem);

                                continue;
                            }

                            SetReference(item);

                            items.Add(item);
                        }

                        var enumerableType = typeof(Enumerable);
                        var castMethod = enumerableType.GetMethod(nameof(Enumerable.Cast)).MakeGenericMethod(itemType);

                        var castedItems = castMethod.Invoke(null, new[] { items });

                        if (valueType.IsArray)
                        {
                            var toArrayMethod = enumerableType.GetMethod(nameof(Enumerable.ToArray)).MakeGenericMethod(itemType);
                            castedItems = toArrayMethod.Invoke(null, new[] { castedItems });
                        }
                        else
                        {
                            var toListMethod = enumerableType.GetMethod(nameof(Enumerable.ToList)).MakeGenericMethod(itemType);
                            castedItems = toListMethod.Invoke(null, new[] { castedItems });

                            var observType = typeof(ObservableCollection<>).MakeGenericType(itemType);

                            if (valueType == observType)
                                castedItems = observType.Object(new object[] { castedItems });
                        }

                        property.SetValue(obj, castedItems);

                        continue;
                    }

                    if (isReference && value is IReference referenceValue)
                    {
                        var sourceValue = Sources.FirstOrDefault(s => s.ReferenceId == referenceValue.ReferenceId && s.GetType() == referenceValue.GetType());

                        if (sourceValue != null)
                            property.SetValue(obj, sourceValue);

                        continue;
                    }

                    SetReference(value);
                }
            }
        }

        private object DeserializeObject(object obj, string data)
        {
            var type = obj?.GetType();

            if (type?.GetCustomAttribute<AESerializableAttribute>() != null)
            {
                foreach (var param in Parse(data))
                {
                    var property = type.GetProperty(param.Key);

                    if (property != null)
                        property.SetValue(obj, DeserializeValue(param.Value));
                }

                return obj;
            }

            try
            {
                return Convert.ChangeType(data, type);
            }
            catch (Exception ex)
            {
                Logger.DefaultLogger?.Log(ex);
            }

            return null;
        }

        private object DeserializeValue(string data)
        {
            if (string.IsNullOrEmpty(data))
                return null;

            Type type = null;

            if (data.StartsWith(StringT))
            {
                data = data.Remove(0, StringT.Length);

                var lenString = data.Substring(0, data.IndexOf(']'));
                var len = int.Parse(lenString);

                data = data.Substring(2 + lenString.Length, len);
                type = typeof(string);
            }
            else
            {
                data = data.Remove(0, 1);
                type = GetSaveType(data.Substring(0, data.IndexOf(')')));
                data = data.Remove(0, data.IndexOf(')') + 1);
            }

            if (type == null)
                throw new ArgumentNullException();

            if (data.IsNull())
                return Convert.ChangeType(null, type) ?? type.Object();

            if (type == typeof(string))
                return data;

            var value = type.Object();

            if (value is IDictionary dictionary)
            {
                var keyType = dictionary.GetType().GenericTypeArguments[0];
                var valueType = dictionary.GetType().GenericTypeArguments[1];

                foreach (var param in Parse(data))
                {
                    var item = param.Value;
                    string k, v;

                    if (item.StartsWith(StringT))
                    {
                        var lenString = item.Substring(StringT.Length, item.IndexOf(']') - StringT.Length);
                        var len = lenString.Int();

                        k = item.Substring(0, len + $"{StringT}{len}])".Length);
                        v = item.Substring(k.Length + 1);
                    }
                    else
                    {
                        k = item.Substring(0, item.IndexOf('|'));
                        v = item.Substring(k.Length + 1);
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
                        return observType.Object(new object[] { castedItems });

                    return castedItems;
                }
            }

            if (type == typeof(double))
            {
                var format = new NumberFormatInfo
                {
                    NumberGroupSeparator = ".",
                    NumberDecimalSeparator = ".",
                };

                return double.Parse(data.Replace(',', '.'), format);
            }

            if (type.IsEnum)
                return Enum.Parse(value.GetType(), data);

            if (value is Guid)
                return new Guid(data);

            return DeserializeObject(value, data);
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
                key = data.Substring(0, data.IndexOf(']'));
                data = data.Remove(0, key.Length + 2);

                var start = 1;
                var end = 0;
                var index = 0;

                for (var i = 0; i < data.Length; ++i)
                {
                    var c = data[i];

                    if (c == FirstST && data.Substring(i).StartsWith(StringT))
                    {
                        i += StringT.Length;

                        var str = data.Substring(i);
                        var lenString = str.Substring(0, str.IndexOf(']'));

                        i += 2 + lenString.Length + lenString.Int();

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

                value = data.Substring(0, index);
                data = data.Remove(0, value.Length);

                result.Add(key, value);

                if (!data.IsNull() && data[0] == '>')
                    data = data.Remove(0, 1);
            }

            return result;
        }
    }
}
