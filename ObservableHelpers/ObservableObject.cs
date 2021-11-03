using ObservableHelpers.Exceptions;
using ObservableHelpers.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ObservableHelpers
{
    /// <summary>
    /// Provides a thread-safe observable object class for use with data binding.
    /// </summary>
    public abstract class ObservableObject :
        ObservableSyncContext
    {
        #region Properties

        private readonly Dictionary<NamedPropertyKey, NamedProperty> namedProperties = new Dictionary<NamedPropertyKey, NamedProperty>();

        private readonly RWLock rwLock = new RWLock();

        #endregion

        #region Initializers

        /// <summary>
        /// Creates new instance of the <see cref="ObservableObject"/> class.
        /// </summary>
        public ObservableObject()
        {
            InitializeProperties();
        }

        /// <summary>
        /// Creates new instance of the <see cref="ObservableObject"/> class.
        /// </summary>
        /// <param name="prePropertiesInitialization">
        /// Action that will be executed before properties initialization.
        /// </param>
        public ObservableObject(Action prePropertiesInitialization)
        {
            prePropertiesInitialization?.Invoke();
            InitializeProperties();
        }

        private void InitializeProperties()
        {
            foreach (PropertyInfo property in GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                try
                {
                    property.GetValue(this);
                }
                catch { }
            }
        }

        #endregion

        #region Methods

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
        /// <param name="setValidate">
        /// The value set validator function.
        /// </param>
        /// <param name="postAction">
        /// The callback after operation.
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
            Func<(T oldValue, T newValue), bool> setValidate = null,
            Action<(string key, string propertyName, string group, T oldValue, T newValue, bool HasChanges)> postAction = null)
        {
            return SetProperty(value, null, propertyName, group, setValidate, postAction);
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
        /// <param name="setValidate">
        /// The value set validator function.
        /// </param>
        /// <param name="postAction">
        /// The callback after operation.
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
            Func<(T oldValue, T newValue), bool> setValidate = null,
            Action<(string key, string propertyName, string group, T oldValue, T newValue, bool HasChanges)> postAction = null)
        {
            return SetProperty(value, key, propertyName, group, setValidate, postAction);
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
        /// <param name="setValidate">
        /// The value set validator function.
        /// </param>
        /// <param name="postAction">
        /// The callback after operation.
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
            Func<(T oldValue, T newValue), bool> setValidate = null,
            Action<(string key, string propertyName, string group, T oldValue, T newValue, bool HasChanges)> postAction = null)
        {
            return GetProperty(defaultValue, null, propertyName, group, setValidate, postAction);
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
        /// <param name="setValidate">
        /// The value set validator function.
        /// </param>
        /// <param name="postAction">
        /// The callback after operation.
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
            Func<(T oldValue, T newValue), bool> setValidate = null,
            Action<(string key, string propertyName, string group, T oldValue, T newValue, bool HasChanges)> postAction = null)
        {
            return GetProperty(defaultValue, key, propertyName, group, setValidate, postAction);
        }

        /// <summary>
        /// Removes a property with the provided <paramref name="propertyName"/>
        /// </summary>
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
            return RemoveProperty(null, propertyName);
        }

        /// <summary>
        /// Removes a property with the provided <paramref name="key"/>
        /// </summary>
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
            return RemoveProperty(key, null);
        }

        /// <summary>
        /// Checks if property exists with the provided <paramref name="propertyName"/>.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property to find.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the property exists; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="PropertyKeyAndNameNullException">
        /// Throws when <paramref name="propertyName"/> is not provided.
        /// </exception>
        protected bool ContainsProperty(string propertyName)
        {
            return ContainsProperty(null, propertyName);
        }

        /// <summary>
        /// Checks if property exists with the provided <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key of the property to find.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the property exists; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="PropertyKeyAndNameNullException">
        /// Throws when <paramref name="key"/> is not provided.
        /// </exception>
        protected bool ContainsPropertyWithKey(string key)
        {
            return ContainsProperty(key, null);
        }

        /// <summary>
        /// Gets the raw properties <see cref="NamedProperty"/> from the property collection with the provided <paramref name="group"/>.
        /// </summary>
        /// <param name="group">
        /// The group of the properties to get. If provided with a <c>null</c> group, all properties will be returned.
        /// </param>
        /// <returns>
        /// The raw properties.
        /// </returns>
        protected IEnumerable<NamedProperty> GetRawProperties(string group = null)
        {
            return rwLock.LockRead(() =>
            {
                return group == null ?
                    namedProperties.Values.ToList() :
                    namedProperties.Values.Where(i => i.Group == group).ToList();
            });
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
                if (e.PropertyName == nameof(namedProperty.Property.Value))
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
        /// <param name="namedProperty">
        /// The found <see cref="NamedProperty"/> from the property collection.
        /// </param>
        /// <exception cref="PropertyKeyAndNameNullException">
        /// Throws when both <paramref name="key"/> and <paramref name="propertyName"/> are not provided.
        /// </exception>
        protected bool TryGetNamedProperty(string key, string propertyName, out NamedProperty namedProperty)
        {
            if (key == null && propertyName == null)
            {
                throw new PropertyKeyAndNameNullException();
            }

            namedProperty = default;
            NamedProperty proxy = default;
            var ret = rwLock.LockRead(() => namedProperties.TryGetValue(new NamedPropertyKey(key, propertyName), out proxy));
            namedProperty = proxy;
            return ret;
        }

        /// <summary>
        /// The core implementation for getting the <see cref="NamedProperty"/> from the property collection with the provided <paramref name="key"/> and <paramref name="propertyName"/>, or adds a value to the collection by using the specified function if the property does not already exist.
        /// </summary>
        /// <remarks>
        /// Both <paramref name="key"/> and <paramref name="propertyName"/> should not be null. If both are provided, <paramref name="key"/> will be used to find the property.
        /// </remarks>
        /// <param name="defaultValue">
        /// The default value if the property is null.
        /// </param>
        /// <param name="key">
        /// The key of the property to get.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property to get.
        /// </param>
        /// <param name="group">
        /// The group of the property to get or add.
        /// </param>
        /// <param name="createValidation">
        /// The function validation if create property is executed.
        /// </param>
        /// <param name="postAction">
        /// The action after the get or add operation.
        /// </param>
        /// <returns>
        /// The found <see cref="NamedProperty"/> from the property collection.
        /// </returns>
        /// <exception cref="PropertyKeyAndNameNullException">
        /// Throws when both <paramref name="key"/> and <paramref name="propertyName"/> are not provided.
        /// </exception>
        protected NamedProperty GetOrCreateNamedProperty<T>(
            T defaultValue,
            string key,
            string propertyName,
            string group,
            Func<(NamedProperty namedProperty, T oldValue), bool> createValidation = null,
            Action<(NamedProperty namedProperty, T oldValue, T newValue, bool hasChanges)> postAction = null)
        {
            return CreateOrUpdateNamedProperty(defaultValue, key, propertyName, group, createValidation, args => false, postAction);
        }

        /// <summary>
        /// Adds or updates a property to the property collection.
        /// </summary>
        /// <param name="value">
        /// The value of the property to add or update.
        /// </param>
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
        /// <returns>
        /// The added or updated <see cref="NamedProperty"/> from the property collection.
        /// </returns>
        /// <exception cref="PropertyKeyAndNameNullException">
        /// Throws when both <paramref name="key"/> and <paramref name="propertyName"/> are not provided.
        /// </exception>
        protected NamedProperty CreateOrUpdateNamedProperty<T>(
            T value,
            string key,
            string propertyName,
            string group,
            Func<(NamedProperty namedProperty, T oldValue), bool> addValidate = null,
            Func<(NamedProperty namedProperty, T oldValue), bool> updateValidate = null,
            Action<(NamedProperty namedProperty, T oldValue, T newValue, bool hasChanges)> postAction = null)
        {
            if (key == null && propertyName == null)
            {
                throw new PropertyKeyAndNameNullException();
            }

            T oldValue = default;
            T newValue = default;
            bool hasChanges = false;
            NamedPropertyKey namedPropertyKey = new NamedPropertyKey(key, propertyName);
            return rwLock.LockRead(() =>
            {
                if (namedProperties.TryGetValue(namedPropertyKey, out NamedProperty namedProperty))
                {
                    oldValue = namedProperty.Property.GetValue<T>();
                    newValue = oldValue;
                    rwLock.LockWrite(() =>
                    {
                        if (key != null && namedProperty.Key != key)
                        {
                            namedProperty.Key = key;
                            hasChanges = true;
                        }

                        if (propertyName != null && namedProperty.PropertyName != propertyName)
                        {
                            namedProperty.PropertyName = propertyName;
                            hasChanges = true;
                        }

                        if (group != null && namedProperty.Group != group)
                        {
                            namedProperty.Group = group;
                            hasChanges = true;
                        }

                        if (updateValidate?.Invoke((namedProperty, oldValue)) ?? true)
                        {
                            if (namedProperty.Property.SetValue(value))
                            {
                                newValue = value;
                                hasChanges = true;
                            }
                            else if (hasChanges)
                            {
                                OnPropertyChanged(namedProperty.Key, namedProperty.PropertyName, namedProperty.Group);
                            }
                        }
                        else if (hasChanges)
                        {
                            OnPropertyChanged(namedProperty.Key, namedProperty.PropertyName, namedProperty.Group);
                        }
                    });
                }
                else
                {
                    rwLock.LockWrite(() =>
                    {
                        namedProperty = NamedPropertyFactory(key, propertyName, group);
                        if (namedProperty != null)
                        {
                            if (addValidate?.Invoke((namedProperty, oldValue)) ?? true)
                            {
                                namedPropertyKey.Update(namedProperty);
                                namedProperties.Add(namedPropertyKey, namedProperty);
                                WireNamedProperty(namedProperty);
                                if (!namedProperty.Property.SetValue(value))
                                {
                                    OnPropertyChanged(namedProperty.Key, namedProperty.PropertyName, namedProperty.Group);
                                }
                                newValue = value;
                                hasChanges = true;
                            }
                        }
                    });
                }

                postAction?.Invoke((namedProperty, oldValue, newValue, hasChanges));

                return namedProperty;
            });
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
        protected void OnPropertyChangedWithKey(string key)
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
        protected void OnPropertyChangedWithName(string name)
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
            return new NamedProperty()
            {
                Property = new ObservableProperty(),
                Key = key,
                PropertyName = propertyName,
                Group = group
            };
        }

        private bool SetProperty<T>(
            T value,
            string key,
            string propertyName,
            string group,
            Func<(T oldValue, T newValue), bool> setValidate,
            Action<(string key, string propertyName, string group, T oldValue, T newValue, bool HasChanges)> postAction)
        {
            bool hasChanges = false;

            CreateOrUpdateNamedProperty(value, key, propertyName, group,
                args =>
                {
                    bool validated = setValidate?.Invoke((args.oldValue, value)) ?? true;
                    if (validated && args.namedProperty.IsDefault)
                    {
                        args.namedProperty.IsDefault = false;
                    }
                    return validated;
                },
                args =>
                {
                    bool validated = setValidate?.Invoke((args.oldValue, value)) ?? true;
                    if (validated && args.namedProperty.IsDefault)
                    {
                        args.namedProperty.IsDefault = false;
                    }
                    return validated;
                },
                subPostAction =>
                {
                    hasChanges = subPostAction.hasChanges;

                    postAction?.Invoke((
                        subPostAction.namedProperty.Key,
                        subPostAction.namedProperty.PropertyName,
                        subPostAction.namedProperty.Group,
                        subPostAction.oldValue,
                        subPostAction.newValue,
                        subPostAction.hasChanges));
                });


            return hasChanges;
        }

        private T GetProperty<T>(
            T defaultValue,
            string key,
            string propertyName,
            string group,
            Func<(T oldValue, T newValue), bool> setValidate,
            Action<(string key, string propertyName, string group, T oldValue, T newValue, bool HasChanges)> postAction)
        {
            T returnValue = default;

            NamedProperty namedProperty = GetOrCreateNamedProperty(defaultValue, key, propertyName, group,
                args =>
                {
                    bool validated = setValidate?.Invoke((args.oldValue, defaultValue)) ?? true;
                    if (validated && !args.namedProperty.IsDefault)
                    {
                        args.namedProperty.IsDefault = true;
                    }
                    return validated;
                },
                subPostAction =>
                {
                    returnValue = subPostAction.newValue;

                    postAction?.Invoke((
                        subPostAction.namedProperty.Key,
                        subPostAction.namedProperty.PropertyName,
                        subPostAction.namedProperty.Group,
                        subPostAction.oldValue,
                        subPostAction.newValue,
                        subPostAction.hasChanges));
                });

            return returnValue;
        }

        private bool RemoveProperty(string key, string propertyName)
        {
            if (key == null && propertyName == null)
            {
                throw new PropertyKeyAndNameNullException();
            }

            return rwLock.LockRead(() =>
            {
                var namedPropertyKey = new NamedPropertyKey(key, propertyName);
                if (namedProperties.TryGetValue(namedPropertyKey, out NamedProperty removedProperty))
                {
                    if (rwLock.LockWrite(() => namedProperties.Remove(namedPropertyKey)))
                    {
                        OnPropertyChanged(removedProperty.Key, removedProperty.PropertyName, removedProperty.Group);
                        return true;
                    }
                }
                return false;
            });
        }

        private bool ContainsProperty(string key, string propertyName)
        {
            if (key == null && propertyName == null)
            {
                throw new PropertyKeyAndNameNullException();
            }

            return rwLock.LockRead(() => namedProperties.ContainsKey(new NamedPropertyKey(key, propertyName)));
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
            if (IsDisposed)
            {
                return default;
            }

            return namedProperties.All(i => i.Value.Property.IsNull());
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Provides the property with key, name and group.
        /// </summary>
        public class NamedProperty
        {
            #region Properties

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

            #endregion

            #region Initializers

            /// <summary>
            /// Creates new instance for <see cref="NamedProperty"/>
            /// </summary>
            public NamedProperty()
            {

            }

            #endregion

            #region Object Members

            /// <inheritdoc/>
            public override string ToString()
            {
                return "(" + (Key ?? "null") + ", " + (PropertyName ?? "null") + ", " + (Group ?? "null") + ") = " + (Property?.Value?.ToString() ?? "null");
            }

            #endregion
        }

        private class NamedPropertyKey
        {
            #region Properties

            public string Key => property == null ? key : property.Key;

            public string PropertyName => property == null ? propertyName : property.PropertyName;

            private readonly string key;
            private readonly string propertyName;
            private NamedProperty property;

            #endregion

            #region Initializers

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

            #endregion

            #region Methods

            internal void Update(NamedProperty property)
            {
                this.property = property;
            }

            #endregion

            #region Object Members

            public override bool Equals(object obj)
            {
                if (obj is NamedPropertyKey namedPropertyKey)
                {
                    return Key == null && namedPropertyKey.Key == null ?
                        PropertyName == namedPropertyKey.PropertyName :
                        Key == namedPropertyKey.Key;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return 990326508 + (Key == null ? 0 : EqualityComparer<string>.Default.GetHashCode(Key));
            }

            #endregion
        }

        /// <summary>
        /// The <see cref="PropertyChangedEventArgs"/> derived implementations for <see cref="INotifyPropertyChanged.PropertyChanged"/> events
        /// </summary>
        public class ObjectPropertyChangesEventArgs :
            PropertyChangedEventArgs
        {
            #region Properties

            /// <summary>
            /// Gets the key of the property that changed.
            /// </summary>
            public string Key { get; }

            /// <summary>
            /// Gets the group of the property that changed.
            /// </summary>
            public string Group { get; }

            #endregion

            #region Initializers

            internal ObjectPropertyChangesEventArgs(
                string key,
                string propertyName,
                string group)
                : base(propertyName)
            {
                Key = key;
                Group = group;
            }

            #endregion
        }

        #endregion
    }
}
