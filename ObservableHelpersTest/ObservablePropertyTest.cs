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
            var prop1 = new ObservableProperty<string>();

            Assert.Null(prop1.Value);
        }
    }

    public class PropertyTest
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

    public class ValueTest
    {
        [Fact]
        public void Normal()
        {
            var raiseCol1 = new List<PropertyChangedEventArgs>();
            var prop1 = new ObservableProperty<string>();

            prop1.PropertyChanged += (s, e) =>
            {
                raiseCol1.Add(e);
            };

            Assert.Null(prop1.Value);

            prop1.Value = "test";
            prop1.Value = "test";
            prop1.Value = "test1";
            prop1.Value = "test1";

            Assert.Equal("test1", prop1.Value);

            Assert.Equal(2, raiseCol1.Count);

            Assert.Equal(nameof(prop1.Value), raiseCol1[0].PropertyName);
            Assert.Equal(nameof(prop1.Value), raiseCol1[1].PropertyName);
        }
    }

    public class GetValueAndSetValueTest
    {
        [Fact]
        public void Normal()
        {
            var raiseCol1 = new List<PropertyChangedEventArgs>();
            var prop1 = new ObservableProperty<string>();

            prop1.PropertyChanged += (s, e) =>
            {
                raiseCol1.Add(e);
            };

            Assert.Null(prop1.GetValue());

            Assert.True(prop1.SetValue("test"));
            Assert.False(prop1.SetValue("test"));
            Assert.True(prop1.SetValue("test1"));
            Assert.False(prop1.SetValue("test1"));

            Assert.Equal("test1", prop1.GetValue());

            Assert.Equal(2, raiseCol1.Count);

            Assert.Equal(nameof(prop1.Value), raiseCol1[0].PropertyName);
            Assert.Equal(nameof(prop1.Value), raiseCol1[1].PropertyName);
        }
    }

    public class SetNullAndIsNullTest
    {
        [Fact]
        public void Normal()
        {
            var raiseCol1 = new List<PropertyChangedEventArgs>();
            var prop1 = new ObservableProperty<string>();

            prop1.PropertyChanged += (s, e) =>
            {
                raiseCol1.Add(e);
            };

            Assert.True(prop1.IsNull());

            Assert.False(prop1.SetNull());

            Assert.True(prop1.SetValue("test"));

            Assert.False(prop1.IsNull());

            Assert.True(prop1.SetNull());

            Assert.True(prop1.IsNull());

            Assert.Null(prop1.Value);

            Assert.Equal(2, raiseCol1.Count);

            Assert.Equal(nameof(prop1.Value), raiseCol1[0].PropertyName);
            Assert.Equal(nameof(prop1.Value), raiseCol1[1].PropertyName);
        }
    }
}
