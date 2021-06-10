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

        protected class PropertyHolder
        {
            public ObservableProperty Property { get; set; }
            public string Key { get; set; }
            public string PropertyName { get; set; }
            public string Group { get; set; }
        }

        #endregion

        #region Properties

        private readonly List<PropertyHolder> propertyHolders = new List<PropertyHolder>();

        #endregion

        #region Methods

        public override bool SetNull()
        {
            VerifyNotDisposed();

            var hasChanges = false;
            lock (propertyHolders)
            {
                foreach (var propHolder in propertyHolders)
                {
                    if (propHolder.Property.SetNull()) hasChanges = true;
                }
            }
            return hasChanges;
        }

        public override bool IsNull()
        {
            VerifyNotDisposed();

            lock (propertyHolders)
            {
                return propertyHolders.All(i => i.Property.IsNull());
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

            PropertyHolder propHolder = null;
            lock (propertyHolders)
            {
                propHolder = propertyHolders.FirstOrDefault(i => i.PropertyName == propertyName);
            }
            if (propHolder == null) return false;
            return propHolder.Property.SetNull();
        }

        protected bool DeletePropertyWithKey(string key)
        {
            VerifyNotDisposed();

            PropertyHolder propHolder = null;
            lock (propertyHolders)
            {
                propHolder = propertyHolders.FirstOrDefault(i => i.Key == key);
            }
            if (propHolder == null) return false;
            return propHolder.Property.SetNull();
        }

        protected IEnumerable<PropertyHolder> GetRawProperties(string group = null)
        {
            VerifyNotDisposed();

            lock (propertyHolders)
            {
                return group == null ? propertyHolders.ToList() : propertyHolders.Where(i => i.Group == group).ToList();
            }
        }

        protected void AddCore(PropertyHolder propertyHolder)
        {
            VerifyNotDisposed();

            bool exists = false;
            lock (propertyHolders)
            {
                if (propertyHolder.Key == null)
                {
                    if (propertyHolders.Any(i => i.PropertyName == propertyHolder.PropertyName))
                    {
                        exists = true;
                        throw new Exception("Property already exists");
                    }
                }
                else
                {
                    if (propertyHolders.Any(i => i.Key == propertyHolder.Key))
                    {
                        exists = true;
                    }
                }
                if (!exists)
                {
                    propertyHolders.Add(propertyHolder);
                }
            }
            if (exists)
            {
                throw new Exception("Property already exists");
            }
        }

        protected PropertyHolder GetCore(string key, string propertyName)
        {
            VerifyNotDisposed();

            if (key == null && propertyName == null) throw new Exception("key and propertyName should not be both null");

            lock (propertyHolders)
            {
                if (key == null)
                {
                    return propertyHolders.FirstOrDefault(i => i.PropertyName == propertyName);
                }
                else
                {
                    return propertyHolders.FirstOrDefault(i => i.Key == key);
                }
            }
        }

        protected bool RemoveCore(string key, string propertyName)
        {
            VerifyNotDisposed();

            if (key == null && propertyName == null) throw new Exception("key and propertyName should not be both null");

            int removedCount = 0;
            lock (propertyHolders)
            {
                if (key == null)
                {
                    removedCount = propertyHolders.RemoveAll(i => i.PropertyName == propertyName);
                }
                else
                {
                    removedCount = propertyHolders.RemoveAll(i => i.Key == key);
                }
            }
            return removedCount != 0;
        }

        protected bool ExistsCore(string key, string propertyName)
        {
            VerifyNotDisposed();

            if (key == null && propertyName == null) throw new Exception("key and propertyName should not be both null");

            lock (propertyHolders)
            {
                if (key == null)
                {
                    if (propertyHolders.Any(i => i.PropertyName == propertyName))
                    {
                        return true;
                    }
                }
                else
                {
                    if (propertyHolders.Any(i => i.Key == key))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected void OnPropertyChanged(string key, string propertyName, string group)
        {
            VerifyNotDisposed();

            OnPropertyChanged(new ObservableObjectChangesEventArgs(key, propertyName, group));
        }

        protected void OnPropertyChangedWithKey(string key)
        {
            VerifyNotDisposed();

            PropertyHolder propHolder = null;
            lock (propertyHolders)
            {
                propHolder = propertyHolders.FirstOrDefault(i => i.Key == key);
            }
            if (propHolder != null) OnPropertyChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
        }

        protected virtual PropertyHolder PropertyFactory(string key, string propertyName, string group)
        {
            VerifyNotDisposed();

            return new PropertyHolder()
            {
                Property = new ObservableProperty(),
                Key = key,
                PropertyName = propertyName,
                Group = group
            };
        }

        private bool SetPropertyInternal<T>(
            T value,
            string key = null,
            string propertyName = null,
            string group = null)
        {
            bool hasChanges = false;
            PropertyHolder propHolder = GetCore(key, propertyName);

            if (propHolder != null)
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
            else
            {
                propHolder = MakePropertyHolder(key, propertyName, group);
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
            bool hasChanges = false;
            PropertyHolder propHolder = GetCore(key, propertyName);

            if (propHolder == null)
            {
                propHolder = MakePropertyHolder(key, propertyName, group);
                AddCore(propHolder);
                propHolder.Property.SetValue(defaultValue);
            }
            else
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

            return propHolder.Property.GetValue<T>();
        }

        private PropertyHolder MakePropertyHolder(string key, string propertyName, string group)
        {
            VerifyNotDisposed();

            PropertyHolder propHolder = PropertyFactory(key, propertyName, group);
            propHolder.Property.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(propHolder.Property.Property))
                {
                    OnPropertyChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
                }
            };
            return propHolder;
        }

        #endregion
    }
}
