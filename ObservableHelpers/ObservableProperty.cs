using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace ObservableHelpers
{
    public class ObservableProperty : Observable
    {
        #region Properties

        private object objectHolder;

        public object Property
        {
            get => GetObject();
            set => SetObject(value);
        }

        #endregion

        #region Methods

        public virtual bool SetValue<T>(T value, object parameter = null)
        {
            VerifyNotDisposed();

            return SetObject(value, parameter);
        }

        public virtual T GetValue<T>(T defaultValue = default, object parameter = null)
        {
            VerifyNotDisposed();

            return (T)GetObject(defaultValue, parameter);
        }

        public virtual bool SetNull(object parameter = null)
        {
            VerifyNotDisposed();

            return SetObject(null, parameter);
        }

        public virtual bool IsNull(object parameter = null)
        {
            VerifyNotDisposed();

            return GetObject(null, parameter) == null;
        }

        protected virtual bool SetObject(object obj, object parameter = null)
        {
            VerifyNotDisposed();

            var hasChanges = false;
            lock (this)
            {
                hasChanges = objectHolder != obj;
                if (hasChanges) objectHolder = obj;
            }
            if (hasChanges)
            {
                OnPropertyChanged(nameof(Property));
                return true;
            }
            return hasChanges;
        }

        protected virtual object GetObject(object defaultValue = null, object parameter = null)
        {
            VerifyNotDisposed();

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
