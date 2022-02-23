using ObservableHelpers.Abstraction;
using ObservableHelpers.Utilities;
using System;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using SynchronizationContextHelpers;

namespace ObservableHelpers
{
    /// <summary>
    /// Provides a thread-safe observable object property for use with data binding.
    /// </summary>
    public class ObservableProperty :
        ObservableSyncContext
    {
        #region Properties

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        public object? Value
        {
            get => GetObject(null);
            set => SetObject(value?.GetType(), value);
        }

        private object? valueHolder;

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
        /// <typeparam name="T">
        /// The underlying type of the value to set.
        /// </typeparam>
        /// <returns>
        /// <c>true</c> whether the property has changed; otherwise <c>false</c>.
        /// </returns>
        public bool SetValue<T>(T value) => SetObject(typeof(T), value);

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        /// <typeparam name="T">
        /// The underlying type of the value to get.
        /// </typeparam>
        /// <returns>
        /// The value of the property.
        /// </returns>
        public T? GetValue<T>()
        {
            if (GetObject(typeof(T)) is T value)
            {
                return value;
            }
            return default;
        }

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        /// <param name="defaultValueFactory">
        /// The default value factory if the property is disposed or null.
        /// </param>
        /// <typeparam name="T">
        /// The underlying type of the value to get.
        /// </typeparam>
        /// <returns>
        /// The value of the property.
        /// </returns>
        public T GetValue<T>(Func<T> defaultValueFactory)
        {
            if (GetObject(typeof(T), () => defaultValueFactory()) is T value)
            {
                return value;
            }
            return defaultValueFactory();
        }

        /// <summary>
        /// Sets the object of the property.
        /// </summary>
        /// <param name="type">
        /// Underlying type of the object to get.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the property has changed; otherwise <c>false</c>.
        /// </returns>
        public bool SetObject(Type? type, object? value)
        {
            if (RWLock.LockWrite(() =>
            {
                if (InternalSetObject(type, value))
                {
                    if (value is SyncContext sync)
                    {
                        sync.SyncOperation.SetContext(this);
                    }

                    return true;
                }
                return false;
            }))
            {
                RWLock.InvokeOnLockExit(() => OnPropertyChanged(nameof(Value)));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the object of the property.
        /// </summary>
        /// <param name="type">
        /// Underlying type of the object to get.
        /// </param>
        /// <param name="defaultValueFactory">
        /// The default value factory if the property is disposed or null.
        /// </param>
        /// <returns>
        /// The value of the property.
        /// </returns>
        public object? GetObject(Type? type, Func<object?>? defaultValueFactory = default)
        {
            return RWLock.LockRead(() =>
            {
                object? obj = InternalGetObject(type);
                if (type == null || (obj != null && type.IsAssignableFrom(obj.GetType())))
                {
                    return obj;
                }
                else
                {
                    object? defaultValue = defaultValueFactory?.Invoke();

                    if (defaultValue is SyncContext sync)
                    {
                        sync.SyncOperation.SetContext(this);
                    }

                    return defaultValue;
                }
            });
        }

        /// <summary>
        /// Internal implementation for <see cref="SetObject(Type, object)"/>.
        /// </summary>
        /// <param name="type">
        /// Underlying type of the object to set.
        /// </param>
        /// <param name="obj">
        /// The value object of the property.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the property has changed; otherwise <c>false</c>.
        /// </returns>
        protected virtual bool InternalSetObject(Type? type, object? obj)
        {
            if (!(valueHolder?.Equals(obj) ?? obj == null))
            {
                valueHolder = obj;

                return true;
            }
            return false;
        }

        /// <summary>
        /// Internal implementation for <see cref="GetObject(Type?, Func{object?}?)"/>.
        /// </summary>
        /// <param name="type">
        /// Underlying type of the object to get.
        /// </param>
        /// <returns>
        /// The value object of the property.
        /// </returns>
        protected virtual object? InternalGetObject(Type? type)
        {
            return valueHolder;
        }

        #endregion

        #region Object Members

        /// <inheritdoc/>
        public override string ToString()
        {
            return valueHolder?.ToString() ?? "";
        }

        #endregion

        #region INullableObject Members

        /// <inheritdoc/>
        public override bool SetNull()
        {
            return RWLock.LockUpgradeableRead(() =>
            {
                if (GetObject(null) is INullableObject model)
                {
                    return model.SetNull();
                }
                else
                {
                    return SetObject(default, default);
                }
            });
        }

        /// <inheritdoc/>
        public override bool IsNull()
        {
            return RWLock.LockRead(() =>
            {
                object? obj = GetObject(null);

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

    /// <summary>
    /// Provides a thread-safe observable object property for use with data binding.
    /// </summary>
    /// <typeparam name="T">
    /// The undelying type of the property.
    /// </typeparam>
    public class ObservableProperty<T> :
        ObservableProperty
    {
        #region Properties

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        public new T? Value
        {
            get => GetValue();
            set => SetValue<T?>(value);
        }

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
        public bool SetValue(T value) => SetValue<T>(value);

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        /// <returns>
        /// The value of the property.
        /// </returns>
        public T? GetValue() => GetValue<T>();

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        /// <param name="defaultValue">
        /// The default value return if the property is disposed or null.
        /// </param>
        /// <returns>
        /// The value of the property.
        /// </returns>
        public T GetValue(Func<T> defaultValue) => GetValue<T>(defaultValue);

        #endregion
    }
}
