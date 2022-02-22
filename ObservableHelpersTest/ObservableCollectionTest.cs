using ObservableHelpers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ObservableCollectionTest;

public class ReadOnlyCollection : ObservableCollection<int>
{
    public ReadOnlyCollection()
    {
        IsReadOnly = true;
    }

    public ReadOnlyCollection(IEnumerable<int> initial)
        : base(initial)
    {
        IsReadOnly = true;
    }
}

public class ConcurrencyTest
{
    [Fact]
    public async void WithConcurrency1()
    {
        var raiseCount1 = 0;
        var col = new ObservableCollection<int>();

        Assert.Empty(col);

        col.ImmediateCollectionChanged += (s, e) =>
        {
            Interlocked.Increment(ref raiseCount1);
        };

        await Task.WhenAll(
            Task.Run(delegate
            {
                for (int i = 0; i < 10000; i++)
                {
                    col.Add(i);
                }
            }),
            Task.Run(delegate
            {
                for (int i = 10000; i < 20000; i++)
                {
                    col.Add(i);
                }
            }));

        Assert.Equal(20000, raiseCount1);
        Assert.Equal(20000, col.Count);

        for (int i = 0; i < 20000; i++)
        {
            Assert.Contains(i, col);
        }
    }

    [Fact]
    public async void WithConcurrency2()
    {
        var raiseCount1 = 0;
        var col = new ObservableCollection<int>();

        col.ImmediateCollectionChanged += (s, e) =>
        {
            Interlocked.Increment(ref raiseCount1);
        };

        await Task.WhenAll(
            Task.Run(delegate
            {
                for (int i = 0; i < 10000; i++)
                {
                    col.AddRange(i, i + 10000, i + 20000);
                }
            }),
            Task.Run(delegate
            {
                for (int i = 30000; i < 40000; i++)
                {
                    col.AddRange(new int[] { i, i + 10000, i + 20000 } as IEnumerable<int>);
                }
            }));

        Assert.Equal(20000, raiseCount1);
        Assert.Equal(60000, col.Count);

        for (int i = 0; i < 60000; i++)
        {
            Assert.Contains(i, col);
        }
    }


    [Fact]
    public async void WithConcurrency3()
    {
        var raiseCount1 = 0;
        var col = new ObservableCollection<int>();

        col.ImmediateCollectionChanged += (s, e) =>
        {
            Interlocked.Increment(ref raiseCount1);
        };

        await Task.WhenAll(
            Task.Run(delegate
            {
                for (int i = 0; i < 10000; i++)
                {
                    col.AddRange(i, i + 10000, i + 20000);
                }
            }),
            Task.Run(delegate
            {
                for (int i = 30000; i < 40000; i++)
                {
                    col.AddRange(new int[] { i, i + 10000, i + 20000 } as IEnumerable<int>);
                }
            }));

        for (int i = 0; i < 60000; i++)
        {
            Assert.Contains(i, col);
        }

        col.Clear();

        Assert.Equal(20001, raiseCount1);
        Assert.Empty(col);
    }

    [Fact]
    public async void WithConcurrency4()
    {
        var raiseCount1 = 0;
        var col = new ObservableCollection<int>();

        col.ImmediateCollectionChanged += (s, e) =>
        {
            Interlocked.Increment(ref raiseCount1);
        };

        await Task.WhenAll(
            Task.Run(delegate
            {
                for (int i = 0; i < 10000; i++)
                {
                    col.AddRange(i, i + 10000, i + 20000);
                }
            }),
            Task.Run(delegate
            {
                for (int i = 30000; i < 40000; i++)
                {
                    col.AddRange(new int[] { i, i + 10000, i + 20000 } as IEnumerable<int>);
                }
            }));

        Assert.Equal(20000, raiseCount1);
        Assert.Equal(60000, col.Count);

        for (int i = 0; i < 70000; i++)
        {
            if (i >= 60000)
            {
                Assert.False(col.Contains(i));
            }
            else
            {
                Assert.True(col.Contains(i));
            }
        }
    }
}

