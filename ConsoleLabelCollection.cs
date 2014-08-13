using System.Collections;
using System.Collections.Generic;

namespace xr
{
    public class ConsoleLabelCollection : IList<ConsoleLabel>
    {
        private List<ConsoleLabel> labels = new List<ConsoleLabel>();

        public int Count
        {
            get { return labels.Count; }
        }

        public ConsoleLabel this[int i]
        {
            get { return labels[i]; }
            set { labels[i] = value; }
        }

        public void Add(ConsoleLabel label)
        {
            labels.Add(label);
        }

        public bool Remove(ConsoleLabel label)
        {
            return labels.Remove(label);
        }

        public void RemoveAt(int index)
        {
            labels.RemoveAt(index);
        }

        public int IndexOf(ConsoleLabel item)
        {
            return labels.IndexOf(item);
        }

        public void Insert(int index, ConsoleLabel item)
        {
            labels.Insert(index, item);
        }

        public void Clear()
        {
            labels.Clear();
        }

        public bool Contains(ConsoleLabel item)
        {
            return labels.Contains(item);
        }

        public void CopyTo(ConsoleLabel[] array, int arrayIndex)
        {
            labels.CopyTo(array, arrayIndex);
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public IEnumerator<ConsoleLabel> GetEnumerator()
        {
            return labels.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return labels.GetEnumerator();
        }
    }
}
