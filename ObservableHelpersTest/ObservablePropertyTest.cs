using ObservableHelpers;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit;

namespace ObservablePropertyTest
{
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
            var raiseCol = new List<PropertyChangedEventArgs>();
            var prop = new ObservableProperty();

            prop.PropertyChanged += (s, e) =>
            {
                raiseCol.Add(e);
            };

            prop.SetValue("test");
            Assert.Equal("test", prop.Value);
            prop.SetValue("test");
            Assert.Equal("test", prop.Value);
            prop.SetValue("test1");
            Assert.Equal("test1", prop.Value);
            prop.SetValue("test1");
            Assert.Equal("test1", prop.Value);

            Assert.Equal(2, raiseCol.Count);

            Assert.Equal(nameof(prop.Value), raiseCol[0].PropertyName);
            Assert.Equal(nameof(prop.Value), raiseCol[1].PropertyName);
        }
    }

    public class TypeTest
    {
        [Fact]
        public void Normal()
        {
            var raiseCol = new List<PropertyChangedEventArgs>();
            var prop = new ObservableProperty();

            prop.PropertyChanged += (s, e) =>
            {
                raiseCol.Add(e);
            };

            prop.SetValue("test");
            Assert.Equal("test", prop.Value);
            prop.SetValue("test");
            Assert.Equal("test", prop.Value);

            prop.SetValue(1);
            Assert.Equal(1, prop.Value);
            prop.SetValue(1);
            Assert.Equal(1, prop.Value);

            Assert.Equal(2, raiseCol.Count);

            Assert.Equal(nameof(prop.Value), raiseCol[0].PropertyName);
            Assert.Equal(nameof(prop.Value), raiseCol[1].PropertyName);
        }
    }

    public class ValueTest
    {
        [Fact]
        public void Normal()
        {
            var raiseCol = new List<PropertyChangedEventArgs>();
            var prop = new ObservableProperty<string>();

            prop.PropertyChanged += (s, e) =>
            {
                raiseCol.Add(e);
            };

            prop.Value = "test";
            prop.Value = "test";
            prop.Value = "test1";
            prop.Value = "test1";

            Assert.Equal("test1", prop.Value);

            Assert.Equal(2, raiseCol.Count);

            Assert.Equal(nameof(prop.Value), raiseCol[0].PropertyName);
            Assert.Equal(nameof(prop.Value), raiseCol[1].PropertyName);
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

            prop1.PropertyChanged += (s, e) =>
            {
                raiseCol1.Add(e);
            };
            prop2.PropertyChanged += (s, e) =>
            {
                raiseCol2.Add(e);
            };

            Assert.Null(prop1.GetValue<string>());
            Assert.Null(prop2.GetValue());

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

            prop1.PropertyChanged += (s, e) =>
            {
                raiseCol1.Add(e);
            };
            prop2.PropertyChanged += (s, e) =>
            {
                raiseCol2.Add(e);
            };

            Assert.True(prop1.IsNull());
            Assert.True(prop2.IsNull());

            Assert.False(prop1.SetNull());
            Assert.False(prop2.SetNull());

            prop1.SetValue("test");
            prop2.SetValue("test");

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
}
