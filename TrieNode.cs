using System.Collections.Generic;

namespace xr
{
    public class TrieNode<T>
    {
        public SortedList<T, TrieNode<T>> Children { get; set; }

        public TrieNode()
        {
            Children = null;
        }
    }
}
