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
    public class ObservableSerializableProperty : ObservableProperty, IObservable
    {
        #region Properties

        private string BlobHolder
        {
            get => Holder.GetAttribute<string>();
            set => Holder.SetAttribute(value);
        }

        public string Blob
        {
            get => GetBlob();
            set => SetBlob(value);
        }

        #endregion

        #region Initializers

        public ObservableSerializableProperty(IAttributed attributed)
            : base (attributed)
        {

        }

        public ObservableSerializableProperty()
            : this(null)
        {

        }

        #endregion

        #region Methods

        public virtual bool SetBlob(string blob, string tag = null)
        {
            var hasChanges = false;
            lock (this)
            {
                try
                {
                    hasChanges = BlobHolder != blob;
                    if (hasChanges) BlobHolder = blob;
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            }
            if (hasChanges)
            {
                OnChanged(nameof(Blob));
                return true;
            }
            return hasChanges;
        }

        public virtual string GetBlob(string defaultValue = null, string tag = null)
        {
            lock (this)
            {
                return BlobHolder == null ? defaultValue : BlobHolder;
            }
        }

        public override bool SetValue<T>(T value, string tag = null)
        {
            try
            {
                lock (this)
                {
                    return SetBlob(Serializer.Serialize(value), tag);
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
                    return SetBlob(null, tag);
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
                    return GetBlob(null, tag) == null;
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
                    return Serializer.Deserialize(GetBlob(default, tag), defaultValue);
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

    public class ObservableSerializableProperty<T> : ObservableSerializableProperty
    {
        #region Properties

        public T Value
        {
            get => GetValue<T>();
            set => SetValue(Value);
        }

        #endregion

        #region Initializers

        public ObservableSerializableProperty(IAttributed attributed)
            : base(attributed)
        {

        }

        public ObservableSerializableProperty()
            : this(null)
        {

        }

        #endregion
    }
}
