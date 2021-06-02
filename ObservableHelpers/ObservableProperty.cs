using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace ObservableHelpers
{
    public class ObservableProperty : IObservable
    {
        #region Properties

        private readonly SynchronizationContext context = AsyncOperationManager.SynchronizationContext;

        private object objectHolder;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<Exception> PropertyError;

        public object Property
        {
            get => GetObject();
            set => SetObject(value);
        }

        #endregion

        #region Methods

        public virtual bool SetValue<T>(T value, object parameter = null)
        {
            try
            {
                return SetObject(value, parameter);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return false;
        }

        public virtual T GetValue<T>(T defaultValue = default, object parameter = null)
        {
            try
            {
                return (T)GetObject(defaultValue, parameter);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return defaultValue;
        }

        public virtual bool SetNull(object parameter = null)
        {
            try
            {
                return SetObject(null, parameter);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return false;
        }

        public virtual bool IsNull(object parameter = null)
        {
            try
            {
                return GetObject(null, parameter) == null;
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return true;
        }

        public virtual void OnChanged(string propertyName = "")
        {
            context.Post(s =>
            {
                lock (this)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }
            }, null);
        }

        public virtual void OnError(Exception exception)
        {
            PropertyError?.Invoke(this, exception);
        }

        protected virtual bool SetObject(object obj, object parameter = null)
        {
            var hasChanges = false;
            lock (this)
            {
                hasChanges = objectHolder != obj;
                if (hasChanges) objectHolder = obj;
            }
            if (hasChanges)
            {
                OnChanged(nameof(Property));
                return true;
            }
            return hasChanges;
        }

        protected virtual object GetObject(object defaultValue = null, object parameter = null)
        {
            lock (this)
            {
                return objectHolder ?? defaultValue;
            }
        }

        #endregion
    }

    public class ObservableProperty<T> : ObservableProperty
    {
        #region Properties

        public T Value
        {
            get => GetValue<T>();
            set => SetValue(Value);
        }

        #endregion
    }
}
