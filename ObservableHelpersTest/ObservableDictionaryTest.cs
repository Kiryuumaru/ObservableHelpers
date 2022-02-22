using ObservableHelpers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ObservableDictionaryTest;

public class ReadOnlyDictionary : ObservableDictionary<string, string>
{
    public ReadOnlyDictionary()
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
        var dict = new ObservableDictionary<string, string>();

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            Interlocked.Increment(ref raiseCount1);
        };

        await Task.WhenAll(
            Task.Run(delegate
            {
                for (int i = 0; i < 10000; i++)
                {
                    dict.Add(i.ToString(), (i + 1).ToString());
                }
            }),
            Task.Run(delegate
            {
                for (int i = 10000; i < 20000; i++)
                {
                    dict.Add(i.ToString(), (i + 1).ToString());
                }
            }));

        Assert.Equal(20000, raiseCount1);
        Assert.Equal(20000, dict.Count);

        for (int i = 0; i < 20000; i++)
        {
            Assert.Contains(i.ToString(), dict as IDictionary<string, string>);
        }
    }

    [Fact]
    public async void WithConcurrency2()
    {
        var raiseCount1 = 0;
        var dict = new ObservableDictionary<string, string>();

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            Interlocked.Increment(ref raiseCount1);
        };

        await Task.WhenAll(
            Task.Run(delegate
            {
                for (int i = 0; i < 10000; i++)
                {
                    dict.AddRange(
                        new KeyValuePair<string, string>(i.ToString(), i.ToString()),
                        new KeyValuePair<string, string>((i + 10000).ToString(), (i + 10000).ToString()),
                        new KeyValuePair<string, string>((i + 20000).ToString(), (i + 20000).ToString()));
                }
            }),
            Task.Run(delegate
            {
                for (int i = 30000; i < 40000; i++)
                {
                    dict.AddRange(
                        new KeyValuePair<string, string>(i.ToString(), i.ToString()),
                        new KeyValuePair<string, string>((i + 10000).ToString(), (i + 10000).ToString()),
                        new KeyValuePair<string, string>((i + 20000).ToString(), (i + 20000).ToString()));
                }
            }));

        Assert.Equal(20000, raiseCount1);
        Assert.Equal(60000, dict.Count);

        for (int i = 0; i < 60000; i++)
        {
            Assert.Contains(i.ToString(), dict as IDictionary<string, string>);
        }
    }

    [Fact]
    public async void WithConcurrency3()
    {
        var raiseCount1 = 0;
        var dict = new ObservableDictionary<string, string>();

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            Interlocked.Increment(ref raiseCount1);
        };

        for (int i = 65000; i < 70000; i++)
        {
            var one = i.ToString();
            var two = (i + 10000).ToString();
            var three = (i + 20000).ToString();
            var four = (i + 30000).ToString();
            var five = (i + 40000).ToString();
            var six = (i + 50000).ToString();
            Assert.Equal("oneAddOrUpdate" + one, dict.AddOrUpdate(one, "oneAddOrUpdate" + one));
            Assert.Equal("twoAddOrUpdate" + two, dict.AddOrUpdate(two, _ => "twoAddOrUpdate" + two));
            Assert.Equal("threeAdd" + three, dict.AddOrUpdate(three, "threeAdd" + three, "threeUpdate" + three));
            Assert.Equal("fourAdd" + four, dict.AddOrUpdate(four, _ => "fourAdd" + four, "fourUpdate" + four));
            Assert.Equal("fiveAdd" + five, dict.AddOrUpdate(five, "fiveAdd" + five, _ => "fiveUpdate" + five));
            Assert.Equal("sixAdd" + six, dict.AddOrUpdate(six, _ => "sixAdd" + six, _ => "sixUpdate" + six));
        }

        await Task.WhenAll(
            Task.Run(delegate
            {
                for (int i = 0; i < 10000; i++)
                {
                    var one = i.ToString();
                    var two = (i + 10000).ToString();
                    var three = (i + 20000).ToString();
                    var four = (i + 30000).ToString();
                    var five = (i + 40000).ToString();
                    var six = (i + 50000).ToString();
                    Assert.Equal("oneAddOrUpdate" + one, dict.AddOrUpdate(one, "oneAddOrUpdate" + one));
                    Assert.Equal("twoAddOrUpdate" + two, dict.AddOrUpdate(two, _ => "twoAddOrUpdate" + two));
                    Assert.Equal("threeAdd" + three, dict.AddOrUpdate(three, "threeAdd" + three, "threeUpdate" + three));
                    Assert.Equal("fourAdd" + four, dict.AddOrUpdate(four, _ => "fourAdd" + four, "fourUpdate" + four));
                    Assert.Equal("fiveAdd" + five, dict.AddOrUpdate(five, "fiveAdd" + five, _ => "fiveUpdate" + five));
                    Assert.Equal("sixAdd" + six, dict.AddOrUpdate(six, _ => "sixAdd" + six, _ => "sixUpdate" + six));
                }
            }),
            Task.Run(delegate
            {
                for (int i = 60000; i < 70000; i++)
                {
                    var one = i.ToString();
                    var two = (i + 10000).ToString();
                    var three = (i + 20000).ToString();
                    var four = (i + 30000).ToString();
                    var five = (i + 40000).ToString();
                    var six = (i + 50000).ToString();
                    if (i >= 65000)
                    {
                        Assert.Equal("oneAddOrUpdate" + one, dict.AddOrUpdate(one, "oneAddOrUpdate" + one));
                        Assert.Equal("twoAddOrUpdate" + two, dict.AddOrUpdate(two, _ => "twoAddOrUpdate" + two));
                        Assert.Equal("threeUpdate" + three, dict.AddOrUpdate(three, "threeAdd" + three, "threeUpdate" + three));
                        Assert.Equal("fourUpdate" + four, dict.AddOrUpdate(four, _ => "fourAdd" + four, "fourUpdate" + four));
                        Assert.Equal("fiveUpdate" + five, dict.AddOrUpdate(five, "fiveAdd" + five, _ => "fiveUpdate" + five));
                        Assert.Equal("sixUpdate" + six, dict.AddOrUpdate(six, _ => "sixAdd" + six, _ => "sixUpdate" + six));
                    }
                    else
                    {
                        Assert.Equal("oneAddOrUpdate" + one, dict.AddOrUpdate(one, "oneAddOrUpdate" + one));
                        Assert.Equal("twoAddOrUpdate" + two, dict.AddOrUpdate(two, _ => "twoAddOrUpdate" + two));
                        Assert.Equal("threeAdd" + three, dict.AddOrUpdate(three, "threeAdd" + three, "threeUpdate" + three));
                        Assert.Equal("fourAdd" + four, dict.AddOrUpdate(four, _ => "fourAdd" + four, "fourUpdate" + four));
                        Assert.Equal("fiveAdd" + five, dict.AddOrUpdate(five, "fiveAdd" + five, _ => "fiveUpdate" + five));
                        Assert.Equal("sixAdd" + six, dict.AddOrUpdate(six, _ => "sixAdd" + six, _ => "sixUpdate" + six));
                    }
                }
            }));

        Assert.Equal(150000, raiseCount1);
        Assert.Equal(120000, dict.Count);

        for (int i = 0; i < 10000; i++)
        {
            var one = i.ToString();
            var two = (i + 10000).ToString();
            var three = (i + 20000).ToString();
            var four = (i + 30000).ToString();
            var five = (i + 40000).ToString();
            var six = (i + 50000).ToString();

            Assert.Equal("oneAddOrUpdate" + one, Assert.Contains(one, dict as IDictionary<string, string>));
            Assert.Equal("twoAddOrUpdate" + two, Assert.Contains(two, dict as IDictionary<string, string>));
            Assert.Equal("threeAdd" + three, Assert.Contains(three, dict as IDictionary<string, string>));
            Assert.Equal("fourAdd" + four, Assert.Contains(four, dict as IDictionary<string, string>));
            Assert.Equal("fiveAdd" + five, Assert.Contains(five, dict as IDictionary<string, string>));
            Assert.Equal("sixAdd" + six, Assert.Contains(six, dict as IDictionary<string, string>));
        }


        for (int i = 60000; i < 70000; i++)
        {
            var one = i.ToString();
            var two = (i + 10000).ToString();
            var three = (i + 20000).ToString();
            var four = (i + 30000).ToString();
            var five = (i + 40000).ToString();
            var six = (i + 50000).ToString();

            if (i >= 65000)
            {
                Assert.Equal("oneAddOrUpdate" + one, Assert.Contains(one, dict as IDictionary<string, string>));
                Assert.Equal("twoAddOrUpdate" + two, Assert.Contains(two, dict as IDictionary<string, string>));
                Assert.Equal("threeUpdate" + three, Assert.Contains(three, dict as IDictionary<string, string>));
                Assert.Equal("fourUpdate" + four, Assert.Contains(four, dict as IDictionary<string, string>));
                Assert.Equal("fiveUpdate" + five, Assert.Contains(five, dict as IDictionary<string, string>));
                Assert.Equal("sixUpdate" + six, Assert.Contains(six, dict as IDictionary<string, string>));
            }
            else
            {
                Assert.Equal("oneAddOrUpdate" + one, Assert.Contains(one, dict as IDictionary<string, string>));
                Assert.Equal("twoAddOrUpdate" + two, Assert.Contains(two, dict as IDictionary<string, string>));
                Assert.Equal("threeAdd" + three, Assert.Contains(three, dict as IDictionary<string, string>));
                Assert.Equal("fourAdd" + four, Assert.Contains(four, dict as IDictionary<string, string>));
                Assert.Equal("fiveAdd" + five, Assert.Contains(five, dict as IDictionary<string, string>));
                Assert.Equal("sixAdd" + six, Assert.Contains(six, dict as IDictionary<string, string>));
            }
        }
    }

    [Fact]
    public async void WithConcurrency4()
    {
        var raiseCount1 = 0;
        var dict = new ObservableDictionary<string, string>();

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            Interlocked.Increment(ref raiseCount1);
        };

        await Task.WhenAll(
            Task.Run(delegate
            {
                for (int i = 0; i < 10000; i++)
                {
                    dict.AddRange(
                        new KeyValuePair<string, string>(i.ToString(), i.ToString()),
                        new KeyValuePair<string, string>((i + 10000).ToString(), (i + 10000).ToString()),
                        new KeyValuePair<string, string>((i + 20000).ToString(), (i + 20000).ToString()));
                }
            }),
            Task.Run(delegate
            {
                for (int i = 30000; i < 40000; i++)
                {
                    dict.AddRange(
                        new KeyValuePair<string, string>(i.ToString(), i.ToString()),
                        new KeyValuePair<string, string>((i + 10000).ToString(), (i + 10000).ToString()),
                        new KeyValuePair<string, string>((i + 20000).ToString(), (i + 20000).ToString()));
                }
            }));

        Assert.Equal(20000, raiseCount1);
        Assert.Equal(60000, dict.Count);

        for (int i = 0; i < 70000; i++)
        {
            if (i >= 60000)
            {
                Assert.False(dict.ContainsKey(i.ToString()));
            }
            else
            {
                Assert.True(dict.ContainsKey(i.ToString()));
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
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three")
        });

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        Assert.Equal("one", dict["1"]);
        Assert.Equal("two", dict["2"]);
        Assert.Equal("three", dict["3"]);

        dict["3"] = "threeUpdate";
        dict["4"] = "four";
        dict["5"] = "five";
        dict["6"] = "six";

        Assert.Equal(6, dict.Count);
        Assert.Equal(4, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[0].Action);
        Assert.Equal(2, raiseCol[0].OldStartingIndex);
        Assert.Equal(2, raiseCol[0].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("3", "three"), raiseCol[0]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("3", "threeUpdate"), raiseCol[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
        Assert.Equal(3, raiseCol[1].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("4", "four"), raiseCol[1]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[2].Action);
        Assert.Equal(4, raiseCol[2].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("5", "five"), raiseCol[2]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[3].Action);
        Assert.Equal(5, raiseCol[3].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("6", "six"), raiseCol[3]?.NewItems?[0]);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("1", "one"), i),
            i => Assert.Equal(KeyValuePair.Create("2", "two"), i),
            i => Assert.Equal(KeyValuePair.Create("3", "threeUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("4", "four"), i),
            i => Assert.Equal(KeyValuePair.Create("5", "five"), i),
            i => Assert.Equal(KeyValuePair.Create("6", "six"), i));
    }

    [Fact]
    public void Throws()
    {
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three")
        });

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => dict[null]);
        Assert.Throws<KeyNotFoundException>(() => dict["4"]);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        var dict2 = new ReadOnlyDictionary();

        Assert.Throws<NotSupportedException>(() => dict2["5"] = "five");
    }
}

