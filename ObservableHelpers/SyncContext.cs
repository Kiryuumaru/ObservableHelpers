using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;

namespace ObservableHelpers
{
    public class SyncContext : Disposable, ISynchronizationObject
    {
        public SynchronizationOperation SynchronizationOperation { get; private set; }

        public SyncContext()
        {
            SynchronizationOperation = new SynchronizationOperation();
        }
    }
}
