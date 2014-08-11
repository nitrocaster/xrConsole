#if DEBUG
#define CircularBuffer_check_args
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace xr.Collections.Generic
{
    [DebuggerDisplay("Count = {Count}")]
    public class CircularBuffer<T> : IEnumerable<T>
    {
        private static readonly T[] EmptyData = new T[0];
        private T[] data;
        private int head, tail;

        public CircularBuffer(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity", "Non-negative number is required");
            }
            data = capacity > 0 ? new T[capacity] : EmptyData;
            head = 0;
            tail = 0;
        }

        public int Count { get; private set; }

        public int Capacity
        {
            get { return data.Length; }
            set
            {
                if (value == data.Length)
                {
                    return;
                }
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Non-negative number is required");
                }
                if (value == 0)
                {
                    Clear();
                    data = EmptyData;
                    return;
                }
                var newData = new T[value];
                var newCount = Count <= value ? Count : value;
                var lastIndex = tail >= 1 ? tail - 1 : data.Length - 1;
                if (head <= lastIndex) // single chunk
                {
                    Array.Copy(data, lastIndex + 1 - newCount, newData, 0, newCount);
                }
                else // split chunk
                {
                    var tailSize = lastIndex + 1;
                    if (newCount <= tailSize) // copy newCount last items from tail
                    {
                        Array.Copy(data, lastIndex + 1 - newCount, newData, 0, newCount);
                    }
                    else
                    {
                        var headItemCount = newCount - tailSize;
                        Array.Copy(data, data.Length - headItemCount, newData, 0, headItemCount);
                        Array.Copy(data, 0, newData, headItemCount, newCount - headItemCount);
                    }
                }
                data = newData;
                head = 0;
                tail = value == newCount ? 0 : newCount;
                Count = newCount;
            }
        }

        public void Clear()
        {
            head = 0;
            tail = 0;
            Count = 0;
        }

        public void Enqueue(T item)
        {
            if (data.Length == 0)
            {
                return;
            }
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