public class KeysTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>();

        dict.Keys.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        dict.Add("1", "one");
        dict.Add("2", "two");
        dict.Add("3", "three");
        dict.Insert(2, KeyValuePair.Create("2b", "twob"));
        dict.InsertRange(3, KeyValuePair.Create("2c", "twoc"), KeyValuePair.Create("2d", "twod"));

        Assert.Equal(6, dict.Count);
        Assert.Equal(5, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].NewStartingIndex);
        Assert.Equal("1", raiseCol[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
        Assert.Equal(1, raiseCol[1].NewStartingIndex);
        Assert.Equal("2", raiseCol[1]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[2].Action);
        Assert.Equal(2, raiseCol[2].NewStartingIndex);
        Assert.Equal("3", raiseCol[2]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[3].Action);
        Assert.Equal(2, raiseCol[3].NewStartingIndex);
        Assert.Equal("2b", raiseCol[3]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[4].Action);
        Assert.Equal(3, raiseCol[4].NewStartingIndex);
        Assert.Equal("2c", raiseCol[4]?.NewItems?[0]);
        Assert.Equal("2d", raiseCol[4]?.NewItems?[1]);

        Assert.Collection(dict.Keys,
            i => Assert.Equal("1", i),
            i => Assert.Equal("2", i),
            i => Assert.Equal("2b", i),
            i => Assert.Equal("2c", i),
            i => Assert.Equal("2d", i),
            i => Assert.Equal("3", i));
    }

    [Fact]
    public void Throws()
    {
        var dict = new ObservableDictionary<string, string>();

        Assert.Throws<NotSupportedException>(() => dict.Keys.Add("1"));
        Assert.Throws<NotSupportedException>(() => dict.Keys.AddRange("1", "2"));
        Assert.Throws<NotSupportedException>(() => dict.Keys.Insert(0, "2"));
        Assert.Throws<NotSupportedException>(() => dict.Keys.InsertRange(0, "1", "2"));
    }
}

public class ValuesTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>();

        dict.Values.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        dict.Add("1", "one");
        dict.Add("2", "two");
        dict.Add("3", "three");
        dict.Insert(2, KeyValuePair.Create("2b", "twob"));
        dict.InsertRange(3, KeyValuePair.Create("2c", "twoc"), KeyValuePair.Create("2d", "twod"));

        Assert.Equal(6, dict.Count);
        Assert.Equal(5, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].NewStartingIndex);
        Assert.Equal("one", raiseCol[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
        Assert.Equal(1, raiseCol[1].NewStartingIndex);
        Assert.Equal("two", raiseCol[1]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[2].Action);
        Assert.Equal(2, raiseCol[2].NewStartingIndex);
        Assert.Equal("three", raiseCol[2]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[3].Action);
        Assert.Equal(2, raiseCol[3].NewStartingIndex);
        Assert.Equal("twob", raiseCol[3]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[4].Action);
        Assert.Equal(3, raiseCol[4].NewStartingIndex);
        Assert.Equal("twoc", raiseCol[4]?.NewItems?[0]);
        Assert.Equal("twod", raiseCol[4]?.NewItems?[1]);

        Assert.Collection(dict.Values,
            i => Assert.Equal("one", i),
            i => Assert.Equal("two", i),
            i => Assert.Equal("twob", i),
            i => Assert.Equal("twoc", i),
            i => Assert.Equal("twod", i),
            i => Assert.Equal("three", i));
    }

    [Fact]
    public void Throws()
    {
        var dict = new ObservableDictionary<string, string>();

        Assert.Throws<NotSupportedException>(() => dict.Values.Add("one"));
        Assert.Throws<NotSupportedException>(() => dict.Values.AddRange("one", "two"));
        Assert.Throws<NotSupportedException>(() => dict.Values.Insert(0, "two"));
        Assert.Throws<NotSupportedException>(() => dict.Values.InsertRange(0, "one", "two"));
    }
}

public class ConstructorTest
{
    [Fact]
    public void Parameterless()
    {
        var dict = new ObservableDictionary<string, string>();

        Assert.Empty(dict);
    }

    [Fact]
    public void WithInitialItems()
    {
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four")
        });

        Assert.Equal(4, dict.Count);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("1", "one"), i),
            i => Assert.Equal(KeyValuePair.Create("2", "two"), i),
            i => Assert.Equal(KeyValuePair.Create("3", "three"), i),
            i => Assert.Equal(KeyValuePair.Create("4", "four"), i));
    }

    private class EqualityComparer : IEqualityComparer<string>
    {
        public bool Equals([AllowNull] string x, [AllowNull] string y) => x?.Equals(y) ?? y == null;

        public int GetHashCode([DisallowNull] string obj) => obj.GetHashCode();
    }

    [Fact]
    public void WithEqulityComparer()
    {
        var dict = new ObservableDictionary<string, string>(new EqualityComparer());

        Assert.Empty(dict);
    }

    [Fact]
    public void WithInitialItemsAndEqulityComparer()
    {
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four")
        }, new EqualityComparer());

        Assert.Equal(4, dict.Count);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("1", "one"), i),
            i => Assert.Equal(KeyValuePair.Create("2", "two"), i),
            i => Assert.Equal(KeyValuePair.Create("3", "three"), i),
            i => Assert.Equal(KeyValuePair.Create("4", "four"), i));
    }

    [Fact]
    public void Throws()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => new ObservableDictionary<int, int>((IEnumerable<KeyValuePair<int, int>>?)null));
        Assert.Throws<ArgumentNullException>(() => new ObservableDictionary<int, int>((IEqualityComparer<int>?)null));
        Assert.Throws<ArgumentNullException>(() => new ObservableDictionary<int, int>(null, null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}

public class AddTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>();

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        dict.Add("1", "one");
        dict.Add("2", "two");
        dict.Add("3", "three");

        Assert.Equal(3, dict.Count);
        Assert.Equal(3, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("1", "one"), raiseCol[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
        Assert.Equal(1, raiseCol[1].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("2", "two"), raiseCol[1]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[2].Action);
        Assert.Equal(2, raiseCol[2].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("3", "three"), raiseCol[2]?.NewItems?[0]);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("1", "one"), i),
            i => Assert.Equal(KeyValuePair.Create("2", "two"), i),
            i => Assert.Equal(KeyValuePair.Create("3", "three"), i));
    }

    [Fact]
    public void Throws()
    {
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four")
        });

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => dict.Add(null, "exists"));
        Assert.Throws<ArgumentException>(() => dict.Add("3", "exists"));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        var dict1 = new ReadOnlyDictionary();

        Assert.Throws<NotSupportedException>(() => dict1.Add("1", "one"));
    }
}

