using ObservableHelpers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ObservableObjectTest
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
}
