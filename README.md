# Observable Helpers

Observable helpers with short-and-easy and UI-safe property implementations. Can be used for any MVVM software architectural applications.

**NuGets**

|Name|Info|
| ------------------- | :------------------: |
|ObservableHelpers|[![NuGet](https://buildstats.info/nuget/ObservableHelpers?includePreReleases=true)](https://www.nuget.org/packages/ObservableHelpers/)|

## Get Started

All observable events are executed on thread that was used to create the object instance.
To use in UI safe updates, create the object instances at the UI thread or manually configure the ISyncObject.SyncOperation to use UI thread.

### ObservableObject
```csharp
public string Property
{
    get => GetProperty<string>();
    set => SetProperty(value);
}
public string PropertyWithKey
{
    get => GetPropertyWithKey<string>("property_key");
    set => SetPropertyWithKey(value, "property_key");
}
```

Code & Inspiration from the following:
* [MVVM helpers](https://github.com/jamesmontemagno/mvvm-helpers) by [@jamesmontemagno](https://github.com/jamesmontemagno)


### Want To Support This Project?
All I have ever asked is to be active by submitting bugs, features, and sending those pull requests down!.
