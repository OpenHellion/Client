using System;
using System.Collections.Generic;

namespace ProBuilder2.Common
{
	[Serializable]
	public class pb_IntArray
	{
		public int[] array;

		public int this[int i]
		{
			get
			{
				return array[i];
			}
			set
			{
				array[i] = value;
			}
		}

		public int Length => array.Length;

		public pb_IntArray(int[] intArray)
		{
			array = intArray;
		}

		public pb_IntArray(pb_IntArray intArray)
		{
			array = intArray.array;
		}

		public List<int> ToList()
		{
			return new List<int>(array);
		}

		public static implicit operator int[](pb_IntArray intArr)
		{
			return intArr.array;
		}

		public static explicit operator pb_IntArray(int[] arr)
		{
			return new pb_IntArray(arr);
		}

		public override string ToString()
		{
			return array.ToString(",");
		}

		public bool IsEmpty()
		{
			return array == null || array.Length < 1;
		}

		public static void RemoveEmptyOrNull(ref pb_IntArray[] val)
		{
			List<pb_IntArray> list = new List<pb_IntArray>();
			pb_IntArray[] array = val;
			foreach (pb_IntArray pb_IntArray2 in array)
			{
				if (pb_IntArray2 != null && !pb_IntArray2.IsEmpty())
				{
					list.Add(pb_IntArray2);
				}
			}
			val = list.ToArray();
		}
	}
}
