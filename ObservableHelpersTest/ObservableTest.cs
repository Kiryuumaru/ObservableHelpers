using ObservableHelpers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ObservableHelpersTest
{
    public class TestObject : ObservableObject
    {
        public string Prop1
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string Prop2
        {
            get => GetPropertyWithKey<string>("prop2");
            set => SetPropertyWithKey(value, "prop2");
        }
    }

    public class ObservableTest
    {
        [Fact]
        public async void ObservablePropertyTest()
        {
            var raiseCount = 0;
            var prop = new ObservableProperty();
            prop.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(prop.Property))
                {
                    raiseCount++;
                }
            };
            prop.SetValue("test");
            prop.SetValue("test");
            prop.SetValue("test1");
            await Task.Delay(500);
            Assert.Equal(2, raiseCount);
        }

        [Fact]
        public async void ObservableObjectTest()
        {
            var raiseProp1Count = 0;
            var raiseProp2Count = 0;
            var obj = new TestObject();
            obj.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(obj.Prop1))
                {
                    raiseProp1Count++;
                }
                if (e.PropertyName == nameof(obj.Prop2))
                {
                    raiseProp2Count++;
                }
            };
            obj.Prop1 = "test";
            obj.Prop1 = "test";
            obj.Prop1 = "test1";
            obj.Prop1 = "test1";
            obj.Prop2 = "test";
            obj.Prop2 = "test";
            obj.Prop2 = "test1";
            obj.Prop2 = "test1";
            await Task.Delay(500);
            Assert.True(raiseProp1Count == 2 && raiseProp2Count == 2);
        }

        [Fact]
        public async void ObservableGroupTest()
        {
            var raiseCount = 0;
            var group = new ObservableGroup<string>();
            group.CollectionChanged += (s, e) =>
            {
                raiseCount++;
            };
            group.Add("test");
            group.Add("test");
            group.Add("test");
            group.Add("test");
            group.Add("test");
            await Task.Delay(500);
            Assert.True(raiseCount == 5 && group.Count == 5);
        }

        [Fact]
        public async void ObservableDictionaryTest()
        {
            var raiseCount = 0;
            var dictionary = new ObservableDictionary<string, string>();
            dictionary.CollectionChanged += (s, e) =>
            {
                raiseCount++;
            };
            dictionary.Add("test", "test");
            dictionary.Add("test", "test");
            dictionary.Add("test1", "test");
            dictionary.Add("test1", "test");
            dictionary.Add("test1", "test");
            await Task.Delay(500);
            Assert.True(raiseCount == 2 && dictionary.Count == 2);
        }
    }
}
