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
    #region Helpers

    public class PropertyHolder
    {
        public ObservableProperty Property { get; set; }
        public string Key { get; set; }
        public string PropertyName { get; set; }
        public string Group { get; set; }
    }

    #endregion

    public class ObservableObject : Observable
    {
        #region Properties

        protected List<PropertyHolder> PropertyHolders { get; set; } = new List<PropertyHolder>();

        #endregion

        #region Methods

        public override bool SetNull()
        {
            VerifyNotDisposed();

            var hasChanges = false;
            lock (PropertyHolders)
            {
                foreach (var propHolder in PropertyHolders)
                {
                    if (propHolder.Property.SetNull()) hasChanges = true;
                }
            }
            return hasChanges;
        }

        public override bool IsNull()
        {
            VerifyNotDisposed();

            lock (PropertyHolders)
            {
                return PropertyHolders.All(i => i.Property.IsNull());
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
            lock (PropertyHolders)
            {
                propHolder = PropertyHolders.FirstOrDefault(i => i.Key == key);
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

        protected virtual PropertyHolder PropertyFactory(string key, string propertyName, string group)
        {
            VerifyNotDisposed();

            ObservableProperty prop;
            prop = new ObservableProperty();
            var propHolder = new PropertyHolder()
            {
                Property = prop,
                Key = key,
                PropertyName = propertyName,
                Group = group
            };
            prop.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(prop.Property))
                {
                    OnPropertyChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
                }
            };
            return propHolder;
        }

        protected bool SetProperty<T>(
            T value,
            [CallerMemberName] string propertyName = null,
            string group = null,
            Func<T, T, bool> validateValue = null)
        {
            VerifyNotDisposed();

            return SetPropertyInternal(value, null, propertyName, group, validateValue);
        }

        protected bool SetPropertyWithKey<T>(
            T value,
            string key,
            [CallerMemberName] string propertyName = null,
            string group = null,
            Func<T, T, bool> validateValue = null)
        {
            VerifyNotDisposed();

            return SetPropertyInternal(value, key, propertyName, group, validateValue);
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

        protected virtual bool DeleteProperty(string propertyName)
        {
            VerifyNotDisposed();

            PropertyHolder propHolder = null;
            lock (PropertyHolders)
            {
                propHolder = PropertyHolders.FirstOrDefault(i => i.PropertyName == propertyName);
            }
            if (propHolder == null) return false;
            return propHolder.Property.SetNull();
        }

        protected virtual bool DeletePropertyWithKey(string key)
        {
            VerifyNotDisposed();

            PropertyHolder propHolder = null;
            lock (PropertyHolders)
            {
                propHolder = PropertyHolders.FirstOrDefault(i => i.Key == key);
            }
            if (propHolder == null) return false;
            return propHolder.Property.SetNull();
        }

        protected IEnumerable<PropertyHolder> GetRawProperties(string group = null)
        {
            VerifyNotDisposed();

            lock (PropertyHolders)
            {
                return group == null ? PropertyHolders : PropertyHolders.Where(i => i.Group == group);
            }
        }

        private bool SetPropertyInternal<T>(
            T value,
            string key = null,
            string propertyName = null,
            string group = null,
            Func<T, T, bool> validateValue = null)
        {
            VerifyNotDisposed();

            if (key == null && propertyName == null) throw new Exception("key and propertyName should not be both null");

            PropertyHolder propHolder = null;
            bool hasChanges = false;

            lock (PropertyHolders)
            {
                if (key == null) propHolder = PropertyHolders.FirstOrDefault(i => i.PropertyName == propertyName);
                else propHolder = PropertyHolders.FirstOrDefault(i => i.Key == key);
            }

            if (propHolder != null)
            {
                var existingValue = propHolder.Property.GetValue<T>();

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

                if (validateValue?.Invoke(existingValue, value) ?? true)
                {
                    if (propHolder.Property.SetValue(value))
                    {
                        hasSetChanges = true;
                        hasChanges = true;
                    }
                }
                if (!hasSetChanges && hasChanges) OnPropertyChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
            }
            else
            {
                propHolder = PropertyFactory(key, propertyName, group);
                lock (PropertyHolders)
                {
                    PropertyHolders.Add(propHolder);
                }
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

            if (key == null && propertyName == null) throw new Exception("key and propertyName should not be both null");

            bool hasChanges = false;

            PropertyHolder propHolder = null;
            lock (PropertyHolders)
            {
                if (key == null) propHolder = PropertyHolders.FirstOrDefault(i => i.PropertyName == propertyName);
                else propHolder = PropertyHolders.FirstOrDefault(i => i.Key == key);
            }

            if (propHolder == null)
            {
                propHolder = PropertyFactory(key, propertyName, group);
                lock (PropertyHolders)
                {
                    PropertyHolders.Add(propHolder);
                }
                propHolder.Property.SetValue(defaultValue);
                hasChanges = true;
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

                if (hasChanges) OnPropertyChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
            }

            return propHolder.Property.GetValue<T>();
        }

        #endregion
    }
}
