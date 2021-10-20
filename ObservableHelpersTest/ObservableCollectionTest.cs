using ObservableHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ObservableCollectionTest
{
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

            Assert.Equal(1, col[0]);
            Assert.Equal(2, col[1]);
            Assert.Equal(3, col[2]);
            Assert.Equal(4, col[3]);
        }

        [Fact]
        public void Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ObservableCollection<int>(null));
        }
    }

    public class AddTest
    {
        [Fact]
        public async void Normal()
        {
            var raiseCount = 0;
            var col = new ObservableCollection<int>();

            col.CollectionChanged += (s, e) =>
            {
                raiseCount++;
            };

            col.Add(1);
            col.Add(2);
            col.Add(3);

            await Task.Delay(500);

            Assert.Equal(3, raiseCount);
            Assert.Equal(3, col.Count);

            Assert.Equal(1, col[0]);
            Assert.Equal(2, col[1]);
            Assert.Equal(3, col[2]);
        }

        [Fact]
        public async void WithConcurrency()
        {
            var raiseCount1 = 0;
            var raiseCount2 = 0;
            var col = new ObservableCollection<int>();

            Assert.Empty(col);

            col.ImmediateCollectionChanged += (s, e) =>
            {
                Interlocked.Increment(ref raiseCount1);
            };
            col.CollectionChanged += (s, e) =>
            {
                Interlocked.Increment(ref raiseCount2);
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
            Assert.Equal(20000, col.Count);

            for (int i = 0; i < 20000; i++)
            {
                Assert.Contains(i, col);
            }
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
        public async void Normal()
        {
            var raiseCount = 0;
            var col = new ObservableCollection<int>();

            col.CollectionChanged += (s, e) =>
            {
                raiseCount++;
            };

            col.AddRange(1, 2);
            col.AddRange(new int[] { 3, 4 } as IEnumerable<int>);

            await Task.Delay(500);

            Assert.Equal(2, raiseCount);
            Assert.Equal(4, col.Count);
            Assert.Equal(1, col[0]);
            Assert.Equal(2, col[1]);
            Assert.Equal(3, col[2]);
            Assert.Equal(4, col[3]);
        }

        [Fact]
        public async void WithConcurrency()
        {
            var raiseCount1 = 0;
            var raiseCount2 = 0;
            var col = new ObservableCollection<int>();

            col.ImmediateCollectionChanged += (s, e) =>
            {
                Interlocked.Increment(ref raiseCount1);
            };
            col.CollectionChanged += (s, e) =>
            {
                Interlocked.Increment(ref raiseCount2);
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
            Assert.Equal(60000, col.Count);

            for (int i = 0; i < 60000; i++)
            {
                Assert.Contains(i, col);
            }
        }

        [Fact]
        public void Throws()
        {
            var col = new ObservableCollection<int>();

            Assert.Throws<ArgumentNullException>(() => col.AddRange(null));
            Assert.Throws<ArgumentNullException>(() => col.AddRange((IEnumerable<int>)null));

            var col1 = new ReadOnlyCollection();

            Assert.Throws<NotSupportedException>(() => col1.AddRange(1));
        }
    }

    public class Clear
    {
        [Fact]
        public async void Normal()
        {
            var raiseCount = 0;
            var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4, 5, 6 });

            col.CollectionChanged += (s, e) =>
            {
                raiseCount++;
            };

            col.Clear();

            await Task.Delay(500);

            Assert.Equal(1, raiseCount);
            Assert.Empty(col);
        }

        [Fact]
        public async void WithConcurrency()
        {
            var raiseCount1 = 0;
            var raiseCount2 = 0;
            var col = new ObservableCollection<int>();

            col.ImmediateCollectionChanged += (s, e) =>
            {
                Interlocked.Increment(ref raiseCount1);
            };
            col.CollectionChanged += (s, e) =>
            {
                Interlocked.Increment(ref raiseCount2);
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

            await Task.WhenAny(
                Task.Delay(60000),
                Task.Run(async delegate
                {
                    while (20000 != raiseCount1 ||
                           20000 != raiseCount2)
                    {
                        await Task.Delay(500);
                    }
                    for (int i = 0; i < 60000; i++)
                    {
                        Assert.Contains(i, col);
                    }
                    col.Clear();
                }));

            await Task.Delay(500);

            Assert.Equal(20001, raiseCount1);
            Assert.Equal(20001, raiseCount2);
            Assert.Empty(col);
        }

        [Fact]
        public void Throws()
        {
            var col = new ReadOnlyCollection();

            Assert.Throws<NotSupportedException>(() => col.Clear());
        }
    }

    public class Contains
    {
        [Fact]
        public async void Normal()
        {
            var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

            await Task.Delay(500);

            Assert.True(col.Contains(1));
            Assert.True(col.Contains(2));
            Assert.True(col.Contains(3));
            Assert.True(col.Contains(4));
            Assert.False(col.Contains(5));
            Assert.False(col.Contains(6));
        }

        [Fact]
        public async void WithConcurrency()
        {
            var raiseCount1 = 0;
            var raiseCount2 = 0;
            var col = new ObservableCollection<int>();

            col.ImmediateCollectionChanged += (s, e) =>
            {
                Interlocked.Increment(ref raiseCount1);
            };
            col.CollectionChanged += (s, e) =>
            {
                Interlocked.Increment(ref raiseCount2);
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

    public class CopyTo
    {
        [Fact]
        public async void Normal()
        {
            var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });
            int[] clone1 = new int[4];
            col.CopyTo(clone1);

            await Task.Delay(500);

            Assert.Equal(1, clone1[0]);
            Assert.Equal(2, clone1[1]);
            Assert.Equal(3, clone1[2]);
            Assert.Equal(4, clone1[3]);
        }

        [Fact]
        public async void WithArrayIndex()
        {
            var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });
            int[] clone2 = new int[4];
            col.CopyTo(clone2, 0);

            await Task.Delay(500);

            Assert.Equal(1, clone2[0]);
            Assert.Equal(2, clone2[1]);
            Assert.Equal(3, clone2[2]);
            Assert.Equal(4, clone2[3]);
        }


        [Fact]
        public async void ArrayWithArrayIndex()
        {
            var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });
            Array clone3 = new int[6];
            col.CopyTo(clone3, 2);

            await Task.Delay(500);

            Assert.Equal(0, clone3.Cast<int>().ElementAt(0));
            Assert.Equal(0, clone3.Cast<int>().ElementAt(1));
            Assert.Equal(1, clone3.Cast<int>().ElementAt(2));
            Assert.Equal(2, clone3.Cast<int>().ElementAt(3));
            Assert.Equal(3, clone3.Cast<int>().ElementAt(4));
            Assert.Equal(4, clone3.Cast<int>().ElementAt(5));
        }

        [Fact]
        public async void WithArrayIndexAndSourceIndex()
        {
            var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });
            int[] clone4 = new int[6];
            col.CopyTo(2, clone4, 2, 2);

            await Task.Delay(500);

            Assert.Equal(0, clone4[0]);
            Assert.Equal(0, clone4[1]);
            Assert.Equal(3, clone4[2]);
            Assert.Equal(4, clone4[3]);
            Assert.Equal(0, clone4[4]);
            Assert.Equal(0, clone4[5]);
        }
    }

    public class GetEnumerator
    {
        [Fact]
        public async void ForLoop()
        {
            var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

            await Task.Delay(500);

            var enumerator = col.GetEnumerator();
            for (int i = 0; i < col.Count; i++)
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal(i + 1, enumerator.Current);
            }

            Assert.False(enumerator.MoveNext());
        }
    }

    public class IndexOf
    {
        [Fact]
        public async void Normal()
        {
            var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

            await Task.Delay(500);

            Assert.Equal(0, col.IndexOf(1));
            Assert.Equal(1, col.IndexOf(2));
            Assert.Equal(2, col.IndexOf(3));
            Assert.Equal(3, col.IndexOf(4));
        }
    }

    public class Insert
    {
        [Fact]
        public async void AtBottom()
        {
            var raiseCount = 0;
            var col = new ObservableCollection<int>();

            col.CollectionChanged += (s, e) =>
            {
                raiseCount++;
            };

            col.Insert(0, 1);
            col.Insert(0, 2);
            col.Insert(0, 3);
            col.Insert(0, 4);

            await Task.Delay(500);

            Assert.Equal(4, raiseCount);
            Assert.Equal(4, col.Count);

            Assert.Equal(4, col[0]);
            Assert.Equal(3, col[1]);
            Assert.Equal(2, col[2]);
            Assert.Equal(1, col[3]);
        }
        [Fact]
        public async void AtTop()
        {
            var raiseCount = 0;
            var col = new ObservableCollection<int>();

            col.CollectionChanged += (s, e) =>
            {
                raiseCount++;
            };

            col.Insert(col.Count, 1);
            col.Insert(col.Count, 2);
            col.Insert(col.Count, 3);
            col.Insert(col.Count, 4);

            await Task.Delay(500);

            Assert.Equal(4, raiseCount);
            Assert.Equal(4, col.Count);

            Assert.Equal(1, col[0]);
            Assert.Equal(2, col[1]);
            Assert.Equal(3, col[2]);
            Assert.Equal(4, col[3]);
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
        public async void AtBottom()
        {
            var raiseCount = 0;
            var col = new ObservableCollection<int>();

            col.CollectionChanged += (s, e) =>
            {
                raiseCount++;
            };

            col.InsertRange(0, 1, 2);
            col.InsertRange(0, new int[] { 3, 4 } as IEnumerable<int>);

            await Task.Delay(500);

            Assert.Equal(2, raiseCount);
            Assert.Equal(4, col.Count);

            Assert.Equal(3, col[0]);
            Assert.Equal(4, col[1]);
            Assert.Equal(1, col[2]);
            Assert.Equal(2, col[3]);
        }
        [Fact]
        public async void AtTop()
        {
            var raiseCount = 0;
            var col = new ObservableCollection<int>();

            col.CollectionChanged += (s, e) =>
            {
                raiseCount++;
            };

            col.InsertRange(col.Count, 1, 2);
            col.InsertRange(col.Count, new int[] { 3, 4 } as IEnumerable<int>);

            await Task.Delay(500);

            Assert.Equal(2, raiseCount);
            Assert.Equal(4, col.Count);

            Assert.Equal(1, col[0]);
            Assert.Equal(2, col[1]);
            Assert.Equal(3, col[2]);
            Assert.Equal(4, col[3]);
        }

        [Fact]
        public void Throws()
        {
            var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

            Assert.Throws<ArgumentNullException>(() => col.InsertRange(4, null));
            Assert.Throws<ArgumentNullException>(() => col.InsertRange(4, (IEnumerable<int>)null));
            Assert.Throws<ArgumentOutOfRangeException>(() => col.InsertRange(5, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => col.InsertRange(-1, 1));

            var col1 = new ReadOnlyCollection();

            Assert.Throws<NotSupportedException>(() => col1.InsertRange(0, 1));
        }
    }

    public class Move
    {
        [Fact]
        public async void Normal()
        {
            var raiseCount = 0;
            var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

            col.CollectionChanged += (s, e) =>
            {
                raiseCount++;
            };

            col.Move(3, 0);
            col.Move(3, 1);
            col.Move(3, 2);

            await Task.Delay(500);

            Assert.Equal(3, raiseCount);
            Assert.Equal(4, col.Count);

            Assert.Equal(4, col[0]);
            Assert.Equal(3, col[1]);
            Assert.Equal(2, col[2]);
            Assert.Equal(1, col[3]);
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

    public class ObservableFilter
    {
        [Fact]
        public async void Normal()
        {
            var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });
            var col2 = col.ObservableFilter(i => i % 2 == 0);

            await Task.Delay(500);

            Assert.Equal(2, col2.Count);

            Assert.Equal(2, col2[0]);
            Assert.Equal(4, col2[1]);
        }

        [Fact]
        public void Throws()
        {
            var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });
            var col2 = col.ObservableFilter(i => i % 2 == 0);

            Assert.Throws<ArgumentNullException>(() => col2.ObservableFilter(null));
        }
    }

    public class Remove
    {
        [Fact]
        public async void Normal()
        {
            var raiseCount = 0;
            var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

            col.CollectionChanged += (s, e) =>
            {
                raiseCount++;
            };

            col.Remove(1);
            col.Remove(3);

            await Task.Delay(500);

            Assert.Equal(2, raiseCount);
            Assert.Equal(2, col.Count);

            Assert.Equal(2, col[0]);
            Assert.Equal(4, col[1]);
        }

        [Fact]
        public async void All()
        {
            var raiseCount = 0;
            var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

            col.CollectionChanged += (s, e) =>
            {
                raiseCount++;
            };

            col.Remove(1);
            col.Remove(2);
            col.Remove(3);
            col.Remove(4);

            await Task.Delay(500);

            Assert.Equal(4, raiseCount);
            Assert.Empty(col);
        }

        [Fact]
        public void Throws()
        {
            var col1 = new ReadOnlyCollection();

            Assert.Throws<NotSupportedException>(() => col1.Remove(1));
        }
    }

    public class RemoveAt
    {
        [Fact]
        public async void Normal()
        {
            var raiseCount = 0;
            var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

            col.CollectionChanged += (s, e) =>
            {
                raiseCount++;
            };

            col.RemoveAt(0);
            col.RemoveAt(1);

            await Task.Delay(500);

            Assert.Equal(2, raiseCount);
            Assert.Equal(2, col.Count);

            Assert.Equal(2, col[0]);
            Assert.Equal(4, col[1]);
        }

        [Fact]
        public async void All()
        {
            var raiseCount = 0;
            var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

            col.CollectionChanged += (s, e) =>
            {
                raiseCount++;
            };

            col.RemoveAt(0);
            col.RemoveAt(0);
            col.RemoveAt(0);
            col.RemoveAt(0);

            await Task.Delay(500);

            Assert.Equal(4, raiseCount);
            Assert.Empty(col);
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

    public class RemoveRange
    {
        [Fact]
        public async void Normal()
        {
            var raiseCount = 0;
            var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

            col.CollectionChanged += (s, e) =>
            {
                raiseCount++;
            };

            col.RemoveRange(2, 2);

            await Task.Delay(500);

            Assert.Equal(1, raiseCount);
            Assert.Equal(2, col.Count);

            Assert.Equal(1, col[0]);
            Assert.Equal(2, col[1]);
        }

        [Fact]
        public async void All()
        {
            var raiseCount = 0;
            var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

            col.CollectionChanged += (s, e) =>
            {
                raiseCount++;
            };

            col.RemoveRange(0, 4);

            await Task.Delay(500);

            Assert.Equal(1, raiseCount);
            Assert.Empty(col);
        }

        [Fact]
        public void Throws()
        {
            var col = new ObservableCollection<int>(new int[] { 1, 2, 3, 4 });

            Assert.Throws<ArgumentOutOfRangeException>(() => col.RemoveRange(-1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => col.RemoveRange(0, 0));
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

    public class Lock : ObservableCollection<int>
    {
        public Lock()
        {

        }

        [Fact]
        public async void Normal()
        {
            var col = new Lock();
            bool isReadLocked = false;
            bool isWriteLocked = false;

            await Task.WhenAny(
                Task.Delay(60000),
                Task.WhenAll(
                    Task.Run(async delegate
                    {
                        await Task.Delay(500);
                        col.LockRead(() =>
                        {
                            Thread.Sleep(100);
                            Assert.False(isWriteLocked);
                        });
                    }),
                    Task.Run(delegate
                    {
                        col.LockWrite(() =>
                        {
                            isWriteLocked = true;
                            Thread.Sleep(1000);
                        });
                        isWriteLocked = false;
                    })));

            Assert.False(isReadLocked);
            Assert.False(isWriteLocked);
        }

        [Fact]
        public async void WithReturn()
        {
            var col = new Lock();
            bool isReadLocked = false;
            bool isWriteLocked = false;

            await Task.WhenAny(
                Task.Delay(60000),
                Task.WhenAll(
                    Task.Run(async delegate
                    {
                        await Task.Delay(500);
                        isReadLocked = col.LockRead(() =>
                        {
                            isReadLocked = true;
                            Thread.Sleep(100);
                            Assert.False(isWriteLocked);
                            return false;
                        });
                        Assert.False(isReadLocked);
                    }),
                    Task.Run(delegate
                    {
                        isWriteLocked = col.LockWrite(() =>
                        {
                            isWriteLocked = true;
                            Thread.Sleep(1000);
                            return false;
                        });
                        Assert.False(isWriteLocked);
                    })));

            Assert.False(isReadLocked);
            Assert.False(isWriteLocked);
        }

        [Fact]
        public void Throws()
        {
            var col = new Lock();
            col.AddRange(new int[] { 1, 2, 3, 4 });

            Assert.Throws<ArgumentNullException>(() => col.LockRead(null));
            Assert.Throws<ArgumentNullException>(() => col.LockRead((Func<int>)null));
            Assert.Throws<ArgumentNullException>(() => col.LockWrite(null));
            Assert.Throws<ArgumentNullException>(() => col.LockWrite((Func<int>)null));
        }
    }
}