public class AddRangeTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>();

        Assert.Empty(dict);

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };
        dict.AddRange(
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two")
        );
        dict.AddRange(new KeyValuePair<string, string>[]
        {
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four")
        } as IEnumerable<KeyValuePair<string, string>>);

        Assert.Equal(4, dict.Count);
        Assert.Equal(2, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("1", "one"), raiseCol[0]?.NewItems?[0]);
        Assert.Equal(KeyValuePair.Create("2", "two"), raiseCol[0]?.NewItems?[1]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
        Assert.Equal(2, raiseCol[1].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("3", "three"), raiseCol[1]?.NewItems?[0]);
        Assert.Equal(KeyValuePair.Create("4", "four"), raiseCol[1]?.NewItems?[1]);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("1", "one"), i),
            i => Assert.Equal(KeyValuePair.Create("2", "two"), i),
            i => Assert.Equal(KeyValuePair.Create("3", "three"), i),
            i => Assert.Equal(KeyValuePair.Create("4", "four"), i));
    }

    [Fact]
    public void Throws()
    {
        var dict = new ObservableDictionary<string, string>();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => dict.AddRange(null));
        Assert.Throws<ArgumentNullException>(() => dict.AddRange((IEnumerable<KeyValuePair<string, string>>?)null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        var dict1 = new ReadOnlyDictionary();

        Assert.Throws<NotSupportedException>(() => dict1.AddRange(
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two")
        ));
    }
}

public class AddOrUpdateTest
{
    [Fact]
    public void WithAdd()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>();

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        Assert.Equal("oneAdd", dict.AddOrUpdate("1", "oneAdd"));
        Assert.Equal("twoAdd", dict.AddOrUpdate("2", _ => "twoAdd"));
        Assert.Equal("threeAdd", dict.AddOrUpdate("3", "threeAdd", "threeUpdate"));
        Assert.Equal("fourAdd", dict.AddOrUpdate("4", _ => "fourAdd", "fourUpdate"));
        Assert.Equal("fiveAdd", dict.AddOrUpdate("5", "fiveAdd", _ => "fiveAdd"));
        Assert.Equal("sixAdd", dict.AddOrUpdate("6", _ => "sixAdd", _ => "sixUpdate"));

        Assert.Equal(6, dict.Count);
        Assert.Equal(6, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("1", "oneAdd"), raiseCol[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
        Assert.Equal(1, raiseCol[1].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("2", "twoAdd"), raiseCol[1]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[2].Action);
        Assert.Equal(2, raiseCol[2].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("3", "threeAdd"), raiseCol[2]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[3].Action);
        Assert.Equal(3, raiseCol[3].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("4", "fourAdd"), raiseCol[3]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[4].Action);
        Assert.Equal(4, raiseCol[4].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("5", "fiveAdd"), raiseCol[4]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[5].Action);
        Assert.Equal(5, raiseCol[5].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("6", "sixAdd"), raiseCol[5]?.NewItems?[0]);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("1", "oneAdd"), i),
            i => Assert.Equal(KeyValuePair.Create("2", "twoAdd"), i),
            i => Assert.Equal(KeyValuePair.Create("3", "threeAdd"), i),
            i => Assert.Equal(KeyValuePair.Create("4", "fourAdd"), i),
            i => Assert.Equal(KeyValuePair.Create("5", "fiveAdd"), i),
            i => Assert.Equal(KeyValuePair.Create("6", "sixAdd"), i));
    }

    [Fact]
    public void WithUpdate()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four"),
            new KeyValuePair<string, string>("5", "five"),
            new KeyValuePair<string, string>("6", "six")
        });

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        Assert.Equal("oneAddOrUpdate", dict.AddOrUpdate("1", "oneAddOrUpdate"));
        Assert.Equal("twoAddOrUpdate", dict.AddOrUpdate("2", _ => "twoAddOrUpdate"));
        Assert.Equal("threeUpdate", dict.AddOrUpdate("3", "threeAdd", "threeUpdate"));
        Assert.Equal("fourUpdate", dict.AddOrUpdate("4", _ => "fourAdd", "fourUpdate"));
        Assert.Equal("fiveUpdate", dict.AddOrUpdate("5", "fiveAdd", _ => "fiveUpdate"));
        Assert.Equal("sixUpdate", dict.AddOrUpdate("6", _ => "sixAdd", _ => "sixUpdate"));

        Assert.Equal(6, dict.Count);
        Assert.Equal(6, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].OldStartingIndex);
        Assert.Equal(0, raiseCol[0].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("1", "one"), raiseCol[0]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("1", "oneAddOrUpdate"), raiseCol[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[1].Action);
        Assert.Equal(1, raiseCol[1].OldStartingIndex);
        Assert.Equal(1, raiseCol[1].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("2", "two"), raiseCol[1]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("2", "twoAddOrUpdate"), raiseCol[1]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[2].Action);
        Assert.Equal(2, raiseCol[2].OldStartingIndex);
        Assert.Equal(2, raiseCol[2].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("3", "three"), raiseCol[2]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("3", "threeUpdate"), raiseCol[2]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[3].Action);
        Assert.Equal(3, raiseCol[3].OldStartingIndex);
        Assert.Equal(3, raiseCol[3].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("4", "four"), raiseCol[3]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("4", "fourUpdate"), raiseCol[3]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[4].Action);
        Assert.Equal(4, raiseCol[4].OldStartingIndex);
        Assert.Equal(4, raiseCol[4].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("5", "five"), raiseCol[4]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("5", "fiveUpdate"), raiseCol[4]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[5].Action);
        Assert.Equal(5, raiseCol[5].OldStartingIndex);
        Assert.Equal(5, raiseCol[5].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("6", "six"), raiseCol[5]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("6", "sixUpdate"), raiseCol[5]?.NewItems?[0]);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("1", "oneAddOrUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("2", "twoAddOrUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("3", "threeUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("4", "fourUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("5", "fiveUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("6", "sixUpdate"), i));
    }

    [Fact]
    public void WithAddAndUpdate()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three")
        });

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        Assert.Equal("oneAddOrUpdate", dict.AddOrUpdate("1", "oneAddOrUpdate"));
        Assert.Equal("twoAddOrUpdate", dict.AddOrUpdate("2", _ => "twoAddOrUpdate"));
        Assert.Equal("threeUpdate", dict.AddOrUpdate("3", "threeAdd", "threeUpdate"));
        Assert.Equal("fourAdd", dict.AddOrUpdate("4", _ => "fourAdd", "fourUpdate"));
        Assert.Equal("fiveAdd", dict.AddOrUpdate("5", "fiveAdd", _ => "fiveUpdate"));
        Assert.Equal("sixAdd", dict.AddOrUpdate("6", _ => "sixAdd", _ => "sixUpdate"));

        Assert.Equal(6, dict.Count);
        Assert.Equal(6, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].OldStartingIndex);
        Assert.Equal(0, raiseCol[0].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("1", "one"), raiseCol[0]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("1", "oneAddOrUpdate"), raiseCol[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[1].Action);
        Assert.Equal(1, raiseCol[1].OldStartingIndex);
        Assert.Equal(1, raiseCol[1].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("2", "two"), raiseCol[1]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("2", "twoAddOrUpdate"), raiseCol[1]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[2].Action);
        Assert.Equal(2, raiseCol[2].OldStartingIndex);
        Assert.Equal(2, raiseCol[2].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("3", "three"), raiseCol[2]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("3", "threeUpdate"), raiseCol[2]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[3].Action);
        Assert.Equal(3, raiseCol[3].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("4", "fourAdd"), raiseCol[3]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[4].Action);
        Assert.Equal(4, raiseCol[4].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("5", "fiveAdd"), raiseCol[4]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[5].Action);
        Assert.Equal(5, raiseCol[5].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("6", "sixAdd"), raiseCol[5]?.NewItems?[0]);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("1", "oneAddOrUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("2", "twoAddOrUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("3", "threeUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("4", "fourAdd"), i),
            i => Assert.Equal(KeyValuePair.Create("5", "fiveAdd"), i),
            i => Assert.Equal(KeyValuePair.Create("6", "sixAdd"), i));
    }

    [Fact]
    public void Throws()
    {
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three")
        });

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => dict.AddOrUpdate(null, "oneAddOrUpdate"));
        Assert.Throws<ArgumentNullException>(() => dict.AddOrUpdate(null, _ => "twoAddOrUpdate"));
        Assert.Throws<ArgumentNullException>(() => dict.AddOrUpdate(null, (Func<string, string>?)null));
        Assert.Throws<ArgumentNullException>(() => dict.AddOrUpdate(null, "threeAdd", "threeUpdate"));
        Assert.Throws<ArgumentNullException>(() => dict.AddOrUpdate(null, _ => "fourAdd", "fourUpdate"));
        Assert.Throws<ArgumentNullException>(() => dict.AddOrUpdate(null, (Func<string, string>?)null, "fourUpdate"));
        Assert.Throws<ArgumentNullException>(() => dict.AddOrUpdate(null, "fiveAdd", _ => "fiveUpdate"));
        Assert.Throws<ArgumentNullException>(() => dict.AddOrUpdate(null, "fiveAdd", (Func<(string key, string oldValue), string>?)null));
        Assert.Throws<ArgumentNullException>(() => dict.AddOrUpdate(null, _ => "sixAdd", _ => "sixUpdate"));
        Assert.Throws<ArgumentNullException>(() => dict.AddOrUpdate(null, _ => "sixAdd", (Func<(string key, string oldValue), string>?)null));
        Assert.Throws<ArgumentNullException>(() => dict.AddOrUpdate(null, (Func<string, string>?)null, _ => "sixUpdate"));
        Assert.Throws<ArgumentNullException>(() => dict.AddOrUpdate(null, (Func<string, string>?)null, (Func<(string key, string oldValue), string>?)null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        var dict2 = new ReadOnlyDictionary();

        Assert.Throws<NotSupportedException>(() => dict2.AddOrUpdate("1", "oneAddOrUpdate"));
        Assert.Throws<NotSupportedException>(() => dict2.AddOrUpdate("2", _ => "twoAddOrUpdate"));
        Assert.Throws<NotSupportedException>(() => dict2.AddOrUpdate("3", "threeAdd", "threeUpdate"));
        Assert.Throws<NotSupportedException>(() => dict2.AddOrUpdate("4", _ => "fourAdd", "fourUpdate"));
        Assert.Throws<NotSupportedException>(() => dict2.AddOrUpdate("5", "fiveAdd", _ => "fiveUpdate"));
        Assert.Throws<NotSupportedException>(() => dict2.AddOrUpdate("6", _ => "sixAdd", _ => "sixUpdate"));
    }
}

