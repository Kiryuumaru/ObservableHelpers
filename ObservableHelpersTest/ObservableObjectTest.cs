using ObservableHelpers.ComponentModel;
using ObservableHelpers.ComponentModel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArgumentNullException = System.ArgumentNullException;

namespace ObservableObjectTest;

[ObservableObject]
public partial class ModelWithObservableObjectAttribute
{
    [ObservableProperty(Access = AccessModifier.ProtectedInternalWithPrivateSetter)]
    private string? _myProperty;
}

public abstract partial class ModelWithObservablePropertyAndMethod : ObservableObject
{
    [ObservableProperty]
    private bool canSave;

    /// <summary>
    /// Base method to then generate a command.
    /// </summary>
    public abstract void Save();
}

[INotifyPropertyChanged]
public partial class SampleModelWithINPCAndObservableProperties
{
    [ObservableProperty]
    private int x;

    [ObservableProperty]
    private int y;
}
