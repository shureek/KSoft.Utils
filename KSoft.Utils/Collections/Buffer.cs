using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSoft.Collections
{
    /// <summary>
    /// Class to encapsulate work with buffers.
    /// </summary>
    /// <remarks>
    /// Class to encapsulate work with buffers. It's similar to <see cref="T:System.ArraySegment`1" />, but more powerfull.
    /// </remarks>
    /// <typeparam name="T">Type of elements stored in buffer.</typeparam>
    public class Buffer<T> : IList<T>, IReadOnlyList<T>
    {
        int startOffset = 0;
        int endOffset = 0;
        long absolutePosition = 0L;

        T[] array = null;

        public Buffer()
        { }

        public Buffer(int capacity)
        {
            SetCapacity(capacity);
        }

        public T[] Array
        {
            get
            {
                if (array == null)
                    throw new InvalidOperationException("Buffer capacity is not set");
                return array;
            }
        }

        public int Capacity
        {
            get
            {
                if (array == null)
                    throw new InvalidOperationException("Buffer capacity is not set");
                return array.Length;
            }
        }

        /// <summary>
        /// Sets new buffer capacity.
        /// </summary>
        /// <remarks>
        /// Sets new buffer capacity by creating new array and copying all data to it.
        /// </remarks>
        /// <param name="capacity">New capacity.</param>
        /// <param name="preserveOffsets">If <value>false</value>, all data will be placed in the beginning of new array (offsets may change).</param>
        public void SetCapacity(int capacity, bool preserveOffsets = false)
        {
            if (capacity < 1)
                throw new ArgumentOutOfRangeException("capacity", capacity, "Capacity must be positive");
            if (capacity < Count)
                throw new ArgumentOutOfRangeException("capacity", capacity, String.Format("Capacity must not be less then current length, which is {0}", Count));

            if (array != null && capacity != array.Length)
            {
                var newArray = new T[capacity];
                if (Count > 0)
                    System.Array.Copy(array, startOffset, newArray, preserveOffsets ? startOffset : 0, Count);
                array = newArray;
                if (!preserveOffsets)
                {
                    endOffset = Count;
                    startOffset = 0;
                }
            }
        }

        public int StartOffset
        {
            get { return startOffset; }
            set
            {
                if (value < 0 || value > endOffset)
                    throw new ArgumentOutOfRangeException("value", value, "StartOffset must be between 0 and EndOffset");
                absolutePosition += value - startOffset;
                startOffset = value;
            }
        }

        public int EndOffset
        {
            get { return endOffset; }
            set { SetBounds(startOffset, value); }
        }

        public void SetBounds(int startOffset, int endOffset)
        {
            if (startOffset >= 0 && startOffset <= endOffset && endOffset < Capacity)
            {
                this.startOffset = startOffset;
                this.endOffset = endOffset;
            }
            else
                throw new ArgumentException("Offsets must be in range 0 <= startOffset <= endOffset < Capacity");
        }

        public int Count
        {
            get { return endOffset - startOffset; }
            set
            {
                if (value < 0 || value > array.Length - startOffset)
                    throw new ArgumentOutOfRangeException("value", value, "Count must be in range 0 <= value <= Capacity - StartOffset");
                endOffset = startOffset + value;
            }
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException("index");
                return Array[startOffset + index];
            }
            set
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException("index");
                Array[startOffset + index] = value;
            }
        }

        public void Clear()
        {
            startOffset = 0;
            endOffset = 0;
        }

        #region Interface members

        int IList<T>.IndexOf(T item)
        {
            int index = System.Array.IndexOf<T>(Array, item, startOffset, Count);
            return index >= 0 ? index - startOffset : -1;
        }

        bool ICollection<T>.Contains(T item)
        {
            int index = System.Array.IndexOf<T>(Array, item, startOffset, Count);
            return index >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            System.Array.Copy(Array, startOffset, array, arrayIndex, Count);
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new ArraySegmentEnumerator<T>(Array, startOffset, Count);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<T>).GetEnumerator();
        }

        #endregion

        #region Not supported interface members

        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
