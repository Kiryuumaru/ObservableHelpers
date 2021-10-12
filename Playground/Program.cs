using ObservableHelpers;
using System.Collections.Concurrent;
using System.Diagnostics;

Stopwatch sw = new ();

sw.Restart();

var list1 = new Swordfish.NET.Collections.ConcurrentObservableDictionary<int, int>();
for (int i = 0; i < 1000000; i++) list1.TryAdd(i, 123);
Console.WriteLine("list baseline = " + sw.Elapsed);

sw.Stop();

sw.Restart();

var list2 = new Swordfish.NET.Collections.ConcurrentObservableCollection<int>();
for (int i = 0; i < 1000000; i++) list2.Add(123);
Console.WriteLine("list baseline = " + sw.Elapsed);

sw.Stop();

ObservableCollection1<int> s = new ObservableCollection1<int>();