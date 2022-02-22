using ObservableHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit;

namespace ObservablePropertyTest;

public class ConstructorTest
{
    [Fact]
    public void Parameterless()
    {
        var prop1 = new ObservableProperty();
        var prop2 = new ObservableProperty<string>();

        Assert.Null(prop1.Value);
        Assert.Null(prop2.Value);
    }
}

public class PropertyTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol1 = new List<PropertyChangedEventArgs>();
        var raiseCol2 = new List<PropertyChangedEventArgs>();
        var prop1 = new ObservableProperty();
        var prop2 = new ObservableProperty<string>();

        prop1.ImmediatePropertyChanged += (s, e) =>
        {
            raiseCol1.Add(e);
        };
        prop2.ImmediatePropertyChanged += (s, e) =>
        {
            raiseCol2.Add(e);
        };

        prop1.SetValue("test");
        Assert.Equal("test", prop1.Value);
        prop1.SetValue("test");
        Assert.Equal("test", prop1.Value);
        prop1.SetValue("test1");
        Assert.Equal("test1", prop1.Value);
        prop1.SetValue("test1");
        Assert.Equal("test1", prop1.Value);
        prop2.SetValue("test");
        Assert.Equal("test", prop2.Value);
        prop2.SetValue("test");
        Assert.Equal("test", prop2.Value);
        prop2.SetValue("test1");
        Assert.Equal("test1", prop2.Value);
        prop2.SetValue("test1");
        Assert.Equal("test1", prop2.Value);

        Assert.Equal(2, raiseCol1.Count);
        Assert.Equal(2, raiseCol2.Count);

        Assert.Equal(nameof(prop1.Value), raiseCol1[0].PropertyName);
        Assert.Equal(nameof(prop1.Value), raiseCol1[1].PropertyName);
        Assert.Equal(nameof(prop2.Value), raiseCol2[0].PropertyName);
        Assert.Equal(nameof(prop2.Value), raiseCol2[1].PropertyName);
    }
}

public class TypeTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol1 = new List<PropertyChangedEventArgs>();
        var prop1 = new ObservableProperty();

        prop1.ImmediatePropertyChanged += (s, e) =>
        {
            raiseCol1.Add(e);
        };

        prop1.SetValue("test");
        Assert.Equal("test", prop1.Value);
        prop1.SetValue("test");
        Assert.Equal("test", prop1.Value);

        prop1.SetValue(1);
        Assert.Equal(1, prop1.Value);
        prop1.SetValue(1);
        Assert.Equal(1, prop1.Value);

        Assert.Equal(2, raiseCol1.Count);

        Assert.Equal(nameof(prop1.Value), raiseCol1[0].PropertyName);
        Assert.Equal(nameof(prop1.Value), raiseCol1[1].PropertyName);
    }
}

public class ValueTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol1 = new List<PropertyChangedEventArgs>();
        var raiseCol2 = new List<PropertyChangedEventArgs>();
        var prop1 = new ObservableProperty();
        var prop2 = new ObservableProperty<string>();

        prop1.ImmediatePropertyChanged += (s, e) =>
        {
            raiseCol1.Add(e);
        };
        prop2.ImmediatePropertyChanged += (s, e) =>
        {
            raiseCol2.Add(e);
        };

        prop1.Value = "test";
        prop1.Value = "test";
        prop1.Value = "test1";
        prop1.Value = "test1";
        prop2.Value = "test";
        prop2.Value = "test";
        prop2.Value = "test1";
        prop2.Value = "test1";

        Assert.Equal("test1", prop1.Value);
        Assert.Equal("test1", prop2.Value);

        Assert.Equal(2, raiseCol1.Count);
        Assert.Equal(2, raiseCol2.Count);

        Assert.Equal(nameof(prop1.Value), raiseCol1[0].PropertyName);
        Assert.Equal(nameof(prop1.Value), raiseCol1[1].PropertyName);
        Assert.Equal(nameof(prop2.Value), raiseCol2[0].PropertyName);
        Assert.Equal(nameof(prop2.Value), raiseCol2[1].PropertyName);
    }
}

