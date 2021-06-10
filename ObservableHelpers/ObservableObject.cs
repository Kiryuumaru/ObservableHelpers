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
                    if (propHolder.Property != null)
                    {
                        propHolder.Property = null;
                        hasChanges = true;
                    }
                }
            }
            return hasChanges;
        }

        public override bool IsNull()
        {
            VerifyNotDisposed();

            lock (propertyHolders)
            {
                return propertyHolders.All(i => i == null);
            }
        }

        protected virtual void OnPropertyChanged(string key, string propertyName, string group)
        {
            VerifyNotDisposed();

            OnPropertyChanged(new ObservableObjectChangesEventArgs(key, propertyName, group));
        }

        protected virtual void OnPropertyChangedWithKey(string key)
        {
            VerifyNotDisposed();

            PropertyHolder propHolder = null;
            lock (propertyHolders)
            {
                propHolder = propertyHolders.FirstOrDefault(i => i.Key == key);
            }
            if (propHolder != null) OnPropertyChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
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
            return SetPropertyInternal(value, null, propertyName, group);
        }

        protected bool SetPropertyWithKey<T>(
            T value,
            string key,
            [CallerMemberName] string propertyName = null,
            string group = null)
        {
            return SetPropertyInternal(value, key, propertyName, group);
        }

        protected T GetProperty<T>(
            T defaultValue = default,
            [CallerMemberName] string propertyName = null,
            string group = null)
        {
            return GetPropertyInternal(defaultValue, null, propertyName, group);
        }

        protected T GetPropertyWithKey<T>(
            string key,
            T defaultValue = default,
            [CallerMemberName] string propertyName = null,
            string group = null)
        {
            return GetPropertyInternal(defaultValue, key, propertyName, group);
        }

        protected virtual bool DeleteProperty(string propertyName)
        {
            return Delete(null, propertyName);
        }

        protected virtual bool DeletePropertyWithKey(string key)
        {
            return Delete(key, null);
        }

        protected IEnumerable<PropertyHolder> GetRawProperties(string group = null)
        {
            VerifyNotDisposed();

            lock (propertyHolders)
            {
                return group == null ? propertyHolders.ToList() : propertyHolders.Where(i => i.Group == group).ToList();
            }
        }

        protected bool Set(PropertyHolder propertyHolder)
        {
            VerifyNotDisposed();

            bool hasChanges = false;
            var existing = Get(propertyHolder.Key, propertyHolder.PropertyName);

            if (existing == null)
            {
                existing = propertyHolder;
                lock (propertyHolders)
                {
                    propertyHolders.Add(propertyHolder);
                }
                hasChanges = true;
            }
            else
            {
                if (existing.CopyToSelf(propertyHolder))
                {
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                OnPropertyChanged(existing.Key, existing.PropertyName, existing.Group);
            }

            return hasChanges;
        }

        protected PropertyHolder Get(string key, string propertyName)
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

        protected bool Delete(string key, string propertyName)
        {
            VerifyNotDisposed();

            if (key == null && propertyName == null) throw new Exception("key and propertyName should not be both null");

            bool hasChanges = false;
            PropertyHolder propHolder = null;
            lock (propertyHolders)
            {
                if (key == null)
                {
                    propHolder = propertyHolders.FirstOrDefault(i => i.PropertyName == propertyName);
                }
                else
                {
                    propHolder = propertyHolders.FirstOrDefault(i => i.Key == key);
                }
            }
            if (propHolder?.Property != null)
            {
                propHolder.Property = null;
                hasChanges = true;
                OnPropertyChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
            }

            return hasChanges;
        }

        private bool SetPropertyInternal<T>(
            T value,
            string key = null,
            string propertyName = null,
            string group = null)
        {
            return Set(new PropertyHolder()
            {
                Property = value,
                Key = key,
                PropertyName = propertyName,
                Group = group
            });
        }

        private T GetPropertyInternal<T>(
            T defaultValue = default,
            string key = null,
            [CallerMemberName] string propertyName = null,
            string group = null)
        {
            PropertyHolder propHolder = Get(key, propertyName);

            if (propHolder != null)
            {
                if (propHolder.CopyToSelf(propHolder.Property, key, propertyName, group))
                {
                    OnPropertyChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
                }
            }
            else
            {
                propHolder = new PropertyHolder()
                {
                    Property = defaultValue,
                    Key = key,
                    PropertyName = propertyName,
                    Group = group
                };
                Set(propHolder);
            }

            if (propHolder.Property is T tObj)
            {
                return tObj;
            }
            else
            {
                return defaultValue;
            }
        }

        #endregion
    }
}
