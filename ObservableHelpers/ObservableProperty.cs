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

        public virtual bool SetValue<T>(T value)
        {
            VerifyNotDisposed();

            return SetObject(value);
        }

        public virtual T GetValue<T>()
        {
            VerifyNotDisposed();

            return (T)GetObject();
        }

        public virtual bool SetNull()
        {
            VerifyNotDisposed();

            return SetObject(null);
        }

        public virtual bool IsNull()
        {
            VerifyNotDisposed();

            return GetObject() == null;
        }

        protected virtual bool SetObject(object obj)
        {
            VerifyNotDisposed();

            if (objectHolder != obj)
            {
                objectHolder = obj;
                OnPropertyChanged(nameof(Property));
                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual object GetObject()
        {
            VerifyNotDisposed();

            return objectHolder;
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
