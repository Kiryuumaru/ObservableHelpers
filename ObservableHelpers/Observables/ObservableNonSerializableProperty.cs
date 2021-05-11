using ObservableHelpers.Serializers;
using ObservableHelpers.Serializers.Additionals;
using ObservableHelpers.Serializers.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace ObservableHelpers.Observables
{
    public class ObservableNonSerializableProperty : ObservableProperty, IObservable
    {
        #region Properties

        private object objectHolder;

        public override object Property
        {
            get => GetObject();
            set => SetObject(value);
        }

        #endregion

        #region Methods

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

        public override bool SetValue<T>(T value, string tag = null)
        {
            try
            {
                lock (this)
                {
                    return SetObject(value, tag);
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return false;
        }

        public override bool SetNull(string tag = null)
        {
            try
            {
                lock (this)
                {
                    return SetObject(null, tag);
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return false;
        }

        public override bool IsNull(string tag = null)
        {
            try
            {
                lock (this)
                {
                    return GetObject(null, tag) == null;
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return true;
        }

        public override T GetValue<T>(T defaultValue = default, string tag = null)
        {
            try
            {
                lock (this)
                {
                    return (T)GetObject(defaultValue, tag);
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return defaultValue;
        }

        #endregion
    }

    public class ObservableNonSerializableProperty<T> : ObservableNonSerializableProperty
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
