using System.Collections.Generic;

namespace Quoridor_AI.Model
{
    internal class PriorityQueue<T>
    {
        private readonly Dictionary<int, List<T>> _dictionary;
        private int _priority;

        public PriorityQueue()
        {
            _dictionary = new Dictionary<int, List<T>>();
        }

        public void Add(T item, int priority)
        {
            if (!_dictionary.ContainsKey(priority))
            {
                _dictionary.Add(priority, new List<T>());
            }
            _dictionary[priority].Add(item);
            if (_priority > priority)
            {
                _priority = priority;
            }
        }

        public bool TryDequeue(out T item, out int priority)
        {
            if (_dictionary[_priority].Count != 0)
            {
                GetLast(out item, out priority);
                return true;
            }
            _priority = int.MaxValue;
            foreach (var (key, list) in _dictionary)
            {
                if (key < _priority && list.Count != 0)
                {
                    _priority = key;
                }
            }
            if (_priority == int.MaxValue)
            {
                item = default;
                priority = _priority;
                return false;
            }
            GetLast(out item, out _priority);
            priority = _priority;
            return true;
        }
        private void GetLast(out T item, out int priority)
        {
            item = _dictionary[_priority][0];
            _dictionary[_priority].RemoveAt(0);
            priority = _priority;
        }
    }
}
