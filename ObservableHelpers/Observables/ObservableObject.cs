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
        public string Group { get; set; }
        public string PropertyName { get; set; }
    }

    #endregion

    public class ObservableObject : IObservable
    {
        #region Properties

        private readonly SynchronizationContext context = AsyncOperationManager.SynchronizationContext;

        protected List<PropertyHolder> PropertyHolders { get; set; } = new List<PropertyHolder>();

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<ContinueExceptionEventArgs> PropertyError;

        #endregion

        #region Methods

        protected void InitializeProperties()
        {
            foreach (var property in GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                property.GetValue(this);
            }
        }

        protected virtual PropertyHolder PropertyFactory(string key, string group, string propertyName, bool serializable)
        {
            ObservableProperty prop;
            if (serializable) prop = new ObservableSerializableProperty();
            else  prop = new ObservableNonSerializableProperty();
            return new PropertyHolder()
            {
                Property = prop,
                Key = key,
                Group = group,
                PropertyName = propertyName
            };
        }

        protected virtual bool SetProperty<T>(
            T value,
            string key,
            bool serializable = false,
            string group = null,
            [CallerMemberName] string propertyName = null,
            Func<T, T, bool> validateValue = null,
            Func<(T value, ObservableProperty property), bool> customValueSetter = null)
        {
            PropertyHolder propHolder = null;
            bool hasChanges = false;

            try
            {
                lock(PropertyHolders)
                {
                    propHolder = PropertyHolders.FirstOrDefault(i => i.Key.Equals(key));
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

                    if (validateValue?.Invoke(existingValue, value) ?? true)
                    {
                        if (customValueSetter == null)
                        {
                            if (propHolder.Property.SetValue(value)) hasChanges = true;
                        }
                        else
                        {
                            if (customValueSetter.Invoke((value, propHolder.Property))) hasChanges = true;
                        }
                    }
                }
                else
                {
                    propHolder = PropertyFactory(key, group, propertyName, serializable);
                    if (customValueSetter == null) propHolder.Property.SetValue(value);
                    else customValueSetter.Invoke((value, propHolder.Property));
                    lock(PropertyHolders)
                    {
                        PropertyHolders.Add(propHolder);
                    }
                    hasChanges = true;
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
                return hasChanges;
            }

            if (hasChanges) OnChanged(propHolder.Key, propHolder.Group, propHolder.PropertyName);
            return hasChanges;
        }

        protected virtual T GetProperty<T>(
            string key,
            bool serializable = false,
            string group = null,
            T defaultValue = default,
            [CallerMemberName] string propertyName = null,
            Func<(T value, ObservableProperty property), bool> customValueSetter = null)
        {
            bool hasChanges = false;

            PropertyHolder propHolder = null;
            lock(PropertyHolders)
            {
                propHolder = PropertyHolders.FirstOrDefault(i => i.Key.Equals(key));
            }

            if (propHolder == null)
            {
                propHolder = PropertyFactory(key, group, propertyName, serializable);
                if (customValueSetter == null) propHolder.Property.SetValue(defaultValue);
                else customValueSetter.Invoke((defaultValue, propHolder.Property));
                lock(PropertyHolders)
                {
                    PropertyHolders.Add(propHolder);
                }
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
            }

            if (hasChanges) OnChanged(propHolder.Key, propHolder.Group, propHolder.PropertyName);
            return propHolder.Property.GetValue<T>();
        }

        protected virtual bool DeleteProperty(string key)
        {
            PropertyHolder propHolder = null;
            lock(PropertyHolders)
            {
                propHolder = PropertyHolders.FirstOrDefault(i => i.Key.Equals(key));
            }
            if (propHolder == null) return false;
            bool hasChanges = propHolder.Property.SetNull();
            if (hasChanges) OnChanged(propHolder.Key, propHolder.Group, propHolder.PropertyName);
            return hasChanges;
        }

        protected IEnumerable<PropertyHolder> GetRawProperties(string group = null)
        {
            lock(PropertyHolders)
            {
                return group == null ? PropertyHolders : PropertyHolders.Where(i => i.Group == group);
            }
        }

        public virtual void OnChanged(
            string key,
            string group,
            string propertyName)
        {
            context.Post(s =>
            {
                lock(this)
                {
                    PropertyChanged?.Invoke(this, new ObservableObjectChangesEventArgs(key, group, propertyName));
                }
            }, null);
        }

        public virtual void OnChanged(string key)
        {
            PropertyHolder propHolder = null;
            lock (PropertyHolders)
            {
                propHolder = PropertyHolders.FirstOrDefault(i => i.Key == key);
            }
            if (propHolder != null) OnChanged(key, propHolder.Group, propHolder.PropertyName);
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
