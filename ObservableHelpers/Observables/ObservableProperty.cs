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
using System.Threading.Tasks;

namespace ObservableHelpers.Observables
{
    public abstract class ObservableProperty : IObservable
    {
        #region Properties

        private readonly SynchronizationContext context = AsyncOperationManager.SynchronizationContext;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<ContinueExceptionEventArgs> PropertyError;

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

        public abstract bool SetValue<T>(T value, string tag = null);

        public abstract bool SetNull(string tag = null);

        public abstract bool IsNull(string tag = null);

        public abstract T GetValue<T>(T defaultValue = default, string tag = null);

        #endregion
    }
}
