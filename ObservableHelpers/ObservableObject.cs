using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    public class ObservableObject : IObservable
    {
        #region Properties

        private readonly SynchronizationContext context = AsyncOperationManager.SynchronizationContext;
        private bool disableOnChanges;

        protected List<PropertyHolder> PropertyHolders { get; set; } = new List<PropertyHolder>();

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangedEventHandler PropertyChangedInternal;
        public event EventHandler<Exception> PropertyError;
        public event EventHandler<Exception> PropertyErrorInternal;

        #endregion

        #region Methods

        public virtual bool SetNull(object parameter = null)
        {
            var hasChanges = false;
            lock (PropertyHolders)
            {
                foreach (var propHolder in PropertyHolders)
                {
                    if (propHolder.Property.SetNull(parameter)) hasChanges = true;
                }
            }
            return hasChanges;
        }

        public virtual bool IsNull(object parameter = null)
        {
            lock (PropertyHolders)
            {
                return PropertyHolders.All(i => i.Property.IsNull(parameter));
            }
        }

        protected virtual void OnChanged(string key, string propertyName, string group)
        {
            if (disableOnChanges) return;
            PropertyChangedInternal?.Invoke(this, new ObservableObjectChangesEventArgs(key, propertyName, group));
            context.Post(s =>
            {
                lock (this)
                {
                    PropertyChanged?.Invoke(this, new ObservableObjectChangesEventArgs(key, propertyName, group));
                }
            }, null);
        }

        protected virtual void OnChanged(string propertyName)
        {
            PropertyHolder propHolder = null;
            lock (PropertyHolders)
            {
                propHolder = PropertyHolders.FirstOrDefault(i => i.PropertyName == propertyName);
            }
            if (propHolder != null) OnChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
        }

        protected virtual void OnChangedWithKey(string key)
        {
            PropertyHolder propHolder = null;
            lock (PropertyHolders)
            {
                propHolder = PropertyHolders.FirstOrDefault(i => i.Key == key);
            }
            if (propHolder != null) OnChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
        }

        protected virtual void OnError(Exception exception)
        {
            PropertyErrorInternal?.Invoke(this, exception);
            context.Post(s =>
            {
                PropertyError?.Invoke(this, exception);
            }, null);
        }

        protected void InitializeProperties(bool invokeOnChanges = true)
        {
            if (!invokeOnChanges) disableOnChanges = true;
            try
            {
                foreach (var property in GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                {
                    property.GetValue(this);
                }
            }
            catch { }
            if (!invokeOnChanges) disableOnChanges = false;
        }

        protected virtual PropertyHolder PropertyFactory(string key, string propertyName, string group)
        {
            ObservableProperty prop;
            prop = new ObservableProperty();
            var propHolder = new PropertyHolder()
            {
                Property = prop,
                Key = key,
                PropertyName = propertyName,
                Group = group
            };
            prop.PropertyChangedInternal += (s, e) =>
            {
                if (e.PropertyName == nameof(prop.Property))
                {
                    OnChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
                }
            };
            return propHolder;
        }

        protected bool SetProperty<T>(
            T value,
            [CallerMemberName] string propertyName = null,
            string group = null,
            object parameter = null,
            Func<T, T, bool> validateValue = null)
        {
            return SetPropertyInternal(value, null, propertyName, group, parameter, validateValue);
        }

        protected bool SetPropertyWithKey<T>(
            T value,
            string key,
            [CallerMemberName] string propertyName = null,
            string group = null,
            object parameter = null,
            Func<T, T, bool> validateValue = null)
        {
            return SetPropertyInternal(value, key, propertyName, group, parameter, validateValue);
        }

        protected T GetProperty<T>(
            T defaultValue = default,
            [CallerMemberName] string propertyName = null,
            string group = null,
            object parameter = null)
        {
            return GetPropertyInternal(defaultValue, null, propertyName, group, parameter);
        }

        protected T GetPropertyWithKey<T>(
            string key,
            T defaultValue = default,
            [CallerMemberName] string propertyName = null,
            string group = null,
            object parameter = null)
        {
            return GetPropertyInternal(defaultValue, key, propertyName, group, parameter);
        }

        protected virtual bool DeleteProperty(string propertyName)
        {
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
            lock(PropertyHolders)
            {
                return group == null ? PropertyHolders : PropertyHolders.Where(i => i.Group == group);
            }
        }

        private bool SetPropertyInternal<T>(
            T value,
            string key = null,
            string propertyName = null,
            string group = null,
            object parameter = null,
            Func<T, T, bool> validateValue = null)
        {
            if (key == null && propertyName == null)
            {
                OnError(new Exception("key and propertyName should not be both null"));
            }

            PropertyHolder propHolder = null;
            bool hasChanges = false;

            try
            {
                lock (PropertyHolders)
                {
                    if (key == null) propHolder = PropertyHolders.FirstOrDefault(i => i.PropertyName == propertyName);
                    else propHolder = PropertyHolders.FirstOrDefault(i => i.Key == key);
                }

                if (propHolder != null)
                {
                    var existingValue = propHolder.Property.GetValue<T>(default, parameter);

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
                        if (propHolder.Property.SetValue(value, parameter))
                        {
                            hasSetChanges = true;
                            hasChanges = true;
                        }
                    }
                    if (!hasSetChanges && hasChanges) OnChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
                }
                else
                {
                    propHolder = PropertyFactory(key, propertyName, group);
                    lock (PropertyHolders)
                    {
                        PropertyHolders.Add(propHolder);
                    }
                    propHolder.Property.SetValue(value, parameter);
                    hasChanges = true;
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
                return hasChanges;
            }

            return hasChanges;
        }

        private T GetPropertyInternal<T>(
            T defaultValue = default,
            string key = null,
            [CallerMemberName] string propertyName = null,
            string group = null,
            object parameter = null)
        {
            if (key == null && propertyName == null)
            {
                OnError(new Exception("key and propertyName should not be both null"));
            }

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
                propHolder.Property.SetValue(defaultValue, parameter);
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

                if (hasChanges) OnChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
            }

            return propHolder.Property.GetValue(defaultValue, parameter);
        }

        #endregion
    }
}
