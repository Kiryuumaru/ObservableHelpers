using ObservableHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ObservableDictionaryTest
{
    public class ReadOnlyDictionary : ObservableDictionary<string, string>
    {
        public ReadOnlyDictionary()
        {
            IsReadOnly = true;
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

            Assert.Equal("one", dict["1"]);
            Assert.Equal("two", dict["2"]);
            Assert.Equal("three", dict["3"]);
            Assert.Equal("four", dict["4"]);
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

            Assert.Equal("one", dict["1"]);
            Assert.Equal("two", dict["2"]);
            Assert.Equal("three", dict["3"]);
            Assert.Equal("four", dict["4"]);
        }

        [Fact]
        public void Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ObservableDictionary<int, int>((IEnumerable<KeyValuePair<int, int>>)null));
            Assert.Throws<ArgumentNullException>(() => new ObservableDictionary<int, int>((IEqualityComparer<int>)null));
            Assert.Throws<ArgumentNullException>(() => new ObservableDictionary<int, int>(null, null));
        }
    }

    public class AddTest
    {
        [Fact]
        public async void Normal()
        {
            var raiseCount = 0;
            var dict = new ObservableDictionary<string, string>();

            dict.CollectionChanged += (s, e) =>
            {
                raiseCount++;
            };

            dict.Add("1", "one");
            dict.Add("2", "two");
            dict.Add("3", "three");

            await Task.Delay(500);

            Assert.Equal(3, raiseCount);
            Assert.Equal(3, dict.Count);

            Assert.Equal("one", dict["1"]);
            Assert.Equal("two", dict["2"]);
            Assert.Equal("three", dict["3"]);
        }

        [Fact]
        public async void WithConcurrency()
        {
            var raiseCount1 = 0;
            var raiseCount2 = 0;
            var dict = new ObservableDictionary<string, string>();

            dict.ImmediateCollectionChanged += (s, e) =>
            {
                Interlocked.Increment(ref raiseCount1);
            };
            dict.CollectionChanged += (s, e) =>
            {
                Interlocked.Increment(ref raiseCount2);
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

            await Task.WhenAny(
                Task.Delay(60000),
                Task.Run(async delegate
                {
                    while (20000 != raiseCount1 ||
                           20000 != raiseCount2)
                    {
                        await Task.Delay(500);
                    }
                }));

            Assert.Equal(20000, raiseCount1);
            Assert.Equal(20000, raiseCount2);
            Assert.Equal(20000, dict.Count);

            for (int i = 0; i < 20000; i++)
            {
                Assert.Contains(i.ToString(), dict as IDictionary<string, string>);
            }
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

            Assert.Throws<ArgumentNullException>(() => dict.Add(null, "exists"));
            Assert.Throws<ArgumentException>(() => dict.Add("3", "exists"));

            var dict1 = new ReadOnlyDictionary();

            Assert.Throws<NotSupportedException>(() => dict1.Add("1", "one"));
        }
    }

    public class AddRangeTest
    {
        [Fact]
        public async void Normal()
        {
            var raiseCount = 0;
            var dict = new ObservableDictionary<string, string>();

            Assert.Empty(dict);

            dict.CollectionChanged += (s, e) =>
            {
                raiseCount++;
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

            await Task.Delay(500);

            Assert.Equal(2, raiseCount);
            Assert.Equal(4, dict.Count);

            Assert.Equal("one", dict["1"]);
            Assert.Equal("two", dict["2"]);
            Assert.Equal("three", dict["3"]);
            Assert.Equal("four", dict["4"]);
        }

        [Fact]
        public async void WithConcurrency()
        {
            var raiseCount1 = 0;
            var raiseCount2 = 0;
            var dict = new ObservableDictionary<string, string>();

            dict.ImmediateCollectionChanged += (s, e) =>
            {
                Interlocked.Increment(ref raiseCount1);
            };
            dict.CollectionChanged += (s, e) =>
            {
                Interlocked.Increment(ref raiseCount2);
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

            await Task.WhenAny(
                Task.Delay(60000),
                Task.Run(async delegate
                {
                    while (20000 != raiseCount1 ||
                           20000 != raiseCount2)
                    {
                        await Task.Delay(500);
                    }
                }));

            Assert.Equal(20000, raiseCount1);
            Assert.Equal(20000, raiseCount2);
            Assert.Equal(60000, dict.Count);

            for (int i = 0; i < 60000; i++)
            {
                Assert.Contains(i.ToString(), dict as IDictionary<string, string>);
            }
        }

        [Fact]
        public void Throws()
        {
            var dict = new ObservableDictionary<string, string>();

            Assert.Throws<ArgumentNullException>(() => dict.AddRange(null));
            Assert.Throws<ArgumentNullException>(() => dict.AddRange((IEnumerable<KeyValuePair<string, string>>)null));

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
            var dict = new ObservableDictionary<string, string>();

            Assert.Equal("oneAdd", dict.AddOrUpdate("1", "oneAdd"));
            Assert.Equal("twoAdd", dict.AddOrUpdate("2", _ => "twoAdd"));
            Assert.Equal("threeAdd", dict.AddOrUpdate("3", "threeAdd", "threeUpdate"));
            Assert.Equal("fourAdd", dict.AddOrUpdate("4", _ => "fourAdd", "fourUpdate"));
            Assert.Equal("fiveAdd", dict.AddOrUpdate("5", "fiveAdd", _ => "fiveAdd"));
            Assert.Equal("sixAdd", dict.AddOrUpdate("6", _ => "sixAdd", _ => "sixUpdate"));

            Assert.Equal("oneAdd", dict["1"]);
            Assert.Equal("twoAdd", dict["2"]);
            Assert.Equal("threeAdd", dict["3"]);
            Assert.Equal("fourAdd", dict["4"]);
            Assert.Equal("fiveAdd", dict["5"]);
            Assert.Equal("sixAdd", dict["6"]);
        }

        [Fact]
        public void WithUpdate()
        {
            var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("1", "one"),
                new KeyValuePair<string, string>("2", "two"),
                new KeyValuePair<string, string>("3", "three"),
                new KeyValuePair<string, string>("4", "four"),
                new KeyValuePair<string, string>("5", "five"),
                new KeyValuePair<string, string>("6", "six")
            });

            Assert.Equal("oneAddOrUpdate", dict.AddOrUpdate("1", "oneAddOrUpdate"));
            Assert.Equal("twoAddOrUpdate", dict.AddOrUpdate("2", _ => "twoAddOrUpdate"));
            Assert.Equal("threeUpdate", dict.AddOrUpdate("3", "threeAdd", "threeUpdate"));
            Assert.Equal("fourUpdate", dict.AddOrUpdate("4", _ => "fourAdd", "fourUpdate"));
            Assert.Equal("fiveUpdate", dict.AddOrUpdate("5", "fiveAdd", _ => "fiveUpdate"));
            Assert.Equal("sixUpdate", dict.AddOrUpdate("6", _ => "sixAdd", _ => "sixUpdate"));

            Assert.Equal("oneAddOrUpdate", dict["1"]);
            Assert.Equal("twoAddOrUpdate", dict["2"]);
            Assert.Equal("threeUpdate", dict["3"]);
            Assert.Equal("fourUpdate", dict["4"]);
            Assert.Equal("fiveUpdate", dict["5"]);
            Assert.Equal("sixUpdate", dict["6"]);
        }

        [Fact]
        public void WithAddAndUpdate()
        {
            var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("1", "one"),
                new KeyValuePair<string, string>("2", "two"),
                new KeyValuePair<string, string>("3", "three")
            });

            Assert.Equal("oneAddOrUpdate", dict.AddOrUpdate("1", "oneAddOrUpdate"));
            Assert.Equal("twoAddOrUpdate", dict.AddOrUpdate("2", _ => "twoAddOrUpdate"));
            Assert.Equal("threeUpdate", dict.AddOrUpdate("3", "threeAdd", "threeUpdate"));
            Assert.Equal("fourAdd", dict.AddOrUpdate("4", _ => "fourAdd", "fourUpdate"));
            Assert.Equal("fiveAdd", dict.AddOrUpdate("5", "fiveAdd", _ => "fiveUpdate"));
            Assert.Equal("sixAdd", dict.AddOrUpdate("6", _ => "sixAdd", _ => "sixUpdate"));

            Assert.Equal("oneAddOrUpdate", dict["1"]);
            Assert.Equal("twoAddOrUpdate", dict["2"]);
            Assert.Equal("threeUpdate", dict["3"]);
            Assert.Equal("fourAdd", dict["4"]);
            Assert.Equal("fiveAdd", dict["5"]);
            Assert.Equal("sixAdd", dict["6"]);
        }


        [Fact]
        public async void WithConcurrency()
        {
            var raiseCount1 = 0;
            var raiseCount2 = 0;
            var dict = new ObservableDictionary<string, string>();

            dict.ImmediateCollectionChanged += (s, e) =>
            {
                Interlocked.Increment(ref raiseCount1);
            };
            dict.CollectionChanged += (s, e) =>
            {
                Interlocked.Increment(ref raiseCount2);
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

            await Task.WhenAny(
                Task.Delay(60000),
                Task.Run(async delegate
                {
                    while (150000 != raiseCount1 ||
                           150000 != raiseCount2)
                    {
                        await Task.Delay(500);
                    }
                }));

            Assert.Equal(150000, raiseCount1);
            Assert.Equal(150000, raiseCount2);
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
        public async void WithConcurrency()
        {
            var raiseCount1 = 0;
            var raiseCount2 = 0;
            var dict = new ObservableDictionary<string, string>();

            dict.ImmediateCollectionChanged += (s, e) =>
            {
                Interlocked.Increment(ref raiseCount1);
            };
            dict.CollectionChanged += (s, e) =>
            {
                Interlocked.Increment(ref raiseCount2);
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

            await Task.WhenAny(
                Task.Delay(60000),
                Task.Run(async delegate
                {
                    while (20000 != raiseCount1 ||
                           20000 != raiseCount2)
                    {
                        await Task.Delay(500);
                    }
                }));

            Assert.Equal(20000, raiseCount1);
            Assert.Equal(20000, raiseCount2);
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

    public class GetEnumerator
    {
        [Fact]
        public async void ForLoop()
        {
            var dict = new ObservableDictionary<string, string>(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("1", "one"),
                new KeyValuePair<string, string>("2", "two"),
                new KeyValuePair<string, string>("3", "three"),
                new KeyValuePair<string, string>("4", "four")
            });

            await Task.Delay(500);

            var enumerator = dict.GetEnumerator();

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
}
