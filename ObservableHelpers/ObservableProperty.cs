using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace ObservableHelpers
{
    /// <summary>
    /// Provides a thread-safe observable object property for use with data binding.
    /// </summary>
    public class ObservableProperty : Observable
    {
        #region Properties

        /// <summary>
        /// Gets or sets the object property
        /// </summary>
        public object Property
        {
            get => GetObject();
            set => SetObject(value);
        }

        private object objectHolder;

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

        /// <inheritdoc/>
        public override bool SetNull()
        {
            if (IsDisposed)
            {
                return false;
            }

            if (GetObject() is INullableObject model)
            {
                return model.SetNull();
            }
            else
            {
                return SetObject(null);
            }
        }

        /// <inheritdoc/>
        public override bool IsNull()
        {
            if (IsDisposed)
            {
                return true;
            }

            var obj = GetObject();

            if (obj is INullableObject model)
            {
                return model.IsNull();
            }
            else
            {
                return obj == null;
            }
        }

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
        public virtual bool SetValue<T>(T value)
        {
            if (IsDisposed)
            {
                return false;
            }

            return SetObject(value);
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
        public virtual T GetValue<T>(T defaultValue = default)
        {
            if (IsDisposed)
            {
                return defaultValue;
            }

            if (GetObject() is T tObj)
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
        /// <returns>
        /// <c>true</c> whether the property has changed; otherwise <c>false</c>.
        /// </returns>
        protected virtual bool SetObject(object obj)
        {
            if (IsDisposed)
            {
                return false;
            }

            if (obj is ISyncObject sync)
            {
                sync.SyncOperation.SetContext(this);
            }

            if (!(objectHolder?.Equals(obj) ?? obj == null))
            {
                objectHolder = obj;
                OnPropertyChanged(nameof(Property));
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the value object of the property.
        /// </summary>
        /// <returns>
        /// The value object of the property.
        /// </returns>
        protected virtual object GetObject()
        {
            if (IsDisposed)
            {
                return default;
            }

            if (objectHolder is ISyncObject sync)
            {
                sync.SyncOperation.SetContext(this);
            }

            return objectHolder;
        }

        #endregion
    }

    /// <summary>
    /// Provides helpers for <see cref="INotifyPropertyChanged"/> for a single value or property.
    /// </summary>
    /// <typeparam name="T">
    /// The undelying type of the property.
    /// </typeparam>
    public class ObservableProperty<T> : ObservableProperty
    {
        #region Properties

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        public T Value
        {
            get => GetValue<T>();
            set => SetValue(Value);
        }

        #endregion

        #region Initializers

        /// <summary>
        /// Creates the instance of the <c>ObservableProperty<T></c> class.
        /// </summary>
        public ObservableProperty()
        {

        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == nameof(Property))
            {
                base.OnPropertyChanged(nameof(Value));
            }
        }

        #endregion
    }
}