public class IndexerTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3 });

        col.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        Assert.Equal(1, col[0]);
        Assert.Equal(2, col[1]);
        Assert.Equal(3, col[2]);

        col[2] = 33;

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[0].Action);
        Assert.Equal(2, raiseCol[0].OldStartingIndex);
        Assert.Equal(2, raiseCol[0].NewStartingIndex);
        Assert.Equal(3, raiseCol[0]?.OldItems?[0]);
        Assert.Equal(33, raiseCol[0]?.NewItems?[0]);

        Assert.Collection(col,
            i => Assert.Equal(1, i),
            i => Assert.Equal(2, i),
            i => Assert.Equal(33, i));
    }

    [Fact]
    public void Throws()
    {
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3 });

        Assert.Throws<ArgumentOutOfRangeException>(() => col[-1]);
        Assert.Throws<ArgumentOutOfRangeException>(() => col[4]);

        var col2 = new ReadOnlyCollection();

        Assert.Throws<NotSupportedException>(() => col2[0] = 11);
    }
}

public class CountTest
{
    [Fact]
    public void Normal()
    {
        var col = new ObservableCollection<int>();

        Assert.Empty(col);

        col.AddRange(1, 2, 3, 4, 5, 6);

        Assert.Equal(6, col.Count);
    }
}

public class IsReadOnlyTest
{
    [Fact]
    public void Normal()
    {
        var listCols = new List<ObservableCollection<int>>(
            new ObservableCollection<int>[]
            {
                new ObservableCollection<int>(),
                new ReadOnlyCollection(),
                new ObservableCollection<int>(),
                new ReadOnlyCollection(),
                new ObservableCollection<int>(),
                new ReadOnlyCollection(),
            });

        foreach (var col in listCols)
        {
            if (col.IsReadOnly)
            {
                Assert.Throws<NotSupportedException>(() => col.Add(1));
            }
            else
            {
                col.Add(1);
            }
        }
    }
}

public class ConstructorTest
{
    [Fact]
    public void Parameterless()
    {
        var col = new ObservableCollection<int>();

        Assert.Empty(col);
    }

    [Fact]
    public void WithInitialItems()
    {
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

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
        Assert.Throws<ArgumentNullException>(() => new ObservableCollection<int>(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}

public class AddTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var col = new ObservableCollection<int>();

        col.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        col.Add(1);
        col.Add(2);
        col.Add(3);

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
        var col = new ReadOnlyCollection();

        Assert.Throws<NotSupportedException>(() => col.Add(1));
    }
}

public class AddRangeTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var col = new ObservableCollection<int>();

        col.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        col.AddRange(1, 2);
        col.AddRange(new int[] { 3, 4 } as IEnumerable<int>);

        Assert.Equal(4, col.Count);
        Assert.Equal(2, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].NewStartingIndex);
        Assert.Equal(1, raiseCol[0]?.NewItems?[0]);
        Assert.Equal(2, raiseCol[0]?.NewItems?[1]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
        Assert.Equal(2, raiseCol[1].NewStartingIndex);
        Assert.Equal(3, raiseCol[1]?.NewItems?[0]);
        Assert.Equal(4, raiseCol[1]?.NewItems?[1]);

        Assert.Collection(col,
            i => Assert.Equal(1, i),
            i => Assert.Equal(2, i),
            i => Assert.Equal(3, i),
            i => Assert.Equal(4, i));
    }

    [Fact]
    public void Throws()
    {
        var col = new ObservableCollection<int>();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => col.AddRange(null));
        Assert.Throws<ArgumentNullException>(() => col.AddRange((IEnumerable<int>?)null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        var col1 = new ReadOnlyCollection();

        Assert.Throws<NotSupportedException>(() => col1.AddRange(1));
    }
}

