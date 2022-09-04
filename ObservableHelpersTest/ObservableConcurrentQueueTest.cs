using ObservableHelpers;
using ObservableHelpers.Collections;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Xunit;
using ArgumentNullException = System.ArgumentNullException;

namespace ObservableConcurrentQueueTest;

public class ReadOnlyQueue : ObservableConcurrentQueue<int>
{
    public ReadOnlyQueue()
    {
        IsReadOnly = true;
    }

    public ReadOnlyQueue(IEnumerable<int> initial)
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
        var col = new ObservableConcurrentQueue<int>(new int[] { 1, 2, 3 });

        col.CollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        Assert.Equal(1, col[0]);
        Assert.Equal(2, col[1]);
        Assert.Equal(3, col[2]);
    }

    [Fact]
    public void Throws()
    {
        var col = new ObservableConcurrentQueue<int>(new int[] { 1, 2, 3 });

        Assert.Throws<ArgumentOutOfRangeException>(() => col[-1]);
        Assert.Throws<ArgumentOutOfRangeException>(() => col[4]);
    }
}

public class IsEmptyTest
{
    [Fact]
    public void Normal()
    {
        var col1 = new ObservableConcurrentQueue<int>();
        var col2 = new ObservableConcurrentQueue<int>(new int[] { 1, 2, 3, 4 });

        Assert.True(col1.IsEmpty);
        Assert.False(col2.IsEmpty);
    }
}

public class ConstructorTest
{
    [Fact]
    public void Parameterless()
    {
        var col = new ObservableConcurrentQueue<int>();

        Assert.Empty(col);
    }

    [Fact]
    public void WithInitialItems()
    {
        var col = new ObservableConcurrentQueue<int>(new int[] { 1, 2, 3, 4 });

        Assert.Equal(4, col.Count);

        Assert.Collection(col,
            i => Assert.Equal(1, i),
            i => Assert.Equal(2, i),
            i => Assert.Equal(3, i),
            i => Assert.Equal(4, i));
    }

    [Fact]
    public void Throws()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => new ObservableConcurrentQueue<int>(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}

public class ClearTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var col = new ObservableConcurrentQueue<int>(new int[] { 1, 2, 3, 4, 5, 6 });

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
        var col = new ReadOnlyQueue();

        Assert.Throws<NotSupportedException>(() => col.Clear());
    }
}

public class DequeueTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var col = new ObservableConcurrentQueue<int>(new int[] { 1, 2, 3 });

        col.CollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        Assert.Equal(1, col.Dequeue());
        Assert.Equal(2, col.Dequeue());
        Assert.Equal(3, col.Dequeue());

        Assert.Equal(0, col.Count);
        Assert.Equal(3, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].OldStartingIndex);
        Assert.Equal(1, raiseCol[0]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[1].Action);
        Assert.Equal(0, raiseCol[1].OldStartingIndex);
        Assert.Equal(2, raiseCol[1]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[2].Action);
        Assert.Equal(0, raiseCol[2].OldStartingIndex);
        Assert.Equal(3, raiseCol[2]?.OldItems?[0]);
    }

    [Fact]
    public void Throws()
    {
        var col1 = new ReadOnlyQueue();
        var col2 = new ObservableConcurrentQueue<int>(new int[] { 1, 2, 3 });

        Assert.Throws<NotSupportedException>(() => col1.Dequeue());

        col2.Dequeue();
        col2.Dequeue();
        col2.Dequeue();

        Assert.Throws<InvalidOperationException>(() => col2.Dequeue());
    }
}

public class EnqueueTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var col = new ObservableConcurrentQueue<int>();

        col.CollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        col.Enqueue(1);
        col.Enqueue(2);
        col.Enqueue(3);

        Assert.Equal(3, col.Count);
        Assert.Equal(3, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].NewStartingIndex);
        Assert.Equal(1, raiseCol[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
        Assert.Equal(1, raiseCol[1].NewStartingIndex);
        Assert.Equal(2, raiseCol[1]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[2].Action);
        Assert.Equal(2, raiseCol[2].NewStartingIndex);
        Assert.Equal(3, raiseCol[2]?.NewItems?[0]);

        Assert.Collection(col,
            i => Assert.Equal(1, i),
            i => Assert.Equal(2, i),
            i => Assert.Equal(3, i));
    }

    [Fact]
    public void Throws()
    {
        var col = new ReadOnlyQueue();

        Assert.Throws<NotSupportedException>(() => col.Enqueue(1));
    }
}

