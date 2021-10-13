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
                return default;
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
                return default;
            }

            object obj = GetObject();

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
                return default;
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
                return default;
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
                return default;
            }

            if (obj is ISyncObject sync)
            {
                sync.SyncOperation.SetContext(this);
            }

            if (!(objectHolder?.Equals(obj) ?? obj == null))
            {
                objectHolder = obj;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Property)));
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
    public class ObservableProperty<T> :
        ObservableProperty
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
        /// Creates new instance of the <see cref="ObservableProperty{T}"/> class.
        /// </summary>
        public ObservableProperty()
        {

        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);
            if (args.PropertyName == nameof(Property))
            {
                base.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        #endregion
    }
}
