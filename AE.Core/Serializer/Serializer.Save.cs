using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

using AE.Dal;

namespace AE.Core.Serializer
{
	public partial class AESerializer : IDisposable
	{
		/// <summary>
		/// Serialize object
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public string Serialize(object obj)
		{
			return Serialize(obj, false);
		}

		/// <summary>
		/// Serialize object without reference
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public string SerializeCopy(object obj)
		{
			return Serialize(obj, true);
		}

		private string Serialize(object obj, bool ignoreReference)
		{
			Init();

			if (!ignoreReference)
				BeforeSerialize(obj);

			SerializeObj(obj);
			AfterSerialize();

			return Builder.ToString();
		}

		private void BeforeSerialize(object obj)
		{
			var type = obj?.GetType();

			if (type?.GetCustomAttribute<AESerializableAttribute>() != null)
			{
				if (!type.IsValueType)
				{
					if (!SetReferenceObject(obj))
						return;
				}

				foreach (var property in type.GetProperties())
				{
					if (property.SetMethod == null || property.GetCustomAttribute<AEIgnoreAttribute>() != null)
						continue;

					var value = property.GetValue(obj);

					if (value is IDictionary dictionary)
					{
						var keys = dictionary.Keys.GetEnumerator();
						var values = dictionary.Values.GetEnumerator();

						for (var i = 0; i < dictionary.Count; ++i)
						{
							keys.MoveNext();
							values.MoveNext();

							BeforeSerialize(keys.Current);
							BeforeSerialize(values.Current);
						}

						continue;
					}
					else if (!(value is string) && value is IEnumerable enumerable)
					{
						foreach (var item in enumerable)
							BeforeSerialize(item);

						continue;
					}

					BeforeSerialize(value);
				}
			}
		}

		private void AfterSerialize()
		{
			var options = "";
			var types = "";
			var referenceBuilder = new StringBuilder();

			var prevCount = -1;
			var newCount = 0;

			while (prevCount != newCount)
			{
				prevCount = ReferenceTable.Count;

				foreach (var reference in ReferenceTable.Where(r => r.HasReference && r.Id > 0).Reverse().ToList())
				{
					var objBuilder = new StringBuilder();
					SerializeObj(reference.Obj, objBuilder);

					referenceBuilder.Insert(0, objBuilder);
					referenceBuilder.Insert(0, $"{objBuilder.Length + reference.Id.ToString().Length + 2}&{reference.Id}&");

					ReferenceTable.Remove(reference);
				}

				newCount = ReferenceTable.Count;
			}

			if (referenceBuilder.Length > 0)
			{
				options += "r";

				referenceBuilder.Insert(0, $"{referenceBuilder.Length + 1}&");
				Builder.Insert(0, referenceBuilder);
			}
			else
				options += "-";

			foreach (var t in TypeTable)
				types += $"&{t}";

			if (types.Length > 0)
			{
				options += "t";

				Builder.Insert(0, types.Length.ToString() + types);
			}
			else
				options += "-";

			if (Builder.Length > 0)
				Builder.Insert(0, options);
		}

		private void SerializeObj(object obj, StringBuilder builder = null)
		{
			builder ??= Builder;

			var type = obj?.GetType();

			if (type?.GetCustomAttribute<AESerializableAttribute>() != null)
			{
				builder.Append($"({SetSaveTypeId(type)})");

				foreach (var property in type.GetProperties())
				{
					if (property.SetMethod == null || property.GetCustomAttribute<AEIgnoreAttribute>() != null)
						continue;

					var value = property.GetValue(obj);

					builder.Append($"<[{property.Name}]=");
					SerializeValue(value, property, builder);
					builder.Append(">");
				}
			}
			else if (type == typeof(DateTime) && obj is DateTime dateTime)
				builder.Append($"({SetSaveTypeId(type)}){dateTime.ToString(Expansions.DATETIME_FORMAT, CultureInfo.InvariantCulture)}");
			else if (type == typeof(TimeSpan) && obj is TimeSpan timeSpan)
				builder.Append($"({SetSaveTypeId(type)}){timeSpan.ToString(Expansions.TIMESPAN_FORMAT, CultureInfo.InvariantCulture)}");
			else if (type.IsPrimitive || type.IsEnum || (type.IsValueType && type.IsSerializable))
				builder.Append($"({SetSaveTypeId(type)}){obj}");
		}

		private void SerializeValue(object value, PropertyInfo valueProperty, StringBuilder builder = null)
		{
			if (value == null)
				return;

			builder ??= Builder;

			if (value is string str)
			{
				builder.Append($"{STRING_T}{str.Length}]{str}");
				return;
			}

			if (value is IDictionary dictionary)
			{
				builder.Append($"({SetSaveTypeId(value)})");

				var keys = dictionary.Keys.GetEnumerator();
				var values = dictionary.Values.GetEnumerator();

				for (var index = 0; index < dictionary.Count; ++index)
				{
					keys.MoveNext();
					values.MoveNext();

					builder.Append($"<[{index}]=");
					SerializeValue(keys.Current, valueProperty, builder);
					builder.Append("|");
					SerializeValue(values.Current, valueProperty, builder);
					builder.Append(">");
				}
			}
			else if (value is IEnumerable enumerable)
			{
				builder.Append($"({SetSaveTypeId(value)})");
				var index = 0;

				foreach (var item in enumerable)
				{
					builder.Append($"<[{index++}]=");
					SerializeValue(item, valueProperty, builder);
					builder.Append(">");
				}
			}
			else
			{
				var id = GetReferenceObjectId(value);
				if (id != -1)
					builder.Append($"({SetSaveTypeId(value)})ref{id}");
				else
					SerializeObj(value, builder);
			}
		}
	}
}