public class ContainsKeyTest
{
    [Fact]
    public void Normal()
    {
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four")
        });

        Assert.True(dict.ContainsKey("1"));
        Assert.True(dict.ContainsKey("2"));
        Assert.True(dict.ContainsKey("3"));
        Assert.True(dict.ContainsKey("4"));
        Assert.False(dict.ContainsKey("5"));
        Assert.False(dict.ContainsKey("6"));
    }

    [Fact]
    public void Throws()
    {
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three")
        });

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => dict.ContainsKey(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}

public class GetEnumeratorTest
{
    [Fact]
    public void ForLoop()
    {
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four")
        });

        var enumerator = (ObservableDictionary<string, string>.DictionaryEnumerator)dict.GetEnumerator();

        Assert.True(enumerator.MoveNext());
        Assert.Equal("1", enumerator.Key);
        Assert.Equal("one", enumerator.Value);

        Assert.True(enumerator.MoveNext());
        Assert.Equal("2", enumerator.Key);
        Assert.Equal("two", enumerator.Value);

        Assert.True(enumerator.MoveNext());
        Assert.Equal("3", enumerator.Key);
        Assert.Equal("three", enumerator.Value);

        Assert.True(enumerator.MoveNext());
        Assert.Equal("4", enumerator.Key);
        Assert.Equal("four", enumerator.Value);

        Assert.False(enumerator.MoveNext());
    }
}

public class GetOrAddTest
{
    [Fact]
    public void WithGet()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four")
        });

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        Assert.Equal("three", dict.GetOrAdd("3", "threeAdd"));
        Assert.Equal("four", dict.GetOrAdd("4", _ => "fourAdd"));

        Assert.Equal(4, dict.Count);
        Assert.Empty(raiseCol);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("1", "one"), i),
            i => Assert.Equal(KeyValuePair.Create("2", "two"), i),
            i => Assert.Equal(KeyValuePair.Create("3", "three"), i),
            i => Assert.Equal(KeyValuePair.Create("4", "four"), i));
    }

    [Fact]
    public void WithAdd()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four")
        });

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        Assert.Equal("fiveAdd", dict.GetOrAdd("5", "fiveAdd"));
        Assert.Equal("sixAdd", dict.GetOrAdd("6", _ => "sixAdd"));

        Assert.Equal(6, dict.Count);
        Assert.Equal(2, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[0].Action);
        Assert.Equal(4, raiseCol[0].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("5", "fiveAdd"), raiseCol[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
        Assert.Equal(5, raiseCol[1].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("6", "sixAdd"), raiseCol[1]?.NewItems?[0]);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("1", "one"), i),
            i => Assert.Equal(KeyValuePair.Create("2", "two"), i),
            i => Assert.Equal(KeyValuePair.Create("3", "three"), i),
            i => Assert.Equal(KeyValuePair.Create("4", "four"), i),
            i => Assert.Equal(KeyValuePair.Create("5", "fiveAdd"), i),
            i => Assert.Equal(KeyValuePair.Create("6", "sixAdd"), i));
    }

    [Fact]
    public void WithGetAndAdd()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("4", "four")
        });

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        Assert.Equal("oneAdd", dict.GetOrAdd("1", "oneAdd"));
        Assert.Equal("two", dict.GetOrAdd("2", _ => "twoAdd"));
        Assert.Equal("threeAdd", dict.GetOrAdd("3", "threeAdd"));
        Assert.Equal("four", dict.GetOrAdd("4", _ => "fourAdd"));

        Assert.Equal(4, dict.Count);
        Assert.Equal(2, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[0].Action);
        Assert.Equal(2, raiseCol[0].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("1", "oneAdd"), raiseCol[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
        Assert.Equal(3, raiseCol[1].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("3", "threeAdd"), raiseCol[1]?.NewItems?[0]);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("2", "two"), i),
            i => Assert.Equal(KeyValuePair.Create("4", "four"), i),
            i => Assert.Equal(KeyValuePair.Create("1", "oneAdd"), i),
            i => Assert.Equal(KeyValuePair.Create("3", "threeAdd"), i));
    }

    [Fact]
    public void Throws()
    {
        var dict = new ObservableDictionary<string, string>();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => dict.GetOrAdd(null, "one"));
        Assert.Throws<ArgumentNullException>(() => dict.GetOrAdd("1", (Func<string, string>?)null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        var dict2 = new ReadOnlyDictionary();

        Assert.Throws<NotSupportedException>(() => dict2.GetOrAdd("1", "one"));
        Assert.Throws<NotSupportedException>(() => dict2.GetOrAdd("1", _ => "one"));
    }
}

public class InsertTest
{
    [Fact]
    public void AtBottom()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>();

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        dict.Insert(0, KeyValuePair.Create("1", "one"));
        dict.Insert(0, KeyValuePair.Create("2", "two"));
        dict.Insert(0, KeyValuePair.Create("3", "three"));
        dict.Insert(0, KeyValuePair.Create("4", "four"));

        Assert.Equal(4, dict.Count);
        Assert.Equal(4, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("1", "one"), raiseCol[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
        Assert.Equal(0, raiseCol[1].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("2", "two"), raiseCol[1]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[2].Action);
        Assert.Equal(0, raiseCol[2].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("3", "three"), raiseCol[2]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[3].Action);
        Assert.Equal(0, raiseCol[3].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("4", "four"), raiseCol[3]?.NewItems?[0]);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("4", "four"), i),
            i => Assert.Equal(KeyValuePair.Create("3", "three"), i),
            i => Assert.Equal(KeyValuePair.Create("2", "two"), i),
            i => Assert.Equal(KeyValuePair.Create("1", "one"), i));
    }

    [Fact]
    public void AtTop()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>();

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        dict.Insert(dict.Count, KeyValuePair.Create("1", "one"));
        dict.Insert(dict.Count, KeyValuePair.Create("2", "two"));
        dict.Insert(dict.Count, KeyValuePair.Create("3", "three"));
        dict.Insert(dict.Count, KeyValuePair.Create("4", "four"));

        Assert.Equal(4, dict.Count);
        Assert.Equal(4, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("1", "one"), raiseCol[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
        Assert.Equal(1, raiseCol[1].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("2", "two"), raiseCol[1]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[2].Action);
        Assert.Equal(2, raiseCol[2].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("3", "three"), raiseCol[2]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[3].Action);
        Assert.Equal(3, raiseCol[3].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("4", "four"), raiseCol[3]?.NewItems?[0]);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("1", "one"), i),
            i => Assert.Equal(KeyValuePair.Create("2", "two"), i),
            i => Assert.Equal(KeyValuePair.Create("3", "three"), i),
            i => Assert.Equal(KeyValuePair.Create("4", "four"), i));
    }
    
    [Fact]
    public void AtAny()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("4", "four")
        });

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        dict.Insert(0, KeyValuePair.Create("1", "one"));
        dict.Insert(2, KeyValuePair.Create("3", "three"));

        Assert.Equal(4, dict.Count);
        Assert.Equal(2, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("1", "one"), raiseCol[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
        Assert.Equal(2, raiseCol[1].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("3", "three"), raiseCol[1]?.NewItems?[0]);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("1", "one"), i),
            i => Assert.Equal(KeyValuePair.Create("2", "two"), i),
            i => Assert.Equal(KeyValuePair.Create("3", "three"), i),
            i => Assert.Equal(KeyValuePair.Create("4", "four"), i));
    }

    [Fact]
    public void Throws()
    {
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four")
        });

        Assert.Throws<ArgumentOutOfRangeException>(() => dict.Insert(-1, KeyValuePair.Create("5", "five")));
        Assert.Throws<ArgumentOutOfRangeException>(() => dict.Insert(5, KeyValuePair.Create("6", "six")));
        Assert.Throws<ArgumentException>(() => dict.Insert(0, KeyValuePair.Create("1", "one")));

        var dict2 = new ReadOnlyDictionary();

        Assert.Throws<NotSupportedException>(() => dict2.Insert(0, KeyValuePair.Create("1", "one")));
    }
}

public class InsertRangeTest
{
    [Fact]
    public void AtBottom()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>();

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        dict.InsertRange(0, KeyValuePair.Create("1", "one"), KeyValuePair.Create("2", "two"));
        dict.InsertRange(0, new KeyValuePair<string, string>[] { KeyValuePair.Create("3", "three"), KeyValuePair.Create("4", "four") } as IEnumerable<KeyValuePair<string, string>>);

