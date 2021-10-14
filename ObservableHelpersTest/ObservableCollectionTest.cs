using ObservableHelpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ObservableHelpersTest
{
    public class ObservableCollectionTest
    {
        [Fact]
        public async void asd()
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
    }
}