public class ClearTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4, 5, 6 });

        col.ImmediateCollectionChanged += (s, e) =>
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
        var col = new ReadOnlyCollection();

        Assert.Throws<NotSupportedException>(() => col.Clear());
    }
}

public class ContainsTest
{
    [Fact]
    public void Normal()
    {
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

        Assert.True(col.Contains(1));
        Assert.True(col.Contains(2));
        Assert.True(col.Contains(3));
        Assert.True(col.Contains(4));
        Assert.False(col.Contains(5));
        Assert.False(col.Contains(6));
    }
}

public class CopyToTest
{
    [Fact]
    public void Normal()
    {
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });
        int[] clone1 = new int[4];
        col.CopyTo(clone1);

        Assert.Collection(clone1,
            i => Assert.Equal(1, i),
            i => Assert.Equal(2, i),
            i => Assert.Equal(3, i),
            i => Assert.Equal(4, i));
    }

    [Fact]
    public void WithArrayIndex()
    {
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });
        int[] clone2 = new int[4];
        col.CopyTo(clone2, 0);

        Assert.Collection(clone2,
            i => Assert.Equal(1, i),
            i => Assert.Equal(2, i),
            i => Assert.Equal(3, i),
            i => Assert.Equal(4, i));
    }


    [Fact]
    public void ArrayWithArrayIndex()
    {
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });
        Array clone3 = new int[6];
        col.CopyTo(clone3, 2);

        Assert.Equal(0, clone3.Cast<int>().ElementAt(0));
        Assert.Equal(0, clone3.Cast<int>().ElementAt(1));
        Assert.Equal(1, clone3.Cast<int>().ElementAt(2));
        Assert.Equal(2, clone3.Cast<int>().ElementAt(3));
        Assert.Equal(3, clone3.Cast<int>().ElementAt(4));
        Assert.Equal(4, clone3.Cast<int>().ElementAt(5));
    }

    [Fact]
    public void WithArrayIndexAndSourceIndex()
    {
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });
        int[] clone4 = new int[6];
        col.CopyTo(2, clone4, 2, 2);

        Assert.Collection(clone4,
            i => Assert.Equal(0, i),
            i => Assert.Equal(0, i),
            i => Assert.Equal(3, i),
            i => Assert.Equal(4, i),
            i => Assert.Equal(0, i),
            i => Assert.Equal(0, i));
    }
}

public class GetEnumeratorTest
{
    [Fact]
    public void ForLoop()
    {
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

        var enumerator = col.GetEnumerator();
        for (int i = 0; i < col.Count; i++)
        {
            Assert.True(enumerator.MoveNext());
            Assert.Equal(i + 1, enumerator.Current);
        }

        Assert.False(enumerator.MoveNext());
    }
}

public class IndexOfTest
{
    [Fact]
    public void Normal()
    {
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

        Assert.Equal(0, col.IndexOf(1));
        Assert.Equal(1, col.IndexOf(2));
        Assert.Equal(2, col.IndexOf(3));
        Assert.Equal(3, col.IndexOf(4));
    }
}

public class InsertTest
{
    [Fact]
    public void AtBottom()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var col = new ObservableCollection<int>();

        col.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        col.Insert(0, 1);
        col.Insert(0, 2);
        col.Insert(0, 3);
        col.Insert(0, 4);

