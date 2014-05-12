using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSoft.Collections
{
    public class ArraySegmentEnumerator<T> : IEnumerator<T>
    {
        T[] array;
        int startIndex;
        int endIndex;
        int index;

        public ArraySegmentEnumerator(T[] array, int startIndex, int count)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (!(startIndex >= 0 && count >= 0 && startIndex + count <= array.Length))
                throw new ArgumentException("startIndex and count must be positive and follow this rule: startIndex + count <= array.Length");
            this.array = array;
            this.startIndex = startIndex;
            this.endIndex = startIndex + count;
        }

        public T Current
        {
            get
            {
                if (index < startIndex)
                    throw new InvalidOperationException("Enumeration is not started");
                if (index >= endIndex)
                    throw new InvalidOperationException("Enumeration is ended");
                return array[index];
            }
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            if (index < endIndex)
                index++;
            return index < endIndex;
        }

        public void Reset()
        {
            index = startIndex - 1;
        }

        public void Dispose()
        { }
    }
}
