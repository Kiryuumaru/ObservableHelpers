# Observable Helpers

Observable helpers with short-and-easy property implementations. Can be use to any MVVM software architectural applications.

**NuGets**

|Name|Info|
| ------------------- | :------------------: |
|ObservableHelpers|[![NuGet](https://buildstats.info/nuget/ObservableHelpers?includePreReleases=true)](https://www.nuget.org/packages/ObservableHelpers/)|

## Get Started

I wanted to keep this library really small and just created it for myself, but I hope others find it useful. Here is what I added in and feel free to request new things in the Issues tab.

### ObservableObject
```csharp
public string Property
{
    get => GetProperty<string>();
    set => SetProperty(value);
}
```
#### With key
```csharp
public string Property
{
    get => GetPropertyWithKey<string>("property_key");
    set => SetPropertyWithKey(value, "property_key");
}
```

Code & Inspiration from the following:
* [MVVM helpers](https://github.com/jamesmontemagno/mvvm-helpers) by [@jamesmontemagno](https://github.com/jamesmontemagno)


### Want To Support This Project?
All I have ever asked is to be active by submitting bugs, features, and sending those pull requests down!.
