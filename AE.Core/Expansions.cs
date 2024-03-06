using AE.Core.Serializer;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

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
		public static object Object(this Type type, object[] @params = null)
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
        public static T Object<T>(this Type type, object[] @params = null)
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
        public static int Int(this string value)
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
        public static long Long(this string value)
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
        public static float Single(this string value)
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
        public static double Double(this string value)
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
		public static decimal Decimal(this string value)
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
		public static DateTime Date(this string value, string format = null, IFormatProvider provider = null)
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
		public static TimeSpan Time(this string value, string format = null, IFormatProvider provider = null)
		{
			if (string.IsNullOrEmpty(format))
				return TimeSpan.Parse(value);

			return TimeSpan.ParseExact(value, format, provider ?? CultureInfo.InvariantCulture);
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
        /// Get type by string name
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Type Type(this string value)
        {
            return SerializerHelper.GetType(value);
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
        /// Unique string for collections [{string}#{index}]
        /// </summary>
        /// <param name="value"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string UniqueFrom(this string value, IEnumerable<string> values)
        {
            var index = 1;
            var newValue = value;

            while (values.Any(v => v.Equals(newValue, StringComparison.InvariantCultureIgnoreCase)))
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
            => values.Any(v => v.Equals(value, StringComparison.InvariantCultureIgnoreCase));

        #endregion
        #region Enum

        /// <summary>
        /// Get all enum values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static IEnumerable<T> Values<T>(this Enum @enum) where T : Enum
        {
            return @enum.Values().Cast<T>();
        }

		/// <summary>
		/// Get all enum values
		/// </summary>
		/// <param name="enum"></param>
		/// <returns></returns>
		public static Array Values(this Enum @enum)
        {
			return Enum.GetValues(@enum.GetType());
		}

		/// <summary>
		/// Get enum name
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enum"></param>
		/// <returns></returns>
		public static string Name(this Enum @enum)
        {
            return Enum.GetName(@enum.GetType(), @enum);
        }

        /// <summary>
        /// Get enum name
        /// </summary>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static string[] Names(this Enum @enum)
        {
            return Enum.GetNames(@enum.GetType());
        }

        /// <summary>
        /// A generic extension method that aids in reflecting and retrieving any attribute that is applied to an `Enum`
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static TAttribute Attribute<TAttribute>(this Enum @enum) 
            where TAttribute : Attribute
        {
            return @enum
                .GetType()
                .GetMember(@enum.Name())
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
		public static void InsertSorted<T>(this IList source, T item) where T : IComparable<T>
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
		public static void InsertSortedDescending<T>(this IList source, T item) where T : IComparable<T>
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
		/// <returns></returns>
		public static string Serialize(this object obj)
        {
            var serializer = new AESerializer();
            return serializer.Serialize(obj);
		}

		/// <summary>
		/// Serialize object without reference
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static string SerializeCopy(this object obj)
		{
			var serializer = new AESerializer();
			return serializer.SerializeCopy(obj);
		}

		/// <summary>
		/// Deserialize string
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <returns></returns>
		public static T Deserialize<T>(this string data) where T : class
        {
            var serializer = new AESerializer();
            return serializer.Deserialize<T>(data);
        }

		/// <summary>
		/// Deserialize string
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <returns></returns>
		public static object Deserialize(this string data)
		{
			var serializer = new AESerializer();
			return serializer.Deserialize(data);
		}

		#endregion
	}
}