        Assert.Equal(4, col.Count);
        Assert.Equal(4, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].NewStartingIndex);
        Assert.Equal(1, raiseCol[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
        Assert.Equal(0, raiseCol[1].NewStartingIndex);
        Assert.Equal(2, raiseCol[1]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[2].Action);
        Assert.Equal(0, raiseCol[2].NewStartingIndex);
        Assert.Equal(3, raiseCol[2]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[3].Action);
        Assert.Equal(0, raiseCol[3].NewStartingIndex);
        Assert.Equal(4, raiseCol[3]?.NewItems?[0]);

        Assert.Collection(col,
            i => Assert.Equal(4, i),
            i => Assert.Equal(3, i),
            i => Assert.Equal(2, i),
            i => Assert.Equal(1, i));
    }
    [Fact]
    public void AtTop()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var col = new ObservableCollection<int>();

        col.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        col.Insert(col.Count, 1);
        col.Insert(col.Count, 2);
        col.Insert(col.Count, 3);
        col.Insert(col.Count, 4);

        Assert.Equal(4, col.Count);
        Assert.Equal(4, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].NewStartingIndex);
        Assert.Equal(1, raiseCol[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
        Assert.Equal(1, raiseCol[1].NewStartingIndex);
        Assert.Equal(2, raiseCol[1]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[2].Action);
        Assert.Equal(2, raiseCol[2].NewStartingIndex);
        Assert.Equal(3, raiseCol[2]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[3].Action);
        Assert.Equal(3, raiseCol[3].NewStartingIndex);
        Assert.Equal(4, raiseCol[3]?.NewItems?[0]);

        Assert.Collection(col,
            i => Assert.Equal(1, i),
            i => Assert.Equal(2, i),
            i => Assert.Equal(3, i),
            i => Assert.Equal(4, i));
    }

    [Fact]
    public void AtAny()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var col = new ObservableCollection<int>(new int[] { 2, 4 });

        col.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        col.Insert(0, 1);
        col.Insert(2, 3);

        Assert.Equal(4, col.Count);
        Assert.Equal(2, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].NewStartingIndex);
        Assert.Equal(1, raiseCol[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
        Assert.Equal(2, raiseCol[1].NewStartingIndex);
        Assert.Equal(3, raiseCol[1]?.NewItems?[0]);

        Assert.Collection(col,
            i => Assert.Equal(1, i),
            i => Assert.Equal(2, i),
            i => Assert.Equal(3, i),
            i => Assert.Equal(4, i));
    }

    [Fact]
    public void Throws()
    {
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

        Assert.Throws<ArgumentOutOfRangeException>(() => col.Insert(5, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => col.Insert(-1, 1));

        var col1 = new ReadOnlyCollection();

        Assert.Throws<NotSupportedException>(() => col1.Insert(0, 1));
    }
}

public class InsertRange
{
    [Fact]
    public void AtBottom()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var col = new ObservableCollection<int>();

        col.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        col.InsertRange(0, 1, 2);
        col.InsertRange(0, new int[] { 3, 4 } as IEnumerable<int>);

        Assert.Equal(4, col.Count);
        Assert.Equal(2, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].NewStartingIndex);
        Assert.Equal(1, raiseCol[0]?.NewItems?[0]);
        Assert.Equal(2, raiseCol[0]?.NewItems?[1]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
        Assert.Equal(0, raiseCol[1].NewStartingIndex);
        Assert.Equal(3, raiseCol[1]?.NewItems?[0]);
        Assert.Equal(4, raiseCol[1]?.NewItems?[1]);

        Assert.Collection(col,
            i => Assert.Equal(3, i),
            i => Assert.Equal(4, i),
            i => Assert.Equal(1, i),
            i => Assert.Equal(2, i));
    }
    [Fact]
    public void AtTop()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var col = new ObservableCollection<int>();

        col.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        col.InsertRange(col.Count, 1, 2);
        col.InsertRange(col.Count, new int[] { 3, 4 } as IEnumerable<int>);

        Assert.Equal(4, col.Count);
        Assert.Equal(2, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].NewStartingIndex);
        Assert.Equal(1, raiseCol[0]?.NewItems?[0]);
        Assert.Equal(2, raiseCol[0]?.NewItems?[1]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
        Assert.Equal(2, raiseCol[1].NewStartingIndex);
        Assert.Equal(3, raiseCol[1]?.NewItems?[0]);
        Assert.Equal(4, raiseCol[1]?.NewItems?[1]);

        Assert.Collection(col,
            i => Assert.Equal(1, i),
            i => Assert.Equal(2, i),
            i => Assert.Equal(3, i),
            i => Assert.Equal(4, i));
    }

    [Fact]
    public void Throws()
    {
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => col.InsertRange(4, null));
        Assert.Throws<ArgumentNullException>(() => col.InsertRange(4, (IEnumerable<int>?)null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentOutOfRangeException>(() => col.InsertRange(5, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => col.InsertRange(-1, 1));

        var col1 = new ReadOnlyCollection();

        Assert.Throws<NotSupportedException>(() => col1.InsertRange(0, 1));
    }
}

public class MoveTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

        col.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        col.Move(3, 0);
        col.Move(3, 1);
        col.Move(3, 2);

        Assert.Equal(4, col.Count);
        Assert.Equal(3, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Move, raiseCol[0].Action);
        Assert.Equal(3, raiseCol[0].OldStartingIndex);
        Assert.Equal(0, raiseCol[0].NewStartingIndex);
        Assert.Equal(4, raiseCol[0]?.OldItems?[0]);
        Assert.Equal(4, raiseCol[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Move, raiseCol[1].Action);
        Assert.Equal(3, raiseCol[1].OldStartingIndex);
        Assert.Equal(1, raiseCol[1].NewStartingIndex);
        Assert.Equal(3, raiseCol[1]?.OldItems?[0]);
        Assert.Equal(3, raiseCol[1]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Move, raiseCol[2].Action);
        Assert.Equal(3, raiseCol[2].OldStartingIndex);
        Assert.Equal(2, raiseCol[2].NewStartingIndex);
        Assert.Equal(2, raiseCol[2]?.OldItems?[0]);
        Assert.Equal(2, raiseCol[2]?.NewItems?[0]);

        Assert.Collection(col,
            i => Assert.Equal(4, i),
            i => Assert.Equal(3, i),
            i => Assert.Equal(2, i),
            i => Assert.Equal(1, i));
    }

    [Fact]
    public void Throws()
    {
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

        Assert.Throws<ArgumentOutOfRangeException>(() => col.Move(4, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => col.Move(-1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => col.Move(1, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => col.Move(1, 4));

        var col1 = new ReadOnlyCollection();

        Assert.Throws<NotSupportedException>(() => col1.Move(0, 1));
    }
}

public class ObservableFilterTest
{
    [Fact]
    public void Normal()
    {
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });
        var col2 = col.ObservableFilter(i => i % 2 == 0);

        Assert.Equal(2, col2.Count);

        Assert.Collection(col2,
            i => Assert.Equal(2, i),
            i => Assert.Equal(4, i));
    }

    [Fact]
    public void Throws()
    {
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });
        var col2 = col.ObservableFilter(i => i % 2 == 0);

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => col2.ObservableFilter(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}

public class RemoveTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

        col.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        col.Remove(1);
        col.Remove(3);

        Assert.Equal(2, col.Count);
        Assert.Equal(2, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].OldStartingIndex);
        Assert.Equal(1, raiseCol[0]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[1].Action);
        Assert.Equal(1, raiseCol[1].OldStartingIndex);
        Assert.Equal(3, raiseCol[1]?.OldItems?[0]);

        Assert.Collection(col,
            i => Assert.Equal(2, i),
            i => Assert.Equal(4, i));
    }

    [Fact]
    public void All()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

        col.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        col.Remove(1);
        col.Remove(2);
        col.Remove(3);
        col.Remove(4);

        Assert.Empty(col);
        Assert.Equal(4, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].OldStartingIndex);
        Assert.Equal(1, raiseCol[0]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[1].Action);
        Assert.Equal(0, raiseCol[1].OldStartingIndex);
        Assert.Equal(2, raiseCol[1]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[2].Action);
        Assert.Equal(0, raiseCol[2].OldStartingIndex);
        Assert.Equal(3, raiseCol[2]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[3].Action);
        Assert.Equal(0, raiseCol[3].OldStartingIndex);
        Assert.Equal(4, raiseCol[3]?.OldItems?[0]);
    }

    [Fact]
    public void Throws()
    {
        var col1 = new ReadOnlyCollection();

        Assert.Throws<NotSupportedException>(() => col1.Remove(1));
    }
}

