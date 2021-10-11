using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace ObservableHelpers
{
    /// <summary>
    /// Provides a thread-safe observable collection for use with data binding.
    /// </summary>
    /// <typeparam name="T">
    /// Specifies the type of the items in this collection.
    /// </typeparam>
    public class ObservableCollectionFilter<T> :
        ObservableCollectionSyncContext,
        IReadOnlyList<T>,
        ICollection,
        INotifyCollectionChanged,
        INotifyPropertyChanged
    {
        #region Base Collection Implementation

        private protected class BaseObservableCollection<U> : System.Collections.ObjectModel.ObservableCollection<U>
        {
            public IList<U> ExposedItems => Items;

            private readonly ObservableCollectionFilter<U> coreObservable;

            public BaseObservableCollection(ObservableCollectionFilter<U> coreObservable)
                : base()
            {
                this.coreObservable = coreObservable;
            }

            public BaseObservableCollection(ObservableCollectionFilter<U> coreObservable, IEnumerable<U> collection)
                : base(collection)
            {
                this.coreObservable = coreObservable;
            }

            public IDisposable ExposedBlockReentrancy() => BlockReentrancy();

            public void ExposedCheckReentrancy() => CheckReentrancy();

            public override bool Equals(object obj) => coreObservable.Equals(obj);
            public bool ExposedEquals(object obj) => base.Equals(obj);

            public override int GetHashCode() => coreObservable.GetHashCode();
            public int ExposedGetHashCode() => base.GetHashCode();

            public override string ToString() => coreObservable.ToString();
            public string ExposedToString() => base.ToString();

            protected override void ClearItems() => coreObservable.ClearItems();
            public void ExposedClearItems() => base.ClearItems();

            protected override void RemoveItem(int index) => coreObservable.RemoveItem(index);
            public void ExposedRemoveItem(int index) => base.RemoveItem(index);

            protected override void InsertItem(int index, U item) => coreObservable.InsertItem(index, item);
            public void ExposedInsertItem(int index, U item) => base.InsertItem(index, item);

            protected override void SetItem(int index, U item) => coreObservable.SetItem(index, item);
            public void ExposedSetItem(int index, U item) => base.SetItem(index, item);

            protected override void MoveItem(int oldIndex, int newIndex) => coreObservable.MoveItem(oldIndex, newIndex);
            public void ExposedMoveItem(int oldIndex, int newIndex) => base.MoveItem(oldIndex, newIndex);

            protected override void OnPropertyChanged(PropertyChangedEventArgs e) => coreObservable.OnPropertyChanged(e);
            public void ExposedOnPropertyChanged(PropertyChangedEventArgs e) => base.OnPropertyChanged(e);

            protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) => coreObservable.OnCollectionChanged(e);
            public void ExposedOnCollectionChanged(NotifyCollectionChangedEventArgs e) => base.OnCollectionChanged(e);
        }

        #endregion

        #region Properties

        /// <inheritdoc/>
        public T this[int index]
        {
            get => InternalObservableCollection[index];
        }

        /// <inheritdoc/>
        public int Count => InternalObservableCollection.Count;

        /// <inheritdoc cref="Collection{T}.Items"/>
        protected IList<T> Items => InternalObservableCollection.ExposedItems;

        /// <summary>
        /// Gets the wrapped core <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/> instance.
        /// </summary>
        protected virtual System.Collections.ObjectModel.ObservableCollection<T> CoreObservableCollection => InternalObservableCollection;

        private protected virtual BaseObservableCollection<T> InternalObservableCollection { get; private set; }

        #endregion

        #region Initializers

        /// <summary>
        /// Initializes a new instance of ObservableCollection that is empty and has default initial capacity.
        /// </summary>
        public ObservableCollectionFilter()
        {
            InternalObservableCollection = new BaseObservableCollection<T>(this);
        }

        /// <summary>
        /// Initializes a new instance of the ObservableCollection class that contains
        /// elements copied from the specified collection and has sufficient capacity
        /// to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new list.</param>
        /// <remarks>
        /// The elements are copied onto the ObservableCollection in the
        /// same order they are read by the enumerator of the collection.
        /// </remarks>
        /// <exception cref="ArgumentNullException"> collection is a null reference </exception>
        public ObservableCollectionFilter(IEnumerable<T> collection)
        {
            InternalObservableCollection = new BaseObservableCollection<T>(this, collection) ?? throw new ArgumentNullException(nameof(collection));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an observable filter that shadows the changes notifications from the parent observable.
        /// </summary>
        /// <param name="predicate">
        /// The predicate filter for child observable.
        /// </param>
        /// <returns>
        /// The created child filter observable.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// 
        /// </exception>
        public ObservableCollectionFilter<T> ObservableFilter(Predicate<T> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            ObservableCollectionFilter<T> filter = new ObservableCollectionFilter<T>(InternalObservableCollection.Where(i => predicate.Invoke(i)));
            void Filter_ImmediateCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (filter.IsDisposed)
                {
                    return;
                }

                if (e.NewItems != null)
                {
                    foreach (T item in e.NewItems)
                    {
                        if (predicate.Invoke(item))
                        {
                            filter.InternalObservableCollection.Add(item);
                        }
                    }
                }

                if (e.OldItems != null)
                {
                    foreach (T item in e.OldItems)
                    {
                        if (predicate.Invoke(item))
                        {
                            filter.InternalObservableCollection.Remove(item);
                        }
                    }
                }

                if (e.NewItems == null && e.OldItems == null && filter.Count != 0)
                {
                    filter.InternalObservableCollection.Clear();
                }
            }
            ImmediateCollectionChanged += Filter_ImmediateCollectionChanged;
            filter.Disposing += (s, e) =>
            {
                ImmediateCollectionChanged -= Filter_ImmediateCollectionChanged;
            };
            return filter;
        }

        /// <inheritdoc/>
        public bool Contains(T item) => InternalObservableCollection.Contains(item);

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex) => InternalObservableCollection.CopyTo(array, arrayIndex);

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator() => InternalObservableCollection.GetEnumerator();

        /// <inheritdoc/>
        public int IndexOf(T item) => InternalObservableCollection.IndexOf(item);

        /// <inheritdoc/>
        public override bool SetNull()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override bool IsNull()
        {
            return InternalObservableCollection.Count == 0;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => InternalObservableCollection.ExposedEquals(obj);

        /// <inheritdoc/>
        public override int GetHashCode() => InternalObservableCollection.ExposedGetHashCode();

        /// <inheritdoc cref="System.Collections.ObjectModel.ObservableCollection{T}.BlockReentrancy()"/>
        protected IDisposable BlockReentrancy() => InternalObservableCollection.ExposedBlockReentrancy();

        /// <inheritdoc cref="System.Collections.ObjectModel.ObservableCollection{T}.CheckReentrancy()"/>
        protected void CheckReentrancy() => InternalObservableCollection.ExposedBlockReentrancy();

        /// <inheritdoc cref="System.Collections.ObjectModel.ObservableCollection{T}.ClearItems()"/>
        protected virtual void ClearItems() => InternalObservableCollection.ExposedClearItems();

        /// <inheritdoc cref="System.Collections.ObjectModel.ObservableCollection{T}.InsertItem(int, T)"/>
        protected virtual void InsertItem(int index, T item) => InternalObservableCollection.ExposedInsertItem(index, item);

        /// <inheritdoc cref="System.Collections.ObjectModel.ObservableCollection{T}.RemoveItem(int)"/>
        protected virtual void RemoveItem(int index) => InternalObservableCollection.ExposedRemoveItem(index);

        /// <inheritdoc cref="System.Collections.ObjectModel.ObservableCollection{T}.SetItem(int, T)"/>
        protected virtual void SetItem(int index, T item) => InternalObservableCollection.ExposedSetItem(index, item);

        /// <inheritdoc cref="System.Collections.ObjectModel.ObservableCollection{T}.MoveItem(int, int)"/>
        protected virtual void MoveItem(int oldIndex, int newIndex) => InternalObservableCollection.ExposedMoveItem(oldIndex, newIndex);
        
        #endregion

        #region IEnumerable Explicit Implementation

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => (InternalObservableCollection as IEnumerable).GetEnumerator();

        #endregion

        #region ICollection Explicit Implementation

        /// <inheritdoc/>
        bool ICollection.IsSynchronized => (InternalObservableCollection as ICollection).IsSynchronized;

        /// <inheritdoc/>
        object ICollection.SyncRoot => (InternalObservableCollection as ICollection).SyncRoot;

        /// <inheritdoc/>
        void ICollection.CopyTo(Array array, int index) => (InternalObservableCollection as IList).CopyTo(array, index);

        #endregion
    }
}
