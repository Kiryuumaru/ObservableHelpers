using DisposableHelpers;
using ObservableHelpers.Exceptions;
using ObservableHelpers.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ObservableHelpers
{
    /// <summary>
    /// Provides a thread-safe observable object class for use with data binding.
    /// </summary>
    public abstract class ObservableObject :
        ObservableSyncContext
    {
        #region Properties

        private readonly Dictionary<int, NamedProperty> namedProperties = new();
        private readonly Dictionary<string, int> keyDictionary = new();
        private readonly Dictionary<string, int> propertyNameDictionary = new();
        private readonly Random random = new();

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

        /// <summary>
        /// Initializes properties with their default values by invoking get property method.
        /// </summary>
        protected void InitializeProperties()
        {
            foreach (PropertyInfo property in GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    property.GetValue(this);
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
        /// <param name="key">
        /// The key of the property value to set.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property to set.
        /// </param>
        /// <param name="groups">
        /// The groups of the property to set.
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
        protected bool SetProperty<T>(
            T value,
            string? key = null,
            [CallerMemberName] string? propertyName = null,
            string[]? groups = null,
            Func<bool>? setValidate = null,
            Action<(string? key, string? propertyName, string[]? groups, T? oldValue, T? newValue, bool hasChanges)>? postAction = null)
        {
            bool hasChanges = false;

            CreateOrUpdateNamedProperty(key, propertyName, groups,
                () => value,
                args =>
                {
                    bool validated = setValidate?.Invoke() ?? true;
                    if (validated && args.namedProperty.IsDefault)
                    {
                        args.namedProperty.IsDefault = false;
                    }
                    return validated;
                },
                args =>
                {
                    bool validated = setValidate?.Invoke() ?? true;
                    if (validated && args.namedProperty.IsDefault)
                    {
                        args.namedProperty.IsDefault = false;
                    }
                    return validated;
                },
                args =>
                {
                    hasChanges = args.hasChanges;

                    postAction?.Invoke((
                        args.namedProperty.Key,
                        args.namedProperty.PropertyName,
                        args.namedProperty.Groups,
                        args.oldValue,
                        args.newValue,
                        args.hasChanges));
                });


            return hasChanges;
        }

        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <typeparam name="T">
        /// The underlying type of the property to get.
        /// </typeparam>
        /// <param name="key">
        /// The key of the property value to get.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property value to get.
        /// </param>
        /// <param name="groups">
        /// The groups of the property value to get.
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
        protected T? GetProperty<T>(
            string? key = null,
            [CallerMemberName] string? propertyName = null,
            string[]? groups = null,
            Action<(string? key, string? propertyName, string[]? groups, T? oldValue, T? newValue, bool hasChanges)>? postAction = null)
        {
            (NamedProperty namedProperty, T? returnValue) = CreateOrUpdateNamedProperty<T?>(key, propertyName, groups,
                () => default,
                args =>
                {
                    if (!args.namedProperty.IsDefault)
                    {
                        args.namedProperty.IsDefault = true;
                    }
                    return true;
                },
                args => false,
                args =>
                {
                    postAction?.Invoke((
                        args.namedProperty.Key,
                        args.namedProperty.PropertyName,
                        args.namedProperty.Groups,
                        args.oldValue,
                        args.newValue,
                        args.hasChanges));
                });

            return returnValue;
        }

        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <typeparam name="T">
        /// The underlying type of the property to get.
        /// </typeparam>
        /// <param name="defaultValueFactory">
        /// The default value factory that sets and returned if the property is null.
        /// </param>
        /// <param name="key">
        /// The key of the property value to get.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property value to get.
        /// </param>
        /// <param name="groups">
        /// The groups of the property value to get.
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
        protected T GetProperty<T>(
            Func<T> defaultValueFactory,
            string? key = null,
            [CallerMemberName] string? propertyName = null,
            string[]? groups = null,
            Action<(string? key, string? propertyName, string[]? groups, T? oldValue, T newValue, bool hasChanges)>? postAction = null)
        {
            (NamedProperty namedProperty, T returnValue) = GetOrAddNamedProperty(key, propertyName, groups,
                defaultValueFactory,
                args =>
                {
                    postAction?.Invoke((
                        args.namedProperty.Key,
                        args.namedProperty.PropertyName,
                        args.namedProperty.Groups,
                        args.oldValue,
                        args.newValue,
                        args.hasChanges));
                });

            return returnValue;
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
        protected IEnumerable<NamedProperty> GetRawProperties(string? group = null)
        {
            return RWLock.LockRead(() =>
            {
                return group == null ?
                    namedProperties.Values.ToList() :
                    namedProperties.Values.Where(i => i.Groups?.Contains(group) ?? false).ToList();
            });
        }

        /// <summary>
        /// Gets the raw properties <see cref="NamedProperty"/> from the property collection with the provided <paramref name="groupPredicate"/>.
        /// </summary>
        /// <param name="groupPredicate">
        /// The groups predicate used to filter the properties to get.
        /// </param>
        /// <returns>
        /// The raw properties.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="groupPredicate"/> is a <c>null</c> reference.
        /// </exception>
        protected IEnumerable<NamedProperty> GetRawProperties(Func<NamedProperty, bool> groupPredicate)
        {
            if (groupPredicate == null)
            {
                throw new ArgumentNullException(nameof(groupPredicate));
            }

            return RWLock.LockRead(namedProperties.Values.Where(i => groupPredicate.Invoke(i)).ToList);
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

            PropertyChangedEventHandler eventProxy = new((s, e) =>
            {
                if (IsDisposed)
                {
                    return;
                }
                if (e.PropertyName == nameof(namedProperty.Property.Value))
                {
                    OnPropertyChanged(namedProperty.Key, namedProperty.PropertyName, namedProperty.Groups);
                }
            });

            if (namedProperty.Property != null)
            {
                namedProperty.Property.PropertyChanged += eventProxy;
            }
            Disposing += (s, e) =>
            {
                if (namedProperty.Property != null)
                {
                    namedProperty.Property.PropertyChanged -= eventProxy;
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
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        protected bool TryGetNamedProperty(string? key, string? propertyName, [MaybeNullWhen(false)] out NamedProperty namedProperty)
#else
        protected bool TryGetNamedProperty(string? key, string? propertyName, out NamedProperty? namedProperty)
#endif
        {
            if (key == null && propertyName == null)
            {
                throw new PropertyKeyAndNameNullException();
            }

            namedProperty = default;
            NamedProperty? proxy = default;
            var ret = RWLock.LockRead(() =>
            {
                if (key != null && keyDictionary.TryGetValue(key, out int namedPropertyKey))
                {
                    proxy = namedProperties[namedPropertyKey];
                    return true;
                }
                else if (propertyName != null && propertyNameDictionary.TryGetValue(propertyName, out namedPropertyKey))
                {
                    proxy = namedProperties[namedPropertyKey];
                    return true;
                }

                return false;
            });
            namedProperty = proxy;
            return ret;
        }

        /// <summary>
        /// The core implementation for creating the <see cref="NamedProperty"/> from the property collection with the provided <paramref name="key"/> and <paramref name="propertyName"/>, or updates a value to the collection by using the specified function if the property already exist.
        /// </summary>
        /// <param name="key">
        /// The key of the property to add or update.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property to add or update.
        /// </param>
        /// <param name="groups">
        /// The groups of the property to add or update.
        /// </param>
        /// <param name="valueFactory">
        /// The value factory of the property to add or update.
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
        protected (NamedProperty namedProperty, T? value) CreateOrUpdateNamedProperty<T>(
            string? key,
            string? propertyName,
            string[]? groups,
            Func<T> valueFactory,
            Func<(NamedProperty namedProperty, T? oldValue), bool>? addValidate = null,
            Func<(NamedProperty namedProperty, T? oldValue), bool>? updateValidate = null,
            Action<(NamedProperty namedProperty, T? oldValue, T? newValue, bool hasChanges)>? postAction = null)
        {
            if (key == null && propertyName == null)
            {
                throw new PropertyKeyAndNameNullException();
            }

            T? oldValue = default;
            T? newValue = default;
            bool hasChanges = false;
            bool invokePropertyChanged = false;

            NamedProperty ret = RWLock.LockUpgradeableRead(() =>
            {
                NamedProperty? namedProperty = default;
                int namedPropertyKey = default;
                bool fromKey = false;

                if (key != null && keyDictionary.TryGetValue(key, out namedPropertyKey))
                {
                    namedProperty = namedProperties[namedPropertyKey];
                    fromKey = true;
                }
                else if (propertyName != null && propertyNameDictionary.TryGetValue(propertyName, out namedPropertyKey))
                {
                    namedProperty = namedProperties[namedPropertyKey];
                }

                if (namedProperty != null)
                {
                    oldValue = namedProperty.Property.GetValue<T>();
                    newValue = oldValue;
                    RWLock.LockWrite(() =>
                    {
                        if (fromKey)
                        {
                            if (propertyName != null)
                            {
                                if (namedProperty.PropertyName != propertyName)
                                {
                                    if (namedProperty.PropertyName != null)
                                    {
                                        propertyNameDictionary.Remove(namedProperty.PropertyName);
                                    }
                                    propertyNameDictionary[propertyName] = namedPropertyKey;
                                    namedProperty.PropertyName = propertyName;
                                    hasChanges = true;
                                }
                            }
                        }
                        else
                        {
                            if (key != null)
                            {
                                if (namedProperty.Key != key)
                                {
                                    if (namedProperty.Key != null)
                                    {
                                        keyDictionary.Remove(namedProperty.Key);
                                    }
                                    keyDictionary[key] = namedPropertyKey;
                                    namedProperty.Key = key;
                                    hasChanges = true;
                                }
                            }
                        }

                        if (groups != null)
                        {
                            if (namedProperty.Groups == null)
                            {
                                namedProperty.Groups = groups;
                                hasChanges = true;
                            }
                            else if (!IsGroupsEqual(namedProperty.Groups, groups))
                            {
                                List<string> toAddGroups = new();
                                foreach (string group in groups)
                                {
                                    if (!namedProperty.Groups.Contains(group))
                                    {
                                        toAddGroups.Add(group);
                                    }
                                }
                                if (toAddGroups.Count != 0)
                                {
                                    string[] newGroups = new string[namedProperty.Groups.Length + toAddGroups.Count];
                                    Array.Copy(namedProperty.Groups, 0, newGroups, 0, namedProperty.Groups.Length);
                                    for (int i = 0; i < toAddGroups.Count; i++)
                                    {
                                        newGroups[namedProperty.Groups.Length + i] = toAddGroups[i];
                                    }
                                    namedProperty.Groups = newGroups;
                                    hasChanges = true;
                                }
                            }
                        }

                        if (updateValidate?.Invoke((namedProperty, oldValue)) ?? true)
                        {
                            T? value = valueFactory.Invoke();
                            if (namedProperty.Property.SetValue(value))
                            {
                                newValue = value;
                                hasChanges = true;
                            }
                            else if (hasChanges)
                            {
                                invokePropertyChanged = true;
                            }
                        }
                        else if (hasChanges)
                        {
                            invokePropertyChanged = true;
                        }
                    });
                }
                else
                {
                    (namedProperty, newValue) = RWLock.LockWrite(() =>
                    {
                        NamedProperty newNamedProperty = NamedPropertyFactory(key, propertyName, groups);
                        T? value = default;
                        if (addValidate?.Invoke((newNamedProperty, oldValue)) ?? true)
                        {
                            while (true)
                            {
                                namedPropertyKey = random.Next(int.MinValue, int.MaxValue);
                                if (!namedProperties.ContainsKey(namedPropertyKey))
                                {
                                    break;
                                }
                            }
                            namedProperties[namedPropertyKey] = newNamedProperty;
                            if (key != null)
                            {
                                keyDictionary[key] = namedPropertyKey;
                            }
                            if (propertyName != null)
                            {
                                propertyNameDictionary[propertyName] = namedPropertyKey;
                            }
                            WireNamedProperty(newNamedProperty);
                            value = valueFactory.Invoke();
                            if (!newNamedProperty.Property.SetValue(value))
                            {
                                invokePropertyChanged = true;
                            }
                            hasChanges = true;
                        }
                        return (newNamedProperty, value);
                    });
                }

                postAction?.Invoke((namedProperty, oldValue, newValue, hasChanges));

                return namedProperty;
            });

            if (invokePropertyChanged)
            {
                RWLock.InvokeOnLockExit(() => OnPropertyChanged(ret.Key, ret.PropertyName, ret.Groups));
            }

            return (ret, newValue);
        }

        /// <summary>
        /// The core implementation for creating exact nullability typed of the <see cref="NamedProperty"/> from the property collection with the provided <paramref name="key"/> and <paramref name="propertyName"/>, or updates a value to the collection by using the specified function if the property already exist.
        /// </summary>
        /// <param name="key">
        /// The key of the property to add or update.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property to add or update.
        /// </param>
        /// <param name="groups">
        /// The groups of the property to add or update.
        /// </param>
        /// <param name="valueFactory">
        /// The value factory of the property to add or update.
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
        protected (NamedProperty namedProperty, T value) GetOrAddNamedProperty<T>(
            string? key,
            string? propertyName,
            string[]? groups,
            Func<T> valueFactory,
            Action<(NamedProperty namedProperty, T? oldValue, T newValue, bool hasChanges)>? postAction = null)
        {
            if (key == null && propertyName == null)
            {
                throw new PropertyKeyAndNameNullException();
            }

            T? oldValue = default;
            //T? newValue = default;
            bool hasChanges = false;
            bool invokePropertyChanged = false;

            (NamedProperty retProperty, T retValue) = RWLock.LockUpgradeableRead(() =>
            {
                NamedProperty? namedProperty = default;
                T? newValue = default;

                if (key != null && keyDictionary.TryGetValue(key, out int namedPropertyKey))
                {
                    namedProperty = namedProperties[namedPropertyKey];
                }
                else if (propertyName != null && propertyNameDictionary.TryGetValue(propertyName, out namedPropertyKey))
                {
                    namedProperty = namedProperties[namedPropertyKey];
                }

                if (namedProperty == null)
                {
                    (namedProperty, newValue) = RWLock.LockWrite(() =>
                    {
                        NamedProperty newNamedProperty = NamedPropertyFactory(key, propertyName, groups);
                        while (true)
                        {
                            namedPropertyKey = random.Next(int.MinValue, int.MaxValue);
                            if (!namedProperties.ContainsKey(namedPropertyKey))
                            {
                                break;
                            }
                        }
                        namedProperties[namedPropertyKey] = newNamedProperty;
                        if (key != null)
                        {
                            keyDictionary[key] = namedPropertyKey;
                        }
                        if (propertyName != null)
                        {
                            propertyNameDictionary[propertyName] = namedPropertyKey;
                        }
                        WireNamedProperty(newNamedProperty);
                        T value = valueFactory.Invoke();
                        if (!newNamedProperty.Property.SetValue(value))
                        {
                            invokePropertyChanged = true;
                        }
                        hasChanges = true;
                        return (newNamedProperty, value);
                    });
                }
                else
                {
                    newValue = namedProperty.Property.GetValue(() => valueFactory.Invoke());
                    oldValue = newValue;
                }

                postAction?.Invoke((namedProperty, oldValue, newValue, hasChanges));

                return (namedProperty, newValue);
            });

            if (invokePropertyChanged)
            {
                RWLock.InvokeOnLockExit(() => OnPropertyChanged(retProperty.Key, retProperty.PropertyName, retProperty.Groups));
            }

            return (retProperty, retValue);
        }

        /// <summary>
        /// Specifically attach <see cref="ObservableSyncContext.PropertyChanged"/> event on single property.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value of the property.
        /// </typeparam>
        /// <param name="onPropertyChanged">
        /// The action if the property has changed.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property to attach the event.
        /// </param>
        /// <param name="key">
        /// The key of the property to attach the event.
        /// </param>
        /// <returns>
        /// The <see cref="IDisposable"/> to dispose the event.
        /// </returns>
        /// <exception cref="PropertyKeyAndNameNullException">
        /// Throws when both <paramref name="key"/> and <paramref name="propertyName"/> are not provided.
        /// </exception>
        protected IDisposable AttachOnPropertyChanged<T>(
            Action<T?> onPropertyChanged,
            string? propertyName = null,
            string? key = null)
        {
            if (key == null && propertyName == null)
            {
                throw new PropertyKeyAndNameNullException();
            }

            void invoke()
            {
                RWLock.LockRead(() =>
                {
                    if (key != null && keyDictionary.TryGetValue(key, out int namedPropertyKey))
                    {
                        onPropertyChanged?.Invoke(namedProperties[namedPropertyKey].Property.GetValue<T>());
                    }
                    else if (propertyName != null && propertyNameDictionary.TryGetValue(propertyName, out namedPropertyKey))
                    {
                        onPropertyChanged?.Invoke(namedProperties[namedPropertyKey].Property.GetValue<T>());
                    }
                });
            }

            void handler(object? s, PropertyChangedEventArgs e)
            {
                if (key != null)
                {
                    if (e is ObjectPropertyChangesEventArgs objArgs)
                    {
                        if (key == objArgs.Key)
                        {
                            invoke();
                            return;
                        }
                    }
                }

                if (propertyName == e.PropertyName)
                {
                    invoke();
                }
            }
            PropertyChanged += handler;

            invoke();

            return new Disposable(delegate
            {
                PropertyChanged -= handler;
            });
        }

        /// <summary>
        /// Specifically attach <see cref="ObservableSyncContext.SynchronizedPropertyChanged"/> event on single property.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value of the property.
        /// </typeparam>
        /// <param name="onPropertyChanged">
        /// The action if the property has changed.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property to attach the event.
        /// </param>
        /// <param name="key">
        /// The key of the property to attach the event.
        /// </param>
        /// <returns>
        /// The <see cref="IDisposable"/> to dispose the event.
        /// </returns>
        /// <exception cref="PropertyKeyAndNameNullException">
        /// Throws when both <paramref name="key"/> and <paramref name="propertyName"/> are not provided.
        /// </exception>
        protected IDisposable AttachOnSynchronizedPropertyChanged<T>(
            Action<T?> onPropertyChanged,
            string? propertyName = null,
            string? key = null)
        {
            if (key == null && propertyName == null)
            {
                throw new PropertyKeyAndNameNullException();
            }

            void invoke()
            {
                RWLock.LockRead(() =>
                {
                    if (key != null && keyDictionary.TryGetValue(key, out int namedPropertyKey))
                    {
                        onPropertyChanged?.Invoke(namedProperties[namedPropertyKey].Property.GetValue<T>());
                    }
                    else if (propertyName != null && propertyNameDictionary.TryGetValue(propertyName, out namedPropertyKey))
                    {
                        onPropertyChanged?.Invoke(namedProperties[namedPropertyKey].Property.GetValue<T>());
                    }
                });
            }

            void handler(object? s, PropertyChangedEventArgs e)
            {
                if (key != null)
                {
                    if (e is ObjectPropertyChangesEventArgs objArgs)
                    {
                        if (key == objArgs.Key)
                        {
                            invoke();
                            return;
                        }
                    }
                }

                if (propertyName == e.PropertyName)
                {
                    invoke();
                }
            }
            PropertyChanged += handler;

            invoke();

            return new Disposable(delegate
            {
                PropertyChanged -= handler;
            });
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
        /// <param name="groups">
        /// The groups of the property to create.
        /// </param>
        /// <returns>
        /// The created instance of <see cref="NamedProperty"/>.
        /// </returns>
        protected virtual NamedProperty NamedPropertyFactory(string? key, string? propertyName, string[]? groups)
        {
            return new NamedProperty(new ObservableProperty(), key, propertyName, groups);
        }

        private bool RemoveProperty(string? key, string? propertyName)
        {
            if (key == null && propertyName == null)
            {
                throw new PropertyKeyAndNameNullException();
            }

            NamedProperty? namedProperty = default;
            if (RWLock.LockUpgradeableRead(() =>
            {
                int namedPropertyKey = default;

                if (key != null && keyDictionary.TryGetValue(key, out namedPropertyKey))
                {
                    namedProperty = namedProperties[namedPropertyKey];
                }
                else if (propertyName != null && propertyNameDictionary.TryGetValue(propertyName, out namedPropertyKey))
                {
                    namedProperty = namedProperties[namedPropertyKey];
                }

                if (namedProperty != null)
                {
                    return RWLock.LockWrite(() =>
                    {
                        if (namedProperty.Key != null)
                        {
                            keyDictionary.Remove(namedProperty.Key);
                        }
                        if (namedProperty.PropertyName != null)
                        {
                            propertyNameDictionary.Remove(namedProperty.PropertyName);
                        }
                        if (namedProperties.Remove(namedPropertyKey))
                        {
                            return true;
                        }
                        return false;
                    });
                }
                return false;
            }))
            {
                if (namedProperty != null)
                {
                    RWLock.InvokeOnLockExit(() => OnPropertyChanged(namedProperty.Key, namedProperty.PropertyName, namedProperty.Groups));
                    return true;
                }
            }
            return false;
        }

        private bool ContainsProperty(string? key, string? propertyName)
        {
            if (key == null && propertyName == null)
            {
                throw new PropertyKeyAndNameNullException();
            }

            return RWLock.LockRead(() =>
            {
                if (key != null && keyDictionary.TryGetValue(key, out int namedPropertyKey))
                {
                    return true;
                }
                else if (propertyName != null && propertyNameDictionary.TryGetValue(propertyName, out namedPropertyKey))
                {
                    return true;
                }

                return false;
            });
        }

        private void OnPropertyChanged(string? key, string? propertyName, string[]? groups)
        {
            if (IsDisposed)
            {
                return;
            }

            OnPropertyChanged(new ObjectPropertyChangesEventArgs(key, propertyName, groups));
        }

        private static bool IsGroupsEqual(string[]? groups1, string[]? groups2)
        {
            if (groups1 == groups2)
            {
                return true;
            }
            else if (groups1 == null || groups2 == null)
            {
                return false;
            }
            var cnt = new Dictionary<string, int>();
            foreach (string s in groups1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (string s in groups2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
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
            foreach (NamedProperty propHolder in namedProperties.Values)
            {
                if (propHolder.Property.SetNull())
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
        /// Provides the property with key, name and groups.
        /// </summary>
        public class NamedProperty
        {
            #region Properties

            /// <summary>
            /// Gets or sets the <see cref="ObservableProperty"/> of the object.
            /// </summary>
            public ObservableProperty Property { get; internal set; }

            /// <summary>
            /// Gets or sets the key of the <see cref="Property"/>
            /// </summary>
            public string? Key { get; internal set; }

            /// <summary>
            /// Gets or sets the name of the <see cref="Property"/>
            /// </summary>
            public string? PropertyName { get; internal set; }

            /// <summary>
            /// Gets or sets the groups of the <see cref="Property"/>
            /// </summary>
            public string[]? Groups { get; internal set; }

            /// <summary>
            /// Gets or sets <c>true</c> if the property is from default value; otherwise <c>false</c>.
            /// </summary>
            public bool IsDefault { get; internal set; }

            #endregion

            #region Initializers

            /// <summary>
            /// Creates new instance for <see cref="NamedProperty"/>
            /// </summary>
            public NamedProperty(
                ObservableProperty property,
                string? key,
                string? propertyName,
                string[]? groups)
            {
                Property = property;
                Key = key;
                PropertyName = propertyName;
                Groups = groups;
            }

            #endregion

            #region Object Members

            /// <inheritdoc/>
            public override string ToString()
            {
                return "(" + (Key ?? "null") + ", " + (PropertyName ?? "null") + ") = " + (Property?.Value?.ToString() ?? "null");
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
            public string? Key { get; }

            /// <summary>
            /// Gets the groups of the property that changed.
            /// </summary>
            public string[]? Groups { get; }

            #endregion

            #region Initializers

            internal ObjectPropertyChangesEventArgs(
                string? key,
                string? propertyName,
                string[]? groups)
                : base(propertyName)
            {
                Key = key;
                Groups = groups;
            }

            #endregion
        }

        #endregion
    }
}
