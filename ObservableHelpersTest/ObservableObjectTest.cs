using ObservableHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ObservableObjectTest;

public class SampleObject : ObservableObject
{
    public string? SampleProp1
    {
        get => GetProperty<string?>();
        set => SetProperty(value);
    }

    public DateTime SampleProp2
    {
        get => GetProperty<DateTime>(groups: new string[] { "group" });
        set => SetProperty(value);
    }

    public string SampleProp3
    {
        get => GetProperty(() => "defaultValue");
        set => SetProperty(value);
    }

    public string? SampleKeyProp1
    {
        get => GetProperty<string?>(() => default, "keySample1");
        set => SetProperty(value, "keySample1");
    }

    public DateTime SampleKeyProp2
    {
        get => GetProperty<DateTime>(() => default, "keySample2", groups: new string[] { "group" });
        set => SetProperty(value, "keySample2");
    }

    public string SampleKeyProp3
    {
        get => GetProperty(() => "defaultValue", "keySample3");
        set => SetProperty(value, "keySample3");
    }

    public IEnumerable<NamedProperty> GetRawPropertiesExposed(string? group = null) => base.GetRawProperties(group);
}

public class ConstructorTest
{
    [Fact]
    public void Parameterless()
    {
        var obj = new SampleObject();

        Assert.Null(obj.SampleProp1);
        Assert.Equal(default, obj.SampleProp2);
        Assert.Equal("defaultValue", obj.SampleProp3);

        var props = obj.GetRawPropertiesExposed();

        Assert.Equal(3, props.Count());
    }
}

