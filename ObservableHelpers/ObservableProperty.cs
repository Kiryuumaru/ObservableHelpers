using ObservableHelpers.Abstraction;
using ObservableHelpers.Utilities;
using System;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;

namespace ObservableHelpers
{
    /// <summary>
    /// Provides a thread-safe observable object property for use with data binding.
    /// </summary>
    /// <typeparam name="T">
    /// The undelying type of the property.
    /// </typeparam>
    public class ObservableProperty<T> :
        ObservableSyncContext
    {
        #region Properties

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        public T Value
        {
            get
            {
                if (IsDisposed)
                {
                    return default;
                }

                return RWLock.LockRead(() => InternalGetObject());
            }
            set
            {
                if (IsDisposed)
                {
                    return;
                }

                RWLock.LockWrite(() =>
                {
                    if (InternalSetObject(value))
                    {
                        if (value is ISyncObject sync)
                        {
                            sync.SyncOperation.SetContext(this);
                        }

                        OnPropertyChanged(nameof(Value));

                        return true;
                    }
                    return false;
                });
            }
        }
        
        /// <summary>
        /// Gets the read-write lock for concurrency.
        /// </summary>
        protected RWLock RWLock { get; } = new RWLock(LockRecursionPolicy.SupportsRecursion);

        private T valueHolder;

        #endregion

        #region Initializers

        /// <summary>
        /// Creates new instance of the <see cref="ObservableProperty{T}"/> class.
        /// </summary>
        public ObservableProperty()
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the value of the property.
        /// </summary>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the property has changed; otherwise <c>false</c>.
        /// </returns>
        public bool SetValue(T value)
        {
            if (IsDisposed)
            {
                return default;
            }

            return RWLock.LockWrite(() =>
            {
                if (InternalSetObject(value))
                {
                    if (value is ISyncObject sync)
                    {
                        sync.SyncOperation.SetContext(this);
                    }

                    OnPropertyChanged(nameof(Value));

                    return true;
                }
                return false;
            });
        }

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        /// <param name="defaultValue">
        /// The default value return if the property is disposed or null.
        /// </param>
        /// <returns>
        /// The value of the property.
        /// </returns>
        public T GetValue(T defaultValue = default)
        {
            if (IsDisposed)
            {
                return defaultValue;
            }

            return RWLock.LockRead(() =>
            {
                if (InternalGetObject() is T tObj)
                {
                    return tObj;
                }
                else
                {
                    if (defaultValue is ISyncObject sync)
                    {
                        sync.SyncOperation.SetContext(this);
                    }

                    return defaultValue;
                }
            });
        }

        /// <summary>
        /// Internal implementation for <see cref="SetValue(T)"/>.
        /// </summary>
        /// <param name="obj">
        /// The value object of the property.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the property has changed; otherwise <c>false</c>.
        /// </returns>
        protected virtual bool InternalSetObject(T obj)
        {
            if (!(valueHolder?.Equals(obj) ?? obj == null))
            {
                valueHolder = obj;

                return true;
            }
            return false;
        }

        /// <summary>
        /// Internal implementation for <see cref="GetValue(T)"/>.
        /// </summary>
        /// <returns>
        /// The value object of the property.
        /// </returns>
        protected virtual T InternalGetObject()
        {
            return valueHolder;
        }

        #endregion

        #region Object Members

        /// <inheritdoc/>
        public override string ToString()
        {
            return valueHolder?.ToString();
        }

        #endregion

        #region INullableObject Members

        /// <inheritdoc/>
        public override bool SetNull()
        {
            if (IsDisposed)
            {
                return default;
            }

            return RWLock.LockRead(() =>
            {
                if (InternalGetObject() is INullableObject model)
                {
                    return model.SetNull();
                }
                else
                {
                    return RWLock.LockWrite(() =>
                    {
                        if (InternalSetObject(default))
                        {
                            OnPropertyChanged(nameof(Value));

                            return true;
                        }
                        return false;
                    });
                }
            });
        }

        /// <inheritdoc/>
        public override bool IsNull()
        {
            if (IsDisposed)
            {
                return default;
            }

            return RWLock.LockRead(() =>
            {
                object obj = InternalGetObject();

                if (obj is INullableObject model)
                {
                    return model.IsNull();
                }
                else
                {
                    return obj == null;
                }
            });
        }

        #endregion
    }
}