public class GetValueAndSetValueTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol1 = new List<PropertyChangedEventArgs>();
        var raiseCol2 = new List<PropertyChangedEventArgs>();
        var prop1 = new ObservableProperty();
        var prop2 = new ObservableProperty<string>();

        prop1.ImmediatePropertyChanged += (s, e) =>
        {
            raiseCol1.Add(e);
        };
        prop2.ImmediatePropertyChanged += (s, e) =>
        {
            raiseCol2.Add(e);
        };

        Assert.Null(prop1.GetValue<string>());
        Assert.Null(prop2.GetValue());

        Assert.Null(prop1.GetValue<string?>(() => null));
        Assert.Equal("", prop2.GetValue(() => ""));

        Assert.True(prop1.SetValue("test"));
        Assert.False(prop1.SetValue("test"));
        Assert.True(prop1.SetValue("test1"));
        Assert.False(prop1.SetValue("test1"));
        Assert.True(prop2.SetValue("test"));
        Assert.False(prop2.SetValue("test"));
        Assert.True(prop2.SetValue("test1"));
        Assert.False(prop2.SetValue("test1"));

        Assert.Equal("test1", prop1.GetValue<string>());
        Assert.Equal("test1", prop2.GetValue());

        Assert.Equal("test1", prop1.GetValue<string?>(() => null));
        Assert.Equal("test1", prop2.GetValue(() => ""));

        Assert.Equal(2, raiseCol1.Count);
        Assert.Equal(2, raiseCol2.Count);

        Assert.Equal(nameof(prop1.Value), raiseCol1[0].PropertyName);
        Assert.Equal(nameof(prop1.Value), raiseCol1[1].PropertyName);
        Assert.Equal(nameof(prop2.Value), raiseCol2[0].PropertyName);
        Assert.Equal(nameof(prop2.Value), raiseCol2[1].PropertyName);
    }
}

public class GetObjectAndSetObjectTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol1 = new List<PropertyChangedEventArgs>();
        var raiseCol2 = new List<PropertyChangedEventArgs>();
        var prop1 = new ObservableProperty();
        var prop2 = new ObservableProperty<string>();

        prop1.ImmediatePropertyChanged += (s, e) =>
        {
            raiseCol1.Add(e);
        };
        prop2.ImmediatePropertyChanged += (s, e) =>
        {
            raiseCol2.Add(e);
        };

        Assert.Null(prop1.GetObject(typeof(string), default));
        Assert.Null(prop2.GetObject(typeof(string), default));

        Assert.True(prop1.SetObject(typeof(string), "test"));
        Assert.False(prop1.SetObject(typeof(string), "test"));
        Assert.True(prop1.SetObject(typeof(string), "test1"));
        Assert.False(prop1.SetObject(typeof(string), "test1"));
        Assert.True(prop2.SetObject(typeof(string), "test"));
        Assert.False(prop2.SetObject(typeof(string), "test"));
        Assert.True(prop2.SetObject(typeof(string), "test1"));
        Assert.False(prop2.SetObject(typeof(string), "test1"));

        Assert.Equal("test1", prop1.GetObject(typeof(string), default));
        Assert.Equal("test1", prop2.GetObject(typeof(string), default));

        Assert.Equal(2, raiseCol1.Count);
        Assert.Equal(2, raiseCol2.Count);

        Assert.Equal(nameof(prop1.Value), raiseCol1[0].PropertyName);
        Assert.Equal(nameof(prop1.Value), raiseCol1[1].PropertyName);
        Assert.Equal(nameof(prop2.Value), raiseCol2[0].PropertyName);
        Assert.Equal(nameof(prop2.Value), raiseCol2[1].PropertyName);
    }
}

public class SetNullAndIsNullTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol1 = new List<PropertyChangedEventArgs>();
        var raiseCol2 = new List<PropertyChangedEventArgs>();
        var prop1 = new ObservableProperty();
        var prop2 = new ObservableProperty<string>();

        prop1.ImmediatePropertyChanged += (s, e) =>
        {
            raiseCol1.Add(e);
        };
        prop2.ImmediatePropertyChanged += (s, e) =>
        {
            raiseCol2.Add(e);
        };

        Assert.True(prop1.IsNull());
        Assert.True(prop2.IsNull());

        Assert.False(prop1.SetNull());
        Assert.False(prop2.SetNull());

        Assert.True(prop1.SetValue("test"));
        Assert.True(prop2.SetValue("test"));

        Assert.False(prop1.IsNull());
        Assert.False(prop2.IsNull());

        Assert.True(prop1.SetNull());
        Assert.True(prop2.SetNull());

        Assert.True(prop1.IsNull());
        Assert.True(prop2.IsNull());

        Assert.Null(prop1.Value);
        Assert.Null(prop2.Value);

        Assert.Equal(2, raiseCol1.Count);
        Assert.Equal(2, raiseCol2.Count);

        Assert.Equal(nameof(prop1.Value), raiseCol1[0].PropertyName);
        Assert.Equal(nameof(prop1.Value), raiseCol1[1].PropertyName);

        Assert.Equal(nameof(prop2.Value), raiseCol2[0].PropertyName);
        Assert.Equal(nameof(prop2.Value), raiseCol2[1].PropertyName);
    }
}
