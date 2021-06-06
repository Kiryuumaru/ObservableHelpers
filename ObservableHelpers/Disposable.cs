﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;

namespace ObservableHelpers
{
    public abstract class Disposable : object, IDisposable
    {
        private const int DisposalNotStarted = 0;
        private const int DisposalStarted = 1;
        private const int DisposalComplete = 2;

#if DEBUG
        // very useful diagnostics when a failure to dispose is detected
        private readonly StackTrace creationStackTrace;
#endif

        // see the constants defined above for valid values
        private int disposeStage;

#if DEBUG
        /// <summary>
        /// Initializes a new instance of the DisposableBase class.
        /// </summary>
        protected DisposableBase()
        {
            creationStackTrace = new StackTrace(1, true);
        }

        /// <summary>
        /// Finalizes an instance of the DisposableBase class.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1063", Justification = "The enforced behavior of CA1063 is not thread-safe or full-featured enough for our purposes here.")]
        ~DisposableBase()
        {
            var message = string.Format(CultureInfo.InvariantCulture, "Failed to proactively dispose of object, so it is being finalized: {0}.{1}Object creation stack trace:{1}{2}", ObjectName, Environment.NewLine, creationStackTrace);
            Debug.Assert(false, message);
            Dispose(false);
        }
#endif

        /// <summary>
        /// Occurs when this object is about to be disposed.
        /// </summary>
        public event EventHandler Disposing;

        /// <summary>
        /// Gets a value indicating whether this object is in the process of disposing.
        /// </summary>
        public bool IsDisposing
        {
            get { return Interlocked.CompareExchange(ref disposeStage, DisposalStarted, DisposalStarted) == DisposalStarted; }
        }

        /// <summary>
        /// Gets a value indicating whether this object has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return Interlocked.CompareExchange(ref disposeStage, DisposalComplete, DisposalComplete) == DisposalComplete; }
        }

        /// <summary>
        /// Gets a value indicating whether this object has been disposed or is in the process of being disposed.
        /// </summary>
        public bool IsDisposedOrDisposing
        {
            get { return Interlocked.CompareExchange(ref disposeStage, DisposalNotStarted, DisposalNotStarted) != DisposalNotStarted; }
        }

        /// <summary>
        /// Gets the object name, for use in any <see cref="ObjectDisposedException"/> thrown by this object.
        /// </summary>
        /// <remarks>
        /// Subclasses can override this property if they would like more control over the object name appearing in any <see cref="ObjectDisposedException"/>
        /// thrown by this <c>DisposableBase</c>. This can be particularly useful in debugging and diagnostic scenarios.
        /// </remarks>
        /// <returns>
        /// The object name, which defaults to the class name.
        /// </returns>
        protected virtual string ObjectName
        {
            get { return GetType().FullName; }
        }

        /// <summary>
        /// Disposes of this object, if it hasn't already been disposed.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1063", Justification = "The enforced behavior of CA1063 is not thread-safe or full-featured enough for our purposes here.")]
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref disposeStage, DisposalStarted, DisposalNotStarted) != DisposalNotStarted)
            {
                return;
            }

            OnDisposing();
            Disposing = null;

            Dispose(true);
            MarkAsDisposed();
        }

        /// <summary>
        /// Verifies that this object is not in the process of disposing, throwing an exception if it is.
        /// </summary>
        protected void VerifyNotDisposing()
        {
            if (IsDisposing)
            {
                throw new ObjectDisposedException(ObjectName);
            }
        }

        /// <summary>
        /// Verifies that this object has not been disposed, throwing an exception if it is.
        /// </summary>
        protected void VerifyNotDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(ObjectName);
            }
        }

        /// <summary>
        /// Verifies that this object is not being disposed or has been disposed, throwing an exception if either of these are true.
        /// </summary>
        protected void VerifyNotDisposedOrDisposing()
        {
            if (IsDisposedOrDisposing)
            {
                throw new ObjectDisposedException(ObjectName);
            }
        }

        /// <summary>
        /// Allows subclasses to provide dispose logic.
        /// </summary>
        /// <param name="disposing">
        /// Whether the method is being called in response to disposal, or finalization.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Raises the <see cref="Disposing"/> event.
        /// </summary>
        protected virtual void OnDisposing()
        {
            Disposing?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Marks this object as disposed without running any other dispose logic.
        /// </summary>
        /// <remarks>
        /// Use this method with caution. It is helpful when you have an object that can be disposed in multiple fashions, such as through a <c>CloseAsync</c> method.
        /// </remarks>
        protected void MarkAsDisposed()
        {
            GC.SuppressFinalize(this);
            Interlocked.Exchange(ref disposeStage, DisposalComplete);
        }
    }
}