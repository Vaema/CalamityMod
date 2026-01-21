using System;
using System.Collections.Generic;

namespace CalamityMod.DataStructures
{
    /// <summary>
    /// A <see cref="HeapDict{TKey, TValue}"/> is a Dictionary-like data structure 
    /// that also keeps its entries in a binary-heap ordered by <see cref="TValue"/> (The priority).<br/>
    /// This allows for O(log n) retrievals of the <see cref="TKey"/> with the lowest <see cref="TValue"/>
    /// (Thanks to the MinHeap/PriorityQueue structure) and O(1) lookups of any element (Thanks to the Dictionary structure).<br/>
    /// This data structure is specially useful for graph algorithms where getting the minimum element is a constant process, like A*.<br/>
    /// It essentially combines a <see cref="PriorityQueue{TElement, TPriority}"/> and a <see cref="Dictionary{TKey, TValue}"/> into one data structure.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class HeapDict<TKey, TValue> where TValue : IComparable<TValue>
    {
        private readonly Dictionary<TKey, int> _indexMap = [];
        private readonly List<(TKey Key, TValue Value)> _heap = [];

        public int Count => _heap.Count;

        public void Add(TKey key, TValue value)
        {
            if (_indexMap.TryGetValue(key, out int index))
            {
                TValue oldValue = _heap[index].Value;
                _heap[index] = (key, value);

                int cmp = value.CompareTo(oldValue);
                if (cmp < 0)
                    HeapifyUp(index);
                else if (cmp > 0)
                    HeapifyDown(index);
            }
            else
            {
                _heap.Add((key, value));
                index = _heap.Count - 1;
                _indexMap[key] = index;
                HeapifyUp(index);
            }
        }

        public (TKey, TValue) PopMin()
        {
            if (_heap.Count == 0)
                throw new InvalidOperationException("Heap is empty");

            var min = _heap[0];
            var last = _heap[^1];
            _heap[0] = last;
            _indexMap[last.Key] = 0;
            _heap.RemoveAt(_heap.Count - 1);
            _indexMap.Remove(min.Key);
            if (_heap.Count > 0)
                HeapifyDown(0);

            return min;
        }

        public (TKey, TValue) PeekMin()
        {
            if (_heap.Count == 0)
                throw new InvalidOperationException("Heap is empty");

            return _heap[0];
        }

        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (_heap[index].Value.CompareTo(_heap[parent].Value) >= 0)
                    break;

                Swap(index, parent);
                index = parent;
            }
        }

        private void HeapifyDown(int index)
        {
            int lastIndex = _heap.Count - 1;
            while (true)
            {
                int left = 2 * index + 1;
                int right = 2 * index + 2;
                int smallest = index;

                if (left <= lastIndex && _heap[left].Value.CompareTo(_heap[smallest].Value) < 0)
                    smallest = left;

                if (right <= lastIndex && _heap[right].Value.CompareTo(_heap[smallest].Value) < 0)
                    smallest = right;

                if (smallest == index)
                    break;

                Swap(index, smallest);
                index = smallest;
            }
        }

        private void Swap(int i, int j)
        {
            (_heap[i], _heap[j]) = (_heap[j], _heap[i]);
            _indexMap[_heap[i].Key] = i;
            _indexMap[_heap[j].Key] = j;
        }

        public bool ContainsKey(TKey key) => _indexMap.ContainsKey(key);

        public TValue GetValue(TKey key) => _heap[_indexMap[key]].Value;
    }
}
