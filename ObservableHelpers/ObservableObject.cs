using ObservableHelpers.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObservableHelpers
{
    /// <summary>
    /// Provides a thread-safe observable object class for use with data binding.
    /// </summary>
    public class ObservableObject : Observable
    {
        #region Helpers

        /// <summary>
        /// Provides the property with key, name and group.
        /// </summary>
        protected class NamedProperty
        {
            /// <summary>
            /// Creates new instance for <see cref="NamedProperty"/>
            /// </summary>
            public NamedProperty()
            {

            } 

            /// <summary>
            /// Gets or sets the <see cref="ObservableProperty"/> of the object.
            /// </summary>
            public ObservableProperty Property { get; set; }

            /// <summary>
            /// Gets or sets the key of the <see cref="Property"/>
            /// </summary>
            public string Key { get; set; }

            /// <summary>
            /// Gets or sets the name of the <see cref="Property"/>
            /// </summary>
            public string PropertyName { get; set; }

            /// <summary>
            /// Gets or sets the group of the <see cref="Property"/>
            /// </summary>
            public string Group { get; set; }
        }

        #endregion

        #region Properties

        private readonly List<NamedProperty> namedProperties = new List<NamedProperty>();

        #endregion

        #region Initializers

        /// <summary>
        /// Creates new instance of the <see cref="ObservableObject"/> class.
        /// </summary>
        public ObservableObject()
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

            var hasChanges = false;
            lock (namedProperties)
            {
                foreach (var propHolder in namedProperties)
                {
                    if (propHolder.Property.SetNull())
                    {
                        hasChanges = true;
                    }
                }
            }
            return hasChanges;
        }

        /// <inheritdoc/>
        public override bool IsNull()
        {
            if (IsDisposed)
            {
                return true;
            }

            lock (namedProperties)
            {
                return namedProperties.All(i => i.Property.IsNull());
            }
        }

        /// <summary>
        /// Initializes all property with its key, name and group.
        /// </summary>
        protected void InitializeProperties()
        {
            if (IsDisposed)
            {
                return;
            }

            try
            {
                foreach (var property in GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                {
                    property.GetValue(this);
                }
            }
            catch { }
        }

        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <typeparam name="T">
        /// The underlying type of the property to set.
        /// </typeparam>
        /// <param name="value">
        /// The value of the property to set.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property to set.
        /// </param>
        /// <param name="group">
        /// The group of the property to set.
        /// </param>
        /// <param name="validate">
        /// Value validator function for set.
        /// </param>
        /// <param name="onSet">
        /// Callback after set operation.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the property was set; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="PropertyKeyAndNameNullException">
        /// Throws when <paramref name="propertyName"/> is not provided.
        /// </exception>
        protected bool SetProperty<T>(
            T value,
            [CallerMemberName] string propertyName = null,
            string group = null,
            Func<(T oldValue, T newValue), bool> validate = null,
            EventHandler<ObjectPropertySetEventArgs<T>> onSet = null)
        {
            if (IsDisposed)
            {
                return false;
            }

            return SetPropertyInternal(value, null, propertyName, group, validate, onSet);
        }

        /// <summary>
        /// Sets the property value with the provided <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The underlying type of the property to set.
        /// </typeparam>
        /// <param name="key">
        /// The key of the property value to set.
        /// </param>
        /// <param name="value">
        /// The value of the property to set.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property to set.
        /// </param>
        /// <param name="group">
        /// The group of the property to set.
        /// </param>
        /// <param name="validate">
        /// Value validator function for set.
        /// </param>
        /// <param name="onSet">
        /// Callback after set operation.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the property was set; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="PropertyKeyAndNameNullException">
        /// Throws when <paramref name="key"/> is not provided.
        /// </exception>
        protected bool SetPropertyWithKey<T>(
            T value,
            string key,
            [CallerMemberName] string propertyName = null,
            string group = null,
            Func<(T oldValue, T newValue), bool> validate = null,
            EventHandler<ObjectPropertySetEventArgs<T>> onSet = null)
        {
            if (IsDisposed)
            {
                return false;
            }

            return SetPropertyInternal(value, key, propertyName, group, validate, onSet);
        }

        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <typeparam name="T">
        /// The underlying type of the property to get.
        /// </typeparam>
        /// <param name="defaultValue">
        /// The default value sets and returned if the property is null.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property value to get.
        /// </param>
        /// <param name="group">
        /// The group of the property value to get.
        /// </param>
        /// <param name="validate">
        /// Value validator function for set.
        /// </param>
        /// <param name="onSet">
        /// Callback after set operation.
        /// </param>
        /// <returns>
        /// The found <typeparamref name="T"/> property value.
        /// </returns>
        /// <exception cref="PropertyKeyAndNameNullException">
        /// Throws when <paramref name="propertyName"/> is not provided.
        /// </exception>
        protected T GetProperty<T>(
            T defaultValue = default,
            [CallerMemberName] string propertyName = null,
            string group = null,
            Func<(T oldValue, T newValue), bool> validate = null,
            EventHandler<ObjectPropertySetEventArgs<T>> onSet = null)
        {
            if (IsDisposed)
            {
                return defaultValue;
            }

            return GetPropertyInternal(defaultValue, null, propertyName, group, validate, onSet);
        }

        /// <summary>
        /// Gets the property value with the provided <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The underlying type of the property to get.
        /// </typeparam>
        /// <param name="key">
        /// The key of the property value to get.
        /// </param>
        /// <param name="defaultValue">
        /// The default value sets and returned if the property is null.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property value to get.
        /// </param>
        /// <param name="group">
        /// The group of the property value to get.
        /// </param>
        /// <param name="validate">
        /// Value validator function for set.
        /// </param>
        /// <param name="onSet">
        /// Callback after set operation.
        /// </param>
        /// <returns>
        /// The found <typeparamref name="T"/> property value.
        /// </returns>
        /// <exception cref="PropertyKeyAndNameNullException">
        /// Throws when <paramref name="key"/> is not provided.
        /// </exception>
        protected T GetPropertyWithKey<T>(
            string key,
            T defaultValue = default,
            [CallerMemberName] string propertyName = null,
            string group = null,
            Func<(T oldValue, T newValue), bool> validate = null,
            EventHandler<ObjectPropertySetEventArgs<T>> onSet = null)
        {
            if (IsDisposed)
            {
                return defaultValue;
            }

            return GetPropertyInternal(defaultValue, key, propertyName, group, validate, onSet);
        }

        /// <summary>
        /// Removes the property with the provided <paramref name="propertyName"/>
        /// </summary>
        /// <remarks>
        /// This implementation completely removes the property from the property collection. Use <see cref="DeleteProperty"/> to set the property to null.
        /// </remarks>
        /// <param name="propertyName">
        /// The name of the property to remove.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the property was removed; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="PropertyKeyAndNameNullException">
        /// Throws when <paramref name="propertyName"/> is not provided.
        /// </exception>
        protected bool RemoveProperty(string propertyName)
        {
            if (IsDisposed)
            {
                return false;
            }

            return RemovePropertyCore(null, propertyName);
        }

        /// <summary>
        /// Deletes the property with the provided <paramref name="key"/>
        /// </summary>
        /// <remarks>
        /// This implementation completely removes the property from the property collection. Use <see cref="DeletePropertyWithKey"/> to set the property to null.
        /// </remarks>
        /// <param name="key">
        /// The key of the property to remove.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the property was removed; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="PropertyKeyAndNameNullException">
        /// Throws when <paramref name="key"/> is not provided.
        /// </exception>
        protected bool RemovePropertyWithKey(string key)
        {
            if (IsDisposed)
            {
                return false;
            }

            return RemovePropertyCore(key, null);
        }

        /// <summary>
        /// Deletes the property with the provided <paramref name="propertyName"/>
        /// </summary>
        /// <remarks>
        /// This implementation simply calls <see cref="ObservableProperty.SetNull"/> for deletion but not removing it from the property collection. Use <see cref="RemoveProperty"/> to completely remove the property.
        /// </remarks>
        /// <param name="propertyName">
        /// The name of the property to delete.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the property was deleted; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="PropertyKeyAndNameNullException">
        /// Throws when <paramref name="propertyName"/> is not provided.
        /// </exception>
        protected bool DeleteProperty(string propertyName)
        {
            if (IsDisposed)
            {
                return false;
            }

            return DeletePropertyCore(null, propertyName);
        }

        /// <summary>
        /// Deletes the property with the provided <paramref name="key"/>
        /// </summary>
        /// <remarks>
        /// This implementation simply calls <see cref="ObservableProperty.SetNull"/> for deletion but not removing it from the property collection. Use <see cref="RemovePropertyWithKey"/> to completely remove the property.
        /// </remarks>
        /// <param name="key">
        /// The key of the property to delete.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the property was deleted; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="PropertyKeyAndNameNullException">
        /// Throws when <paramref name="key"/> is not provided.
        /// </exception>
        protected bool DeletePropertyWithKey(string key)
        {
            if (IsDisposed)
            {
                return false;
            }

            return DeletePropertyCore(key, null);
        }

        /// <summary>
        /// Wire <see cref="NamedProperty"/> to the <see cref="INotifyPropertyChanged.PropertyChanged"/> event and the current context of this <see cref="ObservableObject"/> instance. 
        /// </summary>
        /// <param name="namedProperty">
        /// The <see cref="NamedProperty"/> to be wired.
        /// </param>
        protected void WireNamedProperty(NamedProperty namedProperty)
        {
            namedProperty.Property.SyncOperation.SetContext(this);
            namedProperty.Property.PropertyChanged += (s, e) =>
            {
                if (IsDisposed)
                {
                    return;
                }
                if (e.PropertyName == nameof(namedProperty.Property.Property))
                {
                    OnPropertyChanged(namedProperty.Key, namedProperty.PropertyName, namedProperty.Group);
                }
            };
        }

        /// <summary>
        /// Gets the raw properties <see cref="NamedProperty"/> from the property collection with the provided <paramref name="group"/>.
        /// </summary>
        /// <param name="group">
        /// The group of the properties to get. If null, all properties will be returned.
        /// </param>
        /// <returns>
        /// The raw properties.
        /// </returns>
        protected IEnumerable<NamedProperty> GetRawProperties(string group = null)
        {
            if (IsDisposed)
            {
                return default;
            }

            lock (namedProperties)
            {
                return group == null ? namedProperties.ToList() : namedProperties.Where(i => i.Group == group).ToList();
            }
        }

        /// <summary>
        /// The core implementation for adding the <see cref="NamedProperty"/> to the property collection.
        /// </summary>
        /// <param name="namedProperty">
        /// The <see cref="NamedProperty"/> to be added in the property collection.
        /// </param>
        /// <exception cref="PropertyAlreadyExistsException">
        /// Throws when the provided <paramref name="namedProperty"/> already exists.
        /// </exception>
        protected void AddCore(NamedProperty namedProperty)
        {
            if (IsDisposed)
            {
                return;
            }

            bool exists = false;
            lock (namedProperties)
            {
                if (namedProperty.Key == null)
                {
                    if (namedProperties.Any(i => i.PropertyName == namedProperty.PropertyName))
                    {
                        exists = true;
                    }
                }
                else
                {
                    if (namedProperties.Any(i => i.Key == namedProperty.Key))
                    {
                        exists = true;
                    }
                }
                if (!exists)
                {
                    namedProperties.Add(namedProperty);
                }
            }
            if (exists)
            {
                throw new PropertyAlreadyExistsException(namedProperty.Key, namedProperty.PropertyName);
            }
        }

        /// <summary>
        /// The core implementation for getting the <see cref="NamedProperty"/> from the property collection with the provided <paramref name="key"/> or <paramref name="propertyName"/>.
        /// </summary>
        /// <remarks>
        /// Both <paramref name="key"/> and <paramref name="propertyName"/> should not be null. If both are provided, <paramref name="key"/> will be used to find the property.
        /// </remarks>
        /// <param name="key">
        /// The key of the property to get.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property to get.
        /// </param>
        /// <returns>
        /// The found <see cref="NamedProperty"/> from the property collection.
        /// </returns>
        /// <exception cref="PropertyKeyAndNameNullException">
        /// Throws when both <paramref name="key"/> and <paramref name="propertyName"/> are not provided.
        /// </exception>
        protected NamedProperty GetCore(string key, string propertyName)
        {
            if (IsDisposed)
            {
                return default;
            }

            if (key == null && propertyName == null) throw new PropertyKeyAndNameNullException();

            lock (namedProperties)
            {
                if (key == null)
                {
                    return namedProperties.FirstOrDefault(i => i.PropertyName == propertyName);
                }
                else
                {
                    return namedProperties.FirstOrDefault(i => i.Key == key);
                }
            }
        }

        /// <summary>
        /// The core implementation for removing a property with the provided <paramref name="key"/> or <paramref name="propertyName"/>.
        /// </summary>
        /// <remarks>
        /// <para>This implementation completely removes the property from the property collection. Use <see cref="DeletePropertyCore"/> to set the property to null.</para>
        /// <para>Both <paramref name="key"/> and <paramref name="propertyName"/> should not be null. If both are provided, <paramref name="key"/> will be used to find the property.</para>
        /// </remarks>
        /// <param name="key">
        /// The key of the property to remove.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property to remove.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the property was removed; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="PropertyKeyAndNameNullException">
        /// Throws when both <paramref name="key"/> and <paramref name="propertyName"/> are not provided.
        /// </exception>
        protected bool RemovePropertyCore(string key, string propertyName)
        {
            if (IsDisposed)
            {
                return false;
            }

            if (key == null && propertyName == null) throw new PropertyKeyAndNameNullException();

            int removedCount = 0;
            lock (namedProperties)
            {
                if (key == null)
                {
                    removedCount = namedProperties.RemoveAll(i => i.PropertyName == propertyName);
                }
                else
                {
                    removedCount = namedProperties.RemoveAll(i => i.Key == key);
                }
            }
            return removedCount != 0;
        }

        /// <summary>
        /// The core implementation for deleting a property with the provided <paramref name="key"/> or <paramref name="propertyName"/>.
        /// </summary>
        /// <remarks>
        /// <para>This implementation simply calls <see cref="ObservableProperty.SetNull"/> for deletion but not removing it from the property collection. Use <see cref="RemovePropertyCore"/> to completely remove the property.</para>
        /// <para>Both <paramref name="key"/> and <paramref name="propertyName"/> should not be null. If both are provided, <paramref name="key"/> will be used to find the property.</para>
        /// </remarks>
        /// <param name="key">
        /// The key of the property to delete.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property to delete.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the property was deleted; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="PropertyKeyAndNameNullException">
        /// Throws when both <paramref name="key"/> and <paramref name="propertyName"/> are not provided.
        /// </exception>
        protected bool DeletePropertyCore(string key, string propertyName)
        {
            if (IsDisposed)
            {
                return false;
            }

            if (key == null && propertyName == null) throw new PropertyKeyAndNameNullException();

            NamedProperty propHolder = null;
            lock (namedProperties)
            {
                if (key == null)
                {
                    propHolder = namedProperties.FirstOrDefault(i => i.PropertyName == propertyName);
                }
                else
                {
                    propHolder = namedProperties.FirstOrDefault(i => i.Key == key);
                }
            }

            return propHolder?.Property.SetNull() ?? false;
        }

        /// <summary>
        /// The core implementation for checking if property exists with the provided <paramref name="key"/> or <paramref name="propertyName"/>.
        /// </summary>
        /// <remarks>
        /// Both <paramref name="key"/> and <paramref name="propertyName"/> should not be null. If both are provided, <paramref name="key"/> will be used to find the property.
        /// </remarks>
        /// <param name="key">
        /// The key of the property to find.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property to find.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the property exists; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="PropertyKeyAndNameNullException">
        /// Throws when both <paramref name="key"/> and <paramref name="propertyName"/> are not provided.
        /// </exception>
        protected bool ExistsCore(string key, string propertyName)
        {
            if (IsDisposed)
            {
                return false;
            }

            if (key == null && propertyName == null) throw new PropertyKeyAndNameNullException();

            lock (namedProperties)
            {
                if (key == null)
                {
                    if (namedProperties.Any(i => i.PropertyName == propertyName))
                    {
                        return true;
                    }
                }
                else
                {
                    if (namedProperties.Any(i => i.Key == key))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Virtual factory used to create all property holders.
        /// </summary>
        /// <param name="key">
        /// The key of the property to create.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property to create.
        /// </param>
        /// <param name="group">
        /// The group of the property to create.
        /// </param>
        /// <returns>
        /// The created instance of <see cref="NamedProperty"/>.
        /// </returns>
        protected virtual NamedProperty NamedPropertyFactory(string key, string propertyName, string group)
        {
            if (IsDisposed)
            {
                return null;
            }

            return new NamedProperty()
            {
                Property = new ObservableProperty(),
                Key = key,
                PropertyName = propertyName,
                Group = group
            };
        }

        /// <summary>
        /// Invokes <see cref="INotifyPropertyChanged.PropertyChanged"/> into the current context.
        /// </summary>
        /// <param name="key">
        /// The key of the property changed.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property changed.
        /// </param>
        /// <param name="group">
        /// The group of the property changed.
        /// </param>
        protected virtual void OnPropertyChanged(string key, string propertyName, string group)
        {
            if (IsDisposed)
            {
                return;
            }

            OnPropertyChanged(new ObjectPropertyChangesEventArgs(key, propertyName, group));
        }

        /// <summary>
        /// Invokes <see cref="INotifyPropertyChanged.PropertyChanged"/> into the current context if the provided <paramref name="key"/> is found in this object.
        /// </summary>
        /// <param name="key">
        /// The key of the property changed.
        /// </param>
        protected virtual void OnPropertyChangedWithKey(string key)
        {
            if (IsDisposed)
            {
                return;
            }

            NamedProperty propHolder = null;
            lock (namedProperties)
            {
                propHolder = namedProperties.FirstOrDefault(i => i.Key == key);
            }
            if (propHolder != null) OnPropertyChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
        }

        private bool SetPropertyInternal<T>(
            T value,
            string key,
            string propertyName,
            string group,
            Func<(T oldValue, T newValue), bool> validate,
            EventHandler<ObjectPropertySetEventArgs<T>> onSet)
        {
            if (IsDisposed)
            {
                return false;
            }

            lock (this)
            {
                bool hasChanges = false;
                NamedProperty propHolder = GetCore(key, propertyName);
                T oldValue = default;

                if (propHolder == null)
                {
                    propHolder = NamedPropertyFactory(key, propertyName, group);
                    if (propHolder != null)
                    {
                        if (validate?.Invoke((oldValue, value)) ?? true)
                        {
                            WireNamedProperty(propHolder);
                            AddCore(propHolder);
                            propHolder.Property.SetValue(value);
                            hasChanges = true;
                        }
                    }
                }
                else
                {
                    propHolder.Property.SyncOperation.SetContext(this);

                    oldValue = propHolder.Property.GetValue<T>();

                    if (validate?.Invoke((oldValue, value)) ?? true)
                    {
                        if (propHolder.Group != group)
                        {
                            propHolder.Group = group;
                            hasChanges = true;
                        }

                        if (propHolder.PropertyName != propertyName)
                        {
                            propHolder.PropertyName = propertyName;
                            hasChanges = true;
                        }

                        var hasSetChanges = false;

                        if (propHolder.Property.SetValue(value))
                        {
                            hasSetChanges = true;
                            hasChanges = true;
                        }

                        if (!hasSetChanges && hasChanges)
                        {
                            OnPropertyChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
                        }
                    }
                }

                onSet?.Invoke(this, new ObjectPropertySetEventArgs<T>(key, propertyName, group, oldValue, hasChanges ? value : oldValue, hasChanges));

                return hasChanges;
            }
        }

        private T GetPropertyInternal<T>(
            T defaultValue,
            string key,
            string propertyName,
            string group,
            Func<(T oldValue, T newValue), bool> validate,
            EventHandler<ObjectPropertySetEventArgs<T>> onSet)
        {
            if (IsDisposed)
            {
                return defaultValue;
            }

            lock (this)
            {
                bool hasChanges = false;
                NamedProperty propHolder = GetCore(key, propertyName);
                T oldValue = default;

                if (propHolder == null)
                {
                    propHolder = NamedPropertyFactory(key, propertyName, group);
                    if (propHolder != null)
                    {
                        if (validate?.Invoke((oldValue, defaultValue)) ?? true)
                        {
                            WireNamedProperty(propHolder);
                            AddCore(propHolder);
                            propHolder.Property.SetValue(defaultValue);
                            hasChanges = true;
                        }
                    }
                }
                else
                {
                    propHolder.Property.SyncOperation.SetContext(this);

                    oldValue = propHolder.Property.GetValue<T>();

                    if (validate?.Invoke((oldValue, defaultValue)) ?? true)
                    {
                        if (propHolder.Group != group)
                        {
                            propHolder.Group = group;
                            hasChanges = true;
                        }

                        if (propHolder.PropertyName != propertyName)
                        {
                            propHolder.PropertyName = propertyName;
                            hasChanges = true;
                        }

                        if (hasChanges)
                        {
                            OnPropertyChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
                        }
                    }
                }

                onSet?.Invoke(this, new ObjectPropertySetEventArgs<T>(key, propertyName, group, oldValue, hasChanges ? defaultValue : oldValue, hasChanges));

                if (propHolder == null)
                {
                    return defaultValue;
                }
                else
                {
                    return propHolder.Property.GetValue(defaultValue);
                }
            }
        }

        #endregion
    }
}