public class RemoveAtTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

        col.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        col.RemoveAt(0);
        col.RemoveAt(1);

        Assert.Equal(2, col.Count);
        Assert.Equal(2, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].OldStartingIndex);
        Assert.Equal(1, raiseCol[0]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[1].Action);
        Assert.Equal(1, raiseCol[1].OldStartingIndex);
        Assert.Equal(3, raiseCol[1]?.OldItems?[0]);

        Assert.Collection(col,
            i => Assert.Equal(2, i),
            i => Assert.Equal(4, i));
    }

    [Fact]
    public void All()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

        col.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        col.RemoveAt(0);
        col.RemoveAt(0);
        col.RemoveAt(0);
        col.RemoveAt(0);

        Assert.Empty(col);
        Assert.Equal(4, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].OldStartingIndex);
        Assert.Equal(1, raiseCol[0]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[1].Action);
        Assert.Equal(0, raiseCol[1].OldStartingIndex);
        Assert.Equal(2, raiseCol[1]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[2].Action);
        Assert.Equal(0, raiseCol[2].OldStartingIndex);
        Assert.Equal(3, raiseCol[2]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[3].Action);
        Assert.Equal(0, raiseCol[3].OldStartingIndex);
        Assert.Equal(4, raiseCol[3]?.OldItems?[0]);
    }

    [Fact]
    public void Throws()
    {
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

        col.RemoveAt(0);
        col.RemoveAt(0);
        col.RemoveAt(0);
        col.RemoveAt(0);

        Assert.Throws<ArgumentOutOfRangeException>(() => col.RemoveAt(0));

        var col1 = new ReadOnlyCollection(new int[] { 1, 2, 3, 4 });

        Assert.Throws<NotSupportedException>(() => col1.RemoveAt(0));
    }
}

