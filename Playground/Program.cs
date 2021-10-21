using ObservableHelpers;
using System.Collections.Concurrent;
using System.Diagnostics;

//ObservableCollection<int> s = new ObservableCollection<int>();
//s.ImmediateCollectionChanged += (s, e) =>
//{

//};
//s.Add(1);
//s.Add(2);
//s.Add(3);
//s.AddRange(99, 999, 9999);
//s.Insert(0, 10);
//s.Insert(0, 11);
//s.InsertRange(0, 12, 13, 14);
//s.RemoveAt(0);
//s.RemoveAt(0);
//s.Remove(1);
//s.Remove(9999);
//s.Clear();

//ObservableDictionary<string, int> d = new ObservableDictionary<string, int>();
//d.ImmediateCollectionChanged += (s, e) =>
//{

//};
//d.Add("1", 1);
//d.Add("2", 2);
//d.Add("3", 3);
//d.Remove("1");
//d.AddOrUpdate("2", 222);
//d.AddOrUpdate("2", 2222);

var list = new List<int>();

list[0] = 1;

Console.WriteLine("");