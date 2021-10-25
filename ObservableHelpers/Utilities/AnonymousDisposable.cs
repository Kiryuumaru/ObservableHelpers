using System;
using System.Collections.Generic;
using System.Text;

namespace ObservableHelpers.Utilities
{
    /// <summary>
    /// Provides disposable action with <see cref="IDisposable"/> implementation.
    /// </summary>
    public class AnonymousDisposable :
        Disposable
    {
        private readonly Action dispose;

        /// <summary>
        /// Creates an instance of <see cref="AnonymousDisposable"/> class.
        /// </summary>
        public AnonymousDisposable(Action dispose)
        {
            this.dispose = dispose;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dispose?.Invoke();
            }
            base.Dispose(disposing);
        }
    }
}
