#if DEBUG
#define CircularBuffer_check_args
#endif

using System.Diagnostics;

namespace System.Collections.Generic
{
    [DebuggerDisplay("Count = {Count}")]
    public class CircularBuffer<T> : IEnumerable<T>
    {
        private T[] data;
        private int head, tail;

        public CircularBuffer(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity", "Non-negative number is required");
            }
            data = new T[capacity];
            head = 0;
            tail = 0;
        }

        public int Count { get; private set; }

        public int Capacity
        {
            get { return data.Length; }
        }

        public void Clear()
        {
            head = 0;
            tail = 0;
            Count = 0;
        }

        public void Enqueue(T item)
        {
            data[tail] = item;
            tail = (tail + 1) % data.Length;
            if (Count == Capacity)
            {
                Dequeue();
            }
            Count++;
        }

        public T Dequeue()
        {
            #if CircularBuffer_check_args
            if (Count == 0)
            {
                throw new InvalidOperationException("CircularBuffer is empty");
            }
            #endif
            var item = data[head];
            head = (head + 1) % data.Length;
            Count--;
            return item;
        }

        public T Peek()
        {
            #if CircularBuffer_check_args
            if (Count == 0)
            {
                throw new InvalidOperationException("CircularBuffer is empty");
            }
            #endif
            return data[head];
        }

        public T this[int i]
        {
            get
            {
                #if CircularBuffer_check_args
                if (i >= Count)
                {
                    throw new InvalidOperationException("i >= Count");
                }
                #endif
                return GetItemAt(i);
            }
        }

        private T GetItemAt(int i)
        {
            return data[(head + i) % data.Length];
        }
        
        private struct Enumerator : IEnumerator<T>
        {
            private CircularBuffer<T> queue;
            private int index;
            private T currentItem;

            public T Current
            {
                get
                {
                    if (index < 0)
                    {
                        throw new InvalidOperationException(
                            index == -1 ? "Enum not started" : "Enum ended");
                    }
                    return currentItem;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public Enumerator(CircularBuffer<T> q)
            {
                queue = q;
                index = -1;
                currentItem = default(T);
            }

            public void Dispose()
            {
                index = -2;
                currentItem = default(T);
            }

            public bool MoveNext()
            {
                if (index == -2)
                {
                    return false;
                }
                index++;
                if (index == queue.data.Length)
                {
                    index = -2;
                    currentItem = default(T);
                    return false;
                }
                else
                {
                    currentItem = queue.GetItemAt(index);
                    return true;
                }
            }

            void IEnumerator.Reset()
            {
                index = -1;
                currentItem = default(T);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