public class RemoveRangeTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

        col.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        col.RemoveRange(2, 2);

        Assert.Equal(2, col.Count);
        Assert.Single(raiseCol);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[0].Action);
        Assert.Equal(2, raiseCol[0].OldStartingIndex);
        Assert.Equal(3, raiseCol[0]?.OldItems?[0]);
        Assert.Equal(4, raiseCol[0]?.OldItems?[1]);

        Assert.Collection(col,
            i => Assert.Equal(1, i),
            i => Assert.Equal(2, i));
    }

    [Fact]
    public void All()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

        col.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        col.RemoveRange(0, 4);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].OldStartingIndex);
        Assert.Equal(1, raiseCol[0]?.OldItems?[0]);
        Assert.Equal(2, raiseCol[0]?.OldItems?[1]);
        Assert.Equal(3, raiseCol[0]?.OldItems?[2]);
        Assert.Equal(4, raiseCol[0]?.OldItems?[3]);

        Assert.Empty(col);
        Assert.Single(raiseCol);
    }

    [Fact]
    public void Throws()
    {
        var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

        Assert.Throws<ArgumentOutOfRangeException>(() => col.RemoveRange(-1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => col.RemoveRange(0, -1));
        Assert.Throws<ArgumentException>(() => col.RemoveRange(0, 5));
        Assert.Throws<ArgumentException>(() => col.RemoveRange(1, 4));
        Assert.Throws<ArgumentException>(() => col.RemoveRange(4, 1));

        col.RemoveAt(0);
        col.RemoveAt(0);
        col.RemoveAt(0);
        col.RemoveAt(0);

        Assert.Throws<ArgumentException>(() => col.RemoveRange(0, 1));

        var col1 = new ReadOnlyCollection(new int[] { 1, 2, 3, 4 });

        Assert.Throws<NotSupportedException>(() => col1.RemoveRange(0, 1));
    }
}
