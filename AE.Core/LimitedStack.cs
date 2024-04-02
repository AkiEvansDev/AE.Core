using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AE.Core
{
	[Serializable]
	public class LimitedStack<T> : IEnumerable<T>, ICollection, IReadOnlyCollection<T>
	{
		private T[] array;
		private int size;
		private readonly int maxSize;

		private const int DefaultCapacity = 4;

		public LimitedStack(int maxSize)
		{
			if (maxSize <= 0)
				throw new ArgumentOutOfRangeException(nameof(maxSize));

			array = Array.Empty<T>();
			this.maxSize = maxSize;
		}

		public int Count => size;

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot => this;

		public Enumerator GetEnumerator() => new Enumerator(this);

		public bool Contains(T item)
		{
			return size != 0 && Array.LastIndexOf(array, item, size - 1) != -1;
		}

		public void Clear()
		{
			if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
				Array.Clear(array, 0, size);

			size = 0;
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			if (array == null)
				throw new ArgumentNullException(nameof(array));

			if (arrayIndex < 0 || arrayIndex > array.Length)
				throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, "Index must be less or equal");

			if (array.Length - arrayIndex < size)
				throw new ArgumentException("Invalid off len");

			var srcIndex = 0;
			var dstIndex = arrayIndex + size;

			while (srcIndex < size)
			{
				array[--dstIndex] = this.array[srcIndex++];
			}
		}

		void ICollection.CopyTo(Array array, int arrayIndex)
		{
			if (array == null)
				throw new ArgumentNullException(nameof(array));

			if (array.Rank != 1)
				throw new ArgumentException("Rank multi dim not supported", nameof(array));

			if (array.GetLowerBound(0) != 0)
				throw new ArgumentException("Non zero lower bound", nameof(array));

			if (arrayIndex < 0 || arrayIndex > array.Length)
				throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, "Index must be less or equal");

			if (array.Length - arrayIndex < size)
				throw new ArgumentException("Invalid off len");

			try
			{
				Array.Copy(this.array, 0, array, arrayIndex, size);
				Array.Reverse(array, arrayIndex, size);
			}
			catch (ArrayTypeMismatchException)
			{
				throw new ArgumentException("Incompatible array type", nameof(array));
			}
		}

		/// <internalonly/>
		IEnumerator<T> IEnumerable<T>.GetEnumerator() => Count == 0
			? Enumerable.Empty<T>().GetEnumerator()
			: GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();

		public void TrimExcess()
		{
			int threshold = (int)(array.Length * 0.9);

			if (size < threshold)
				Array.Resize(ref array, size);
		}

		public T Peek()
		{
			var size = this.size - 1;
			var array = this.array;

			if ((uint)size >= (uint)array.Length)
				throw new InvalidOperationException("Empty stack");

			return array[size];
		}

		public bool TryPeek([MaybeNullWhen(false)] out T result)
		{
			var size = this.size - 1;
			var array = this.array;

			if ((uint)size >= (uint)array.Length)
			{
				result = default!;
				return false;
			}

			result = array[size];
			return true;
		}

		public T Pop()
		{
			var size = this.size - 1;
			var array = this.array;

			if ((uint)size >= (uint)array.Length)
				throw new InvalidOperationException("Empty stack");

			this.size = size;
			var item = array[size];

			if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
				array[size] = default!;

			return item;
		}

		public bool TryPop([MaybeNullWhen(false)] out T result)
		{
			var size = this.size - 1;
			var array = this.array;

			if ((uint)size >= (uint)array.Length)
			{
				result = default!;
				return false;
			}

			this.size = size;
			result = array[size];

			if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
				array[size] = default!;

			return true;
		}

		public void Push(T item)
		{
			var size = this.size;
			var array = this.array;

			if ((uint)size < (uint)array.Length)
			{
				array[size] = item;
				this.size = size + 1;
			}
			else
				PushWithResize(item);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void PushWithResize(T item)
		{
			if (size == maxSize)
			{
				var newArray = new T[maxSize];

				Array.Copy(array, 1, newArray, 0, maxSize - 1);
				array = newArray;

				array[size - 1] = item;
			}
			else
			{
				Grow(size + 1);

				array[size] = item;
				size++;
			}
		}

		private void Grow(int capacity)
		{
			var newcapacity = array.Length == 0 ? DefaultCapacity : 2 * array.Length;

			if ((uint)newcapacity > maxSize) newcapacity = maxSize;

			if (newcapacity < capacity)
				newcapacity = capacity;

			Array.Resize(ref array, newcapacity);
		}

		public int EnsureCapacity(int capacity)
		{
			if (maxSize <= 0)
				throw new ArgumentOutOfRangeException(nameof(capacity));

			if (array.Length < capacity)
				Grow(capacity);

			return array.Length;
		}

		public T[] ToArray()
		{
			if (size == 0)
				return Array.Empty<T>();

			var objArray = new T[size];
			var i = 0;

			while (i < size)
			{
				objArray[i] = array[size - i - 1];
				i++;
			}

			return objArray;
		}

		[Serializable]
		public struct Enumerator : IEnumerator<T>, IEnumerator
		{
			private readonly LimitedStack<T> stack;
			private int index;
			private T currentElement;

			internal Enumerator(LimitedStack<T> stack)
			{
				this.stack = stack;
				index = -2;
				currentElement = default;
			}

			public bool MoveNext()
			{
				bool retval;

				if (index == -2)
				{
					index = stack.size - 1;
					retval = (index >= 0);

					if (retval)
						currentElement = stack.array[index];

					return retval;
				}
				else if (index == -1)
					return false;

				retval = (--index >= 0);
				if (retval)
					currentElement = stack.array[index];
				else
					currentElement = default;

				return retval;
			}

			public readonly T Current
			{
				get
				{
					if (index < 0)
						throw new InvalidOperationException(index == -2 ? "Enum not started" : "Enum ended");

					return currentElement!;
				}
			}

			readonly object IEnumerator.Current => Current;

			void IEnumerator.Reset()
			{
				index = -2;
				currentElement = default;
			}

			public void Dispose()
			{
				index = -1;
			}
		}
	}
}
