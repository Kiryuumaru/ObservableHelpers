using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObservableHelpers
{
    public interface ISynchronizationObject
    {
        SynchronizationOperation SynchronizationOperation { get; }
    }
}
