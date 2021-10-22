using ObservableHelpers.Abstraction;
using System;
using System.ComponentModel;

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
        public object Value
        {
            get
            {
                if (IsDisposed)
                {
                    return default;
                }

                return GetObject().value;
            }
        }

        /// <summary>
        /// Gets the type of the value of the property.
        /// </summary>
        public Type Type
        {
            get
            {
                if (IsDisposed)
                {
                    return default;
                }

                return GetObject().type;
            }
        }

        private object valueHolder;
        private Type valueType;

        #endregion

        #region Initializers

        /// <summary>
        /// Creates new instance of the <see cref="ObservableProperty"/> class.
        /// </summary>
        public ObservableProperty()
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the value of the property.
        /// </summary>
        /// <typeparam name="T">
        /// The undelying type of the property value.
        /// </typeparam>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the property has changed; otherwise <c>false</c>.
        /// </returns>
        public bool SetValue<T>(T value)
        {
            if (IsDisposed)
            {
                return default;
            }

            return SetObject(value, typeof(T));
        }

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        /// <typeparam name="T">
        /// The underlying type of the property.
        /// </typeparam>
        /// <param name="defaultValue">
        /// The default value return if the property is disposed or null.
        /// </param>
        /// <returns>
        /// The value of the property.
        /// </returns>
        public T GetValue<T>(T defaultValue = default)
        {
            if (IsDisposed)
            {
                return default;
            }

            if (GetObject().value is T tObj)
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
        }

        /// <summary>
        /// Sets the object of the property.
        /// </summary>
        /// <param name="obj">
        /// The value object of the property.
        /// </param>
        /// <param name="objType">
        /// The underlying type of the object.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the property has changed; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="obj"/> <see cref="System.Type"/> and <paramref name="objType"/> mismatch.
        /// </exception>
        protected virtual bool SetObject(object obj, Type objType)
        {
            if (obj != null)
            {
                if (obj.GetType() != objType)
                {
                    throw new ArgumentException("Provided " + nameof(obj) + " type and" + nameof(objType) + " mismatch.");
                }
            }

            bool isValueChanged = false;
            bool isTypeChanged = false;

            if (!(valueHolder?.Equals(obj) ?? obj == null))
            {
                if (obj is ISyncObject sync)
                {
                    sync.SyncOperation.SetContext(this);
                }

                valueHolder = obj;
                OnPropertyChanged(nameof(Value));

                isValueChanged = true;
            }

            if (!(valueType?.Equals(objType) ?? objType == null))
            {
                valueType = objType;
                OnPropertyChanged(nameof(Type));

                isTypeChanged = true;
            }

            return isValueChanged || isTypeChanged;
        }

        /// <summary>
        /// Gets the value object of the property.
        /// </summary>
        /// <returns>
        /// The value object of the property.
        /// </returns>
        protected virtual (object value, Type type) GetObject()
        {
            return (valueHolder, valueType);
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

            if (GetObject().value is INullableObject model)
            {
                return model.SetNull();
            }
            else
            {
                return SetObject(null, Type);
            }
        }

        /// <inheritdoc/>
        public override bool IsNull()
        {
            if (IsDisposed)
            {
                return default;
            }

            object obj = GetObject().value;

            if (obj is INullableObject model)
            {
                return model.IsNull();
            }
            else
            {
                return obj == null;
            }
        }

        #endregion
    }

    /// <summary>
    /// Provides helpers for <see cref="INotifyPropertyChanged"/> for a single value or property.
    /// </summary>
    /// <typeparam name="T">
    /// The undelying type of the property.
    /// </typeparam>
    public class ObservableProperty<T> :
        ObservableProperty
    {
        #region Properties

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        public new T Value
        {
            get => GetValue();
            set => SetValue(value);
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
        public bool SetValue(T value)
        {
            return SetValue<T>(value);
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
            return GetValue<T>(defaultValue);
        }

        #endregion
    }
}