        Assert.Equal(4, dict.Count);
        Assert.Equal(2, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("1", "one"), raiseCol[0]?.NewItems?[0]);
        Assert.Equal(KeyValuePair.Create("2", "two"), raiseCol[0]?.NewItems?[1]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
        Assert.Equal(0, raiseCol[1].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("3", "three"), raiseCol[1]?.NewItems?[0]);
        Assert.Equal(KeyValuePair.Create("4", "four"), raiseCol[1]?.NewItems?[1]);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("3", "three"), i),
            i => Assert.Equal(KeyValuePair.Create("4", "four"), i),
            i => Assert.Equal(KeyValuePair.Create("1", "one"), i),
            i => Assert.Equal(KeyValuePair.Create("2", "two"), i));
    }

    [Fact]
    public void AtTop()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>();

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        dict.InsertRange(dict.Count, KeyValuePair.Create("1", "one"), KeyValuePair.Create("2", "two"));
        dict.InsertRange(dict.Count, new KeyValuePair<string, string>[] { KeyValuePair.Create("3", "three"), KeyValuePair.Create("4", "four") } as IEnumerable<KeyValuePair<string, string>>);

        Assert.Equal(4, dict.Count);
        Assert.Equal(2, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("1", "one"), raiseCol[0]?.NewItems?[0]);
        Assert.Equal(KeyValuePair.Create("2", "two"), raiseCol[0]?.NewItems?[1]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
        Assert.Equal(2, raiseCol[1].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("3", "three"), raiseCol[1]?.NewItems?[0]);
        Assert.Equal(KeyValuePair.Create("4", "four"), raiseCol[1]?.NewItems?[1]);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("1", "one"), i),
            i => Assert.Equal(KeyValuePair.Create("2", "two"), i),
            i => Assert.Equal(KeyValuePair.Create("3", "three"), i),
            i => Assert.Equal(KeyValuePair.Create("4", "four"), i));
    }

    [Fact]
    public void AtAny()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("4", "four")
        });

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        dict.InsertRange(1, KeyValuePair.Create("2", "two"), KeyValuePair.Create("3", "three"));
        dict.InsertRange(4, new KeyValuePair<string, string>[] { KeyValuePair.Create("5", "five"), KeyValuePair.Create("6", "six") } as IEnumerable<KeyValuePair<string, string>>);

        Assert.Equal(6, dict.Count);
        Assert.Equal(2, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[0].Action);
        Assert.Equal(1, raiseCol[0].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("2", "two"), raiseCol[0]?.NewItems?[0]);
        Assert.Equal(KeyValuePair.Create("3", "three"), raiseCol[0]?.NewItems?[1]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
        Assert.Equal(4, raiseCol[1].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("5", "five"), raiseCol[1]?.NewItems?[0]);
        Assert.Equal(KeyValuePair.Create("6", "six"), raiseCol[1]?.NewItems?[1]);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("1", "one"), i),
            i => Assert.Equal(KeyValuePair.Create("2", "two"), i),
            i => Assert.Equal(KeyValuePair.Create("3", "three"), i),
            i => Assert.Equal(KeyValuePair.Create("4", "four"), i),
            i => Assert.Equal(KeyValuePair.Create("5", "five"), i),
            i => Assert.Equal(KeyValuePair.Create("6", "six"), i));
    }

    [Fact]
    public void Throws()
    {
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four")
        });

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => dict.InsertRange(-1, null));
        Assert.Throws<ArgumentNullException>(() => dict.InsertRange(-1, (IEnumerable<KeyValuePair<string, string>>?)null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentOutOfRangeException>(() => dict.InsertRange(-1, KeyValuePair.Create("5", "five"), KeyValuePair.Create("6", "six")));
        Assert.Throws<ArgumentOutOfRangeException>(() => dict.InsertRange(5, new KeyValuePair<string, string>[] { KeyValuePair.Create("7", "seven"), KeyValuePair.Create("8", "eight") } as IEnumerable<KeyValuePair<string, string>>));
        Assert.Throws<ArgumentException>(() => dict.InsertRange(0, KeyValuePair.Create("1", "one"), KeyValuePair.Create("9", "nine")));

        var dict2 = new ReadOnlyDictionary();

        Assert.Throws<NotSupportedException>(() => dict2.InsertRange(0, KeyValuePair.Create("10", "ten"), KeyValuePair.Create("11", "eleven")));
        Assert.Throws<NotSupportedException>(() => dict2.InsertRange(0, new KeyValuePair<string, string>[] { KeyValuePair.Create("12", "twelve"), KeyValuePair.Create("13", "thirteen") } as IEnumerable<KeyValuePair<string, string>>));
    }
}

public class RemoveTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four")
        });

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        Assert.True(dict.Remove("1"));
        Assert.True(dict.Remove("3"));
        Assert.False(dict.Remove("5"));
        Assert.False(dict.Remove("6"));

        Assert.Equal(2, dict.Count);
        Assert.Equal(2, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[0].Action);
        Assert.Equal(0, raiseCol[0].OldStartingIndex);
        Assert.Equal(KeyValuePair.Create("1", "one"), raiseCol[0]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[1].Action);
        Assert.Equal(1, raiseCol[1].OldStartingIndex);
        Assert.Equal(KeyValuePair.Create("3", "three"), raiseCol[1]?.OldItems?[0]);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("2", "two"), i),
            i => Assert.Equal(KeyValuePair.Create("4", "four"), i));
    }

    [Fact]
    public void All()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four")
        });

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        Assert.True(dict.Remove("4"));
        Assert.True(dict.Remove("3"));
        Assert.True(dict.Remove("2"));
        Assert.True(dict.Remove("1"));
        Assert.False(dict.Remove("5"));
        Assert.False(dict.Remove("6"));

        Assert.Empty(dict);
        Assert.Equal(4, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[0].Action);
        Assert.Equal(3, raiseCol[0].OldStartingIndex);
        Assert.Equal(KeyValuePair.Create("4", "four"), raiseCol[0]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[1].Action);
        Assert.Equal(2, raiseCol[1].OldStartingIndex);
        Assert.Equal(KeyValuePair.Create("3", "three"), raiseCol[1]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[2].Action);
        Assert.Equal(1, raiseCol[2].OldStartingIndex);
        Assert.Equal(KeyValuePair.Create("2", "two"), raiseCol[2]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[3].Action);
        Assert.Equal(0, raiseCol[3].OldStartingIndex);
        Assert.Equal(KeyValuePair.Create("1", "one"), raiseCol[3]?.OldItems?[0]);
    }

    [Fact]
    public void Throws()
    {
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four")
        });

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => dict.Remove(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        var dict2 = new ReadOnlyDictionary();

        Assert.Throws<NotSupportedException>(() => dict2.Remove("1"));
    }
}

public class TryAddTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four")
        });

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        Assert.False(dict.TryAdd(KeyValuePair.Create("4", "fourAdd")));
        Assert.True(dict.TryAdd("5", "fiveAdd"));
        Assert.True(dict.TryAdd("6", _ => "sixAdd"));

        Assert.Equal(6, dict.Count);
        Assert.Equal(2, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[0].Action);
        Assert.Equal(4, raiseCol[0].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("5", "fiveAdd"), raiseCol[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Add, raiseCol[1].Action);
        Assert.Equal(5, raiseCol[1].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("6", "sixAdd"), raiseCol[1]?.NewItems?[0]);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("1", "one"), i),
            i => Assert.Equal(KeyValuePair.Create("2", "two"), i),
            i => Assert.Equal(KeyValuePair.Create("3", "three"), i),
            i => Assert.Equal(KeyValuePair.Create("4", "four"), i),
            i => Assert.Equal(KeyValuePair.Create("5", "fiveAdd"), i),
            i => Assert.Equal(KeyValuePair.Create("6", "sixAdd"), i));
    }

    [Fact]
    public void Throws()
    {
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four")
        });

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => dict.TryAdd(KeyValuePair.Create<string, string>(null, "fiveAdd")));
        Assert.Throws<ArgumentNullException>(() => dict.TryAdd(null, "sixAdd"));
        Assert.Throws<ArgumentNullException>(() => dict.TryAdd(null, _ => "sevenAdd"));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        var dict2 = new ReadOnlyDictionary();

        Assert.Throws<NotSupportedException>(() => dict2.TryAdd(KeyValuePair.Create("8", "eightAdd")));
        Assert.Throws<NotSupportedException>(() => dict2.TryAdd("9", "nineAdd"));
        Assert.Throws<NotSupportedException>(() => dict2.TryAdd("10", _ => "tenAdd"));
    }
}

public class TryGetValueTest
{
    [Fact]
    public void Normal()
    {
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four")
        });

        Assert.True(dict.TryGetValue("1"));
        Assert.True(dict.TryGetValue("2", out string? twoValue));
        Assert.False(dict.TryGetValue("5"));
        Assert.False(dict.TryGetValue("6", out string? sixValue));

        Assert.Equal("two", twoValue);
        Assert.Null(sixValue);
    }

    [Fact]
    public void Throws()
    {
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four")
        });

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => dict.TryGetValue(null));
        Assert.Throws<ArgumentNullException>(() => dict.TryGetValue(null, out string? nullValue));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}

