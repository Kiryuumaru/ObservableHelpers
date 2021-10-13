using ObservableHelpers;
using System.Collections.Concurrent;
using System.Diagnostics;

//Stopwatch sw = new ();

//sw.Restart();

//var list1 = new ObservableCollection<int>();
//for (int i = 0; i < 1000000; i++) list1.Add(123);
//Console.WriteLine("list baseline = " + sw.Elapsed);

//sw.Stop();

//sw.Restart();

//var list2 = new Swordfish.NET.Collections.ConcurrentObservableCollection<int>();
//for (int i = 0; i < 1000000; i++) list2.Add(123);
//Console.WriteLine("list baseline = " + sw.Elapsed);

//sw.Stop();

ObservableCollection<int> s = new ObservableCollection<int>();
s.ImmediateCollectionChanged += (s, e) =>
{

};
s.Add(1);
s.Add(2);
s.Add(3);
s.AddRange(99, 999, 9999);
s.Insert(0, 10);
s.Insert(0, 11);
s.InsertRange(0, 12, 13, 14);
s.RemoveAt(0);
s.RemoveAt(0);
s.Remove(1);
s.Remove(9999);
s.Clear();

Console.WriteLine(s.Count);