public class EnqueueRangeTest
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
        var col1 = new ObservableConcurrentQueue<int>();
        var col2 = new ObservableConcurrentQueue<int>();
        var col3 = new ObservableConcurrentQueue<int>();
        var col4 = new ObservableConcurrentQueue<int>();
        var col5 = new ObservableConcurrentQueue<int>(new int[] { 1, 2, 3 });
        var col6 = new ObservableConcurrentQueue<int>();

        col1.CollectionChanged += (s, e) => raiseCol1.Add(e);
        col2.CollectionChanged += (s, e) => raiseCol2.Add(e);
        col3.CollectionChanged += (s, e) => raiseCol3.Add(e);
        col4.CollectionChanged += (s, e) => raiseCol4.Add(e);
        col5.CollectionChanged += (s, e) => raiseCol5.Add(e);
        col6.CollectionChanged += (s, e) => raiseCol6.Add(e);

        col1.EnqueueRange(new int[] { 1, 2 });
        col2.EnqueueRange(new int[] { 1, 2, 3 });
        col3.EnqueueRange(new int[] { 1, 2, 3 }, 0, 1);
        col4.EnqueueRange(new int[] { 1, 2, 3 }, 1, 1);
        col5.EnqueueRange(new int[] { 1, 2, 3 }, 1, 2);
        col6.EnqueueRange(new int[] { 1, 2, 3 }, 2, 1);

        Assert.Collection(col1,
            i => Assert.Equal(1, i),
            i => Assert.Equal(2, i));
        Assert.Collection(col2,
            i => Assert.Equal(1, i),
            i => Assert.Equal(2, i),
            i => Assert.Equal(3, i));
        Assert.Collection(col3,
            i => Assert.Equal(1, i));
        Assert.Collection(col4,
            i => Assert.Equal(2, i));
        Assert.Collection(col5,
            i => Assert.Equal(1, i),
            i => Assert.Equal(2, i),
            i => Assert.Equal(3, i),
            i => Assert.Equal(2, i),
            i => Assert.Equal(3, i));
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
        Assert.Equal(1, raiseCol1[0]?.NewItems?[0]);
        Assert.Equal(2, raiseCol1[0]?.NewItems?[1]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol2[0].Action);
        Assert.Equal(0, raiseCol2[0].NewStartingIndex);
        Assert.Equal(1, raiseCol2[0]?.NewItems?[0]);
        Assert.Equal(2, raiseCol2[0]?.NewItems?[1]);
        Assert.Equal(3, raiseCol2[0]?.NewItems?[2]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol3[0].Action);
        Assert.Equal(0, raiseCol3[0].NewStartingIndex);
        Assert.Equal(1, raiseCol3[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol4[0].Action);
        Assert.Equal(0, raiseCol4[0].NewStartingIndex);
        Assert.Equal(2, raiseCol4[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol5[0].Action);
        Assert.Equal(3, raiseCol5[0].NewStartingIndex);
        Assert.Equal(2, raiseCol5[0]?.NewItems?[0]);
        Assert.Equal(3, raiseCol5[0]?.NewItems?[1]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol6[0].Action);
        Assert.Equal(0, raiseCol6[0].NewStartingIndex);
        Assert.Equal(3, raiseCol6[0]?.NewItems?[0]);
    }

    [Fact]
    public void Throws()
    {
        var col = new ReadOnlyQueue();
        var col1 = new ObservableConcurrentQueue<int>();
        var col2 = new ObservableConcurrentQueue<int>(new int[] { 1, 2, 3 });

        Assert.Throws<NotSupportedException>(() => col.EnqueueRange(new int[] { 1, 2, 3 }));
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => col1.EnqueueRange(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentOutOfRangeException>(() => col1.EnqueueRange(new int[] { 1, 2, 3 }, 0, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => col1.EnqueueRange(new int[] { 1, 2, 3 }, -1, 1));
        Assert.Throws<ArgumentException>(() => col1.EnqueueRange(new int[] { 1, 2, 3 }, 2, 2));
    }
}

public class DequeueAndEnqueueTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var col = new ObservableConcurrentQueue<int>();

        col.CollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        col.Enqueue(1);
        Assert.Collection(col,
            i => Assert.Equal(1, i));

        col.Enqueue(2);
        Assert.Collection(col,
            i => Assert.Equal(1, i),
            i => Assert.Equal(2, i));

        Assert.Equal(1, col.Dequeue());
        Assert.Collection(col,
            i => Assert.Equal(2, i));

        Assert.Equal(2, col.Dequeue());
        Assert.Empty(col);

        col.Enqueue(3);
        Assert.Collection(col,
            i => Assert.Equal(3, i));

        Assert.Equal(3, col.Dequeue());
        Assert.Empty(col);

        Assert.Equal(0, col.Count);
        Assert.Equal(6, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].NewStartingIndex);
        Assert.Equal(1, raiseCol[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
        Assert.Equal(1, raiseCol[1].NewStartingIndex);
        Assert.Equal(2, raiseCol[1]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[2].Action);
        Assert.Equal(0, raiseCol[2].OldStartingIndex);
        Assert.Equal(1, raiseCol[2]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[3].Action);
        Assert.Equal(0, raiseCol[3].OldStartingIndex);
        Assert.Equal(2, raiseCol[3]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[4].Action);
        Assert.Equal(0, raiseCol[4].NewStartingIndex);
        Assert.Equal(3, raiseCol[4]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[5].Action);
        Assert.Equal(0, raiseCol[5].OldStartingIndex);
        Assert.Equal(3, raiseCol[5]?.OldItems?[0]);
    }
}

public class GetEnumeratorTest
{
    [Fact]
    public void ForLoop()
    {
        var col = new ObservableConcurrentQueue<int>(new int[] { 1, 2, 3, 4 });

        var enumerator = col.GetEnumerator();
        for (int i = 0; i < col.Count; i++)
        {
            Assert.True(enumerator.MoveNext());
            Assert.Equal(i + 1, enumerator.Current);
        }

        Assert.False(enumerator.MoveNext());
    }
}

public class PeekTest
{
    [Fact]
    public void Normal()
    {
        var col1 = new ObservableConcurrentQueue<int>(new int[] { 1, 2, 3 });
        var col2 = new ObservableConcurrentQueue<int>(new int[] { 3, 2, 1 });

        Assert.Equal(1, col1.Peek());
        Assert.Equal(3, col2.Peek());
    }

    [Fact]
    public void Throws()
    {
        var col1 = new ObservableConcurrentQueue<int>();

        Assert.Throws<InvalidOperationException>(() => col1.Peek());
    }
}

public class ToArrayTest
{
    [Fact]
    public void Normal()
    {
        var col1 = new ObservableConcurrentQueue<int>();
        var col2 = new ObservableConcurrentQueue<int>(new int[] { 1, 2, 3 });

        var arr1 = col1.ToArray();
        var arr2 = col2.ToArray();

        Assert.Empty(arr1);

        Assert.Collection(arr2,
            i => Assert.Equal(1, i),
            i => Assert.Equal(2, i),
            i => Assert.Equal(3, i));
    }
}

public class TryDequeueTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var col = new ObservableConcurrentQueue<int>(new int[] { 1, 2, 3 });

        col.CollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        Assert.True(col.TryDequeue(out int val1));
        Assert.True(col.TryDequeue(out int val2));
        Assert.True(col.TryDequeue(out int val3));
        Assert.False(col.TryDequeue(out _));

        Assert.Equal(1, val1);
        Assert.Equal(2, val2);
        Assert.Equal(3, val3);

        Assert.Equal(0, col.Count);
        Assert.Equal(3, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].OldStartingIndex);
        Assert.Equal(1, raiseCol[0]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[1].Action);
        Assert.Equal(0, raiseCol[1].OldStartingIndex);
        Assert.Equal(2, raiseCol[1]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[2].Action);
        Assert.Equal(0, raiseCol[2].OldStartingIndex);
        Assert.Equal(3, raiseCol[2]?.OldItems?[0]);
    }

    [Fact]
    public void Throws()
    {
        var col = new ReadOnlyQueue();

        Assert.Throws<NotSupportedException>(() => col.TryDequeue(out _));
    }
}

