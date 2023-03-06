using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace AE.Core.Serializer
{
    public partial class AESerializer : IDisposable
    {
        private int Id { get; set; }
        private StringBuilder Builder { get; set; }

        private void BeforeSerialize(object obj)
        {
            Id = 1;
            Builder = new StringBuilder();
            TypeTabel = new List<string>();

            SetId(obj, zero: true);
            SetId(obj);
        }

        /// <summary>
        /// Serialize object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string Serialize(object obj)
        {
            BeforeSerialize(obj);
            SerializeObj(obj);

            var types = "";

            foreach (var t in TypeTabel)
                types += $"&{t}";

            Builder.Insert(0, types.Length.ToString() + types);
            return Builder.ToString();
        }

        private void SetId(object obj, bool isReference = false, bool zero = false)
        {
            var type = obj?.GetType();

            if (type?.GetCustomAttribute<AESerializableAttribute>() != null)
            {
                if (obj is IReference reference)
                {
                    reference.ReferenceId = zero ? 0 : Id++;

                    if (isReference)
                        return;
                }

                foreach (var property in type.GetProperties())
                {
                    if (property.SetMethod == null || property.GetCustomAttribute<AEIgnoreAttribute>() != null)
                        continue;

                    var value = property.GetValue(obj);
                    isReference = property.GetCustomAttribute<AEReferenceAttribute>() != null;

                    if (value is IDictionary dictionary)
                    {
                        var keys = dictionary.Keys.GetEnumerator();
                        var values = dictionary.Values.GetEnumerator();

                        for (var i = 0; i < dictionary.Count; ++i)
                        {
                            keys.MoveNext();
                            values.MoveNext();

                            SetId(keys.Current, isReference, zero);
                            SetId(values.Current, isReference, zero);
                        }

                        continue;
                    }
                    else if (!(value is string) && value is IEnumerable enumerable)
                    {
                        foreach (var item in enumerable)
                            SetId(item, isReference, zero);

                        continue;
                    }

                    SetId(value, isReference, zero);
                }
            }
        }

        private void SerializeObj(object obj)
        {
            var type = obj?.GetType();

            if (type?.GetCustomAttribute<AESerializableAttribute>() != null)
            {
                Builder.Append($"({GetTypeToSave(type)})");

                foreach (var property in type.GetProperties())
                {
                    if (property.SetMethod == null || property.GetCustomAttribute<AEIgnoreAttribute>() != null)
                        continue;

                    var value = property.GetValue(obj);

                    Builder.Append($"<[{property.Name}]=");
                    SerializeValue(value, property);
                    Builder.Append(">");
                }
            }
            else if (type == typeof(DateTime) && obj is DateTime dateTime)
                Builder.Append($"({GetTypeToSave(type)}){dateTime.ToString(DATETIME_FORMAT, CultureInfo.InvariantCulture)}");
            else if (type.IsPrimitive || type.IsEnum || (type.IsValueType && type.IsSerializable))
                Builder.Append($"({GetTypeToSave(type)}){obj}");
        }

        private void SerializeValue(object value, PropertyInfo valueProperty)
        {
            if (value == null)
                return;

            if (value is string str)
            {
                Builder.Append($"{STRING_T}{str.Length}]){str}");
                return;
            }

            if (value is IDictionary dictionary)
            {
                Builder.Append($"({GetTypeToSave(value)})");

                var keys = dictionary.Keys.GetEnumerator();
                var values = dictionary.Values.GetEnumerator();

                for (var index = 0; index < dictionary.Count; ++index)
                {
                    keys.MoveNext();
                    values.MoveNext();

                    Builder.Append($"<[{index}]=");
                    SerializeValue(keys.Current, valueProperty);
                    Builder.Append("|");
                    SerializeValue(values.Current, valueProperty);
                    Builder.Append(">");
                }
            }
            else if (value is IEnumerable enumerable)
            {
                Builder.Append($"({GetTypeToSave(value)})");
                var index = 0;

                foreach (var item in enumerable)
                {
                    Builder.Append($"<[{index++}]=");
                    SerializeValue(item, valueProperty);
                    Builder.Append(">");
                }
            }
            else if (valueProperty?.GetCustomAttribute<AEReferenceAttribute>() != null && value is IReference reference)
                Builder.Append($"({GetTypeToSave(reference)})<[{nameof(reference.ReferenceId)}]=({GetTypeToSave(reference.ReferenceId)}){reference.ReferenceId}>");
            else
                SerializeObj(value);
        }
    }
}
