# Observable Helpers

Observable helpers with source generator and UI-safe property implementations. Can be used for any MVVM software architectural applications.

**NuGets**

|Name|Info|
| ------------------- | :------------------: |
|ObservableHelpers|[![NuGet](https://buildstats.info/nuget/ObservableHelpers?includePreReleases=true)](https://www.nuget.org/packages/ObservableHelpers/)|

## Installation
```csharp
// Install release version
Install-Package ObservableHelpers

// Install pre-release version
Install-Package ObservableHelpers -pre
```

## Supported frameworks
.NET Standard 2.0 and above - see https://github.com/dotnet/standard/blob/master/docs/versions.md for compatibility matrix

## Get Started

All observable events are executed on thread that was used to create the object instance.
To use in UI safe updates, create the object instances at the UI thread or manually configure the ISyncObject.SyncOperation to use UI thread.

## Usage

### ObservableObject Sample
```csharp
using ObservableHelpers;

namespace YourNamespace
{
    [ObservableObject]
    public class Dinosaur : ObservableObject
    {
        [ObservableProperty]
        string? name;
        
        [ObservableProperty]
        string? family;
        
        [ObservableProperty]
        int Height;
    }
}
```
### UI safe
```csharp
using ObservableHelpers;

namespace YourNamespace
{
    public class Program
    {
        private Dinosaur dinosaur;

        public void UIThread()
        {
            dinosaur = new Dinosaur(); // Must be created on UI thread to synchronize events

            dinosaur.SynchronizeChangedEvent = true;
        }

        public void BackgroundThread()
        {
            dinosaur.PropertyChanged += (s, e) =>
            {
                // Executes on UI thread if dinosaur.SynchronizeChangedEvent is true (default false)
            }
            dinosaur.SynchronizedPropertyChanged += (s, e) =>
            {
                // Executed on UI thread
            }
            dinosaur.UnsynchronizedPropertyChanged += (s, e) =>
            {
                // Executed on current thread
            }
            dinosaur.Name = "Megalosaurus";
        }
    }
}
```

Code & Inspiration from the following:
* [MVVM helpers](https://github.com/jamesmontemagno/mvvm-helpers) by [@jamesmontemagno](https://github.com/jamesmontemagno)
* [.NET Community Toolkit](https://github.com/CommunityToolkit/dotnet) by [@CommunityToolkit](https://github.com/CommunityToolkit)


### Want To Support This Project?
All I have ever asked is to be active by submitting bugs, features, and sending those pull requests down!.

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://ko-fi.com/kiryuumaru)
