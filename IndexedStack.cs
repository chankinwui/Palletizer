using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalletOrganizerV3
{
    internal class IndexedStack<T>
    {
        T[] array;
        int start;
        int len;

        public IndexedStack(int initialBufferSize)
        {
            array = new T[initialBufferSize];
            start = 0;
            len = 0;
        }
        public T[] CopyWholeArray()
        {
            T[] copy = new T[array.Length];
            Array.Copy(array, 0, copy, 0, array.Length);
            return copy;
        }
        public T[] CopyExactNumber()
        {
            T[] copy = new T[Count];
            Array.Copy(array, 0, copy, 0, Count);
            return copy;
        }
        public void Clear()
        {
            while (Count > 0)
            {
                Pop();
            }
        }
        public void Push(T t)
        {
            if (len == array.Length)
            {
                //increase the size of the cicularBuffer, and copy everything
                T[] bigger = new T[array.Length * 2];
                Array.Copy(array, 0, bigger, 0, array.Length);
                start = 0;
                array = bigger;
            }
            array[len] = t;
            ++len;
        }

        public T Pop()
        {
            --len;
            var result = array[len];
            start = 0;
            
            return result;
        }

        public int Count { get { return len; } }

        public T this[int index]
        {
            get
            {
                return array[index];
            }
        }
    }
}
