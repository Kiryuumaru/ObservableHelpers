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
    public class ObservableObject : Observable
    {
        #region Helpers

        protected class NamedProperty
        {
            public ObservableProperty Property { get; set; }
            public string Key { get; set; }
            public string PropertyName { get; set; }
            public string Group { get; set; }
        }

        #endregion

        #region Properties

        private readonly List<NamedProperty> namedProperties = new List<NamedProperty>();

        #endregion

        #region Methods

        public override bool SetNull()
        {
            VerifyNotDisposed();

            var hasChanges = false;
            lock (namedProperties)
            {
                foreach (var propHolder in namedProperties)
                {
                    if (propHolder.Property.SetNull()) hasChanges = true;
                }
            }
            return hasChanges;
        }

        public override bool IsNull()
        {
            VerifyNotDisposed();

            lock (namedProperties)
            {
                return namedProperties.All(i => i.Property.IsNull());
            }
        }

        protected void InitializeProperties()
        {
            VerifyNotDisposed();

            try
            {
                foreach (var property in GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                {
                    property.GetValue(this);
                }
            }
            catch { }
        }

        protected bool SetProperty<T>(
            T value,
            [CallerMemberName] string propertyName = null,
            string group = null)
        {
            VerifyNotDisposed();

            return SetPropertyInternal(value, null, propertyName, group);
        }

        protected bool SetPropertyWithKey<T>(
            T value,
            string key,
            [CallerMemberName] string propertyName = null,
            string group = null)
        {
            VerifyNotDisposed();

            return SetPropertyInternal(value, key, propertyName, group);
        }

        protected T GetProperty<T>(
            T defaultValue = default,
            [CallerMemberName] string propertyName = null,
            string group = null)
        {
            VerifyNotDisposed();

            return GetPropertyInternal(defaultValue, null, propertyName, group);
        }

        protected T GetPropertyWithKey<T>(
            string key,
            T defaultValue = default,
            [CallerMemberName] string propertyName = null,
            string group = null)
        {
            VerifyNotDisposed();

            return GetPropertyInternal(defaultValue, key, propertyName, group);
        }

        protected bool DeleteProperty(string propertyName)
        {
            VerifyNotDisposed();

            return DeletePropertyCore(null, propertyName);
        }

        protected bool DeletePropertyWithKey(string key)
        {
            VerifyNotDisposed();

            return DeletePropertyCore(key, null);
        }

        protected NamedProperty MakeNamedProperty(string key, string propertyName, string group)
        {
            VerifyNotDisposed();

            NamedProperty namedProperty = NamedPropertyFactory(key, propertyName, group);
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
            return namedProperty;
        }

        protected IEnumerable<NamedProperty> GetRawProperties(string group = null)
        {
            VerifyNotDisposed();

            lock (namedProperties)
            {
                return group == null ? namedProperties.ToList() : namedProperties.Where(i => i.Group == group).ToList();
            }
        }

        protected void AddCore(NamedProperty namedProperty)
        {
            VerifyNotDisposed();

            bool exists = false;
            lock (namedProperties)
            {
                if (namedProperty.Key == null)
                {
                    if (namedProperties.Any(i => i.PropertyName == namedProperty.PropertyName))
                    {
                        exists = true;
                        throw new Exception("Property already exists");
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
                throw new Exception("Property already exists");
            }
        }

        protected NamedProperty GetCore(string key, string propertyName)
        {
            VerifyNotDisposed();

            if (key == null && propertyName == null) throw new Exception("key and propertyName should not be both null");

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

        protected bool RemoveCore(string key, string propertyName)
        {
            VerifyNotDisposed();

            if (key == null && propertyName == null) throw new Exception("key and propertyName should not be both null");

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

        protected bool DeletePropertyCore(string key, string propertyName)
        {
            VerifyNotDisposed();

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
            if (propHolder == null) return false;
            return propHolder.Property.SetNull();
        }

        protected bool ExistsCore(string key, string propertyName)
        {
            VerifyNotDisposed();

            if (key == null && propertyName == null) throw new Exception("key and propertyName should not be both null");

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

        protected virtual NamedProperty NamedPropertyFactory(string key, string propertyName, string group)
        {
            VerifyNotDisposed();

            return new NamedProperty()
            {
                Property = new ObservableProperty(),
                Key = key,
                PropertyName = propertyName,
                Group = group
            };
        }

        protected virtual void OnPropertyChanged(string key, string propertyName, string group)
        {
            VerifyNotDisposed();

            OnPropertyChanged(new ObservableObjectChangesEventArgs(key, propertyName, group));
        }

        protected virtual void OnPropertyChangedWithKey(string key)
        {
            VerifyNotDisposed();

            NamedProperty propHolder = null;
            lock (namedProperties)
            {
                propHolder = namedProperties.FirstOrDefault(i => i.Key == key);
            }
            if (propHolder != null) OnPropertyChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
        }

        private bool SetPropertyInternal<T>(
            T value,
            string key = null,
            string propertyName = null,
            string group = null)
        {
            VerifyNotDisposed();

            bool hasChanges = false;
            NamedProperty propHolder = GetCore(key, propertyName);

            if (propHolder != null)
            {
                propHolder.Property.SyncOperation.SetContext(this);

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
            else
            {
                propHolder = MakeNamedProperty(key, propertyName, group);
                AddCore(propHolder);
                propHolder.Property.SetValue(value);
                hasChanges = true;
            }

            return hasChanges;
        }

        private T GetPropertyInternal<T>(
            T defaultValue = default,
            string key = null,
            [CallerMemberName] string propertyName = null,
            string group = null)
        {
            VerifyNotDisposed();

            bool hasChanges = false;
            NamedProperty propHolder = GetCore(key, propertyName);

            if (propHolder == null)
            {
                propHolder = MakeNamedProperty(key, propertyName, group);
                AddCore(propHolder);
                propHolder.Property.SetValue(defaultValue);
            }
            else
            {
                propHolder.Property.SyncOperation.SetContext(this);

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

            return propHolder.Property.GetValue<T>();
        }

        #endregion
    }
}
