using ObservableHelpers;
using System;
using System.Linq;
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

        public int PropCount
        {
            get => GetRawProperties().Count();
        }

        public bool IsAllDefault
        {
            get => GetRawProperties().All(i => i.IsDefault);
        }

        public bool IsAllNotDefault
        {
            get => GetRawProperties().All(i => !i.IsDefault);
        }

        public TestObject()
        {
            InitializeProperties();
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
            var obj2 = new TestObject();
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
            obj.Prop1 = "test";
            obj.Prop1 = "test";
            obj.Prop1 = "test1";
            obj.Prop1 = "test1";
            obj.Prop2 = "test";
            obj.Prop2 = "test";
            obj.Prop2 = "test1";
            obj.Prop2 = "test1";
            obj.Prop2 = "test";
            obj.Prop2 = "test";
            obj.Prop2 = "test1";
            obj.Prop2 = "test1";
            await Task.Delay(500);
            Assert.Equal(4, raiseProp1Count);
            Assert.Equal(4, raiseProp2Count);
            Assert.Equal(obj.Prop1, obj.Prop2);
            Assert.Equal(2, obj.PropCount);
            Assert.True(obj2.IsAllDefault);
            obj2.Prop1 = "test1";
            obj2.Prop2 = "test";
            Assert.True(obj2.IsAllNotDefault);
        }

        [Fact]
        public async void ObservableCollectionTest()
        {
            var raiseCount = 0;
            var collection = new ObservableCollection<string>();
            collection.CollectionChanged += (s, e) =>
            {
                raiseCount++;
            };
            collection.Add("test");
            collection.Add("test");
            await Task.Delay(500);
            Assert.True(raiseCount == 2 && collection.Count == 2);
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

        [Fact]
        public async void SyncContextTest()
        {
            bool hasPostRaised = false;
            bool hasSendRaised = false;
            var contextMain = new SyncContext();
            var context1 = new SyncContext();
            var context2 = new SyncContext();
            var context3 = new SyncContext();
            var context4 = new SyncContext();
            var context5 = new SyncContext();

            context5.SyncOperation.SetContext(context4);
            context3.SyncOperation.SetContext(context2);
            context1.SyncOperation.SetContext(contextMain);
            context4.SyncOperation.SetContext(context3);
            context2.SyncOperation.SetContext(context1);

            contextMain.SyncOperation.SetContext(
                callback =>
                {
                    hasPostRaised = true;
                    callback.callback();
                }, callback =>
                {
                    hasSendRaised = true;
                    callback.callback();
                });

            context5.SyncOperation.ContextPost(delegate { });
            context5.SyncOperation.ContextSend(delegate { });

            await Task.Delay(500);

            Assert.True(hasPostRaised && hasSendRaised);
        }
    }
}