public class TryRemoveTest
{
    [Fact]
    public void Normal()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four"),
            new KeyValuePair<string, string>("5", "five"),
            new KeyValuePair<string, string>("6", "six")
        });

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        Assert.True(dict.TryRemove(KeyValuePair.Create("4", ""), out KeyValuePair<string, string?> fourValue1));
        Assert.True(dict.TryRemove("3", out string? threeValue1));
        Assert.True(dict.TryRemove(KeyValuePair.Create("2", "")));
        Assert.True(dict.TryRemove("1"));
        Assert.False(dict.TryRemove(KeyValuePair.Create("4", ""), out KeyValuePair<string, string?> fourValue2));
        Assert.False(dict.TryRemove("3", out string? threeValue2));
        Assert.False(dict.TryRemove(KeyValuePair.Create("2", "")));
        Assert.False(dict.TryRemove("1"));

        Assert.Equal(2, dict.Count);
        Assert.Equal(4, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[0].Action);
        Assert.Equal(3, raiseCol[0].OldStartingIndex);
        Assert.Equal(KeyValuePair.Create("4", "four"), raiseCol[0]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[1].Action);
        Assert.Equal(2, raiseCol[1].OldStartingIndex);
        Assert.Equal(KeyValuePair.Create("3", "three"), raiseCol[1]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[2].Action);
        Assert.Equal(1, raiseCol[2].OldStartingIndex);
        Assert.Equal(KeyValuePair.Create("2", "two"), raiseCol[2]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[3].Action);
        Assert.Equal(0, raiseCol[3].OldStartingIndex);
        Assert.Equal(KeyValuePair.Create("1", "one"), raiseCol[3]?.OldItems?[0]);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("5", "five"), i),
            i => Assert.Equal(KeyValuePair.Create("6", "six"), i));
    }

    [Fact]
    public void All()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four"),
            new KeyValuePair<string, string>("5", "five"),
            new KeyValuePair<string, string>("6", "six")
        });

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        Assert.True(dict.TryRemove(KeyValuePair.Create("6", ""), out KeyValuePair<string, string?> sixValue1));
        Assert.True(dict.TryRemove(KeyValuePair.Create("5", ""), out KeyValuePair<string, string?> fiveValue1));
        Assert.True(dict.TryRemove(KeyValuePair.Create("4", ""), out KeyValuePair<string, string?> fourValue1));
        Assert.True(dict.TryRemove("3", out string? threeValue1));
        Assert.True(dict.TryRemove(KeyValuePair.Create("2", "")));
        Assert.True(dict.TryRemove("1"));
        Assert.False(dict.TryRemove(KeyValuePair.Create("6", ""), out KeyValuePair<string, string?> sixValue2));
        Assert.False(dict.TryRemove(KeyValuePair.Create("5", ""), out KeyValuePair<string, string?> fiveValue2));
        Assert.False(dict.TryRemove(KeyValuePair.Create("4", ""), out KeyValuePair<string, string?> fourValue2));
        Assert.False(dict.TryRemove("3", out string? threeValue2));
        Assert.False(dict.TryRemove(KeyValuePair.Create("2", "")));
        Assert.False(dict.TryRemove("1"));

        Assert.Equal("three", threeValue1);
        Assert.Equal(KeyValuePair.Create<string, string?>("4", "four"), fourValue1);
        Assert.Equal(KeyValuePair.Create<string, string?>("5", "five"), fiveValue1);
        Assert.Equal(KeyValuePair.Create<string, string?>("6", "six"), sixValue1);
        Assert.Null(threeValue2);
        Assert.Equal(KeyValuePair.Create("4", (string?)null), fourValue2);
        Assert.Equal(KeyValuePair.Create("5", (string?)null), fiveValue2);
        Assert.Equal(KeyValuePair.Create("6", (string?)null), sixValue2);

        Assert.Empty(dict);
        Assert.Equal(6, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[0].Action);
        Assert.Equal(5, raiseCol[0].OldStartingIndex);
        Assert.Equal(KeyValuePair.Create("6", "six"), raiseCol[0]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[1].Action);
        Assert.Equal(4, raiseCol[1].OldStartingIndex);
        Assert.Equal(KeyValuePair.Create("5", "five"), raiseCol[1]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[2].Action);
        Assert.Equal(3, raiseCol[2].OldStartingIndex);
        Assert.Equal(KeyValuePair.Create("4", "four"), raiseCol[2]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[3].Action);
        Assert.Equal(2, raiseCol[3].OldStartingIndex);
        Assert.Equal(KeyValuePair.Create("3", "three"), raiseCol[3]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[4].Action);
        Assert.Equal(1, raiseCol[4].OldStartingIndex);
        Assert.Equal(KeyValuePair.Create("2", "two"), raiseCol[4]?.OldItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Remove, raiseCol[5].Action);
        Assert.Equal(0, raiseCol[5].OldStartingIndex);
        Assert.Equal(KeyValuePair.Create("1", "one"), raiseCol[5]?.OldItems?[0]);
    }

    [Fact]
    public void Throws()
    {
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four")
        });

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => dict.TryRemove(null));
        Assert.Throws<ArgumentNullException>(() => dict.TryRemove(KeyValuePair.Create<string, string>(null, "")));
        Assert.Throws<ArgumentNullException>(() => dict.TryRemove(null, out string? nullValue1));
        Assert.Throws<ArgumentNullException>(() => dict.TryRemove(KeyValuePair.Create<string, string>(null, ""), out KeyValuePair<string, string?> nullValue2));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        var dict2 = new ReadOnlyDictionary();

        Assert.Throws<NotSupportedException>(() => dict2.TryRemove("1"));
        Assert.Throws<NotSupportedException>(() => dict2.TryRemove(KeyValuePair.Create("2", "")));
        Assert.Throws<NotSupportedException>(() => dict2.TryRemove("3", out string? threeValue));
        Assert.Throws<NotSupportedException>(() => dict2.TryRemove(KeyValuePair.Create("4", ""), out KeyValuePair<string, string?> fourValue));
    }
}

