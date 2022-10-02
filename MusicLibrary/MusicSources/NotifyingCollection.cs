using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace MusicLibrary.MusicSources
{
    public class NotifyingCollection<T>: INotifyingCollection<T>, IList
    {
        public NotifyingCollection()
        {
            _collection = new List<T>(1000);
        }

        public NotifyingCollection(int capacity)
        {
            _collection = new List<T>(Math.Max(1, capacity));
        }

        public NotifyingCollection(IEnumerable<T> seed)
        {
            _collection = new List<T>(seed);
        }

        private readonly List<T> _collection;
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public IEnumerator<T> GetEnumerator() => _collection.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_collection).GetEnumerator();

        protected virtual void Added(T value) { }
        protected virtual void AddedMany(IList<T> values) { }
        protected virtual void InsertedMany(IList<T> values) { }
        protected virtual void Removed(T value) { }
        protected virtual void RemovedMany(IList<T> values) { }
        protected virtual void Cleared(IList<T> values) { }

        public void Add(T item)
        {
            if (item == null) return;
            _collection.Add(item);
            CollectionChanged?.Invoke(this, 
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, Count - 1));
            Added(item);
        }

        public int Add(object value)
        {
            if (!(value is T item)) return -1;
            Add(item);
            return Count - 1;
        }

        public void AddRange(IEnumerable<T> items)
        {
            var toAdd = items.ToList();
            if (toAdd.Count == 0) return;
            if (toAdd.Count <= 10)
            {
                // for smaller batches, add individually
                // with individual item notifications
                foreach (var item in toAdd)
                {
                    Add(item);
                }

                return;
            }
            // for bigger batches, just add them then announce a reset
            _collection.AddRange(toAdd);
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            AddedMany(toAdd);
        }

        public void Insert(int index, T item)
        {
            if (item == null) return;
            _collection.Insert(index, item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            Added(item);
        }

        public void InsertRange(int index, IEnumerable<T> items)
        {
            var toAdd = items.ToList();
            if (toAdd.Count == 0) return;
            if (toAdd.Count <= 10)
            {
                // for smaller batches, add individually
                // with individual item notifications
                foreach (var item in toAdd)
                {
                    Insert(index++, item);
                }

                return;
            }
            // for bigger batches, just add them then announce a reset
            _collection.InsertRange(index, toAdd);
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            InsertedMany(toAdd);
        }

        public void Insert(int index, object value)
        {
            if (!(value is T item)) return;
            Insert(index, item);
        }

        public T this[int index]
        {
            get => _collection[index];
            set
            {
                if (value == null) return;
                var existing = this[index];
                _collection.RemoveAt(index);
                _collection.Insert(index, value);
                // WPF ListView doesn't seem to understand NotifyCollectionChangedAction.Replace
                CollectionChanged?.Invoke(this, 
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove,
                        existing, index));
                CollectionChanged?.Invoke(this, 
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add,
                        value, index));
                Removed(existing);
                Added(value);
            }
        }

        object? IList.this[int index]
        {
            get => this[index];
            set
            {
                if (value is T item)
                {
                    this[index] = item;
                }
            }
        }
        public void Replace(T source, T target) => this[IndexOf(source)] = target;

        public int IndexOf(T item) => _collection.IndexOf(item);

        public int IndexOf(object value)
        {
            if (!(value is T item)) return -1;
            return IndexOf(item);
        }

        public bool Remove(T item)
        {
            var index = IndexOf(item);
            var removed = _collection.Remove(item);
            if (removed)
            {
                var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index);
                CollectionChanged?.Invoke(this, args);
                Removed(item);
            }
            return removed;
        }

        public void Remove(object value)
        {
            if (!(value is T item)) return;
            Remove(item);
        }

        public virtual void RemoveRange(IEnumerable<T> items)
        {
            var toRemove = items.ToArray();
            if (toRemove.Length == Count)
            {
                Clear();
                return;
            }

            if (toRemove.Length <= 10)
            {
                foreach (var item in toRemove)
                {
                    Remove(item);
                }
                return;
            }

            foreach (var item in toRemove)
            {
                _collection.Remove(item);
            }
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            RemovedMany(toRemove);
        }

        public void RemoveAt(int index)
        {
            var item = this[index];
            if (item == null) return;
            Remove(item);
        }

        public bool IsFixedSize { get; } = false;

        public void Clear()
        {
            var toClear = _collection.ToList();
            _collection.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            Cleared(toClear);
        }

        public bool Contains(T item) => _collection.Contains(item);

        public bool Contains(object value) => value is T item && Contains(item);


        public void CopyTo(T[] array, int arrayIndex) => _collection.CopyTo(array, arrayIndex);

        public void CopyTo(Array array, int index)
        {
            IList target = array;
            foreach (var item in _collection)
            {
                target[index++] = item;
            }
        }

        public int Count => _collection.Count;

        public bool IsSynchronized { get; } = false;

        public object SyncRoot { get; } = new object();

        public bool IsReadOnly => false;

        public override string ToString() => $"{Count} items";
    }
}
