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
        public object Property { get; set; }
        public string Key { get; set; }
        public string PropertyName { get; set; }
        public string Group { get; set; }

        public bool CopyToSelf(object property, string key, string propertyName, string group)
        {
            var hasChanges = false;
            if (Key != key)
            {
                Key = key;
                hasChanges = true;
            }

            if (PropertyName != propertyName)
            {
                PropertyName = propertyName;
                hasChanges = true;
            }

            if (Group != group)
            {
                Group = group;
                hasChanges = true;
            }

            if (!Property?.Equals(property) ?? property != null)
            {
                Property = property;
                hasChanges = true;
            }
            return hasChanges;
        }

        public bool CopyToSelf(PropertyHolder propertyHolder)
        {
            return CopyToSelf(propertyHolder.Property, propertyHolder.Key, propertyHolder.PropertyName, propertyHolder.Group);
        }

        public override bool Equals(object obj) => Equals(obj as PropertyHolder);

        public bool Equals(PropertyHolder prop)
        {
            if (prop is null)
            {
                return false;
            }

            if (ReferenceEquals(this, prop))
            {
                return true;
            }

            if (GetType() != prop.GetType())
            {
                return false;
            }

            return
                (Key == prop.Key) &&
                (PropertyName == prop.PropertyName) &&
                (Group == prop.Group) &&
                (Property?.Equals(prop.Property) ?? prop.Property == null);
        }

        public override int GetHashCode() => (Key, PropertyName, Group, Property).GetHashCode();

        public static bool operator ==(PropertyHolder left, PropertyHolder right)
        {
            if (left is null)
            {
                if (right is null)
                {
                    return true;
                }

                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(PropertyHolder lhs, PropertyHolder rhs) => !(lhs == rhs);

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

            lock (PropertyHolders)
            {
                return PropertyHolders.All(i => i == null);
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

        protected virtual bool DeleteProperty(string propertyName)
        {
            VerifyNotDisposed();

            PropertyHolder propHolder = null;
            lock (PropertyHolders)
            {
                propHolder = PropertyHolders.FirstOrDefault(i => i.PropertyName == propertyName);
            }
            if (propHolder == null) return false;
            if (propHolder.Property != null)
            {
                propHolder.Property = null;
                return true;
            }
            else
            {
                return false;
            }
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
            if (propHolder.Property != null)
            {
                propHolder.Property = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        protected IEnumerable<PropertyHolder> GetRawProperties(string group = null)
        {
            VerifyNotDisposed();

            lock (PropertyHolders)
            {
                return group == null ? PropertyHolders.ToList() : PropertyHolders.Where(i => i.Group == group).ToList();
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
                lock (PropertyHolders)
                {
                    PropertyHolders.Add(propertyHolder);
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

            lock (PropertyHolders)
            {
                if (key == null)
                {
                    return PropertyHolders.FirstOrDefault(i => i.PropertyName == propertyName);
                }
                else
                {
                    return PropertyHolders.FirstOrDefault(i => i.Key == key);
                }
            }
        }

        protected bool Delete(string key, string propertyName)
        {
            VerifyNotDisposed();

            if (key == null && propertyName == null) throw new Exception("key and propertyName should not be both null");

            bool hasChanges = false;
            PropertyHolder propHolder = null;
            lock (PropertyHolders)
            {
                if (key == null)
                {
                    propHolder = PropertyHolders.FirstOrDefault(i => i.PropertyName == propertyName);
                }
                else
                {
                    propHolder = PropertyHolders.FirstOrDefault(i => i.Key == key);
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
            VerifyNotDisposed();
            
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
            VerifyNotDisposed();

            PropertyHolder propHolder = Get(key, propertyName);
            PropertyHolder newPropHolder = new PropertyHolder()
            {
                Property = defaultValue,
                Key = key,
                PropertyName = propertyName,
                Group = group
            };

            if (propHolder != null)
            {
                if (propHolder.CopyToSelf(propHolder.Property, key, propertyName, group))
                {
                    OnPropertyChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
                }
            }
            else
            {
                propHolder = newPropHolder;
                Set(newPropHolder);
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