public class GetPropertyTest : SampleObject
{
    [Fact]
    public void Normal()
    {
        var raiseCol = new List<ObjectPropertyChangesEventArgs>();
        var obj = new GetPropertyTest();

        obj.InitializeProperties();

        obj.ImmediatePropertyChanged += (s, e) =>
        {
            raiseCol.Add((ObjectPropertyChangesEventArgs)e);
        };

        bool hasChanges = false;

        Assert.Null(obj.GetProperty<string>(null, "Sample1", null,
            args =>
            {
                Assert.Null(args.oldValue);
                Assert.Null(args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.Null(obj.GetProperty<string?>(() => null, null, "Sample2", null,
            args =>
            {
                Assert.Null(args.oldValue);
                Assert.Null(args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.Equal("sampleDefault2", obj.GetProperty(() => "sampleDefault2", null, "Sample3", null,
            args =>
            {
                Assert.Null(args.oldValue);
                Assert.Equal("sampleDefault2", args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.Equal("sampleDefault2", obj.GetProperty(() => "sampleDefault3", null, "Sample3", null,
            args =>
            {
                Assert.Equal("sampleDefault2", args.oldValue);
                Assert.Equal("sampleDefault2", args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.False(hasChanges);

        hasChanges = false;

        Assert.Equal("groupDefault1", obj.GetProperty(() => "groupDefault1", null, "Sample4", new string[] { "group1" },
            args =>
            {
                Assert.Null(args.oldValue);
                Assert.Equal("groupDefault1", args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.Equal("groupDefault2", obj.GetProperty(() => "groupDefault2", null, "Sample5", new string[] { "group1" },
            args =>
            {
                Assert.Null(args.oldValue);
                Assert.Equal("groupDefault2", args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.Equal("groupDefault3", obj.GetProperty(() => "groupDefault3", null, "Sample6", new string[] { "group1" },
            args =>
            {
                Assert.Null(args.oldValue);
                Assert.Equal("groupDefault3", args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.Equal("groupDefault4", obj.GetProperty(() => "groupDefault4", null, "Sample7", new string[] { "group2" },
            args =>
            {
                Assert.Null(args.oldValue);
                Assert.Equal("groupDefault4", args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.Equal("groupDefault5", obj.GetProperty(() => "groupDefault5", null, "Sample8", new string[] { "group2" },
            args =>
            {
                Assert.Null(args.oldValue);
                Assert.Equal("groupDefault5", args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.True(hasChanges);

        hasChanges = false;

        var group0 = obj.GetRawProperties();
        var group1 = obj.GetRawProperties("group1");
        var group2 = obj.GetRawProperties("group2");

        Assert.Equal(14, group0.Count());
        Assert.Equal(3, group1.Count());
        Assert.Equal(2, group2.Count());

        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp1) && i.Groups == null && i.Property.Value == default);
        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp2) && (i.Groups?.Contains("group") ?? false) && (i.Property as ObservableProperty<DateTime>)?.Value == default);
        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp3) && i.Groups == null && i.Property.Value?.ToString() == "defaultValue");
        Assert.Contains(group0, i => i.Key == "keySample1" && i.PropertyName == nameof(SampleKeyProp1) && i.Groups == null && i.Property.Value == default);
        Assert.Contains(group0, i => i.Key == "keySample2" && i.PropertyName == nameof(SampleKeyProp2) && (i.Groups?.Contains("group") ?? false) && (i.Property as ObservableProperty<DateTime>)?.Value == default);
        Assert.Contains(group0, i => i.Key == "keySample3" && i.PropertyName == nameof(SampleKeyProp3) && i.Groups == null && i.Property.Value?.ToString() == "defaultValue");
        Assert.Contains(group0, i => i.PropertyName == "Sample1" && i.Key == null && i.Groups == null && i.Property.Value?.ToString() == null);
        Assert.Contains(group0, i => i.PropertyName == "Sample2" && i.Key == null && i.Groups == null && i.Property.Value?.ToString() == null);
        Assert.Contains(group0, i => i.PropertyName == "Sample3" && i.Key == null && i.Groups == null && i.Property.Value?.ToString() == "sampleDefault2");
        Assert.Contains(group1, i => i.PropertyName == "Sample4" && i.Key == null && (i.Groups?.Contains("group1") ?? false) && i.Property.Value?.ToString() == "groupDefault1");
        Assert.Contains(group1, i => i.PropertyName == "Sample5" && i.Key == null && (i.Groups?.Contains("group1") ?? false) && i.Property.Value?.ToString() == "groupDefault2");
        Assert.Contains(group1, i => i.PropertyName == "Sample6" && i.Key == null && (i.Groups?.Contains("group1") ?? false) && i.Property.Value?.ToString() == "groupDefault3");
        Assert.Contains(group2, i => i.PropertyName == "Sample7" && i.Key == null && (i.Groups?.Contains("group2") ?? false) && i.Property.Value?.ToString() == "groupDefault4");
        Assert.Contains(group2, i => i.PropertyName == "Sample8" && i.Key == null && (i.Groups?.Contains("group2") ?? false) && i.Property.Value?.ToString() == "groupDefault5");

        Assert.Equal(8, raiseCol.Count);

        Assert.Equal("Sample1", raiseCol[0].PropertyName);
        Assert.Null(raiseCol[0].Key);
        Assert.Null(raiseCol[0].Groups);
        Assert.Equal("Sample2", raiseCol[1].PropertyName);
        Assert.Null(raiseCol[1].Key);
        Assert.Null(raiseCol[1].Groups);
        Assert.Equal("Sample3", raiseCol[2].PropertyName);
        Assert.Null(raiseCol[2].Key);
        Assert.Null(raiseCol[2].Groups);
        Assert.Equal("Sample4", raiseCol[3].PropertyName);
        Assert.Null(raiseCol[3].Key);
        Assert.Equal("group1", raiseCol[3].Groups?[0]);
        Assert.Equal("Sample5", raiseCol[4].PropertyName);
        Assert.Null(raiseCol[4].Key);
        Assert.Equal("group1", raiseCol[4].Groups?[0]);
        Assert.Equal("Sample6", raiseCol[5].PropertyName);
        Assert.Null(raiseCol[5].Key);
        Assert.Equal("group1", raiseCol[5].Groups?[0]);
        Assert.Equal("Sample7", raiseCol[6].PropertyName);
        Assert.Null(raiseCol[6].Key);
        Assert.Equal("group2", raiseCol[6].Groups?[0]);
        Assert.Equal("Sample8", raiseCol[7].PropertyName);
        Assert.Null(raiseCol[7].Key);
        Assert.Equal("group2", raiseCol[7].Groups?[0]);
    }

    [Fact]
    public void WithKey()
    {
        var raiseCol = new List<ObjectPropertyChangesEventArgs>();
        var obj = new GetPropertyTest();

        obj.InitializeProperties();

        obj.ImmediatePropertyChanged += (s, e) =>
        {
            raiseCol.Add((ObjectPropertyChangesEventArgs)e);
        };

        bool hasChanges = false;

        Assert.Null(obj.GetProperty<string>("sample1", null, null,
            args =>
            {
                Assert.Null(args.oldValue);
                Assert.Null(args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.Null(obj.GetProperty<string?>(() => null, "sample2", null, null,
            args =>
            {
                Assert.Null(args.oldValue);
                Assert.Null(args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.Equal("sampleDefault2", obj.GetProperty(() => "sampleDefault2", "sample3", null, null,
            args =>
            {
                Assert.Null(args.oldValue);
                Assert.Equal("sampleDefault2", args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.Equal("sampleDefault2", obj.GetProperty(() => "sampleDefault3", "sample3", null, null,
            args =>
            {
                Assert.Equal("sampleDefault2", args.oldValue);
                Assert.Equal("sampleDefault2", args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.False(hasChanges);

        hasChanges = false;

        Assert.Equal("groupDefault1", obj.GetProperty(() => "groupDefault1", "sample4", null, new string[] { "group1" },
            args =>
            {
                Assert.Null(args.oldValue);
                Assert.Equal("groupDefault1", args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.Equal("groupDefault2", obj.GetProperty(() => "groupDefault2", "sample5", null, new string[] { "group1" },
            args =>
            {
                Assert.Null(args.oldValue);
                Assert.Equal("groupDefault2", args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.Equal("groupDefault3", obj.GetProperty(() => "groupDefault3", "sample6", null, new string[] { "group1" },
            args =>
            {
                Assert.Null(args.oldValue);
                Assert.Equal("groupDefault3", args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.Equal("groupDefault4", obj.GetProperty(() => "groupDefault4", "sample7", null, new string[] { "group2" },
            args =>
            {
                Assert.Null(args.oldValue);
                Assert.Equal("groupDefault4", args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.Equal("groupDefault5", obj.GetProperty(() => "groupDefault5", "sample8", null, new string[] { "group2" },
            args =>
            {
                Assert.Null(args.oldValue);
                Assert.Equal("groupDefault5", args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.True(hasChanges);

        hasChanges = false;

        var group0 = obj.GetRawProperties();
        var group1 = obj.GetRawProperties("group1");
        var group2 = obj.GetRawProperties("group2");

        Assert.Equal(14, group0.Count());
        Assert.Equal(3, group1.Count());
        Assert.Equal(2, group2.Count());

        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp1) && i.Groups == null && i.Property.Value == default);
        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp2) && (i.Groups?.Contains("group") ?? false) && (i.Property as ObservableProperty<DateTime>)?.Value == default);
        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp3) && i.Groups == null && i.Property.Value?.ToString() == "defaultValue");
        Assert.Contains(group0, i => i.Key == "keySample1" && i.PropertyName == nameof(SampleKeyProp1) && i.Groups == null && i.Property.Value == default);
        Assert.Contains(group0, i => i.Key == "keySample2" && i.PropertyName == nameof(SampleKeyProp2) && (i.Groups?.Contains("group") ?? false) && (i.Property as ObservableProperty<DateTime>)?.Value == default);
        Assert.Contains(group0, i => i.Key == "keySample3" && i.PropertyName == nameof(SampleKeyProp3) && i.Groups == null && i.Property.Value?.ToString() == "defaultValue");
        Assert.Contains(group0, i => i.Key == "sample1" && i.PropertyName == null && i.Groups == null && i.Property.Value?.ToString() == null);
        Assert.Contains(group0, i => i.Key == "sample2" && i.PropertyName == null && i.Groups == null && i.Property.Value?.ToString() == null);
        Assert.Contains(group0, i => i.Key == "sample3" && i.PropertyName == null && i.Groups == null && i.Property.Value?.ToString() == "sampleDefault2");
        Assert.Contains(group0, i => i.Key == "sample4" && i.PropertyName == null && (i.Groups?.Contains("group1") ?? false) && i.Property.Value?.ToString() == "groupDefault1");
        Assert.Contains(group1, i => i.Key == "sample5" && i.PropertyName == null && (i.Groups?.Contains("group1") ?? false) && i.Property.Value?.ToString() == "groupDefault2");
        Assert.Contains(group1, i => i.Key == "sample6" && i.PropertyName == null && (i.Groups?.Contains("group1") ?? false) && i.Property.Value?.ToString() == "groupDefault3");
        Assert.Contains(group2, i => i.Key == "sample7" && i.PropertyName == null && (i.Groups?.Contains("group2") ?? false) && i.Property.Value?.ToString() == "groupDefault4");
        Assert.Contains(group2, i => i.Key == "sample8" && i.PropertyName == null && (i.Groups?.Contains("group2") ?? false) && i.Property.Value?.ToString() == "groupDefault5");

        Assert.Equal(8, raiseCol.Count);

        Assert.Null(raiseCol[0].PropertyName);
        Assert.Equal("sample1", raiseCol[0].Key);
        Assert.Null(raiseCol[0].Groups);
        Assert.Null(raiseCol[1].PropertyName);
        Assert.Equal("sample2", raiseCol[1].Key);
        Assert.Null(raiseCol[1].Groups);
        Assert.Null(raiseCol[2].PropertyName);
        Assert.Equal("sample3", raiseCol[2].Key);
        Assert.Null(raiseCol[2].Groups);
        Assert.Null(raiseCol[3].PropertyName);
        Assert.Equal("sample4", raiseCol[3].Key);
        Assert.Equal("group1", raiseCol[3].Groups?[0]);
        Assert.Null(raiseCol[4].PropertyName);
        Assert.Equal("sample5", raiseCol[4].Key);
        Assert.Equal("group1", raiseCol[4].Groups?[0]);
        Assert.Null(raiseCol[5].PropertyName);
        Assert.Equal("sample6", raiseCol[5].Key);
        Assert.Equal("group1", raiseCol[5].Groups?[0]);
        Assert.Null(raiseCol[6].PropertyName);
        Assert.Equal("sample7", raiseCol[6].Key);
        Assert.Equal("group2", raiseCol[6].Groups?[0]);
        Assert.Null(raiseCol[7].PropertyName);
        Assert.Equal("sample8", raiseCol[7].Key);
        Assert.Equal("group2", raiseCol[7].Groups?[0]);
    }

    [Fact]
    public void WithBoth()
    {
        var raiseCol = new List<ObjectPropertyChangesEventArgs>();
        var obj = new GetPropertyTest();

        obj.InitializeProperties();

        obj.ImmediatePropertyChanged += (s, e) =>
        {
            raiseCol.Add((ObjectPropertyChangesEventArgs)e);
        };

        bool hasChanges = false;

        Assert.Null(obj.GetProperty<string>("sample1", "Sample1", null,
            args =>
            {
                Assert.Null(args.oldValue);
                Assert.Null(args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.Null(obj.GetProperty<string?>(() => null, "sample2", "Sample2", null,
            args =>
            {
                Assert.Null(args.oldValue);
                Assert.Null(args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.Equal("sampleDefault2", obj.GetProperty(() => "sampleDefault2", "sample3", "Sample3", null,
            args =>
            {
                Assert.Null(args.oldValue);
                Assert.Equal("sampleDefault2", args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.Equal("sampleDefault2", obj.GetProperty(() => "sampleDefault3", "sample3", "Sample3", null,
            args =>
            {
                Assert.Equal("sampleDefault2", args.oldValue);
                Assert.Equal("sampleDefault2", args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.False(hasChanges);

        hasChanges = false;

        Assert.Equal("groupDefault1", obj.GetProperty(() => "groupDefault1", "sample4", "Sample4", new string[] { "group1" },
            args =>
            {
                Assert.Null(args.oldValue);
                Assert.Equal("groupDefault1", args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.Equal("groupDefault2", obj.GetProperty(() => "groupDefault2", "sample5", "Sample5", new string[] { "group1" },
            args =>
            {
                Assert.Null(args.oldValue);
                Assert.Equal("groupDefault2", args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.Equal("groupDefault3", obj.GetProperty(() => "groupDefault3", "sample6", "Sample6", new string[] { "group1" },
            args =>
            {
                Assert.Null(args.oldValue);
                Assert.Equal("groupDefault3", args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.Equal("groupDefault4", obj.GetProperty(() => "groupDefault4", "sample7", "Sample7", new string[] { "group2" },
            args =>
            {
                Assert.Null(args.oldValue);
                Assert.Equal("groupDefault4", args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.Equal("groupDefault5", obj.GetProperty(() => "groupDefault5", "sample8", "Sample8", new string[] { "group2" },
            args =>
            {
                Assert.Null(args.oldValue);
                Assert.Equal("groupDefault5", args.newValue);
                hasChanges = args.hasChanges;
            }));

        Assert.True(hasChanges);

        hasChanges = false;

        var group0 = obj.GetRawProperties();
        var group1 = obj.GetRawProperties("group1");
        var group2 = obj.GetRawProperties("group2");

        Assert.Equal(14, group0.Count());
        Assert.Equal(3, group1.Count());
        Assert.Equal(2, group2.Count());

        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp1) && i.Groups == null && i.Property.Value == default);
        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp2) && (i.Groups?.Contains("group") ?? false) && (i.Property as ObservableProperty<DateTime>)?.Value == default);
        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp3) && i.Groups == null && i.Property.Value?.ToString() == "defaultValue");
        Assert.Contains(group0, i => i.Key == "keySample1" && i.PropertyName == nameof(SampleKeyProp1) && i.Groups == null && i.Property.Value == default);
        Assert.Contains(group0, i => i.Key == "keySample2" && i.PropertyName == nameof(SampleKeyProp2) && (i.Groups?.Contains("group") ?? false) && (i.Property as ObservableProperty<DateTime>)?.Value == default);
        Assert.Contains(group0, i => i.Key == "keySample3" && i.PropertyName == nameof(SampleKeyProp3) && i.Groups == null && i.Property.Value?.ToString() == "defaultValue");
        Assert.Contains(group0, i => i.Key == "sample1" && i.PropertyName == "Sample1" && i.Groups == null && i.Property.Value?.ToString() == null);
        Assert.Contains(group0, i => i.Key == "sample2" && i.PropertyName == "Sample2" && i.Groups == null && i.Property.Value?.ToString() == null);
        Assert.Contains(group0, i => i.Key == "sample3" && i.PropertyName == "Sample3" && i.Groups == null && i.Property.Value?.ToString() == "sampleDefault2");
        Assert.Contains(group0, i => i.Key == "sample4" && i.PropertyName == "Sample4" && (i.Groups?.Contains("group1") ?? false) && i.Property.Value?.ToString() == "groupDefault1");
        Assert.Contains(group1, i => i.Key == "sample5" && i.PropertyName == "Sample5" && (i.Groups?.Contains("group1") ?? false) && i.Property.Value?.ToString() == "groupDefault2");
        Assert.Contains(group1, i => i.Key == "sample6" && i.PropertyName == "Sample6" && (i.Groups?.Contains("group1") ?? false) && i.Property.Value?.ToString() == "groupDefault3");
        Assert.Contains(group2, i => i.Key == "sample7" && i.PropertyName == "Sample7" && (i.Groups?.Contains("group2") ?? false) && i.Property.Value?.ToString() == "groupDefault4");
        Assert.Contains(group2, i => i.Key == "sample8" && i.PropertyName == "Sample8" && (i.Groups?.Contains("group2") ?? false) && i.Property.Value?.ToString() == "groupDefault5");

        Assert.Equal(8, raiseCol.Count);

        Assert.Equal("Sample1", raiseCol[0].PropertyName);
        Assert.Equal("sample1", raiseCol[0].Key);
        Assert.Null(raiseCol[0].Groups);
        Assert.Equal("Sample2", raiseCol[1].PropertyName);
        Assert.Equal("sample2", raiseCol[1].Key);
        Assert.Null(raiseCol[1].Groups);
        Assert.Equal("Sample3", raiseCol[2].PropertyName);
        Assert.Equal("sample3", raiseCol[2].Key);
        Assert.Null(raiseCol[2].Groups);
        Assert.Equal("Sample4", raiseCol[3].PropertyName);
        Assert.Equal("sample4", raiseCol[3].Key);
        Assert.Equal("group1", raiseCol[3].Groups?[0]);
        Assert.Equal("Sample5", raiseCol[4].PropertyName);
        Assert.Equal("sample5", raiseCol[4].Key);
        Assert.Equal("group1", raiseCol[4].Groups?[0]);
        Assert.Equal("Sample6", raiseCol[5].PropertyName);
        Assert.Equal("sample6", raiseCol[5].Key);
        Assert.Equal("group1", raiseCol[5].Groups?[0]);
        Assert.Equal("Sample7", raiseCol[6].PropertyName);
        Assert.Equal("sample7", raiseCol[6].Key);
        Assert.Equal("group2", raiseCol[6].Groups?[0]);
        Assert.Equal("Sample8", raiseCol[7].PropertyName);
        Assert.Equal("sample8", raiseCol[7].Key);
        Assert.Equal("group2", raiseCol[7].Groups?[0]);
    }
}

public class SetPropertyTest : SampleObject
{
    [Fact]
    public void Normal()
    {
        var raiseCol = new List<ObjectPropertyChangesEventArgs>();
        var obj = new SetPropertyTest();

        obj.InitializeProperties();

        obj.ImmediatePropertyChanged += (s, e) =>
        {
            raiseCol.Add((ObjectPropertyChangesEventArgs)e);
        };

        bool hasChanges = false;

        Assert.False(obj.SetProperty(default(string), null, "Sample1", null, () =>
        {
            return false;
        }, args =>
        {
            Assert.Null(args.oldValue);
            Assert.Null(args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.False(hasChanges);

        hasChanges = false;

        Assert.False(obj.SetProperty("sampleDefault1", null, "Sample2", null, () =>
        {
            return false;
        }, args =>
        {
            Assert.Null(args.oldValue);
            Assert.Null(args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.False(hasChanges);

        hasChanges = false;

        Assert.True(obj.SetProperty("sampleDefault2", null, "Sample3", null, () =>
        {
            return true;
        }, args =>
        {
            Assert.Null(args.oldValue);
            Assert.Equal("sampleDefault2", args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.True(obj.SetProperty("sampleDefault3", null, "Sample3", null, () =>
        {
            return true;
        }, args =>
        {
            Assert.Equal("sampleDefault2", args.oldValue);
            Assert.Equal("sampleDefault3", args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.False(obj.SetProperty("sampleDefault3", null, "Sample3", null, () =>
        {
            return true;
        }, args =>
        {
            Assert.Equal("sampleDefault3", args.oldValue);
            Assert.Equal("sampleDefault3", args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.False(hasChanges);

        hasChanges = false;

        Assert.True(obj.SetProperty("groupDefault1", null, "Sample4", new string[] { "group1" }, () =>
        {
            return true;
        }, args =>
        {
            Assert.Null(args.oldValue);
            Assert.Equal("groupDefault1", args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.True(obj.SetProperty("groupDefault2", null, "Sample5", new string[] { "group1" }, () =>
        {
            return true;
        }, args =>
        {
            Assert.Null(args.oldValue);
            Assert.Equal("groupDefault2", args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.True(obj.SetProperty("groupDefault3", null, "Sample6", new string[] { "group1" }, () =>
        {
            return true;
        }, args =>
        {
            Assert.Null(args.oldValue);
            Assert.Equal("groupDefault3", args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.True(obj.SetProperty("groupDefault4", null, "Sample7", new string[] { "group2" }, () =>
        {
            return true;
        }, args =>
        {
            Assert.Null(args.oldValue);
            Assert.Equal("groupDefault4", args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.True(obj.SetProperty("groupDefault5", null, "Sample8", new string[] { "group2" }, () =>
        {
            return true;
        }, args =>
        {
            Assert.Null(args.oldValue);
            Assert.Equal("groupDefault5", args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.True(hasChanges);

        hasChanges = false;

        var group0 = obj.GetRawProperties();
        var group1 = obj.GetRawProperties("group1");
        var group2 = obj.GetRawProperties("group2");

        Assert.Equal(12, group0.Count());
        Assert.Equal(3, group1.Count());
        Assert.Equal(2, group2.Count());

        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp1) && i.Groups == null && i.Property.Value == default);
        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp2) && (i.Groups?.Contains("group") ?? false) && (i.Property as ObservableProperty<DateTime>)?.Value == default);
        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp3) && i.Groups == null && i.Property.Value?.ToString() == "defaultValue");
        Assert.Contains(group0, i => i.Key == "keySample1" && i.PropertyName == nameof(SampleKeyProp1) && i.Groups == null && i.Property.Value == default);
        Assert.Contains(group0, i => i.Key == "keySample2" && i.PropertyName == nameof(SampleKeyProp2) && (i.Groups?.Contains("group") ?? false) && (i.Property as ObservableProperty<DateTime>)?.Value == default);
        Assert.Contains(group0, i => i.Key == "keySample3" && i.PropertyName == nameof(SampleKeyProp3) && i.Groups == null && i.Property.Value?.ToString() == "defaultValue");
        Assert.Contains(group0, i => i.PropertyName == "Sample3" && i.Key == null && i.Groups == null && i.Property.Value?.ToString() == "sampleDefault3");
        Assert.Contains(group1, i => i.PropertyName == "Sample4" && i.Key == null && (i.Groups?.Contains("group1") ?? false) && i.Property.Value?.ToString() == "groupDefault1");
        Assert.Contains(group1, i => i.PropertyName == "Sample5" && i.Key == null && (i.Groups?.Contains("group1") ?? false) && i.Property.Value?.ToString() == "groupDefault2");
        Assert.Contains(group1, i => i.PropertyName == "Sample6" && i.Key == null && (i.Groups?.Contains("group1") ?? false) && i.Property.Value?.ToString() == "groupDefault3");
        Assert.Contains(group2, i => i.PropertyName == "Sample7" && i.Key == null && (i.Groups?.Contains("group2") ?? false) && i.Property.Value?.ToString() == "groupDefault4");
        Assert.Contains(group2, i => i.PropertyName == "Sample8" && i.Key == null && (i.Groups?.Contains("group2") ?? false) && i.Property.Value?.ToString() == "groupDefault5");

        Assert.Equal(7, raiseCol.Count);

        Assert.Equal("Sample3", raiseCol[0].PropertyName);
        Assert.Null(raiseCol[0].Key);
        Assert.Null(raiseCol[0].Groups);
        Assert.Equal("Sample3", raiseCol[1].PropertyName);
        Assert.Null(raiseCol[1].Key);
        Assert.Null(raiseCol[1].Groups);
        Assert.Equal("Sample4", raiseCol[2].PropertyName);
        Assert.Null(raiseCol[2].Key);
        Assert.Equal("group1", raiseCol[2].Groups?[0]);
        Assert.Equal("Sample5", raiseCol[3].PropertyName);
        Assert.Null(raiseCol[3].Key);
        Assert.Equal("group1", raiseCol[3].Groups?[0]);
        Assert.Equal("Sample6", raiseCol[4].PropertyName);
        Assert.Null(raiseCol[4].Key);
        Assert.Equal("group1", raiseCol[4].Groups?[0]);
        Assert.Equal("Sample7", raiseCol[5].PropertyName);
        Assert.Null(raiseCol[5].Key);
        Assert.Equal("group2", raiseCol[5].Groups?[0]);
        Assert.Equal("Sample8", raiseCol[6].PropertyName);
        Assert.Null(raiseCol[6].Key);
        Assert.Equal("group2", raiseCol[6].Groups?[0]);
    }

    [Fact]
    public void WithKey()
    {
        var raiseCol = new List<ObjectPropertyChangesEventArgs>();
        var obj = new SetPropertyTest();

        obj.InitializeProperties();

        obj.ImmediatePropertyChanged += (s, e) =>
        {
            raiseCol.Add((ObjectPropertyChangesEventArgs)e);
        };

        bool hasChanges = false;

        Assert.False(obj.SetProperty(default(string), "sample1", null, null, () =>
        {
            return false;
        }, args =>
        {
            Assert.Null(args.oldValue);
            Assert.Null(args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.False(hasChanges);

        hasChanges = false;

        Assert.False(obj.SetProperty("sampleDefault1", "sample2", null, null, () =>
        {
            return false;
        }, args =>
        {
            Assert.Null(args.oldValue);
            Assert.Null(args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.False(hasChanges);

        hasChanges = false;

        Assert.True(obj.SetProperty("sampleDefault2", "sample3", null, null, () =>
        {
            return true;
        }, args =>
        {
            Assert.Null(args.oldValue);
            Assert.Equal("sampleDefault2", args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.True(obj.SetProperty("sampleDefault3", "sample3", null, null, () =>
        {
            return true;
        }, args =>
        {
            Assert.Equal("sampleDefault2", args.oldValue);
            Assert.Equal("sampleDefault3", args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.False(obj.SetProperty("sampleDefault3", "sample3", null, null, () =>
        {
            return true;
        }, args =>
        {
            Assert.Equal("sampleDefault3", args.oldValue);
            Assert.Equal("sampleDefault3", args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.False(hasChanges);

        hasChanges = false;

        Assert.True(obj.SetProperty("groupDefault1", "sample4", null, new string[] { "group1" }, () =>
        {
            return true;
        }, args =>
        {
            Assert.Null(args.oldValue);
            Assert.Equal("groupDefault1", args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.True(obj.SetProperty("groupDefault2", "sample5", null, new string[] { "group1" }, () =>
        {
            return true;
        }, args =>
        {
            Assert.Null(args.oldValue);
            Assert.Equal("groupDefault2", args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.True(obj.SetProperty("groupDefault3", "sample6", null, new string[] { "group1" }, () =>
        {
            return true;
        }, args =>
        {
            Assert.Null(args.oldValue);
            Assert.Equal("groupDefault3", args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.True(obj.SetProperty("groupDefault4", "sample7", null, new string[] { "group2" }, () =>
        {
            return true;
        }, args =>
        {
            Assert.Null(args.oldValue);
            Assert.Equal("groupDefault4", args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.True(obj.SetProperty("groupDefault5", "sample8", null, new string[] { "group2" }, () =>
        {
            return true;
        }, args =>
        {
            Assert.Null(args.oldValue);
            Assert.Equal("groupDefault5", args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.True(hasChanges);

        hasChanges = false;

        var group0 = obj.GetRawProperties();
        var group1 = obj.GetRawProperties("group1");
        var group2 = obj.GetRawProperties("group2");

        Assert.Equal(12, group0.Count());
        Assert.Equal(3, group1.Count());
        Assert.Equal(2, group2.Count());

        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp1) && i.Groups == null && i.Property.Value == default);
        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp2) && (i.Groups?.Contains("group") ?? false) && (i.Property as ObservableProperty<DateTime>)?.Value == default);
        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp3) && i.Groups == null && i.Property.Value?.ToString() == "defaultValue");
        Assert.Contains(group0, i => i.Key == "keySample1" && i.PropertyName == nameof(SampleKeyProp1) && i.Groups == null && i.Property.Value == default);
        Assert.Contains(group0, i => i.Key == "keySample2" && i.PropertyName == nameof(SampleKeyProp2) && (i.Groups?.Contains("group") ?? false) && (i.Property as ObservableProperty<DateTime>)?.Value == default);
        Assert.Contains(group0, i => i.Key == "keySample3" && i.PropertyName == nameof(SampleKeyProp3) && i.Groups == null && i.Property.Value?.ToString() == "defaultValue");
        Assert.Contains(group0, i => i.Key == "sample3" && i.PropertyName == null && i.Groups == null && i.Property.Value?.ToString() == "sampleDefault3");
        Assert.Contains(group1, i => i.Key == "sample4" && i.PropertyName == null && (i.Groups?.Contains("group1") ?? false) && i.Property.Value?.ToString() == "groupDefault1");
        Assert.Contains(group1, i => i.Key == "sample5" && i.PropertyName == null && (i.Groups?.Contains("group1") ?? false) && i.Property.Value?.ToString() == "groupDefault2");
        Assert.Contains(group1, i => i.Key == "sample6" && i.PropertyName == null && (i.Groups?.Contains("group1") ?? false) && i.Property.Value?.ToString() == "groupDefault3");
        Assert.Contains(group2, i => i.Key == "sample7" && i.PropertyName == null && (i.Groups?.Contains("group2") ?? false) && i.Property.Value?.ToString() == "groupDefault4");
        Assert.Contains(group2, i => i.Key == "sample8" && i.PropertyName == null && (i.Groups?.Contains("group2") ?? false) && i.Property.Value?.ToString() == "groupDefault5");

        Assert.Equal(7, raiseCol.Count);

        Assert.Null(raiseCol[0].PropertyName);
        Assert.Equal("sample3", raiseCol[0].Key);
        Assert.Null(raiseCol[0].Groups);
        Assert.Null(raiseCol[1].PropertyName);
        Assert.Equal("sample3", raiseCol[1].Key);
        Assert.Null(raiseCol[1].Groups);
        Assert.Null(raiseCol[2].PropertyName);
        Assert.Equal("sample4", raiseCol[2].Key);
        Assert.Equal("group1", raiseCol[2].Groups?[0]);
        Assert.Null(raiseCol[3].PropertyName);
        Assert.Equal("sample5", raiseCol[3].Key);
        Assert.Equal("group1", raiseCol[3].Groups?[0]);
        Assert.Null(raiseCol[4].PropertyName);
        Assert.Equal("sample6", raiseCol[4].Key);
        Assert.Equal("group1", raiseCol[4].Groups?[0]);
        Assert.Null(raiseCol[5].PropertyName);
        Assert.Equal("sample7", raiseCol[5].Key);
        Assert.Equal("group2", raiseCol[5].Groups?[0]);
        Assert.Null(raiseCol[6].PropertyName);
        Assert.Equal("sample8", raiseCol[6].Key);
        Assert.Equal("group2", raiseCol[6].Groups?[0]);
    }

    [Fact]
    public void WithBoth()
    {
        var raiseCol = new List<ObjectPropertyChangesEventArgs>();
        var obj = new SetPropertyTest();

        obj.InitializeProperties();

        obj.ImmediatePropertyChanged += (s, e) =>
        {
            raiseCol.Add((ObjectPropertyChangesEventArgs)e);
        };

        bool hasChanges = false;

        Assert.False(obj.SetProperty(default(string), "sample1", "Sample1", null, () =>
        {
            return false;
        }, args =>
        {
            Assert.Null(args.oldValue);
            Assert.Null(args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.False(hasChanges);

        hasChanges = false;

        Assert.False(obj.SetProperty("sampleDefault1", "sample2", "Sample2", null, () =>
        {
            return false;
        }, args =>
        {
            Assert.Null(args.oldValue);
            Assert.Null(args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.False(hasChanges);

        hasChanges = false;

        Assert.True(obj.SetProperty("sampleDefault2", "sample3", "Sample3", null, () =>
        {
            return true;
        }, args =>
        {
            Assert.Null(args.oldValue);
            Assert.Equal("sampleDefault2", args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.True(obj.SetProperty("sampleDefault3", "sample3", "Sample3", null, () =>
        {
            return true;
        }, args =>
        {
            Assert.Equal("sampleDefault2", args.oldValue);
            Assert.Equal("sampleDefault3", args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.False(obj.SetProperty("sampleDefault3", "sample3", "Sample3", null, () =>
        {
            return true;
        }, args =>
        {
            Assert.Equal("sampleDefault3", args.oldValue);
            Assert.Equal("sampleDefault3", args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.False(hasChanges);

        hasChanges = false;

        Assert.True(obj.SetProperty("groupDefault1", "sample4", "Sample4", new string[] { "group1" }, () =>
        {
            return true;
        }, args =>
        {
            Assert.Null(args.oldValue);
            Assert.Equal("groupDefault1", args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.True(obj.SetProperty("groupDefault2", "sample5", "Sample5", new string[] { "group1" }, () =>
        {
            return true;
        }, args =>
        {
            Assert.Null(args.oldValue);
            Assert.Equal("groupDefault2", args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.True(obj.SetProperty("groupDefault3", "sample6", "Sample6", new string[] { "group1" }, () =>
        {
            return true;
        }, args =>
        {
            Assert.Null(args.oldValue);
            Assert.Equal("groupDefault3", args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.True(obj.SetProperty("groupDefault4", "sample7", "Sample7", new string[] { "group2" }, () =>
        {
            return true;
        }, args =>
        {
            Assert.Null(args.oldValue);
            Assert.Equal("groupDefault4", args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.True(hasChanges);

        hasChanges = false;

        Assert.True(obj.SetProperty("groupDefault5", "sample8", "Sample8", new string[] { "group2" }, () =>
        {
            return true;
        }, args =>
        {
            Assert.Null(args.oldValue);
            Assert.Equal("groupDefault5", args.newValue);
            hasChanges = args.hasChanges;
        }));

        Assert.True(hasChanges);

        hasChanges = false;

        var group0 = obj.GetRawProperties();
        var group1 = obj.GetRawProperties("group1");
        var group2 = obj.GetRawProperties("group2");

        Assert.Equal(12, group0.Count());
        Assert.Equal(3, group1.Count());
        Assert.Equal(2, group2.Count());

        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp1) && i.Groups == null && i.Property.Value == default);
        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp2) && (i.Groups?.Contains("group") ?? false) && (i.Property as ObservableProperty<DateTime>)?.Value == default);
        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp3) && i.Groups == null && i.Property.Value?.ToString() == "defaultValue");
        Assert.Contains(group0, i => i.Key == "keySample1" && i.PropertyName == nameof(SampleKeyProp1) && i.Groups == null && i.Property.Value == default);
        Assert.Contains(group0, i => i.Key == "keySample2" && i.PropertyName == nameof(SampleKeyProp2) && (i.Groups?.Contains("group") ?? false) && (i.Property as ObservableProperty<DateTime>)?.Value == default);
        Assert.Contains(group0, i => i.Key == "keySample3" && i.PropertyName == nameof(SampleKeyProp3) && i.Groups == null && i.Property.Value?.ToString() == "defaultValue");
        Assert.Contains(group0, i => i.Key == "sample3" && i.PropertyName == "Sample3" && i.Groups == null && i.Property.Value?.ToString() == "sampleDefault3");
        Assert.Contains(group1, i => i.Key == "sample4" && i.PropertyName == "Sample4" && (i.Groups?.Contains("group1") ?? false) && i.Property.Value?.ToString() == "groupDefault1");
        Assert.Contains(group1, i => i.Key == "sample5" && i.PropertyName == "Sample5" && (i.Groups?.Contains("group1") ?? false) && i.Property.Value?.ToString() == "groupDefault2");
        Assert.Contains(group1, i => i.Key == "sample6" && i.PropertyName == "Sample6" && (i.Groups?.Contains("group1") ?? false) && i.Property.Value?.ToString() == "groupDefault3");
        Assert.Contains(group2, i => i.Key == "sample7" && i.PropertyName == "Sample7" && (i.Groups?.Contains("group2") ?? false) && i.Property.Value?.ToString() == "groupDefault4");
        Assert.Contains(group2, i => i.Key == "sample8" && i.PropertyName == "Sample8" && (i.Groups?.Contains("group2") ?? false) && i.Property.Value?.ToString() == "groupDefault5");

        Assert.Equal(7, raiseCol.Count);

        Assert.Equal("Sample3", raiseCol[0].PropertyName);
        Assert.Equal("sample3", raiseCol[0].Key);
        Assert.Null(raiseCol[0].Groups);
        Assert.Equal("Sample3", raiseCol[1].PropertyName);
        Assert.Equal("sample3", raiseCol[1].Key);
        Assert.Null(raiseCol[1].Groups);
        Assert.Equal("Sample4", raiseCol[2].PropertyName);
        Assert.Equal("sample4", raiseCol[2].Key);
        Assert.Equal("group1", raiseCol[2].Groups?[0]);
        Assert.Equal("Sample5", raiseCol[3].PropertyName);
        Assert.Equal("sample5", raiseCol[3].Key);
        Assert.Equal("group1", raiseCol[3].Groups?[0]);
        Assert.Equal("Sample6", raiseCol[4].PropertyName);
        Assert.Equal("sample6", raiseCol[4].Key);
        Assert.Equal("group1", raiseCol[4].Groups?[0]);
        Assert.Equal("Sample7", raiseCol[5].PropertyName);
        Assert.Equal("sample7", raiseCol[5].Key);
        Assert.Equal("group2", raiseCol[5].Groups?[0]);
        Assert.Equal("Sample8", raiseCol[6].PropertyName);
        Assert.Equal("sample8", raiseCol[6].Key);
        Assert.Equal("group2", raiseCol[6].Groups?[0]);
    }
}

public class RemoveTest : SampleObject
{
    [Fact]
    public void Normal()
    {
        var raiseCol = new List<ObjectPropertyChangesEventArgs>();
        var obj = new RemoveTest();

        obj.InitializeProperties();

        obj.ImmediatePropertyChanged += (s, e) =>
        {
            raiseCol.Add((ObjectPropertyChangesEventArgs)e);
        };

        obj.SampleProp1 = "set1";
        obj.SampleProp2 = DateTime.Now;
        obj.SampleProp3 = "set3";

        Assert.True(obj.RemoveProperty(nameof(SampleProp2)));
        Assert.True(obj.RemoveProperty(nameof(SampleProp3)));

        Assert.Equal(4, obj.GetRawProperties().Count());

        Assert.Equal("set1", obj.SampleProp1);
        Assert.Equal(default, obj.SampleProp2);
        Assert.Equal("defaultValue", obj.SampleProp3);

        var group0 = obj.GetRawProperties();

        Assert.Equal(6, group0.Count());

        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp1) && i.Groups == null && i.Property.Value?.ToString() == "set1");
        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp2) && (i.Groups?.Contains("group") ?? false) && (i.Property as ObservableProperty<DateTime>)?.Value == default);
        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp3) && i.Groups == null && i.Property.Value?.ToString() == "defaultValue");
        Assert.Contains(group0, i => i.Key == "keySample1" && i.PropertyName == nameof(SampleKeyProp1) && i.Groups == null && i.Property.Value == default);
        Assert.Contains(group0, i => i.Key == "keySample2" && i.PropertyName == nameof(SampleKeyProp2) && (i.Groups?.Contains("group") ?? false) && (i.Property as ObservableProperty<DateTime>)?.Value == default);
        Assert.Contains(group0, i => i.Key == "keySample3" && i.PropertyName == nameof(SampleKeyProp3) && i.Groups == null && i.Property.Value?.ToString() == "defaultValue");

        Assert.Equal(7, raiseCol.Count);

        Assert.Equal(nameof(SampleProp1), raiseCol[0].PropertyName);
        Assert.Null(raiseCol[0].Key);
        Assert.Null(raiseCol[0].Groups);
        Assert.Equal(nameof(SampleProp2), raiseCol[1].PropertyName);
        Assert.Null(raiseCol[1].Key);
        Assert.Equal("group", raiseCol[1].Groups?[0]);
        Assert.Equal(nameof(SampleProp3), raiseCol[2].PropertyName);
        Assert.Null(raiseCol[2].Key);
        Assert.Null(raiseCol[2].Groups);
        Assert.Equal(nameof(SampleProp2), raiseCol[3].PropertyName);
        Assert.Null(raiseCol[3].Key);
        Assert.Equal("group", raiseCol[3].Groups?[0]);
        Assert.Equal(nameof(SampleProp3), raiseCol[4].PropertyName);
        Assert.Null(raiseCol[4].Key);
        Assert.Null(raiseCol[4].Groups);
        Assert.Equal(nameof(SampleProp2), raiseCol[5].PropertyName);
        Assert.Null(raiseCol[5].Key);
        Assert.Equal("group", raiseCol[5].Groups?[0]);
        Assert.Equal(nameof(SampleProp3), raiseCol[6].PropertyName);
        Assert.Null(raiseCol[6].Key);
        Assert.Null(raiseCol[6].Groups);
    }

    [Fact]
    public void WithKey()
    {
        var raiseCol = new List<ObjectPropertyChangesEventArgs>();
        var obj = new RemoveTest();

        obj.InitializeProperties();

        obj.ImmediatePropertyChanged += (s, e) =>
        {
            raiseCol.Add((ObjectPropertyChangesEventArgs)e);
        };

        obj.SampleKeyProp1 = "set1";
        obj.SampleKeyProp2 = DateTime.UtcNow;
        obj.SampleKeyProp3 = "set3";

        Assert.True(obj.RemovePropertyWithKey("keySample2"));
        Assert.True(obj.RemovePropertyWithKey("keySample3"));

        Assert.Equal(4, obj.GetRawProperties().Count());

        Assert.Equal("set1", obj.SampleKeyProp1);
        Assert.Equal(default, obj.SampleKeyProp2);
        Assert.Equal("defaultValue", obj.SampleKeyProp3);

        var group0 = obj.GetRawProperties();

        Assert.Equal(6, group0.Count());

        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp1) && i.Groups == null && i.Property.Value == default);
        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp2) && (i.Groups?.Contains("group") ?? false) && (i.Property as ObservableProperty<DateTime>)?.Value == default);
        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp3) && i.Groups == null && i.Property.Value?.ToString() == "defaultValue");
        Assert.Contains(group0, i => i.Key == "keySample1" && i.PropertyName == nameof(SampleKeyProp1) && i.Groups == null && i.Property.Value?.ToString() == "set1");
        Assert.Contains(group0, i => i.Key == "keySample2" && i.PropertyName == nameof(SampleKeyProp2) && (i.Groups?.Contains("group") ?? false) && (i.Property as ObservableProperty<DateTime>)?.Value == default);
        Assert.Contains(group0, i => i.Key == "keySample3" && i.PropertyName == nameof(SampleKeyProp3) && i.Groups == null && i.Property.Value?.ToString() == "defaultValue");

        Assert.Equal(7, raiseCol.Count);

        Assert.Equal(nameof(SampleKeyProp1), raiseCol[0].PropertyName);
        Assert.Equal("keySample1", raiseCol[0].Key);
        Assert.Null(raiseCol[0].Groups);
        Assert.Equal(nameof(SampleKeyProp2), raiseCol[1].PropertyName);
        Assert.Equal("keySample2", raiseCol[1].Key);
        Assert.Equal("group", raiseCol[1].Groups?[0]);
        Assert.Equal(nameof(SampleKeyProp3), raiseCol[2].PropertyName);
        Assert.Equal("keySample3", raiseCol[2].Key);
        Assert.Null(raiseCol[2].Groups);
        Assert.Equal(nameof(SampleKeyProp2), raiseCol[3].PropertyName);
        Assert.Equal("keySample2", raiseCol[3].Key);
        Assert.Equal("group", raiseCol[3].Groups?[0]);
        Assert.Equal(nameof(SampleKeyProp3), raiseCol[4].PropertyName);
        Assert.Equal("keySample3", raiseCol[4].Key);
        Assert.Null(raiseCol[4].Groups);
        Assert.Equal(nameof(SampleKeyProp2), raiseCol[5].PropertyName);
        Assert.Equal("keySample2", raiseCol[5].Key);
        Assert.Equal("group", raiseCol[5].Groups?[0]);
        Assert.Equal(nameof(SampleKeyProp3), raiseCol[6].PropertyName);
        Assert.Equal("keySample3", raiseCol[6].Key);
        Assert.Null(raiseCol[6].Groups);
    }

    [Fact]
    public void WithBoth()
    {
        var raiseCol = new List<ObjectPropertyChangesEventArgs>();
        var obj = new RemoveTest();

        obj.InitializeProperties();

        obj.ImmediatePropertyChanged += (s, e) =>
        {
            raiseCol.Add((ObjectPropertyChangesEventArgs)e);
        };

        obj.SampleProp1 = "set1";
        obj.SampleProp2 = DateTime.UtcNow;
        obj.SampleProp3 = "set3";
        obj.SampleKeyProp1 = "set1";
        obj.SampleKeyProp2 = DateTime.UtcNow;
        obj.SampleKeyProp3 = "set3";

        Assert.True(obj.RemoveProperty(nameof(SampleProp2)));
        Assert.True(obj.RemoveProperty(nameof(SampleProp3)));
        Assert.True(obj.RemovePropertyWithKey("keySample2"));
        Assert.True(obj.RemovePropertyWithKey("keySample3"));

        Assert.Equal(2, obj.GetRawProperties().Count());

        Assert.Equal("set1", obj.SampleProp1);
        Assert.Equal(default, obj.SampleProp2);
        Assert.Equal("defaultValue", obj.SampleProp3);
        Assert.Equal("set1", obj.SampleKeyProp1);
        Assert.Equal(default, obj.SampleKeyProp2);
        Assert.Equal("defaultValue", obj.SampleKeyProp3);

        var group0 = obj.GetRawProperties();

        Assert.Equal(6, group0.Count());

        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp1) && i.Groups == null && i.Property.Value?.ToString() == "set1");
        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp2) && (i.Groups?.Contains("group") ?? false) && (i.Property as ObservableProperty<DateTime>)?.Value == default);
        Assert.Contains(group0, i => i.Key == null && i.PropertyName == nameof(SampleProp3) && i.Groups == null && i.Property.Value?.ToString() == "defaultValue");
        Assert.Contains(group0, i => i.Key == "keySample1" && i.PropertyName == nameof(SampleKeyProp1) && i.Groups == null && i.Property.Value?.ToString() == "set1");
        Assert.Contains(group0, i => i.Key == "keySample2" && i.PropertyName == nameof(SampleKeyProp2) && (i.Groups?.Contains("group") ?? false) && (i.Property as ObservableProperty<DateTime>)?.Value == default);
        Assert.Contains(group0, i => i.Key == "keySample3" && i.PropertyName == nameof(SampleKeyProp3) && i.Groups == null && i.Property.Value?.ToString() == "defaultValue");

        Assert.Equal(14, raiseCol.Count);

        Assert.Equal(nameof(SampleProp1), raiseCol[0].PropertyName);
        Assert.Null(raiseCol[0].Key);
        Assert.Null(raiseCol[0].Groups);
        Assert.Equal(nameof(SampleProp2), raiseCol[1].PropertyName);
        Assert.Null(raiseCol[1].Key);
        Assert.Equal("group", raiseCol[1].Groups?[0]);
        Assert.Equal(nameof(SampleProp3), raiseCol[2].PropertyName);
        Assert.Null(raiseCol[2].Key);
        Assert.Null(raiseCol[2].Groups);
        Assert.Equal(nameof(SampleKeyProp1), raiseCol[3].PropertyName);
        Assert.Equal("keySample1", raiseCol[3].Key);
        Assert.Null(raiseCol[3].Groups);
        Assert.Equal(nameof(SampleKeyProp2), raiseCol[4].PropertyName);
        Assert.Equal("keySample2", raiseCol[4].Key);
        Assert.Equal("group", raiseCol[4].Groups?[0]);
        Assert.Equal(nameof(SampleKeyProp3), raiseCol[5].PropertyName);
        Assert.Equal("keySample3", raiseCol[5].Key);
        Assert.Null(raiseCol[5].Groups);
        Assert.Equal(nameof(SampleProp2), raiseCol[6].PropertyName);
        Assert.Null(raiseCol[6].Key);
        Assert.Equal("group", raiseCol[6].Groups?[0]);
        Assert.Equal(nameof(SampleProp3), raiseCol[7].PropertyName);
        Assert.Null(raiseCol[7].Key);
        Assert.Null(raiseCol[7].Groups);
        Assert.Equal(nameof(SampleKeyProp2), raiseCol[8].PropertyName);
        Assert.Equal("keySample2", raiseCol[8].Key);
        Assert.Equal("group", raiseCol[8].Groups?[0]);
        Assert.Equal(nameof(SampleKeyProp3), raiseCol[9].PropertyName);
        Assert.Equal("keySample3", raiseCol[9].Key);
        Assert.Null(raiseCol[9].Groups);
        Assert.Equal(nameof(SampleProp2), raiseCol[10].PropertyName);
        Assert.Null(raiseCol[10].Key);
        Assert.Equal("group", raiseCol[10].Groups?[0]);
        Assert.Equal(nameof(SampleProp3), raiseCol[11].PropertyName);
        Assert.Null(raiseCol[11].Key);
        Assert.Null(raiseCol[11].Groups);
        Assert.Equal(nameof(SampleKeyProp2), raiseCol[12].PropertyName);
        Assert.Equal("keySample2", raiseCol[12].Key);
        Assert.Equal("group", raiseCol[12].Groups?[0]);
        Assert.Equal(nameof(SampleKeyProp3), raiseCol[13].PropertyName);
        Assert.Equal("keySample3", raiseCol[13].Key);
        Assert.Null(raiseCol[13].Groups);
    }
}

public class ContainsPropertyTest : SampleObject
{
    [Fact]
    public void Normal()
    {
        var obj = new ContainsPropertyTest();

        obj.InitializeProperties();

        Assert.True(obj.ContainsProperty(nameof(SampleProp2)));
        Assert.True(obj.ContainsProperty(nameof(SampleProp3)));
    }

    [Fact]
    public void WithKey()
    {
        var obj = new ContainsPropertyTest();

        obj.InitializeProperties();

        Assert.True(obj.ContainsPropertyWithKey("keySample2"));
        Assert.True(obj.ContainsPropertyWithKey("keySample3"));
    }

    [Fact]
    public void WithBoth()
    {
        var obj = new ContainsPropertyTest();

        obj.InitializeProperties();

        Assert.True(obj.ContainsProperty(nameof(SampleProp2)));
        Assert.True(obj.ContainsProperty(nameof(SampleProp3)));
        Assert.True(obj.ContainsPropertyWithKey("keySample2"));
        Assert.True(obj.ContainsPropertyWithKey("keySample3"));
    }
}

public class GetRawPropertiesTest : SampleObject
{
    public GetRawPropertiesTest()
    {
        InitializeProperties();
    }

    [Fact]
    public async void Normal()
    {
        DateTime date1 = DateTime.UtcNow;
        var obj = new GetRawPropertiesTest
        {
            SampleProp1 = "set1",
            SampleProp2 = date1,
            SampleProp3 = "set3",
            SampleKeyProp1 = "set1",
            SampleKeyProp2 = date1,
            SampleKeyProp3 = "set3"
        };

        await Task.Delay(1000);

        var all = obj.GetRawProperties();
        var group = obj.GetRawProperties("group");

        Assert.Equal(6, all.Count());
        Assert.Equal(2, group.Count());

        Assert.Contains(all, i => i.Key == null && i.PropertyName == nameof(SampleProp1) && i.Groups == null && i.Property.Value?.ToString() == "set1");
        Assert.Contains(all, i => i.Key == null && i.PropertyName == nameof(SampleProp2) && (i.Groups?.Contains("group") ?? false) && i.Property.GetValue<DateTime>() == date1);
        Assert.Contains(all, i => i.Key == null && i.PropertyName == nameof(SampleProp3) && i.Groups == null && i.Property.Value?.ToString() == "set3");
        Assert.Contains(all, i => i.Key == "keySample1" && i.PropertyName == nameof(SampleKeyProp1) && i.Groups == null && i.Property.Value?.ToString() == "set1");
        Assert.Contains(all, i => i.Key == "keySample2" && i.PropertyName == nameof(SampleKeyProp2) && (i.Groups?.Contains("group") ?? false) && i.Property.GetValue<DateTime>() == date1);
        Assert.Contains(all, i => i.Key == "keySample3" && i.PropertyName == nameof(SampleKeyProp3) && i.Groups == null && i.Property.Value?.ToString() == "set3");

        Assert.Contains(group, i => i.Key == null && i.PropertyName == nameof(SampleProp2) && (i.Groups?.Contains("group") ?? false) && i.Property.GetValue<DateTime>() == date1);
        Assert.Contains(group, i => i.Key == "keySample2" && i.PropertyName == nameof(SampleKeyProp2) && (i.Groups?.Contains("group") ?? false) && i.Property.GetValue<DateTime>() == date1);
    }
}

public class AttachOnPropertyChangedTest : SampleObject
{
    [Fact]
    public async void Normal()
    {
        var obj = new AttachOnPropertyChangedTest();

        string? SampleProp1 = null;
        DateTime SampleProp2 = default;
        string? SampleProp3 = null;

        obj.AttachOnPropertyChanged<string?>(v => SampleProp1 = v, nameof(obj.SampleProp1));
        obj.AttachOnPropertyChanged<DateTime>(v => SampleProp2 = v, nameof(obj.SampleProp2));
        obj.AttachOnPropertyChanged<string?>(v => SampleProp3 = v, nameof(obj.SampleProp3));

        obj.InitializeProperties();

        await Task.Delay(1000);

        Assert.Equal(SampleProp1, obj.SampleProp1);
        Assert.Equal(SampleProp2, obj.SampleProp2);
        Assert.Equal(SampleProp3, obj.SampleProp3);

        DateTime date1 = DateTime.UtcNow;

        obj.SampleProp1 = "test1";
        obj.SampleProp2 = date1;
        obj.SampleProp3 = "test3";

        await Task.Delay(1000);

        Assert.Equal("test1", SampleProp1);
        Assert.Equal(date1, SampleProp2);
        Assert.Equal("test3", SampleProp3);

        Assert.Equal(SampleProp1, obj.SampleProp1);
        Assert.Equal(SampleProp2, obj.SampleProp2);
        Assert.Equal(SampleProp3, obj.SampleProp3);
    }
}

public class AttachImmediateOnPropertyChangedTest : SampleObject
{
    [Fact]
    public void Normal()
    {
        var obj = new AttachImmediateOnPropertyChangedTest();

        string? SampleProp1 = null;
        DateTime SampleProp2 = default;
        string? SampleProp3 = null;

        obj.AttachOnImmediatePropertyChanged<string?>(v => SampleProp1 = v, nameof(obj.SampleProp1));
        obj.AttachOnImmediatePropertyChanged<DateTime>(v => SampleProp2 = v, nameof(obj.SampleProp2));
        obj.AttachOnImmediatePropertyChanged<string?>(v => SampleProp3 = v, nameof(obj.SampleProp3));

        obj.InitializeProperties();

        Assert.Equal(SampleProp1, obj.SampleProp1);
        Assert.Equal(SampleProp2, obj.SampleProp2);
        Assert.Equal(SampleProp3, obj.SampleProp3);

        DateTime date1 = DateTime.UtcNow;

        obj.SampleProp1 = "test1";
        obj.SampleProp2 = date1;
        obj.SampleProp3 = "test3";

        Assert.Equal("test1", SampleProp1);
        Assert.Equal(date1, SampleProp2);
        Assert.Equal("test3", SampleProp3);

        Assert.Equal(SampleProp1, obj.SampleProp1);
        Assert.Equal(SampleProp2, obj.SampleProp2);
        Assert.Equal(SampleProp3, obj.SampleProp3);
    }
}