public class TryUpdateTest
{
    [Fact]
    public void Accept()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four"),
            new KeyValuePair<string, string>("5", "five"),
            new KeyValuePair<string, string>("6", "six"),
            new KeyValuePair<string, string>("7", "seven"),
            new KeyValuePair<string, string>("8", "eight")
        });

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        Assert.True(dict.TryUpdate("7", "sevenUpdate"));
        Assert.True(dict.TryUpdate("6", "sixUpdate", "sixx"));
        Assert.True(dict.TryUpdate("5", "fiveUpdate", args =>
        {
            Assert.Equal("5", args.key);
            Assert.Equal("five", args.oldValue);
            Assert.Equal("fiveUpdate", args.newValue);
            return true;
        }));
        Assert.True(dict.TryUpdate("4", key =>
        {
            Assert.Equal("4", key);
            return "fourUpdate";
        }));
        Assert.True(dict.TryUpdate("3", key =>
        {
            Assert.Equal("3", key);
            return "threeUpdate";
        }, "threex"));
        Assert.True(dict.TryUpdate("2", key =>
        {
            Assert.Equal("2", key);
            return "twoUpdate";
        }, args =>
        {
            Assert.Equal("2", args.key);
            Assert.Equal("two", args.oldValue);
            Assert.Equal("twoUpdate", args.newValue);
            return true;
        }));

        Assert.Equal(8, dict.Count);
        Assert.Equal(6, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[0].Action);
        Assert.Equal(6, raiseCol[0].OldStartingIndex);
        Assert.Equal(6, raiseCol[0].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("7", "seven"), raiseCol[0]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("7", "sevenUpdate"), raiseCol[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[1].Action);
        Assert.Equal(5, raiseCol[1].OldStartingIndex);
        Assert.Equal(5, raiseCol[1].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("6", "six"), raiseCol[1]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("6", "sixUpdate"), raiseCol[1]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[2].Action);
        Assert.Equal(4, raiseCol[2].OldStartingIndex);
        Assert.Equal(4, raiseCol[2].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("5", "five"), raiseCol[2]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("5", "fiveUpdate"), raiseCol[2]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[3].Action);
        Assert.Equal(3, raiseCol[3].OldStartingIndex);
        Assert.Equal(3, raiseCol[3].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("4", "four"), raiseCol[3]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("4", "fourUpdate"), raiseCol[3]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[4].Action);
        Assert.Equal(2, raiseCol[4].OldStartingIndex);
        Assert.Equal(2, raiseCol[4].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("3", "three"), raiseCol[4]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("3", "threeUpdate"), raiseCol[4]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[5].Action);
        Assert.Equal(1, raiseCol[5].OldStartingIndex);
        Assert.Equal(1, raiseCol[5].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("2", "two"), raiseCol[5]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("2", "twoUpdate"), raiseCol[5]?.NewItems?[0]);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("1", "one"), i),
            i => Assert.Equal(KeyValuePair.Create("2", "twoUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("3", "threeUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("4", "fourUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("5", "fiveUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("6", "sixUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("7", "sevenUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("8", "eight"), i));
    }

    [Fact]
    public void Denied()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four"),
            new KeyValuePair<string, string>("5", "five"),
            new KeyValuePair<string, string>("6", "six"),
            new KeyValuePair<string, string>("7", "seven"),
            new KeyValuePair<string, string>("8", "eight")
        });

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        Assert.False(dict.TryUpdate("7x", "sevenUpdate"));
        Assert.False(dict.TryUpdate("6", "sixUpdate", "six"));
        Assert.False(dict.TryUpdate("5", "fiveUpdate", args =>
        {
            Assert.Equal("5", args.key);
            Assert.Equal("five", args.oldValue);
            Assert.Equal("fiveUpdate", args.newValue);
            return false;
        }));
        Assert.False(dict.TryUpdate("4x", key =>
        {
            Assert.Equal("4x", key);
            return "fourUpdate";
        }));
        Assert.False(dict.TryUpdate("3", key =>
        {
            Assert.Equal("3", key);
            return "threeUpdate";
        }, "three"));
        Assert.False(dict.TryUpdate("2", key =>
        {
            Assert.Equal("2", key);
            return "twoUpdate";
        }, args =>
        {
            Assert.Equal("2", args.key);
            Assert.Equal("two", args.oldValue);
            Assert.Equal("twoUpdate", args.newValue);
            return false;
        }));

        Assert.Equal(8, dict.Count);
        Assert.Empty(raiseCol);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("1", "one"), i),
            i => Assert.Equal(KeyValuePair.Create("2", "two"), i),
            i => Assert.Equal(KeyValuePair.Create("3", "three"), i),
            i => Assert.Equal(KeyValuePair.Create("4", "four"), i),
            i => Assert.Equal(KeyValuePair.Create("5", "five"), i),
            i => Assert.Equal(KeyValuePair.Create("6", "six"), i),
            i => Assert.Equal(KeyValuePair.Create("7", "seven"), i),
            i => Assert.Equal(KeyValuePair.Create("8", "eight"), i));
    }

    [Fact]
    public void AcceptAndDenied()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four"),
            new KeyValuePair<string, string>("5", "five"),
            new KeyValuePair<string, string>("6", "six"),
            new KeyValuePair<string, string>("7", "seven"),
            new KeyValuePair<string, string>("8", "eight")
        });

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        Assert.True(dict.TryUpdate("7", "sevenUpdate"));
        Assert.True(dict.TryUpdate("6", "sixUpdate", "sixx"));
        Assert.True(dict.TryUpdate("5", "fiveUpdate", args =>
        {
            Assert.Equal("5", args.key);
            Assert.Equal("five", args.oldValue);
            Assert.Equal("fiveUpdate", args.newValue);
            return true;
        }));
        Assert.True(dict.TryUpdate("4", key =>
        {
            Assert.Equal("4", key);
            return "fourUpdate";
        }));
        Assert.True(dict.TryUpdate("3", key =>
        {
            Assert.Equal("3", key);
            return "threeUpdate";
        }, "threex"));
        Assert.True(dict.TryUpdate("2", key =>
        {
            Assert.Equal("2", key);
            return "twoUpdate";
        }, args =>
        {
            Assert.Equal("2", args.key);
            Assert.Equal("two", args.oldValue);
            Assert.Equal("twoUpdate", args.newValue);
            return true;
        }));

        Assert.False(dict.TryUpdate("7x", "sevenUpdate2"));
        Assert.False(dict.TryUpdate("6", "sixUpdate2", "sixUpdate"));
        Assert.False(dict.TryUpdate("5", "fiveUpdate2", args =>
        {
            Assert.Equal("5", args.key);
            Assert.Equal("fiveUpdate", args.oldValue);
            Assert.Equal("fiveUpdate2", args.newValue);
            return false;
        }));
        Assert.False(dict.TryUpdate("4x", key =>
        {
            Assert.Equal("4x", key);
            return "fourUpdate2";
        }));
        Assert.False(dict.TryUpdate("3", key =>
        {
            Assert.Equal("3", key);
            return "threeUpdate2";
        }, "threeUpdate"));
        Assert.False(dict.TryUpdate("2", key =>
        {
            Assert.Equal("2", key);
            return "twoUpdate2";
        }, args =>
        {
            Assert.Equal("2", args.key);
            Assert.Equal("twoUpdate", args.oldValue);
            Assert.Equal("twoUpdate2", args.newValue);
            return false;
        }));

        Assert.Equal(8, dict.Count);
        Assert.Equal(6, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[0].Action);
        Assert.Equal(6, raiseCol[0].OldStartingIndex);
        Assert.Equal(6, raiseCol[0].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("7", "seven"), raiseCol[0]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("7", "sevenUpdate"), raiseCol[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[1].Action);
        Assert.Equal(5, raiseCol[1].OldStartingIndex);
        Assert.Equal(5, raiseCol[1].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("6", "six"), raiseCol[1]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("6", "sixUpdate"), raiseCol[1]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[2].Action);
        Assert.Equal(4, raiseCol[2].OldStartingIndex);
        Assert.Equal(4, raiseCol[2].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("5", "five"), raiseCol[2]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("5", "fiveUpdate"), raiseCol[2]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[3].Action);
        Assert.Equal(3, raiseCol[3].OldStartingIndex);
        Assert.Equal(3, raiseCol[3].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("4", "four"), raiseCol[3]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("4", "fourUpdate"), raiseCol[3]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[4].Action);
        Assert.Equal(2, raiseCol[4].OldStartingIndex);
        Assert.Equal(2, raiseCol[4].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("3", "three"), raiseCol[4]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("3", "threeUpdate"), raiseCol[4]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[5].Action);
        Assert.Equal(1, raiseCol[5].OldStartingIndex);
        Assert.Equal(1, raiseCol[5].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("2", "two"), raiseCol[5]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("2", "twoUpdate"), raiseCol[5]?.NewItems?[0]);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("1", "one"), i),
            i => Assert.Equal(KeyValuePair.Create("2", "twoUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("3", "threeUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("4", "fourUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("5", "fiveUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("6", "sixUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("7", "sevenUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("8", "eight"), i));
    }

    [Fact]
    public void Throws()
    {
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four"),
            new KeyValuePair<string, string>("5", "five"),
            new KeyValuePair<string, string>("6", "six"),
            new KeyValuePair<string, string>("7", "seven"),
            new KeyValuePair<string, string>("8", "eight")
        });

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => dict.TryUpdate(null, "sevenUpdate"));
        Assert.Throws<ArgumentNullException>(() => dict.TryUpdate(null, "sixUpdate", "sixx"));
        Assert.Throws<ArgumentNullException>(() => dict.TryUpdate(null, "fiveUpdate", args =>
        {
            Assert.Equal("5", args.key);
            Assert.Equal("five", args.oldValue);
            Assert.Equal("fiveUpdate", args.newValue);
            return true;
        }));
        Assert.Throws<ArgumentNullException>(() => dict.TryUpdate(null, "fiveUpdate", (Func<(string key, string newValue, string oldValue), bool>?)null));
        Assert.Throws<ArgumentNullException>(() => dict.TryUpdate(null, key =>
        {
            Assert.Equal("4", key);
            return "fourUpdate";
        }));
        Assert.Throws<ArgumentNullException>(() => dict.TryUpdate(null, (Func<string, string>?)null));
        Assert.Throws<ArgumentNullException>(() => dict.TryUpdate(null, key =>
        {
            Assert.Equal("3", key);
            return "threeUpdate";
        }, "threex"));
        Assert.Throws<ArgumentNullException>(() => dict.TryUpdate(null, (Func<string, string>?)null, "threex"));
        Assert.Throws<ArgumentNullException>(() => dict.TryUpdate(null, key =>
        {
            Assert.Equal("2", key);
            return "twoUpdate";
        }, args =>
        {
            Assert.Equal("2", args.key);
            Assert.Equal("two", args.oldValue);
            Assert.Equal("twoUpdate", args.newValue);
            return true;
        }));
        Assert.Throws<ArgumentNullException>(() => dict.TryUpdate(null, (Func<string, string>?)null, args =>
        {
            Assert.Equal("2", args.key);
            Assert.Equal("two", args.oldValue);
            Assert.Equal("twoUpdate", args.newValue);
            return true;
        }));
        Assert.Throws<ArgumentNullException>(() => dict.TryUpdate(null, key =>
        {
            Assert.Equal("2", key);
            return "twoUpdate";
        }, (Func<(string key, string newValue, string oldValue), bool>?)null));
        Assert.Throws<ArgumentNullException>(() => dict.TryUpdate(null, (Func<string, string>?)null, (Func<(string key, string newValue, string oldValue), bool>?)null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        var dict2 = new ReadOnlyDictionary();

        Assert.Throws<NotSupportedException>(() => dict2.TryUpdate("7", "sevenUpdate"));
        Assert.Throws<NotSupportedException>(() => dict2.TryUpdate("6", "sixUpdate", "sixx"));
        Assert.Throws<NotSupportedException>(() => dict2.TryUpdate("5", "fiveUpdate", args =>
        {
            Assert.Equal("5", args.key);
            Assert.Equal("five", args.oldValue);
            Assert.Equal("fiveUpdate", args.newValue);
            return true;
        }));
        Assert.Throws<NotSupportedException>(() => dict2.TryUpdate("4", key =>
        {
            Assert.Equal("4", key);
            return "fourUpdate";
        }));
        Assert.Throws<NotSupportedException>(() => dict2.TryUpdate("3", key =>
        {
            Assert.Equal("3", key);
            return "threeUpdate";
        }, "threex"));
        Assert.Throws<NotSupportedException>(() => dict2.TryUpdate("2", key =>
        {
            Assert.Equal("2", key);
            return "twoUpdate";
        }, args =>
        {
            Assert.Equal("2", args.key);
            Assert.Equal("two", args.oldValue);
            Assert.Equal("twoUpdate", args.newValue);
            return true;
        }));
    }
}

