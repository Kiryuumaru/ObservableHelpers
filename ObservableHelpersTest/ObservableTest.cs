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
            Assert.True(obj.Prop1 == obj.Prop2);
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

            context5.SynchronizationOperation.SetContext(context4);
            context4.SynchronizationOperation.SetContext(context3);
            context3.SynchronizationOperation.SetContext(context2);
            context2.SynchronizationOperation.SetContext(context1);
            context1.SynchronizationOperation.SetContext(contextMain);

            contextMain.SynchronizationOperation.SetContext(
                callback =>
                {
                    hasPostRaised = true;
                    callback();
                }, callback =>
                {
                    hasSendRaised = true;
                    callback();
                });

            context5.SynchronizationOperation.ContextPost(delegate { });
            context5.SynchronizationOperation.ContextSend(delegate { });

            await Task.Delay(500);

            Assert.True(hasPostRaised && hasSendRaised);
        }
    }
}
