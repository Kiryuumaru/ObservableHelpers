using ObservableHelpers.Utilities;
using System;
using System.Collections.Generic;

namespace ObservableHelpers
{
    /// <summary>
    /// Provides a thread-safe observable collection used for data binding.
    /// </summary>
    /// <typeparam name="T">
    /// Specifies the type of the items in this collection.
    /// </typeparam>
    public class ObservableCollection<T> :
        ObservableCollectionBase<T>
    {
        #region Initializers

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollection{T}"/> class that contains empty elements and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        public ObservableCollection()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollection{T}"/> class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="enumerable">
        /// The collection whose elements are copied to the new list.
        /// </param>
        /// <remarks>
        /// The elements are copied onto the <see cref="ObservableCollection{T}"/> in the same order they are read by the enumerator of the collection.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="enumerable"/> is a null reference.
        /// </exception>
        public ObservableCollection(IEnumerable<T> enumerable)
            : base(_ => new List<T>(enumerable))
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollection{T}"/> class.
        /// </summary>
        /// <param name="collectionWrapperFactory">
        /// The function used to create the <see cref="ObservableCollectionBase{T}.Items"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collectionWrapperFactory"/> is a null reference.
        /// </exception>
        public ObservableCollection(Func<ObservableCollectionBase<T>, List<T>> collectionWrapperFactory)
            : base (collectionWrapperFactory)
        {

        }

        #endregion
    }
}