public class UpdateTest
{
    [Fact]
    public void Accept()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four"),
            new KeyValuePair<string, string>("5", "five"),
            new KeyValuePair<string, string>("6", "six"),
            new KeyValuePair<string, string>("7", "seven"),
            new KeyValuePair<string, string>("8", "eight")
        });

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        dict.Update("7", "sevenUpdate");
        dict.Update("6", "sixUpdate", "sixx");
        dict.Update("5", "fiveUpdate", args =>
        {
            Assert.Equal("5", args.key);
            Assert.Equal("five", args.oldValue);
            Assert.Equal("fiveUpdate", args.newValue);
            return true;
        });
        dict.Update("4", key =>
        {
            Assert.Equal("4", key);
            return "fourUpdate";
        });
        dict.Update("3", key =>
        {
            Assert.Equal("3", key);
            return "threeUpdate";
        }, "threex");
        dict.Update("2", key =>
        {
            Assert.Equal("2", key);
            return "twoUpdate";
        }, args =>
        {
            Assert.Equal("2", args.key);
            Assert.Equal("two", args.oldValue);
            Assert.Equal("twoUpdate", args.newValue);
            return true;
        });

        Assert.Equal(8, dict.Count);
        Assert.Equal(6, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[0].Action);
        Assert.Equal(6, raiseCol[0].OldStartingIndex);
        Assert.Equal(6, raiseCol[0].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("7", "seven"), raiseCol[0]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("7", "sevenUpdate"), raiseCol[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[1].Action);
        Assert.Equal(5, raiseCol[1].OldStartingIndex);
        Assert.Equal(5, raiseCol[1].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("6", "six"), raiseCol[1]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("6", "sixUpdate"), raiseCol[1]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[2].Action);
        Assert.Equal(4, raiseCol[2].OldStartingIndex);
        Assert.Equal(4, raiseCol[2].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("5", "five"), raiseCol[2]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("5", "fiveUpdate"), raiseCol[2]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[3].Action);
        Assert.Equal(3, raiseCol[3].OldStartingIndex);
        Assert.Equal(3, raiseCol[3].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("4", "four"), raiseCol[3]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("4", "fourUpdate"), raiseCol[3]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[4].Action);
        Assert.Equal(2, raiseCol[4].OldStartingIndex);
        Assert.Equal(2, raiseCol[4].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("3", "three"), raiseCol[4]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("3", "threeUpdate"), raiseCol[4]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[5].Action);
        Assert.Equal(1, raiseCol[5].OldStartingIndex);
        Assert.Equal(1, raiseCol[5].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("2", "two"), raiseCol[5]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("2", "twoUpdate"), raiseCol[5]?.NewItems?[0]);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("1", "one"), i),
            i => Assert.Equal(KeyValuePair.Create("2", "twoUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("3", "threeUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("4", "fourUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("5", "fiveUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("6", "sixUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("7", "sevenUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("8", "eight"), i));
    }

    [Fact]
    public void Denied()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four"),
            new KeyValuePair<string, string>("5", "five"),
            new KeyValuePair<string, string>("6", "six"),
            new KeyValuePair<string, string>("7", "seven"),
            new KeyValuePair<string, string>("8", "eight")
        });

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        dict.Update("7x", "sevenUpdate");
        dict.Update("6", "sixUpdate", "six");
        dict.Update("5", "fiveUpdate", args =>
        {
            Assert.Equal("5", args.key);
            Assert.Equal("five", args.oldValue);
            Assert.Equal("fiveUpdate", args.newValue);
            return false;
        });
        dict.Update("4x", key =>
        {
            Assert.Equal("4x", key);
            return "fourUpdate";
        });
        dict.Update("3", key =>
        {
            Assert.Equal("3", key);
            return "threeUpdate";
        }, "three");
        dict.Update("2", key =>
        {
            Assert.Equal("2", key);
            return "twoUpdate";
        }, args =>
        {
            Assert.Equal("2", args.key);
            Assert.Equal("two", args.oldValue);
            Assert.Equal("twoUpdate", args.newValue);
            return false;
        });

        Assert.Equal(8, dict.Count);
        Assert.Empty(raiseCol);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("1", "one"), i),
            i => Assert.Equal(KeyValuePair.Create("2", "two"), i),
            i => Assert.Equal(KeyValuePair.Create("3", "three"), i),
            i => Assert.Equal(KeyValuePair.Create("4", "four"), i),
            i => Assert.Equal(KeyValuePair.Create("5", "five"), i),
            i => Assert.Equal(KeyValuePair.Create("6", "six"), i),
            i => Assert.Equal(KeyValuePair.Create("7", "seven"), i),
            i => Assert.Equal(KeyValuePair.Create("8", "eight"), i));
    }

    [Fact]
    public void AcceptAndDenied()
    {
        var raiseCol = new List<NotifyCollectionChangedEventArgs>();
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four"),
            new KeyValuePair<string, string>("5", "five"),
            new KeyValuePair<string, string>("6", "six"),
            new KeyValuePair<string, string>("7", "seven"),
            new KeyValuePair<string, string>("8", "eight")
        });

        dict.ImmediateCollectionChanged += (s, e) =>
        {
            raiseCol.Add(e);
        };

        dict.Update("7", "sevenUpdate");
        dict.Update("6", "sixUpdate", "sixx");
        dict.Update("5", "fiveUpdate", args =>
        {
            Assert.Equal("5", args.key);
            Assert.Equal("five", args.oldValue);
            Assert.Equal("fiveUpdate", args.newValue);
            return true;
        });
        dict.Update("4", key =>
        {
            Assert.Equal("4", key);
            return "fourUpdate";
        });
        dict.Update("3", key =>
        {
            Assert.Equal("3", key);
            return "threeUpdate";
        }, "threex");
        dict.Update("2", key =>
        {
            Assert.Equal("2", key);
            return "twoUpdate";
        }, args =>
        {
            Assert.Equal("2", args.key);
            Assert.Equal("two", args.oldValue);
            Assert.Equal("twoUpdate", args.newValue);
            return true;
        });

        dict.Update("7x", "sevenUpdate2");
        dict.Update("6", "sixUpdate2", "sixUpdate");
        dict.Update("5", "fiveUpdate2", args =>
        {
            Assert.Equal("5", args.key);
            Assert.Equal("fiveUpdate", args.oldValue);
            Assert.Equal("fiveUpdate2", args.newValue);
            return false;
        });
        dict.Update("4x", key =>
        {
            Assert.Equal("4x", key);
            return "fourUpdate2";
        });
        dict.Update("3", key =>
        {
            Assert.Equal("3", key);
            return "threeUpdate2";
        }, "threeUpdate");
        dict.Update("2", key =>
        {
            Assert.Equal("2", key);
            return "twoUpdate2";
        }, args =>
        {
            Assert.Equal("2", args.key);
            Assert.Equal("twoUpdate", args.oldValue);
            Assert.Equal("twoUpdate2", args.newValue);
            return false;
        });

        Assert.Equal(8, dict.Count);
        Assert.Equal(6, raiseCol.Count);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[0].Action);
        Assert.Equal(6, raiseCol[0].OldStartingIndex);
        Assert.Equal(6, raiseCol[0].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("7", "seven"), raiseCol[0]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("7", "sevenUpdate"), raiseCol[0]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[1].Action);
        Assert.Equal(5, raiseCol[1].OldStartingIndex);
        Assert.Equal(5, raiseCol[1].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("6", "six"), raiseCol[1]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("6", "sixUpdate"), raiseCol[1]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[2].Action);
        Assert.Equal(4, raiseCol[2].OldStartingIndex);
        Assert.Equal(4, raiseCol[2].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("5", "five"), raiseCol[2]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("5", "fiveUpdate"), raiseCol[2]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[3].Action);
        Assert.Equal(3, raiseCol[3].OldStartingIndex);
        Assert.Equal(3, raiseCol[3].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("4", "four"), raiseCol[3]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("4", "fourUpdate"), raiseCol[3]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[4].Action);
        Assert.Equal(2, raiseCol[4].OldStartingIndex);
        Assert.Equal(2, raiseCol[4].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("3", "three"), raiseCol[4]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("3", "threeUpdate"), raiseCol[4]?.NewItems?[0]);

        Assert.Equal(NotifyCollectionChangedAction.Replace, raiseCol[5].Action);
        Assert.Equal(1, raiseCol[5].OldStartingIndex);
        Assert.Equal(1, raiseCol[5].NewStartingIndex);
        Assert.Equal(KeyValuePair.Create("2", "two"), raiseCol[5]?.OldItems?[0]);
        Assert.Equal(KeyValuePair.Create("2", "twoUpdate"), raiseCol[5]?.NewItems?[0]);

        Assert.Collection(dict,
            i => Assert.Equal(KeyValuePair.Create("1", "one"), i),
            i => Assert.Equal(KeyValuePair.Create("2", "twoUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("3", "threeUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("4", "fourUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("5", "fiveUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("6", "sixUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("7", "sevenUpdate"), i),
            i => Assert.Equal(KeyValuePair.Create("8", "eight"), i));
    }

    [Fact]
    public void Throws()
    {
        var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "one"),
            new KeyValuePair<string, string>("2", "two"),
            new KeyValuePair<string, string>("3", "three"),
            new KeyValuePair<string, string>("4", "four"),
            new KeyValuePair<string, string>("5", "five"),
            new KeyValuePair<string, string>("6", "six"),
            new KeyValuePair<string, string>("7", "seven"),
            new KeyValuePair<string, string>("8", "eight")
        });

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => dict.Update(null, "sevenUpdate"));
        Assert.Throws<ArgumentNullException>(() => dict.Update(null, "sixUpdate", "sixx"));
        Assert.Throws<ArgumentNullException>(() => dict.Update(null, "fiveUpdate", args =>
        {
            Assert.Equal("5", args.key);
            Assert.Equal("five", args.oldValue);
            Assert.Equal("fiveUpdate", args.newValue);
            return true;
        }));
        Assert.Throws<ArgumentNullException>(() => dict.Update(null, "fiveUpdate", (Func<(string key, string newValue, string oldValue), bool>?)null));
        Assert.Throws<ArgumentNullException>(() => dict.Update(null, key =>
        {
            Assert.Equal("4", key);
            return "fourUpdate";
        }));
        Assert.Throws<ArgumentNullException>(() => dict.Update(null, (Func<string, string>?)null));
        Assert.Throws<ArgumentNullException>(() => dict.Update(null, key =>
        {
            Assert.Equal("3", key);
            return "threeUpdate";
        }, "threex"));
        Assert.Throws<ArgumentNullException>(() => dict.Update(null, (Func<string, string>?)null, "threex"));
        Assert.Throws<ArgumentNullException>(() => dict.Update(null, key =>
        {
            Assert.Equal("2", key);
            return "twoUpdate";
        }, args =>
        {
            Assert.Equal("2", args.key);
            Assert.Equal("two", args.oldValue);
            Assert.Equal("twoUpdate", args.newValue);
            return true;
        }));
        Assert.Throws<ArgumentNullException>(() => dict.Update(null, (Func<string, string>?)null, args =>
        {
            Assert.Equal("2", args.key);
            Assert.Equal("two", args.oldValue);
            Assert.Equal("twoUpdate", args.newValue);
            return true;
        }));
        Assert.Throws<ArgumentNullException>(() => dict.Update(null, key =>
        {
            Assert.Equal("2", key);
            return "twoUpdate";
        }, (Func<(string key, string newValue, string oldValue), bool>?)null));
        Assert.Throws<ArgumentNullException>(() => dict.Update(null, (Func<string, string>?)null, (Func<(string key, string newValue, string oldValue), bool>?)null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        var dict2 = new ReadOnlyDictionary();

        Assert.Throws<NotSupportedException>(() => dict2.Update("7", "sevenUpdate"));
        Assert.Throws<NotSupportedException>(() => dict2.Update("6", "sixUpdate", "sixx"));
        Assert.Throws<NotSupportedException>(() => dict2.Update("5", "fiveUpdate", args =>
        {
            Assert.Equal("5", args.key);
            Assert.Equal("five", args.oldValue);
            Assert.Equal("fiveUpdate", args.newValue);
            return true;
        }));
        Assert.Throws<NotSupportedException>(() => dict2.Update("4", key =>
        {
            Assert.Equal("4", key);
            return "fourUpdate";
        }));
        Assert.Throws<NotSupportedException>(() => dict2.Update("3", key =>
        {
            Assert.Equal("3", key);
            return "threeUpdate";
        }, "threex"));
        Assert.Throws<NotSupportedException>(() => dict2.Update("2", key =>
        {
            Assert.Equal("2", key);
            return "twoUpdate";
        }, args =>
        {
            Assert.Equal("2", args.key);
            Assert.Equal("two", args.oldValue);
            Assert.Equal("twoUpdate", args.newValue);
            return true;
        }));
    }
}
