using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;

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
        #region Properties



        #endregion

        #region Initializers

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollection{T}"/> class that contains empty elements and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        public ObservableCollection()
            : base(() => new List<T>())
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
            : base(() =>
            {
                if (enumerable == null)
                {
                    throw new ArgumentNullException(nameof(enumerable));
                }
                return new List<T>(enumerable);
            })
        {

        }

        #endregion

        #region Members



        #endregion
    }
}
