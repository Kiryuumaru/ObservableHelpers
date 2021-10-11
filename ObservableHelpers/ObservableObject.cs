using ObservableHelpers.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ObservableHelpers
{
    /// <summary>
    /// Provides a thread-safe observable object class for use with data binding.
    /// </summary>
    public class ObservableObject : ObservableSyncContext
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

            /// <summary>
            /// Gets or sets <c>true</c> if the property is from default value; otherwise <c>false</c>.
            /// </summary>
            public bool IsDefault { get; set; }

            /// <inheritdoc/>
            public override string ToString()
            {
                return "(" + (Key ?? "null") + ", " + (PropertyName ?? "null") + ", " + (Group ?? "null") + ") = " + (Property?.Property?.ToString() ?? "null");
            }
        }

        private struct NamedPropertyKey
        {
            public string Key => property == null ? key : property.Key;

            public string PropertyName => property == null ? propertyName : property.PropertyName;

            private readonly string key;
            private readonly string propertyName;
            private NamedProperty property;

            public NamedPropertyKey(string key, string propertyName)
            {
                this.key = key;
                this.propertyName = propertyName;
                property = null;
            }

            public NamedPropertyKey(NamedProperty property)
            {
                key = null;
                propertyName = null;
                this.property = property;
            }

            internal void Update(NamedProperty property)
            {
                this.property = property;
            }

            public override bool Equals(object obj)
            {
                return obj is NamedPropertyKey namedPropertyKey
                    && (Key == null && namedPropertyKey.Key == null ? PropertyName == namedPropertyKey.PropertyName : Key == namedPropertyKey.Key);
            }

            public override int GetHashCode()
            {
                return 990326508 + (Key == null ? 0 : EqualityComparer<string>.Default.GetHashCode(Key));
            }
        }

        #endregion

        #region Properties

        private readonly ConcurrentDictionary<NamedPropertyKey, NamedProperty> namedProperties = new ConcurrentDictionary<NamedPropertyKey, NamedProperty>();

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

            bool hasChanges = false;
            foreach (KeyValuePair<NamedPropertyKey, NamedProperty> propHolder in namedProperties)
            {
                if (propHolder.Value.Property.SetNull())
                {
                    hasChanges = true;
                }
            }
            return hasChanges;
        }

        /// <inheritdoc/>
        public override bool IsNull()
        {
            return IsDisposed || namedProperties.All(i => i.Value.Property.IsNull());
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

            foreach (PropertyInfo property in GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                try
                {
                    _ = property.GetValue(this);
                }
                catch { }
            }
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
        /// The value set validator function.
        /// </param>
        /// <param name="onSet">
        /// The callback after set operation.
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
            Action<ObjectPropertySetEventArgs<T>> onSet = null)
        {
            return !IsDisposed && SetProperty(value, null, propertyName, group, validate, onSet);
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
        /// The value set validator function.
        /// </param>
        /// <param name="onSet">
        /// The callback after set operation.
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
            Action<ObjectPropertySetEventArgs<T>> onSet = null)
        {
            return !IsDisposed && SetProperty(value, key, propertyName, group, validate, onSet);
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
        /// The value set validator function.
        /// </param>
        /// <param name="onSet">
        /// The callback after set operation.
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
            Action<ObjectPropertySetEventArgs<T>> onSet = null)
        {
            return IsDisposed ? defaultValue : GetProperty(defaultValue, null, propertyName, group, validate, onSet);
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
        /// The value set validator function.
        /// </param>
        /// <param name="onSet">
        /// The callback after set operation.
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
            Action<ObjectPropertySetEventArgs<T>> onSet = null)
        {
            return IsDisposed ? defaultValue : GetProperty(defaultValue, key, propertyName, group, validate, onSet);
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
            return !IsDisposed && RemovePropertyCore(null, propertyName);
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
            return !IsDisposed && RemovePropertyCore(key, null);
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
            return !IsDisposed && DeletePropertyCore(null, propertyName);
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
            return !IsDisposed && DeletePropertyCore(key, null);
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

            PropertyChangedEventHandler eventProxy = new PropertyChangedEventHandler((s, e) =>
            {
                if (IsDisposed)
                {
                    return;
                }
                if (e.PropertyName == nameof(namedProperty.Property.Property))
                {
                    OnPropertyChanged(namedProperty.Key, namedProperty.PropertyName, namedProperty.Group);
                }
            });

            if (namedProperty.Property != null)
            {
                namedProperty.Property.ImmediatePropertyChanged += eventProxy;
            }
            Disposing += (s, e) =>
            {
                if (namedProperty.Property != null)
                {
                    namedProperty.Property.ImmediatePropertyChanged -= eventProxy;
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
            return IsDisposed
                ? default
                : (IEnumerable<NamedProperty>)(group == null ? namedProperties.Values.ToList() : namedProperties.Values.Where(i => i.Group == group).ToList());
        }

        /// <summary>
        /// The core implementation for getting the <see cref="NamedProperty"/> from the property collection with the provided <paramref name="key"/> and <paramref name="propertyName"/>, or adds a value to the collection by using the specified function if the property does not already exist.
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
        /// <param name="group">
        /// The group of the property to get or add.
        /// </param>
        /// <param name="addValidate">
        /// The function validation if add property is executed.
        /// </param>
        /// <param name="postAction">
        /// The action after the get or add operation.
        /// </param>
        /// <returns>
        /// The found <see cref="NamedProperty"/> from the property collection.
        /// </returns>
        protected NamedProperty GetOrAddCore(
            string key,
            string propertyName,
            string group,
            Func<NamedProperty, bool> addValidate = null,
            Action<(NamedProperty namedProperty, bool isAdded)> postAction = null)
        {
            if (IsDisposed)
            {
                return null;
            }

            bool isAdded = false;
            NamedPropertyKey namedPropertyKey = new NamedPropertyKey(key, propertyName);
            if (!namedProperties.TryGetValue(namedPropertyKey, out NamedProperty namedProperty))
            {
                namedProperty = NamedPropertyFactory(key, propertyName, group);
                if (namedProperty != null)
                {
                    if (addValidate?.Invoke(namedProperty) ?? true)
                    {
                        namedPropertyKey.Update(namedProperty);
                        _ = namedProperties.TryAdd(namedPropertyKey, namedProperty);
                        WireNamedProperty(namedProperty);
                        isAdded = true;
                    }
                }
            }

            postAction?.Invoke((namedProperty, isAdded));

            return namedProperty;
        }

        /// <summary>
        /// Adds or updates a property to the property collection.
        /// </summary>
        /// <param name="key">
        /// The key of the property to add or update.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property to add or update.
        /// </param>
        /// <param name="group">
        /// The group of the property to add or update.
        /// </param>
        /// <param name="addValidate">
        /// The function validation if add property is executed.
        /// </param>
        /// <param name="updateValidate">
        /// The function validation if update property is executed.
        /// </param>
        /// <param name="postAction">
        /// The action after the add or update operation.
        /// </param>
        protected void AddOrUpdatePropertyCore(
            string key,
            string propertyName,
            string group,
            Func<NamedProperty, bool> addValidate = null,
            Func<NamedProperty, bool> updateValidate = null,
            Action<(NamedProperty namedProperty, bool isUpdate, bool hasChanges)> postAction = null)
        {
            if (IsDisposed)
            {
                return;
            }

            bool hasChanges = false;
            bool isUpdate = false;
            NamedProperty namedProperty = GetOrAddCore(key, propertyName, group,
                addValidate,
                subPostAction =>
                {
                    hasChanges = subPostAction.isAdded;

                    if (!subPostAction.isAdded && subPostAction.namedProperty != null)
                    {
                        subPostAction.namedProperty.Property.SyncOperation.SetContext(this);
                        if (updateValidate?.Invoke(subPostAction.namedProperty) ?? true)
                        {
                            if (key != null && subPostAction.namedProperty.Key != key)
                            {
                                subPostAction.namedProperty.Key = key;
                                hasChanges = true;
                            }

                            if (propertyName != null && subPostAction.namedProperty.PropertyName != propertyName)
                            {
                                subPostAction.namedProperty.PropertyName = propertyName;
                                hasChanges = true;
                            }

                            if (group != null && subPostAction.namedProperty.Group != group)
                            {
                                subPostAction.namedProperty.Group = group;
                                hasChanges = true;
                            }

                            isUpdate = true;
                        }
                    }
                });

            postAction?.Invoke((namedProperty, isUpdate, hasChanges));
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
            NamedPropertyKey key = new NamedPropertyKey(namedProperty);
            if (namedProperties.ContainsKey(key))
            {
                exists = true;
            }
            else
            {
                _ = namedProperties.TryAdd(key, namedProperty);
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
            return IsDisposed
                ? default
                : key == null && propertyName == null
                ? throw new PropertyKeyAndNameNullException()
                : namedProperties.TryGetValue(new NamedPropertyKey(key, propertyName), out NamedProperty namedProperty) ? namedProperty : null;
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
            return !IsDisposed
&& (key == null && propertyName == null
                ? throw new PropertyKeyAndNameNullException()
                : namedProperties.TryRemove(new NamedPropertyKey(key, propertyName), out _));
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
            return !IsDisposed
                && (key == null && propertyName == null
                ? throw new PropertyKeyAndNameNullException()
                : namedProperties.TryGetValue(new NamedPropertyKey(key, propertyName), out NamedProperty propHolder)
                && propHolder.Property.SetNull());
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
            return !IsDisposed
                && (key == null && propertyName == null
                ? throw new PropertyKeyAndNameNullException()
                : namedProperties.ContainsKey(new NamedPropertyKey(key, propertyName)));
        }

        /// <summary>
        /// Virtual factory used to create all properties.
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
            return IsDisposed
                ? null
                : new NamedProperty()
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
        protected void OnPropertyChanged(string key, string propertyName, string group)
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

            if (namedProperties.TryGetValue(new NamedPropertyKey(key, null), out NamedProperty namedProperty))
            {
                OnPropertyChanged(namedProperty.Key, namedProperty.PropertyName, namedProperty.Group);
            }
        }

        /// <summary>
        /// Invokes <see cref="INotifyPropertyChanged.PropertyChanged"/> into the current context if the provided <paramref name="name"/> is found in this object.
        /// </summary>
        /// <param name="name">
        /// The name of the property changed.
        /// </param>
        protected virtual void OnPropertyChangedWithName(string name)
        {
            if (IsDisposed)
            {
                return;
            }

            if (namedProperties.TryGetValue(new NamedPropertyKey(null, name), out NamedProperty namedProperty))
            {
                OnPropertyChanged(namedProperty.Key, namedProperty.PropertyName, namedProperty.Group);
            }
            else
            {
                OnPropertyChanged(new PropertyChangedEventArgs(name));
            }
        }

        private bool SetProperty<T>(
            T value,
            string key,
            string propertyName,
            string group,
            Func<(T oldValue, T newValue), bool> validate,
            Action<ObjectPropertySetEventArgs<T>> onSet)
        {
            if (IsDisposed)
            {
                return false;
            }

            T oldValue = default;
            bool hasChanges = false;
            AddOrUpdatePropertyCore(key, propertyName, group,
                newNamedProperty => validate?.Invoke((oldValue, value)) ?? true,
                existingNamedProperty =>
                {
                    oldValue = existingNamedProperty.Property.GetValue<T>();
                    return validate?.Invoke((oldValue, value)) ?? true;
                },
                postMake =>
                {
                    hasChanges = postMake.hasChanges;

                    postMake.namedProperty.IsDefault = false;

                    if (postMake.isUpdate)
                    {
                        bool hasSetChanges = false;

                        if (postMake.namedProperty.Property.SetValue(value))
                        {
                            hasSetChanges = true;
                            hasChanges = true;
                        }

                        if (!hasSetChanges && hasChanges)
                        {
                            OnPropertyChanged(postMake.namedProperty.Key, postMake.namedProperty.PropertyName, postMake.namedProperty.Group);
                        }
                    }
                    else if (postMake.namedProperty != null)
                    {
                        _ = postMake.namedProperty.Property.SetValue(value);
                    }
                });

            onSet?.Invoke(new ObjectPropertySetEventArgs<T>(key, propertyName, group, oldValue, hasChanges ? value : oldValue, hasChanges));

            return hasChanges;
        }

        private T GetProperty<T>(
            T defaultValue,
            string key,
            string propertyName,
            string group,
            Func<(T oldValue, T newValue), bool> validate,
            Action<ObjectPropertySetEventArgs<T>> onSet)
        {
            if (IsDisposed)
            {
                return defaultValue;
            }

            bool hasChanges = false;
            T oldValue = default;
            T returnValue = defaultValue;
            AddOrUpdatePropertyCore(key, propertyName, group,
                newNamedProperty => validate?.Invoke((oldValue, defaultValue)) ?? true,
                existingNamedProperty =>
                {
                    oldValue = existingNamedProperty.Property.GetValue<T>();
                    return validate?.Invoke((oldValue, defaultValue)) ?? true;
                },
                postMake =>
                {
                    hasChanges = postMake.hasChanges;

                    if (postMake.isUpdate)
                    {
                        if (hasChanges)
                        {
                            OnPropertyChanged(postMake.namedProperty.Key, postMake.namedProperty.PropertyName, postMake.namedProperty.Group);
                        }

                        returnValue = postMake.namedProperty.Property.GetValue(defaultValue);
                    }
                    else if (postMake.namedProperty != null)
                    {
                        postMake.namedProperty.IsDefault = true;
                        _ = postMake.namedProperty.Property.SetValue(defaultValue);
                    }
                });

            onSet?.Invoke(new ObjectPropertySetEventArgs<T>(key, propertyName, group, oldValue, hasChanges ? defaultValue : oldValue, hasChanges));

            return returnValue;
        }

        #endregion
    }
}
