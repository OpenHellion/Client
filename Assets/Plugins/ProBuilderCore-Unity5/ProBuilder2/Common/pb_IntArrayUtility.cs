using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace ProBuilder2.Common
{
	public static class pb_IntArrayUtility
	{
		[CompilerGenerated]
		private sealed class _003CGetIndicesWithCommon_003Ec__AnonStorey0
		{
			internal pb_IntArray[] pbIntArr;

			internal int _003C_003Em__0(int x)
			{
				return pbIntArr[x][0];
			}
		}

		[CompilerGenerated]
		private static Func<KeyValuePair<int, int>, bool> _003C_003Ef__am_0024cache0;

		public static int[][] ToArray(this pb_IntArray[] val)
		{
			int[][] array = new int[val.Length][];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = val[i].array;
			}
			return array;
		}

		public static Dictionary<int, int> ToDictionary(this pb_IntArray[] array)
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			for (int i = 0; i < array.Length; i++)
			{
				for (int j = 0; j < array[i].array.Length; j++)
				{
					if (!dictionary.ContainsKey(array[i][j]))
					{
						dictionary.Add(array[i][j], i);
					}
				}
			}
			return dictionary;
		}

		public static pb_IntArray[] ToSharedIndices(this IEnumerable<KeyValuePair<int, int>> lookup)
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			List<List<int>> list = new List<List<int>>();
			foreach (KeyValuePair<int, int> item in lookup)
			{
				if (item.Value < 0)
				{
					list.Add(new List<int> { item.Key });
					continue;
				}
				int value = -1;
				if (dictionary.TryGetValue(item.Value, out value))
				{
					list[value].Add(item.Key);
					continue;
				}
				dictionary.Add(item.Value, list.Count);
				list.Add(new List<int> { item.Key });
			}
			return list.ToPbIntArray();
		}

		public static pb_IntArray[] ToPbIntArray(this int[][] val)
		{
			pb_IntArray[] array = new pb_IntArray[val.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = (pb_IntArray)val[i];
			}
			return array;
		}

		public static pb_IntArray[] ToPbIntArray(this List<List<int>> val)
		{
			pb_IntArray[] array = new pb_IntArray[val.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = (pb_IntArray)val[i].ToArray();
			}
			return array;
		}

		public static List<List<int>> ToList(this pb_IntArray[] val)
		{
			List<List<int>> list = new List<List<int>>();
			for (int i = 0; i < val.Length; i++)
			{
				list.Add(val[i].ToList());
			}
			return list;
		}

		public static string ToFormattedString(this pb_IntArray[] arr)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < arr.Length; i++)
			{
				stringBuilder.Append("[" + pbUtil.ToString(arr[i].array) + "] ");
			}
			return stringBuilder.ToString();
		}

		public static int IndexOf(this int[] array, int val, pb_IntArray[] sharedIndices)
		{
			int num = sharedIndices.IndexOf(val);
			if (num < 0)
			{
				return -1;
			}
			int[] array2 = sharedIndices[num];
			for (int i = 0; i < array.Length; i++)
			{
				if (Array.IndexOf(array2, array[i]) > -1)
				{
					return i;
				}
			}
			return -1;
		}

		public static int IndexOf(this IList<int> indices, int triangle, ref Dictionary<int, int> lookup)
		{
			int num = lookup[triangle];
			if (num < 0)
			{
				return -1;
			}
			int num2 = indices.Count();
			for (int i = 0; i < num2; i++)
			{
				if (lookup[indices[i]] == num)
				{
					return i;
				}
			}
			return -1;
		}

		public static int IndexOf(this pb_IntArray[] intArray, int index)
		{
			if (intArray == null)
			{
				return -1;
			}
			for (int i = 0; i < intArray.Length; i++)
			{
				for (int j = 0; j < intArray[i].Length; j++)
				{
					if (intArray[i][j] == index)
					{
						return i;
					}
				}
			}
			return -1;
		}

		public static IList<int> AllIndicesWithValues(this pb_IntArray[] pbIntArr, IList<int> indices)
		{
			int[] array = pbIntArr.GetCommonIndices(indices).ToArray();
			List<int> list = new List<int>();
			for (int i = 0; i < array.Length; i++)
			{
				list.AddRange(pbIntArr[array[i]].array);
			}
			return list;
		}

		public static IList<int> AllIndicesWithValues(this pb_IntArray[] pbIntArr, Dictionary<int, int> lookup, IList<int> indices)
		{
			int[] array = GetCommonIndices(lookup, indices).ToArray();
			List<int> list = new List<int>();
			for (int i = 0; i < array.Length; i++)
			{
				list.AddRange(pbIntArr[array[i]].array);
			}
			return list;
		}

		public static IList<int> UniqueIndicesWithValues(this pb_IntArray[] pbIntArr, IList<int> indices)
		{
			Dictionary<int, int> dictionary = pbIntArr.ToDictionary();
			HashSet<int> hashSet = new HashSet<int>();
			foreach (int index in indices)
			{
				hashSet.Add(dictionary[index]);
			}
			List<int> list = new List<int>();
			foreach (int item in hashSet)
			{
				list.Add(pbIntArr[item][0]);
			}
			return list;
		}

		public static HashSet<int> GetCommonIndices(this pb_IntArray[] pbIntArr, IList<int> indices)
		{
			return GetCommonIndices(pbIntArr.ToDictionary(), indices);
		}

		public static HashSet<int> GetCommonIndices(Dictionary<int, int> lookup, IList<int> indices)
		{
			HashSet<int> hashSet = new HashSet<int>();
			foreach (int index in indices)
			{
				hashSet.Add(lookup[index]);
			}
			return hashSet;
		}

		public static IEnumerable<int> GetIndicesWithCommon(this pb_IntArray[] pbIntArr, IEnumerable<int> common)
		{
			_003CGetIndicesWithCommon_003Ec__AnonStorey0 _003CGetIndicesWithCommon_003Ec__AnonStorey = new _003CGetIndicesWithCommon_003Ec__AnonStorey0();
			_003CGetIndicesWithCommon_003Ec__AnonStorey.pbIntArr = pbIntArr;
			return common.Select(_003CGetIndicesWithCommon_003Ec__AnonStorey._003C_003Em__0);
		}

		public static pb_IntArray[] ExtractSharedIndices(Vector3[] v)
		{
			Dictionary<pb_IntVec3, List<int>> dictionary = new Dictionary<pb_IntVec3, List<int>>();
			for (int i = 0; i < v.Length; i++)
			{
				if (dictionary.TryGetValue(v[i], out var value))
				{
					value.Add(i);
					continue;
				}
				dictionary.Add(new pb_IntVec3(v[i]), new List<int> { i });
			}
			pb_IntArray[] array = new pb_IntArray[dictionary.Count];
			int num = 0;
			foreach (KeyValuePair<pb_IntVec3, List<int>> item in dictionary)
			{
				array[num++] = new pb_IntArray(item.Value.ToArray());
			}
			return array;
		}

		public static int MergeSharedIndices(ref pb_IntArray[] sharedIndices, int[] indices)
		{
			if (indices.Length < 2)
			{
				return -1;
			}
			if (sharedIndices == null)
			{
				sharedIndices = new pb_IntArray[1] { (pb_IntArray)indices };
				return 0;
			}
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			for (int i = 0; i < indices.Length; i++)
			{
				int num = sharedIndices.IndexOf(indices[i]);
				if (!list.Contains(num))
				{
					if (num > -1)
					{
						list2.AddRange(sharedIndices[num].array);
						list.Add(num);
					}
					else
					{
						list2.Add(indices[i]);
					}
				}
			}
			int num2 = sharedIndices.Length - list.Count;
			pb_IntArray[] array = new pb_IntArray[num2];
			int num3 = 0;
			for (int j = 0; j < sharedIndices.Length; j++)
			{
				if (!list.Contains(j))
				{
					array[num3++] = sharedIndices[j];
				}
			}
			sharedIndices = array.Add(new pb_IntArray(list2.ToArray()));
			return sharedIndices.Length - 1;
		}

		public static void MergeSharedIndices(ref pb_IntArray[] sharedIndices, int a, int b)
		{
			int sharedIndex = sharedIndices.IndexOf(a);
			int num = sharedIndices.IndexOf(b);
			AddValueAtIndex(ref sharedIndices, sharedIndex, b);
			int[] array = sharedIndices[num].array;
			sharedIndices[num].array = array.RemoveAt(Array.IndexOf(array, b));
			pb_IntArray.RemoveEmptyOrNull(ref sharedIndices);
		}

		public static int AddValueAtIndex(ref pb_IntArray[] sharedIndices, int sharedIndex, int value)
		{
			if (sharedIndex > -1)
			{
				sharedIndices[sharedIndex].array = sharedIndices[sharedIndex].array.Add(value);
			}
			else
			{
				sharedIndices = sharedIndices.Add(new pb_IntArray(new int[1] { value }));
			}
			return (sharedIndex <= -1) ? (sharedIndices.Length - 1) : sharedIndex;
		}

		public static int AddRangeAtIndex(ref pb_IntArray[] sharedIndices, int sharedIndex, int[] indices)
		{
			if (sharedIndex > -1)
			{
				sharedIndices[sharedIndex].array = sharedIndices[sharedIndex].array.AddRange(indices);
			}
			else
			{
				sharedIndices = sharedIndices.Add(new pb_IntArray(indices));
			}
			return (sharedIndex <= -1) ? (sharedIndices.Length - 1) : sharedIndex;
		}

		public static void RemoveValues(ref pb_IntArray[] sharedIndices, int[] remove)
		{
			for (int i = 0; i < sharedIndices.Length; i++)
			{
				for (int j = 0; j < remove.Length; j++)
				{
					int num = Array.IndexOf(sharedIndices[i], remove[j]);
					if (num > -1)
					{
						sharedIndices[i].array = sharedIndices[i].array.RemoveAt(num);
					}
				}
			}
			pb_IntArray.RemoveEmptyOrNull(ref sharedIndices);
		}

		public static void RemoveValuesAndShift(ref pb_IntArray[] sharedIndices, IEnumerable<int> remove)
		{
			Dictionary<int, int> dictionary = sharedIndices.ToDictionary();
			foreach (int item in remove)
			{
				dictionary[item] = -1;
			}
			if (_003C_003Ef__am_0024cache0 == null)
			{
				_003C_003Ef__am_0024cache0 = _003CRemoveValuesAndShift_003Em__0;
			}
			sharedIndices = dictionary.Where(_003C_003Ef__am_0024cache0).ToSharedIndices();
			List<int> list = new List<int>(remove);
			list.Sort();
			for (int i = 0; i < sharedIndices.Length; i++)
			{
				for (int j = 0; j < sharedIndices[i].Length; j++)
				{
					int num = pbUtil.NearestIndexPriorToValue(list, sharedIndices[i][j]);
					sharedIndices[i][j] -= num + 1;
				}
			}
		}

		[CompilerGenerated]
		private static bool _003CRemoveValuesAndShift_003Em__0(KeyValuePair<int, int> x)
		{
			return x.Value > -1;
		}
	}
}
