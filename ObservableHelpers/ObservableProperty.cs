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

        public object Property
        {
            get => GetObject();
            set => SetObject(value);
        }

        private object objectHolder;

        #endregion

        #region Methods

        public override bool SetNull()
        {
            if (IsDisposed)
            {
                return false;
            }

            if (GetObject() is INullableObject model)
            {
                if (model.IsDisposed)
                {
                    return false;
                }
                else
                {
                    return model.SetNull();
                }
            }
            else
            {
                return SetObject(null);
            }
        }

        public override bool IsNull()
        {
            if (IsDisposed)
            {
                return true;
            }

            var obj = GetObject();

            if (obj is INullableObject model)
            {
                return model.IsNull();
            }
            else
            {
                return obj == null;
            }
        }

        public virtual bool SetValue<T>(T value)
        {
            if (IsDisposed)
            {
                return false;
            }

            return SetObject(value);
        }

        public virtual T GetValue<T>(T defaultValue = default)
        {
            if (IsDisposed)
            {
                return defaultValue;
            }

            if (GetObject() is T tObj)
            {
                return tObj;
            }
            else
            {
                if (defaultValue is ISyncObject sync)
                {
                    sync.SyncOperation.SetContext(this);
                }

                return defaultValue;
            }
        }

        protected virtual bool SetObject(object obj)
        {
            if (IsDisposed)
            {
                return false;
            }

            if (obj is ISyncObject sync)
            {
                sync.SyncOperation.SetContext(this);
            }

            if (!(objectHolder?.Equals(obj) ?? obj == null))
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
            if (IsDisposed)
            {
                return default;
            }

            if (objectHolder is ISyncObject sync)
            {
                sync.SyncOperation.SetContext(this);
            }

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

        #region Methods

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == nameof(Property))
            {
                base.OnPropertyChanged(nameof(Value));
            }
        }

        #endregion
    }
}
