using ObservableHelpers;
using ObservableHelpers.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ObservableStackTest
{
    public class ReadOnlyStack : ObservableStack<int>
    {
        public ReadOnlyStack()
        {
            IsReadOnly = true;
        }

        public ReadOnlyStack(IEnumerable<int> initial)
            : base(initial)
        {
            IsReadOnly = true;
        }
    }

    public class IndexerTest
    {
        [Fact]
        public void Normal()
        {
            var raiseCol = new List<NotifyCollectionChangedEventArgs>();
            var col = new ObservableStack<int>(new int[] { 1, 2, 3 });

            col.CollectionChanged += (s, e) =>
            {
                raiseCol.Add(e);
            };

            Assert.Equal(3, col[0]);
            Assert.Equal(2, col[1]);
            Assert.Equal(1, col[2]);
        }

        [Fact]
        public void Throws()
        {
            var col = new ObservableStack<int>(new int[] { 1, 2, 3 });

            Assert.Throws<ArgumentOutOfRangeException>(() => col[-1]);
            Assert.Throws<ArgumentOutOfRangeException>(() => col[4]);
        }
    }

    public class IsEmptyTest
    {
        [Fact]
        public void Normal()
        {
            var col1 = new ObservableStack<int>();
            var col2 = new ObservableStack<int>(new int[] { 1, 2, 3, 4 });

            Assert.True(col1.IsEmpty);
            Assert.False(col2.IsEmpty);
        }
    }

    public class ConstructorTest
    {
        [Fact]
        public void Parameterless()
        {
            var col = new ObservableStack<int>();

            Assert.Empty(col);
        }

        [Fact]
        public void WithInitialItems()
        {
            var col = new ObservableStack<int>(new int[] { 1, 2, 3, 4 });

            Assert.Equal(4, col.Count);

            Assert.Collection(col,
                i => Assert.Equal(4, i),
                i => Assert.Equal(3, i),
                i => Assert.Equal(2, i),
                i => Assert.Equal(1, i));
        }

        [Fact]
        public void Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ObservableStack<int>(null));
        }
    }

    public class ClearTest
    {
        [Fact]
        public void Normal()
        {
            var raiseCol = new List<NotifyCollectionChangedEventArgs>();
            var col = new ObservableStack<int>(new int[] { 1, 2, 3, 4, 5, 6 });

            col.CollectionChanged += (s, e) =>
            {
                raiseCol.Add(e);
            };

            col.Clear();

            Assert.Empty(col);
            Assert.Single(raiseCol);

            Assert.Equal(NotifyCollectionChangedAction.Reset, raiseCol[0].Action);
        }

        [Fact]
        public void Throws()
        {
            var col = new ReadOnlyStack();

            Assert.Throws<NotSupportedException>(() => col.Clear());
        }
    }

    public class GetEnumeratorTest
    {
        [Fact]
        public void ForLoop()
        {
            var col = new ObservableStack<int>(new int[] { 1, 2, 3, 4 });

            var enumerator = col.GetEnumerator();
            for (int i = 0; i < col.Count; i++)
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal(col.Count - i, enumerator.Current);
            }

            Assert.False(enumerator.MoveNext());
        }
    }

    public class PeekTest
    {
        [Fact]
        public void Normal()
        {
            var col1 = new ObservableStack<int>(new int[] { 1, 2, 3 });
            var col2 = new ObservableStack<int>(new int[] { 3, 2, 1 });

            Assert.Equal(3, col1.Peek());
            Assert.Equal(1, col2.Peek());
        }

        [Fact]
        public void Throws()
        {
            var col1 = new ObservableStack<int>();

            Assert.Throws<InvalidOperationException>(() => col1.Peek());
        }
    }

    public class PopTest
    {
        [Fact]
        public void Normal()
        {
            var raiseCol = new List<NotifyCollectionChangedEventArgs>();
            var col = new ObservableStack<int>(new int[] { 1, 2, 3 });

            col.CollectionChanged += (s, e) =>
            {
                raiseCol.Add(e);
            };

            Assert.Equal(3, col.Pop());
            Assert.Equal(2, col.Pop());
            Assert.Equal(1, col.Pop());

            Assert.Equal(0, col.Count);
            Assert.Equal(3, raiseCol.Count);

            Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[0].Action);
            Assert.Equal(0, raiseCol[0].OldStartingIndex);
            Assert.Equal(3, raiseCol[0].OldItems[0]);

            Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[1].Action);
            Assert.Equal(0, raiseCol[1].OldStartingIndex);
            Assert.Equal(2, raiseCol[1].OldItems[0]);

            Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[2].Action);
            Assert.Equal(0, raiseCol[2].OldStartingIndex);
            Assert.Equal(1, raiseCol[2].OldItems[0]);
        }

        [Fact]
        public void Throws()
        {
            var col1 = new ReadOnlyStack();
            var col2 = new ObservableStack<int>(new int[] { 1, 2, 3 });

            Assert.Throws<NotSupportedException>(() => col1.Pop());

            col2.Pop();
            col2.Pop();
            col2.Pop();

            Assert.Throws<InvalidOperationException>(() => col2.Pop());
        }
    }

    public class PushTest
    {
        [Fact]
        public void Normal()
        {
            var raiseCol = new List<NotifyCollectionChangedEventArgs>();
            var col = new ObservableStack<int>();

            col.CollectionChanged += (s, e) =>
            {
                raiseCol.Add(e);
            };

            col.Push(1);
            col.Push(2);
            col.Push(3);

            Assert.Equal(3, col.Count);
            Assert.Equal(3, raiseCol.Count);

            Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[0].Action);
            Assert.Equal(0, raiseCol[0].NewStartingIndex);
            Assert.Equal(1, raiseCol[0].NewItems[0]);

            Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
            Assert.Equal(0, raiseCol[1].NewStartingIndex);
            Assert.Equal(2, raiseCol[1].NewItems[0]);

            Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[2].Action);
            Assert.Equal(0, raiseCol[2].NewStartingIndex);
            Assert.Equal(3, raiseCol[2].NewItems[0]);

            Assert.Collection(col,
                i => Assert.Equal(3, i),
                i => Assert.Equal(2, i),
                i => Assert.Equal(1, i));
        }

        [Fact]
        public void Throws()
        {
            var col = new ReadOnlyStack();

            Assert.Throws<NotSupportedException>(() => col.Push(1));
        }
    }

    public class PushRangeTest
    {
        [Fact]
        public void Normal()
        {
            var raiseCol1 = new List<NotifyCollectionChangedEventArgs>();
            var raiseCol2 = new List<NotifyCollectionChangedEventArgs>();
            var raiseCol3 = new List<NotifyCollectionChangedEventArgs>();
            var raiseCol4 = new List<NotifyCollectionChangedEventArgs>();
            var raiseCol5 = new List<NotifyCollectionChangedEventArgs>();
            var raiseCol6 = new List<NotifyCollectionChangedEventArgs>();
            var col1 = new ObservableStack<int>();
            var col2 = new ObservableStack<int>();
            var col3 = new ObservableStack<int>();
            var col4 = new ObservableStack<int>();
            var col5 = new ObservableStack<int>(new int[] { 1, 2, 3 });
            var col6 = new ObservableStack<int>();

            col1.CollectionChanged += (s, e) => raiseCol1.Add(e);
            col2.CollectionChanged += (s, e) => raiseCol2.Add(e);
            col3.CollectionChanged += (s, e) => raiseCol3.Add(e);
            col4.CollectionChanged += (s, e) => raiseCol4.Add(e);
            col5.CollectionChanged += (s, e) => raiseCol5.Add(e);
            col6.CollectionChanged += (s, e) => raiseCol6.Add(e);

            col1.PushRange(new int[] { 1, 2 });
            col2.PushRange(new int[] { 1, 2, 3 });
            col3.PushRange(new int[] { 1, 2, 3 }, 0, 1);
            col4.PushRange(new int[] { 1, 2, 3 }, 1, 1);
            col5.PushRange(new int[] { 1, 2, 3 }, 1, 2);
            col6.PushRange(new int[] { 1, 2, 3 }, 2, 1);

            Assert.Collection(col1,
                i => Assert.Equal(2, i),
                i => Assert.Equal(1, i));
            Assert.Collection(col2,
                i => Assert.Equal(3, i),
                i => Assert.Equal(2, i),
                i => Assert.Equal(1, i));
            Assert.Collection(col3,
                i => Assert.Equal(1, i));
            Assert.Collection(col4,
                i => Assert.Equal(2, i));
            Assert.Collection(col5,
                i => Assert.Equal(3, i),
                i => Assert.Equal(2, i),
                i => Assert.Equal(3, i),
                i => Assert.Equal(2, i),
                i => Assert.Equal(1, i));
            Assert.Collection(col6,
                i => Assert.Equal(3, i));

            Assert.Equal(2, col1.Count);
            Assert.Equal(3, col2.Count);
            Assert.Equal(1, col3.Count);
            Assert.Equal(1, col4.Count);
            Assert.Equal(5, col5.Count);
            Assert.Equal(1, col6.Count);

            Assert.Single(raiseCol1);
            Assert.Single(raiseCol2);
            Assert.Single(raiseCol3);
            Assert.Single(raiseCol4);
            Assert.Single(raiseCol5);
            Assert.Single(raiseCol6);

            Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol1[0].Action);
            Assert.Equal(0, raiseCol1[0].NewStartingIndex);
            Assert.Equal(2, raiseCol1[0].NewItems[0]);
            Assert.Equal(1, raiseCol1[0].NewItems[1]);

            Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol2[0].Action);
            Assert.Equal(0, raiseCol2[0].NewStartingIndex);
            Assert.Equal(3, raiseCol2[0].NewItems[0]);
            Assert.Equal(2, raiseCol2[0].NewItems[1]);
            Assert.Equal(1, raiseCol2[0].NewItems[2]);

            Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol3[0].Action);
            Assert.Equal(0, raiseCol3[0].NewStartingIndex);
            Assert.Equal(1, raiseCol3[0].NewItems[0]);

            Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol4[0].Action);
            Assert.Equal(0, raiseCol4[0].NewStartingIndex);
            Assert.Equal(2, raiseCol4[0].NewItems[0]);

            Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol5[0].Action);
            Assert.Equal(0, raiseCol5[0].NewStartingIndex);
            Assert.Equal(3, raiseCol5[0].NewItems[0]);
            Assert.Equal(2, raiseCol5[0].NewItems[1]);

            Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol6[0].Action);
            Assert.Equal(0, raiseCol6[0].NewStartingIndex);
            Assert.Equal(3, raiseCol6[0].NewItems[0]);
        }

        [Fact]
        public void Throws()
        {
            var col = new ReadOnlyStack();
            var col1 = new ObservableStack<int>();
            var col2 = new ObservableStack<int>(new int[] { 1, 2, 3 });

            Assert.Throws<NotSupportedException>(() => col.PushRange(new int[] { 1, 2, 3 }));
            Assert.Throws<ArgumentNullException>(() => col1.PushRange(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => col1.PushRange(new int[] { 1, 2, 3 }, 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => col1.PushRange(new int[] { 1, 2, 3 }, -1, 1));
            Assert.Throws<ArgumentException>(() => col1.PushRange(new int[] { 1, 2, 3 }, 2, 2));
        }
    }

    public class PopAndPushTest
    {
        [Fact]
        public void Normal()
        {
            var raiseCol = new List<NotifyCollectionChangedEventArgs>();
            var col = new ObservableStack<int>();

            col.CollectionChanged += (s, e) =>
            {
                raiseCol.Add(e);
            };

            col.Push(1);
            Assert.Collection(col,
                i => Assert.Equal(1, i));

            col.Push(2);
            Assert.Collection(col,
                i => Assert.Equal(2, i),
                i => Assert.Equal(1, i));

            Assert.Equal(2, col.Pop());
            Assert.Collection(col,
                i => Assert.Equal(1, i));

            Assert.Equal(1, col.Pop());
            Assert.Empty(col);

            col.Push(3);
            Assert.Collection(col,
                i => Assert.Equal(3, i));

            Assert.Equal(3, col.Pop());
            Assert.Empty(col);

            Assert.Equal(0, col.Count);
            Assert.Equal(6, raiseCol.Count);

            Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[0].Action);
            Assert.Equal(0, raiseCol[0].NewStartingIndex);
            Assert.Equal(1, raiseCol[0].NewItems[0]);

            Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
            Assert.Equal(0, raiseCol[1].NewStartingIndex);
            Assert.Equal(2, raiseCol[1].NewItems[0]);

            Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[2].Action);
            Assert.Equal(0, raiseCol[2].OldStartingIndex);
            Assert.Equal(2, raiseCol[2].OldItems[0]);

            Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[3].Action);
            Assert.Equal(0, raiseCol[3].OldStartingIndex);
            Assert.Equal(1, raiseCol[3].OldItems[0]);

            Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[4].Action);
            Assert.Equal(0, raiseCol[4].NewStartingIndex);
            Assert.Equal(3, raiseCol[4].NewItems[0]);

            Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[5].Action);
            Assert.Equal(0, raiseCol[5].OldStartingIndex);
            Assert.Equal(3, raiseCol[5].OldItems[0]);
        }
    }

    public class ToArrayTest
    {
        [Fact]
        public void Normal()
        {
            var col1 = new ObservableStack<int>();
            var col2 = new ObservableStack<int>(new int[] { 1, 2, 3 });

            var arr1 = col1.ToArray();
            var arr2 = col2.ToArray();

            Assert.Empty(arr1);

            Assert.Collection(arr2,
                i => Assert.Equal(3, i),
                i => Assert.Equal(2, i),
                i => Assert.Equal(1, i));
        }
    }

    public class TryPeekTest
    {
        [Fact]
        public void Normal()
        {
            var col1 = new ObservableStack<int>(new int[] { 1, 2, 3 });
            var col2 = new ObservableStack<int>(new int[] { 3, 2, 1 });
            var col3 = new ObservableStack<int>();

            Assert.True(col1.TryPeek(out int val1));
            Assert.True(col2.TryPeek(out int val2));
            Assert.False(col3.TryPeek(out _));

            Assert.Equal(3, val1);
            Assert.Equal(1, val2);
        }
    }

    public class TryPopTest
    {
        [Fact]
        public void Normal()
        {
            var raiseCol = new List<NotifyCollectionChangedEventArgs>();
            var col = new ObservableStack<int>(new int[] { 1, 2, 3 });

            col.CollectionChanged += (s, e) =>
            {
                raiseCol.Add(e);
            };

            Assert.True(col.TryPop(out int val1));
            Assert.True(col.TryPop(out int val2));
            Assert.True(col.TryPop(out int val3));
            Assert.False(col.TryPop(out _));

            Assert.Equal(3, val1);
            Assert.Equal(2, val2);
            Assert.Equal(1, val3);

            Assert.Equal(0, col.Count);
            Assert.Equal(3, raiseCol.Count);

            Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[0].Action);
            Assert.Equal(0, raiseCol[0].OldStartingIndex);
            Assert.Equal(3, raiseCol[0].OldItems[0]);

            Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[1].Action);
            Assert.Equal(0, raiseCol[1].OldStartingIndex);
            Assert.Equal(2, raiseCol[1].OldItems[0]);

            Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[2].Action);
            Assert.Equal(0, raiseCol[2].OldStartingIndex);
            Assert.Equal(1, raiseCol[2].OldItems[0]);
        }

        [Fact]
        public void Throws()
        {
            var col = new ReadOnlyStack();

            Assert.Throws<NotSupportedException>(() => col.TryPop(out _));
        }
    }

    public class TryPopRangeTest
    {
        [Fact]
        public void Normal()
        {
            var raiseCol1 = new List<NotifyCollectionChangedEventArgs>();
            var raiseCol2 = new List<NotifyCollectionChangedEventArgs>();
            var raiseCol3 = new List<NotifyCollectionChangedEventArgs>();
            var raiseCol4 = new List<NotifyCollectionChangedEventArgs>();
            var raiseCol5 = new List<NotifyCollectionChangedEventArgs>();
            var raiseCol6 = new List<NotifyCollectionChangedEventArgs>();
            var col1 = new ObservableStack<int>(new int[] { 1, 2, 3 });
            var col2 = new ObservableStack<int>(new int[] { 1, 2, 3 });
            var col3 = new ObservableStack<int>(new int[] { 1, 2, 3 });
            var col4 = new ObservableStack<int>(new int[] { 1, 2, 3 });
            var col5 = new ObservableStack<int>(new int[] { 1, 2, 3 });
            var col6 = new ObservableStack<int>(new int[] { 1, 2, 3 });

            col1.CollectionChanged += (s, e) => raiseCol1.Add(e);
            col2.CollectionChanged += (s, e) => raiseCol2.Add(e);
            col3.CollectionChanged += (s, e) => raiseCol3.Add(e);
            col4.CollectionChanged += (s, e) => raiseCol4.Add(e);
            col5.CollectionChanged += (s, e) => raiseCol5.Add(e);
            col6.CollectionChanged += (s, e) => raiseCol6.Add(e);

            int[] vals1 = new int[2];
            int[] vals2 = new int[3];
            int[] vals3 = new int[5];
            int[] vals4 = new int[4];
            int[] vals5 = new int[4];
            int[] vals6 = new int[4];

            Assert.Equal(2, col1.TryPopRange(vals1));
            Assert.Equal(3, col2.TryPopRange(vals2));
            Assert.Equal(3, col3.TryPopRange(vals3, 1, 4));
            Assert.Equal(1, col4.TryPopRange(vals4, 3, 1));
            Assert.Equal(2, col5.TryPopRange(vals5, 2, 2));
            Assert.Equal(3, col6.TryPopRange(vals6, 1, 3));

            Assert.Collection(vals1,
                i => Assert.Equal(3, i),
                i => Assert.Equal(2, i));
            Assert.Collection(vals2,
                i => Assert.Equal(3, i),
                i => Assert.Equal(2, i),
                i => Assert.Equal(1, i));
            Assert.Collection(vals3,
                i => Assert.Equal(0, i),
                i => Assert.Equal(3, i),
                i => Assert.Equal(2, i),
                i => Assert.Equal(1, i),
                i => Assert.Equal(0, i));
            Assert.Collection(vals4,
                i => Assert.Equal(0, i),
                i => Assert.Equal(0, i),
                i => Assert.Equal(0, i),
                i => Assert.Equal(3, i));
            Assert.Collection(vals5,
                i => Assert.Equal(0, i),
                i => Assert.Equal(0, i),
                i => Assert.Equal(3, i),
                i => Assert.Equal(2, i));
            Assert.Collection(vals6,
                i => Assert.Equal(0, i),
                i => Assert.Equal(3, i),
                i => Assert.Equal(2, i),
                i => Assert.Equal(1, i));

            Assert.Equal(1, col1.Count);
            Assert.Equal(0, col2.Count);
            Assert.Equal(0, col3.Count);
            Assert.Equal(2, col4.Count);
            Assert.Equal(1, col5.Count);
            Assert.Equal(0, col6.Count);

            Assert.Single(raiseCol1);
            Assert.Single(raiseCol2);
            Assert.Single(raiseCol3);
            Assert.Single(raiseCol4);
            Assert.Single(raiseCol5);
            Assert.Single(raiseCol6);

            Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol1[0].Action);
            Assert.Equal(0, raiseCol1[0].OldStartingIndex);
            Assert.Equal(3, raiseCol1[0].OldItems[0]);
            Assert.Equal(2, raiseCol1[0].OldItems[1]);

            Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol2[0].Action);
            Assert.Equal(0, raiseCol2[0].OldStartingIndex);
            Assert.Equal(3, raiseCol2[0].OldItems[0]);
            Assert.Equal(2, raiseCol2[0].OldItems[1]);
            Assert.Equal(1, raiseCol2[0].OldItems[2]);

            Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol3[0].Action);
            Assert.Equal(0, raiseCol3[0].OldStartingIndex);
            Assert.Equal(3, raiseCol3[0].OldItems[0]);
            Assert.Equal(2, raiseCol3[0].OldItems[1]);
            Assert.Equal(1, raiseCol3[0].OldItems[2]);

            Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol4[0].Action);
            Assert.Equal(0, raiseCol4[0].OldStartingIndex);
            Assert.Equal(3, raiseCol4[0].OldItems[0]);

            Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol5[0].Action);
            Assert.Equal(0, raiseCol5[0].OldStartingIndex);
            Assert.Equal(3, raiseCol5[0].OldItems[0]);
            Assert.Equal(2, raiseCol5[0].OldItems[1]);

            Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol6[0].Action);
            Assert.Equal(0, raiseCol6[0].OldStartingIndex);
            Assert.Equal(3, raiseCol6[0].OldItems[0]);
            Assert.Equal(2, raiseCol6[0].OldItems[1]);
            Assert.Equal(1, raiseCol6[0].OldItems[2]);
        }

        [Fact]
        public void Throws()
        {
            var col = new ReadOnlyStack();
            var col1 = new ObservableStack<int>(new int[] { 1, 2, 3 });

            int[] vals = Array.Empty<int>();
            int[] vals1 = new int[2];

            Assert.Throws<NotSupportedException>(() => col.TryPopRange(vals));
            Assert.Throws<ArgumentNullException>(() => col1.TryPopRange(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => col1.TryPopRange(vals1, 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => col1.TryPopRange(vals1, -1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => col1.TryPopRange(vals1, 2, 1));
            Assert.Throws<ArgumentException>(() => col1.TryPopRange(vals1, 1, 2));
        }
    }
}
