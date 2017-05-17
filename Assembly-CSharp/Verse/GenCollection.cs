using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Verse
{
	public static class GenCollection
	{
		public static bool SharesElementWith<T>(this IEnumerable<T> source, IEnumerable<T> other)
		{
			return source.Any((T item) => other.Contains(item));
		}

		[DebuggerHidden]
		public static IEnumerable<T> InRandomOrder<T>(this IEnumerable<T> source, IList<T> workingList = null)
		{
			GenCollection.<InRandomOrder>c__Iterator248<T> <InRandomOrder>c__Iterator = new GenCollection.<InRandomOrder>c__Iterator248<T>();
			<InRandomOrder>c__Iterator.source = source;
			<InRandomOrder>c__Iterator.workingList = workingList;
			<InRandomOrder>c__Iterator.<$>source = source;
			<InRandomOrder>c__Iterator.<$>workingList = workingList;
			GenCollection.<InRandomOrder>c__Iterator248<T> expr_23 = <InRandomOrder>c__Iterator;
			expr_23.$PC = -2;
			return expr_23;
		}

		public static T RandomElement<T>(this IEnumerable<T> source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			IList<T> list = source as IList<T>;
			if (list == null)
			{
				list = source.ToList<T>();
			}
			if (list.Count == 0)
			{
				Log.Warning("Getting random element from empty collection.");
				return default(T);
			}
			return list[Rand.Range(0, list.Count)];
		}

		public static T RandomElementWithFallback<T>(this IEnumerable<T> source, T fallback = null)
		{
			T result;
			if (source.TryRandomElement(out result))
			{
				return result;
			}
			return fallback;
		}

		public static bool TryRandomElement<T>(this IEnumerable<T> source, out T result)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			IList<T> list = source as IList<T>;
			if (list != null)
			{
				if (list.Count == 0)
				{
					result = default(T);
					return false;
				}
			}
			else if (!source.Any<T>())
			{
				result = default(T);
				return false;
			}
			result = source.RandomElement<T>();
			return true;
		}

		public static T RandomElementByWeight<T>(this IEnumerable<T> source, Func<T, float> weightSelector)
		{
			float num = 0f;
			IList<T> list = source as IList<T>;
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					float num2 = weightSelector(list[i]);
					if (num2 < 0f)
					{
						Log.Error(string.Concat(new object[]
						{
							"Negative weight in selector: ",
							num2,
							" from ",
							list[i]
						}));
						num2 = 0f;
					}
					num += num2;
				}
				if (list.Count == 1 && num > 0f)
				{
					return list[0];
				}
			}
			else
			{
				int num3 = 0;
				foreach (T current in source)
				{
					num3++;
					float num4 = weightSelector(current);
					if (num4 < 0f)
					{
						Log.Error(string.Concat(new object[]
						{
							"Negative weight in selector: ",
							num4,
							" from ",
							current
						}));
						num4 = 0f;
					}
					num += num4;
				}
				if (num3 == 1 && num > 0f)
				{
					return source.First<T>();
				}
			}
			if (num <= 0f)
			{
				Log.Error("RandomElementByWeight with totalWeight=" + num + " - use TryRandomElementByWeight.");
				return default(T);
			}
			float num5 = Rand.Value * num;
			float num6 = 0f;
			if (list != null)
			{
				for (int j = 0; j < list.Count; j++)
				{
					float num7 = weightSelector(list[j]);
					if (num7 > 0f)
					{
						num6 += num7;
						if (num6 >= num5)
						{
							return list[j];
						}
					}
				}
			}
			else
			{
				foreach (T current2 in source)
				{
					float num8 = weightSelector(current2);
					if (num8 > 0f)
					{
						num6 += num8;
						if (num6 >= num5)
						{
							return current2;
						}
					}
				}
			}
			return default(T);
		}

		public static T RandomElementByWeightWithFallback<T>(this IEnumerable<T> source, Func<T, float> weightSelector, T fallback = null)
		{
			T result;
			if (source.TryRandomElementByWeight(weightSelector, out result))
			{
				return result;
			}
			return fallback;
		}

		public static bool TryRandomElementByWeight<T>(this IEnumerable<T> source, Func<T, float> weightSelector, out T result)
		{
			float num = 0f;
			IList<T> list = source as IList<T>;
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					float num2 = weightSelector(list[i]);
					if (num2 < 0f)
					{
						Log.Error(string.Concat(new object[]
						{
							"Negative weight in selector: ",
							num2,
							" from ",
							list[i]
						}));
						num2 = 0f;
					}
					num += num2;
				}
				if (list.Count == 1 && num > 0f)
				{
					result = list[0];
					return true;
				}
			}
			else
			{
				int num3 = 0;
				foreach (T current in source)
				{
					num3++;
					float num4 = weightSelector(current);
					if (num4 < 0f)
					{
						Log.Error(string.Concat(new object[]
						{
							"Negative weight in selector: ",
							num4,
							" from ",
							current
						}));
						num4 = 0f;
					}
					num += num4;
				}
				if (num3 == 1 && num > 0f)
				{
					result = source.First<T>();
					return true;
				}
			}
			if (num <= 0f)
			{
				result = default(T);
				return false;
			}
			float num5 = Rand.Value * num;
			float num6 = 0f;
			if (list != null)
			{
				for (int j = 0; j < list.Count; j++)
				{
					float num7 = weightSelector(list[j]);
					if (num7 > 0f)
					{
						num6 += num7;
						if (num6 >= num5)
						{
							result = list[j];
							return true;
						}
					}
				}
			}
			else
			{
				foreach (T current2 in source)
				{
					float num8 = weightSelector(current2);
					if (num8 > 0f)
					{
						num6 += num8;
						if (num6 >= num5)
						{
							result = current2;
							return true;
						}
					}
				}
			}
			result = default(T);
			return false;
		}

		public static T RandomElementByWeightWithDefault<T>(this IEnumerable<T> source, Func<T, float> weightSelector, float defaultValueWeight)
		{
			if (defaultValueWeight < 0f)
			{
				Log.Error("Negative default value weight.");
				defaultValueWeight = 0f;
			}
			float num = 0f;
			foreach (T current in source)
			{
				float num2 = weightSelector(current);
				if (num2 < 0f)
				{
					Log.Error(string.Concat(new object[]
					{
						"Negative weight in selector: ",
						num2,
						" from ",
						current
					}));
					num2 = 0f;
				}
				num += num2;
			}
			float num3 = defaultValueWeight + num;
			if (num3 <= 0f)
			{
				Log.Error("RandomElementByWeightWithDefault with totalWeight=" + num3);
				return default(T);
			}
			if (Rand.Value < defaultValueWeight / num3 || num == 0f)
			{
				return default(T);
			}
			return source.RandomElementByWeight(weightSelector);
		}

		public static T RandomEnumValue<T>()
		{
			return Enum.GetValues(typeof(T)).Cast<T>().RandomElement<T>();
		}

		public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
		{
			return source.MaxBy(selector, Comparer<TKey>.Default);
		}

		public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (selector == null)
			{
				throw new ArgumentNullException("selector");
			}
			if (comparer == null)
			{
				throw new ArgumentNullException("comparer");
			}
			TSource result;
			using (IEnumerator<TSource> enumerator = source.GetEnumerator())
			{
				if (!enumerator.MoveNext())
				{
					throw new InvalidOperationException("Sequence contains no elements");
				}
				TSource tSource = enumerator.Current;
				TKey y = selector(tSource);
				while (enumerator.MoveNext())
				{
					TSource current = enumerator.Current;
					TKey tKey = selector(current);
					if (comparer.Compare(tKey, y) > 0)
					{
						tSource = current;
						y = tKey;
					}
				}
				result = tSource;
			}
			return result;
		}

		public static TSource MaxByWithFallback<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, TSource fallback = null)
		{
			return source.MaxByWithFallback(selector, Comparer<TKey>.Default, fallback);
		}

		public static TSource MaxByWithFallback<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer, TSource fallback = null)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (selector == null)
			{
				throw new ArgumentNullException("selector");
			}
			if (comparer == null)
			{
				throw new ArgumentNullException("comparer");
			}
			TSource result;
			using (IEnumerator<TSource> enumerator = source.GetEnumerator())
			{
				if (!enumerator.MoveNext())
				{
					result = fallback;
				}
				else
				{
					TSource tSource = enumerator.Current;
					TKey y = selector(tSource);
					while (enumerator.MoveNext())
					{
						TSource current = enumerator.Current;
						TKey tKey = selector(current);
						if (comparer.Compare(tKey, y) > 0)
						{
							tSource = current;
							y = tKey;
						}
					}
					result = tSource;
				}
			}
			return result;
		}

		public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
		{
			return source.MinBy(selector, Comparer<TKey>.Default);
		}

		public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (selector == null)
			{
				throw new ArgumentNullException("selector");
			}
			if (comparer == null)
			{
				throw new ArgumentNullException("comparer");
			}
			TSource result;
			using (IEnumerator<TSource> enumerator = source.GetEnumerator())
			{
				if (!enumerator.MoveNext())
				{
					throw new InvalidOperationException("Sequence contains no elements");
				}
				TSource tSource = enumerator.Current;
				TKey y = selector(tSource);
				while (enumerator.MoveNext())
				{
					TSource current = enumerator.Current;
					TKey tKey = selector(current);
					if (comparer.Compare(tKey, y) < 0)
					{
						tSource = current;
						y = tKey;
					}
				}
				result = tSource;
			}
			return result;
		}

		public static void SortBy<T, TSortBy>(this List<T> list, Func<T, TSortBy> selector) where TSortBy : IComparable<TSortBy>
		{
			if (list.Count <= 1)
			{
				return;
			}
			list.Sort(delegate(T a, T b)
			{
				TSortBy tSortBy = selector(a);
				return tSortBy.CompareTo(selector(b));
			});
		}

		public static void SortBy<T, TSortBy, TThenBy>(this List<T> list, Func<T, TSortBy> selector, Func<T, TThenBy> thenBySelector) where TSortBy : IComparable<TSortBy>, IEquatable<TSortBy> where TThenBy : IComparable<TThenBy>
		{
			if (list.Count <= 1)
			{
				return;
			}
			list.Sort(delegate(T a, T b)
			{
				TSortBy tSortBy = selector(a);
				TSortBy other = selector(b);
				if (!tSortBy.Equals(other))
				{
					return tSortBy.CompareTo(other);
				}
				TThenBy tThenBy = thenBySelector(a);
				return tThenBy.CompareTo(thenBySelector(b));
			});
		}

		public static void SortByDescending<T, TSortByDescending>(this List<T> list, Func<T, TSortByDescending> selector) where TSortByDescending : IComparable<TSortByDescending>
		{
			if (list.Count <= 1)
			{
				return;
			}
			list.Sort(delegate(T a, T b)
			{
				TSortByDescending tSortByDescending = selector(b);
				return tSortByDescending.CompareTo(selector(a));
			});
		}

		public static void SortByDescending<T, TSortByDescending, TThenByDescending>(this List<T> list, Func<T, TSortByDescending> selector, Func<T, TThenByDescending> thenByDescendingSelector) where TSortByDescending : IComparable<TSortByDescending>, IEquatable<TSortByDescending> where TThenByDescending : IComparable<TThenByDescending>
		{
			if (list.Count <= 1)
			{
				return;
			}
			list.Sort(delegate(T a, T b)
			{
				TSortByDescending other = selector(a);
				TSortByDescending other2 = selector(b);
				if (!other.Equals(other2))
				{
					return other2.CompareTo(other);
				}
				TThenByDescending tThenByDescending = thenByDescendingSelector(b);
				return tThenByDescending.CompareTo(thenByDescendingSelector(a));
			});
		}

		public static int RemoveAll<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Predicate<KeyValuePair<TKey, TValue>> predicate)
		{
			List<TKey> list = null;
			int result;
			try
			{
				foreach (KeyValuePair<TKey, TValue> current in dictionary)
				{
					if (predicate(current))
					{
						if (list == null)
						{
							list = SimplePool<List<TKey>>.Get();
						}
						list.Add(current.Key);
					}
				}
				if (list != null)
				{
					int i = 0;
					int count = list.Count;
					while (i < count)
					{
						dictionary.Remove(list[i]);
						i++;
					}
					result = list.Count;
				}
				else
				{
					result = 0;
				}
			}
			finally
			{
				if (list != null)
				{
					list.Clear();
					SimplePool<List<TKey>>.Return(list);
				}
			}
			return result;
		}

		public static void RemoveAll<T>(this List<T> list, Func<T, int, bool> predicate)
		{
			int num = 0;
			int count = list.Count;
			while (num < count && !predicate(list[num], num))
			{
				num++;
			}
			if (num >= count)
			{
				return;
			}
			int i = num + 1;
			while (i < count)
			{
				while (i < count && predicate(list[i], i))
				{
					i++;
				}
				if (i < count)
				{
					list[num++] = list[i++];
				}
			}
		}

		public static void RemoveLast<T>(this List<T> list)
		{
			list.RemoveAt(list.Count - 1);
		}

		public static bool Any<T>(this List<T> list, Predicate<T> predicate)
		{
			return list.FindIndex(predicate) != -1;
		}

		public static bool Any<T>(this List<T> list)
		{
			return list.Count != 0;
		}

		public static void AddRange<T>(this HashSet<T> set, List<T> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				set.Add(list[i]);
			}
		}

		public static float AverageWeighted<T>(this IEnumerable<T> list, Func<T, float> weight, Func<T, float> value)
		{
			float num = 0f;
			float num2 = 0f;
			foreach (T current in list)
			{
				float num3 = weight(current);
				num += num3;
				num2 += value(current) * num3;
			}
			return num2 / num;
		}

		public static void ExecuteEnumerable(this IEnumerable enumerable)
		{
			foreach (object current in enumerable)
			{
			}
		}

		public static int FirstIndexOf<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
		{
			int num = 0;
			foreach (T current in enumerable)
			{
				if (predicate(current))
				{
					break;
				}
				num++;
			}
			return num;
		}
	}
}