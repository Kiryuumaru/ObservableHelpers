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
            get => GetObjectCore();
            set => SetObjectCore(value);
        }

        private object objectHolder;

        #endregion

        #region Methods

        public override bool SetNull()
        {
            VerifyNotDisposed();

            if (GetObjectCore() is INullableObject model)
            {
                return model.SetNull();
            }
            else
            {
                return SetObjectCore(null);
            }
        }

        public override bool IsNull()
        {
            VerifyNotDisposed();

            var obj = GetObjectCore();

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
            VerifyNotDisposed();

            return SetObjectCore(value);
        }

        public virtual T GetValue<T>(T defaultValue = default)
        {
            VerifyNotDisposed();

            if (GetObjectCore() is T tObj)
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

        protected virtual bool SetObjectCore(object obj)
        {
            VerifyNotDisposed();

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

        protected virtual object GetObjectCore()
        {
            VerifyNotDisposed();

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