public class TryDequeueRangeTest
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
        var col1 = new ObservableConcurrentQueue<int>(new int[] { 1, 2, 3 });
        var col2 = new ObservableConcurrentQueue<int>(new int[] { 1, 2, 3 });
        var col3 = new ObservableConcurrentQueue<int>(new int[] { 1, 2, 3 });
        var col4 = new ObservableConcurrentQueue<int>(new int[] { 1, 2, 3 });
        var col5 = new ObservableConcurrentQueue<int>(new int[] { 1, 2, 3 });
        var col6 = new ObservableConcurrentQueue<int>(new int[] { 1, 2, 3 });

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

        Assert.Equal(2, col1.TryDequeueRange(vals1));
        Assert.Equal(3, col2.TryDequeueRange(vals2));
        Assert.Equal(3, col3.TryDequeueRange(vals3, 1, 4));
        Assert.Equal(1, col4.TryDequeueRange(vals4, 3, 1));
        Assert.Equal(2, col5.TryDequeueRange(vals5, 2, 2));
        Assert.Equal(3, col6.TryDequeueRange(vals6, 1, 3));

        Assert.Collection(vals1,
            i => Assert.Equal(1, i),
            i => Assert.Equal(2, i));
        Assert.Collection(vals2,
            i => Assert.Equal(1, i),
            i => Assert.Equal(2, i),
            i => Assert.Equal(3, i));
        Assert.Collection(vals3,
            i => Assert.Equal(0, i),
            i => Assert.Equal(1, i),
            i => Assert.Equal(2, i),
            i => Assert.Equal(3, i),
            i => Assert.Equal(0, i));
        Assert.Collection(vals4,
            i => Assert.Equal(0, i),
            i => Assert.Equal(0, i),
            i => Assert.Equal(0, i),
            i => Assert.Equal(1, i));
        Assert.Collection(vals5,
            i => Assert.Equal(0, i),
            i => Assert.Equal(0, i),
            i => Assert.Equal(1, i),
            i => Assert.Equal(2, i));
        Assert.Collection(vals6,
            i => Assert.Equal(0, i),
            i => Assert.Equal(1, i),
            i => Assert.Equal(2, i),
            i => Assert.Equal(3, i));

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
        Assert.Equal(1, raiseCol1[0]?.OldItems?[0]);
        Assert.Equal(2, raiseCol1[0]?.OldItems?[1]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol2[0].Action);
        Assert.Equal(0, raiseCol2[0].OldStartingIndex);
        Assert.Equal(1, raiseCol2[0]?.OldItems?[0]);
        Assert.Equal(2, raiseCol2[0]?.OldItems?[1]);
        Assert.Equal(3, raiseCol2[0]?.OldItems?[2]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol3[0].Action);
        Assert.Equal(0, raiseCol3[0].OldStartingIndex);
        Assert.Equal(1, raiseCol3[0]?.OldItems?[0]);
        Assert.Equal(2, raiseCol3[0]?.OldItems?[1]);
        Assert.Equal(3, raiseCol3[0]?.OldItems?[2]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol4[0].Action);
        Assert.Equal(0, raiseCol4[0].OldStartingIndex);
        Assert.Equal(1, raiseCol4[0]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol5[0].Action);
        Assert.Equal(0, raiseCol5[0].OldStartingIndex);
        Assert.Equal(1, raiseCol5[0]?.OldItems?[0]);
        Assert.Equal(2, raiseCol5[0]?.OldItems?[1]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol6[0].Action);
        Assert.Equal(0, raiseCol6[0].OldStartingIndex);
        Assert.Equal(1, raiseCol6[0]?.OldItems?[0]);
        Assert.Equal(2, raiseCol6[0]?.OldItems?[1]);
        Assert.Equal(3, raiseCol6[0]?.OldItems?[2]);
    }

    [Fact]
    public void Throws()
    {
        var col = new ReadOnlyQueue();
        var col1 = new ObservableConcurrentQueue<int>(new int[] { 1, 2, 3 });

        int[] vals = Array.Empty<int>();
        int[] vals1 = new int[2];

        Assert.Throws<NotSupportedException>(() => col.TryDequeueRange(vals));
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => col1.TryDequeueRange(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentOutOfRangeException>(() => col1.TryDequeueRange(vals1, 0, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => col1.TryDequeueRange(vals1, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => col1.TryDequeueRange(vals1, 2, 1));
        Assert.Throws<ArgumentException>(() => col1.TryDequeueRange(vals1, 1, 2));
    }
}

public class TryPeekTest
{
    [Fact]
    public void Normal()
    {
        var col1 = new ObservableConcurrentQueue<int>(new int[] { 1, 2, 3 });
        var col2 = new ObservableConcurrentQueue<int>(new int[] { 3, 2, 1 });
        var col3 = new ObservableConcurrentQueue<int>();

        Assert.True(col1.TryPeek(out int val1));
        Assert.True(col2.TryPeek(out int val2));
        Assert.False(col3.TryPeek(out _));

        Assert.Equal(1, val1);
        Assert.Equal(3, val2);
    }
}
