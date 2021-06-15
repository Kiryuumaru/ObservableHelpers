using System;
using System.Collections.Generic;
using System.Text;

namespace ObservableHelpers
{
    public interface INullableObject : IDisposableObject
    {
        bool SetNull();
        bool IsNull();
    }
}
