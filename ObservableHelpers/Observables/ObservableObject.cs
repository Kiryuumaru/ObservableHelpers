using ObservableHelpers.Serializers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObservableHelpers.Observables
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
        public event EventHandler<ContinueExceptionEventArgs> PropertyError;

        #endregion

        #region Methods

        private bool SetPropertyInternal<T>(
            T value,
            string key = null,
            string propertyName = null,
            string group = null,
            bool serializable = false,
            Func<T, T, bool> validateValue = null,
            Func<(T value, ObservableProperty property), bool> customValueSetter = null)
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
                        if (customValueSetter == null)
                        {
                            if (propHolder.Property.SetValue(value))
                            {
                                hasSetChanges = true;
                                hasChanges = true;
                            }
                        }
                        else
                        {
                            if (customValueSetter.Invoke((value, propHolder.Property)))
                            {
                                hasSetChanges = true;
                                hasChanges = true;
                            }
                        }
                    }
                    if (!hasSetChanges && hasChanges) OnChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
                }
                else
                {
                    propHolder = PropertyFactory(key, propertyName, group, serializable);
                    lock (PropertyHolders)
                    {
                        PropertyHolders.Add(propHolder);
                    }
                    if (customValueSetter == null) propHolder.Property.SetValue(value);
                    else customValueSetter.Invoke((value, propHolder.Property));
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
            bool serializable = false,
            Func<(T value, ObservableProperty property), bool> customValueSetter = null)
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
                propHolder = PropertyFactory(key, propertyName, group, serializable);
                lock (PropertyHolders)
                {
                    PropertyHolders.Add(propHolder);
                }
                if (customValueSetter == null) propHolder.Property.SetValue(defaultValue);
                else customValueSetter.Invoke((defaultValue, propHolder.Property));
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

            return propHolder.Property.GetValue<T>();
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

        protected virtual PropertyHolder PropertyFactory(string key, string propertyName, string group, bool serializable)
        {
            ObservableProperty prop;
            if (serializable) prop = new ObservableSerializableProperty();
            else  prop = new ObservableNonSerializableProperty();
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
                    OnChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
                }
            };
            return propHolder;
        }

        protected bool SetProperty<T>(
            T value,
            [CallerMemberName] string propertyName = null,
            string group = null,
            bool serializable = false,
            Func<T, T, bool> validateValue = null,
            Func<(T value, ObservableProperty property), bool> customValueSetter = null)
        {
            return SetPropertyInternal(value, null, propertyName, group, serializable, validateValue, customValueSetter);
        }

        protected bool SetPropertyWithKey<T>(
            T value,
            string key,
            [CallerMemberName] string propertyName = null,
            string group = null,
            bool serializable = false,
            Func<T, T, bool> validateValue = null,
            Func<(T value, ObservableProperty property), bool> customValueSetter = null)
        {
            return SetPropertyInternal(value, key, propertyName, group, serializable, validateValue, customValueSetter);
        }

        protected T GetProperty<T>(
            T defaultValue = default,
            [CallerMemberName] string propertyName = null,
            string group = null,
            bool serializable = false,
            Func<(T value, ObservableProperty property), bool> customValueSetter = null)
        {
            return GetPropertyInternal<T>(defaultValue, null, propertyName, group, serializable, customValueSetter);
        }

        protected T GetPropertyWithKey<T>(
            string key,
            T defaultValue = default,
            [CallerMemberName] string propertyName = null,
            string group = null,
            bool serializable = false,
            Func<(T value, ObservableProperty property), bool> customValueSetter = null)
        {
            return GetPropertyInternal(defaultValue, key, propertyName, group, serializable, customValueSetter);
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

        public virtual void OnChanged(string key, string propertyName, string group)
        {
            if (disableOnChanges) return;
            context.Post(s =>
            {
                lock(this)
                {
                    PropertyChanged?.Invoke(this, new ObservableObjectChangesEventArgs(key, propertyName, group));
                }
            }, null);
        }

        public virtual void OnChanged(string propertyName)
        {
            PropertyHolder propHolder = null;
            lock (PropertyHolders)
            {
                propHolder = PropertyHolders.FirstOrDefault(i => i.PropertyName == propertyName);
            }
            if (propHolder != null) OnChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
        }

        public virtual void OnChangedWithKey(string key)
        {
            PropertyHolder propHolder = null;
            lock (PropertyHolders)
            {
                propHolder = PropertyHolders.FirstOrDefault(i => i.Key == key);
            }
            if (propHolder != null) OnChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
        }

        public virtual void OnError(Exception exception, bool defaultIgnoreAndContinue = true)
        {
            var args = new ContinueExceptionEventArgs(exception, defaultIgnoreAndContinue);
            PropertyError?.Invoke(this, args);
            if (!args.IgnoreAndContinue)
            {
                throw args.Exception;
            }
        }

        public virtual void OnError(ContinueExceptionEventArgs args)
        {
            PropertyError?.Invoke(this, args);
            if (!args.IgnoreAndContinue)
            {
                throw args.Exception;
            }
        }

        #endregion
    }
}
