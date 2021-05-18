﻿using System;
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
        public event EventHandler<ContinueExceptionEventArgs> PropertyError;

        public object Property
        {
            get => GetObject();
            set => SetObject(value);
        }

        #endregion

        #region Methods

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

        public virtual bool SetObject(object obj, string tag = null)
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

        public virtual object GetObject(object defaultValue = null, string tag = null)
        {
            lock (this)
            {
                return objectHolder ?? defaultValue;
            }
        }

        public virtual bool SetValue<T>(T value, string tag = null)
        {
            try
            {
                return SetObject(value, tag);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return false;
        }

        public virtual T GetValue<T>(T defaultValue = default, string tag = null)
        {
            try
            {
                return (T)GetObject(defaultValue, tag);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return defaultValue;
        }

        public bool SetNull(string tag = null)
        {
            try
            {
                return SetObject(null, tag);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return false;
        }

        public bool IsNull(string tag = null)
        {
            try
            {
                return GetObject(null, tag) == null;
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return true;
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
