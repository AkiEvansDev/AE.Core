using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using AE.Core.Serializer;
using JsonSerializer = AE.Core.Serializer.JsonSerializer;

namespace AE.Core
{
	/// <summary>
	/// Expansions
	/// </summary>
	public static class Expansions
	{
		public const string DATETIME_FORMAT = "MM.dd.yyyy HH:mm:ss.f";
		public const string TIMESPAN_FORMAT = "c";

		public readonly static NumberFormatInfo NumberFormat = new NumberFormatInfo
		{
			NumberGroupSeparator = ".",
			NumberDecimalSeparator = ".",
		};

		#region Type

		/// <summary>
		/// Get object from type
		/// </summary>
		/// <param name="type"></param>
		/// <param name="params"></param>
		/// <returns></returns>
		public static object ToObject(this Type type, object[] @params = null)
		{
			return SerializerHelper.GetObject(type, @params);
		}

		/// <summary>
		/// Get object from type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type"></param>
		/// <param name="params"></param>
		/// <returns></returns>
		public static T ToObject<T>(this Type type, object[] @params = null)
		{
			return (T)SerializerHelper.GetObject(type, @params);
		}

		#endregion
		#region String

		/// <summary>
		/// Convert string to int
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int ToInt(this string value)
		{
			return int.Parse(value);
		}

		/// <summary>
		/// Try convert string to int
		/// </summary>
		/// <param name="value"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryInt(this string value, out int result)
		{
			return int.TryParse(value, out result);
		}

		/// <summary>
		/// Convert string to long
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static long ToLong(this string value)
		{
			return long.Parse(value);
		}

		/// <summary>
		/// Try convert string to long
		/// </summary>
		/// <param name="value"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryLong(this string value, out long result)
		{
			return long.TryParse(value, out result);
		}

		/// <summary>
		/// Convert string to float
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static float ToSingle(this string value)
		{
			return float.Parse(value.Replace(',', '.'), NumberFormat);
		}

		/// <summary>
		/// Try convert string to float
		/// </summary>
		/// <param name="value"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TrySingle(this string value, out float result)
		{
			return float.TryParse(value.Replace(',', '.'), NumberStyles.AllowThousands | NumberStyles.Float, NumberFormat, out result);
		}

		/// <summary>
		/// Convert string to double
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static double ToDouble(this string value)
		{
			return double.Parse(value.Replace(',', '.'), NumberFormat);
		}

		/// <summary>
		/// Try convert string to double
		/// </summary>
		/// <param name="value"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryDouble(this string value, out double result)
		{
			return double.TryParse(value.Replace(',', '.'), NumberStyles.AllowThousands | NumberStyles.Float, NumberFormat, out result);
		}

		/// <summary>
		/// Convert string to decimal
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static decimal ToDecimal(this string value)
		{
			return decimal.Parse(value.Replace(',', '.'), NumberFormat);
		}

		/// <summary>
		/// Try convert string to decimal
		/// </summary>
		/// <param name="value"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryDecimal(this string value, out decimal result)
		{
			return decimal.TryParse(value.Replace(',', '.'), NumberStyles.Number, NumberFormat, out result);
		}

