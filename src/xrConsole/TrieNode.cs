using System.Collections.Generic;

namespace xr.Detail
{
    internal class TrieNode<T>
    {
        public SortedList<T, TrieNode<T>> Children { get; set; }

        public TrieNode()
        {
            Children = null;
        }
    }
}