		/// <summary>
		/// Convert string to DateTime
		/// </summary>
		/// <param name="value"></param>
		/// <param name="format"></param>
		/// <param name="provider"></param>
		/// <returns></returns>
		public static DateTime ToDate(this string value, string format = null, IFormatProvider provider = null)
		{
			if (string.IsNullOrEmpty(format))
				return DateTime.Parse(value);

			return DateTime.ParseExact(value, format, provider ?? CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Convert string to TimeSpan
		/// </summary>
		/// <param name="value"></param>
		/// <param name="format"></param>
		/// <param name="provider"></param>
		/// <returns></returns>
		public static TimeSpan ToTime(this string value, string format = null, IFormatProvider provider = null)
		{
			if (string.IsNullOrEmpty(format))
				return TimeSpan.Parse(value);

			return TimeSpan.ParseExact(value, format, provider ?? CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Get type by string name
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Type ToType(this string value)
        {
            return SerializerHelper.GetType(value);
        }

        /// <summary>
        /// Equals string with StringComparison.OrdinalIgnoreCase
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static bool EqualsIgnoreCase(this string value1, string value2)
		{
			return string.Equals(value1, value2, StringComparison.OrdinalIgnoreCase);
		}

        /// <summary>
        /// Search any word of value2 (split by space) in string with StringComparison.OrdinalIgnoreCase
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static bool Search(this string value1, string value2)
		{
			var parts = value2.Split(' ');
			return parts.Any(p => value1.Contains(p, StringComparison.OrdinalIgnoreCase));
		}

        /// <summary>
        /// Search any word of value2 (split by separator) in string with StringComparison.OrdinalIgnoreCase
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static bool Search(this string value1, string value2, params char[] separator)
        {
            var parts = value2.Split(separator);
            return parts.Any(p => value1.Contains(p, StringComparison.OrdinalIgnoreCase));
        }

		/// <summary>
		/// Upper first char in string
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string UpFirst(this string value)
		{
			return value[..1].ToUpper() + value[1..];
		}

		/// <summary>
		/// Lower first char in string
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string LowFirst(this string value)
		{
			return value[..1].ToLower() + value[1..];
		}

		/// <summary>
		/// String IsNullOrEmpty
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsNull(this string value)
		{
			return string.IsNullOrEmpty(value);
		}

        /// <summary>
        /// Return value2 if value1 is null or value1
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static string Empty(this string value1, string value2)
        {
            return string.IsNullOrEmpty(value1) ? value2 : value1;
        }

        /// <summary>
        /// Unique string for collections [{string}#{index}]
        /// </summary>
        /// <param name="value"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string UniqueFrom(this string value, IEnumerable<string> values)
		{
			var index = 1;
			var newValue = value;

			while (values.Any(v => v.EqualsIgnoreCase(newValue)))
			{
				newValue = $"{value}#{index}";
				index++;
			}

			return newValue;
		}

		/// <summary>
		/// Return true if collections contains string
		/// </summary>
		/// <param name="value"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		public static bool AnyFrom(this string value, IEnumerable<string> values)
			=> values.Any(v => v.EqualsIgnoreCase(value));

		/// <summary>
		/// Return only value string without space after split
		/// </summary>
		/// <param name="data"></param>
		/// <param name="separator"></param>
		/// <returns></returns>
		public static IEnumerable<string> SmartSplit(this string data, string separator)
        {
            return data
                .Split(separator)
                .Select(e => e.Trim())
                .Where(e => !string.IsNullOrWhiteSpace(e));
        }

		/// <summary>
		/// Convert string to base64
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
        public static string ToBase64(this string data)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
        }

		/// <summary>
		/// Get string from base64
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
        public static string FromBase64(this string data)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(data));
        }

        #endregion
        #region Enum

        /// <summary>
        /// Get enum name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static string GetName(this Enum @enum)
		{
			return Enum.GetName(@enum.GetType(), @enum);
		}

		/// <summary>
		/// Get enum name
		/// </summary>
		/// <param name="enum"></param>
		/// <returns></returns>
		public static string[] GetNames(this Enum @enum)
		{
			return Enum.GetNames(@enum.GetType());
        }

        /// <summary>
        /// Get all enum values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetValues<T>(this Enum @enum) 
			where T : Enum
        {
            return Enum.GetValues(@enum.GetType()).Cast<T>();
        }

        /// <summary>
        /// Get enum value description from DescriptionAttribute
        /// </summary>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum @enum)
        {
			return @enum.GetAttribute<DescriptionAttribute>().Description;
        }

        /// <summary>
        /// Get enum value and description from DescriptionAttribute
        /// </summary>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static IEnumerable<(T Value, string Description)> GetDescriptions<T>(this Enum @enum) 
			where T : Enum
        {
			foreach (T value in @enum.GetValues<T>())
				yield return (value, value.GetAttribute<DescriptionAttribute>().Description);
		}

		/// <summary>
		/// A generic extension method that aids in reflecting and retrieving any attribute that is applied to an `Enum`
		/// </summary>
		/// <typeparam name="TAttribute"></typeparam>
		/// <param name="enum"></param>
		/// <returns></returns>
		public static TAttribute GetAttribute<TAttribute>(this Enum @enum)
			where TAttribute : Attribute
		{
			return @enum
				.GetType()
				.GetMember(@enum.GetName())
				.First()
				.GetCustomAttribute<TAttribute>();
		}

		#endregion
		#region IEnumerable

		/// <summary>
		/// Groups 'source' to 'groupCount' group, keeping the queue
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="groupCount"></param>
		/// <returns></returns>
		public static IEnumerable<IEnumerable<T>> Groups<T>(this IEnumerable<T> source, int groupCount)
		{
			var count = source.Count();
			var e = (int)Math.Floor(count / (double)groupCount);
			var ae = count - e * groupCount;

			var skip = 0;
			for (var i = 0; i < groupCount; ++i)
			{
				var c = i < ae ? e + 1 : e;
				yield return source.Skip(skip).Take(c);

				skip += c;
			}
		}

		/// <summary>
		/// Divides 'source' into 'partitionSize' parts, keeping the queue
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="partitionSize"></param>
		/// <returns></returns>
		public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> source, int partitionSize)
		{
			var i = 0;
			return source.GroupBy(x => i++ / partitionSize).Select(g => g.ToArray()).ToArray();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="item"></param>
		public static void InsertSorted<T>(this IList source, T item) 
			where T : IComparable<T>
		{
			var index = source.OfType<T>().Count(i => i.CompareTo(item) < 0);
			source.Insert(index, item);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="item"></param>
		public static void InsertSortedDescending<T>(this IList source, T item) 
			where T : IComparable<T>
		{
			var index = source.OfType<T>().Count(i => i.CompareTo(item) > 0);
			source.Insert(index, item);
		}

        #endregion
        #region Serializer

        /// <summary>
        /// Serialize object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="useBase64"></param>
        /// <returns></returns>
        public static string Serialize(this object obj, bool useBase64 = true)
		{
			var serializer = new AESerializer();
			return serializer.Serialize(obj, useBase64);
		}

        /// <summary>
        /// Serialize object without reference
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="useBase64"></param>
        /// <returns></returns>
        public static string SerializeCopy(this object obj, bool useBase64 = true)
		{
			var serializer = new AESerializer();
			return serializer.SerializeCopy(obj, useBase64);
        }

        /// <summary>
        /// Deserialize string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="useBase64"></param>
        /// <returns></returns>
        public static object Deserialize(this string data, bool useBase64 = true)
        {
            var serializer = new AESerializer();
            return serializer.Deserialize(data, useBase64);
        }

        /// <summary>
        /// Deserialize string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="useBase64"></param>
        /// <returns></returns>
        public static T Deserialize<T>(this string data, bool useBase64 = true) 
			where T : class
		{
			var serializer = new AESerializer();
			return serializer.Deserialize<T>(data, useBase64);
		}

        #endregion
        #region JSON

        /// <summary>
        /// Serialize object to json
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeJson(this object obj, JsonSerializerOptions options = null)
        {
            return JsonSerializer.Serialize(obj, options);
        }

        /// <summary>
        /// Deserialize json string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T DeserializeJson<T>(this string data, JsonSerializerOptions options = null)
            where T : class
        {
            return JsonSerializer.Deserialize<T>(data, options);
        }

        #endregion
    }
}
