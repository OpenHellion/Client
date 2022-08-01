using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ZeroGravity.Data
{
	public static class ObjectCopier
	{
		[CompilerGenerated]
		private sealed class _003CDeepCopy_003Ec__AnonStorey0<T>
		{
			internal T source;

			internal IEnumerable<int> _003C_003Em__0(int x)
			{
				return Enumerable.Range((source as Array).GetLowerBound(x), (source as Array).GetUpperBound(x) - (source as Array).GetLowerBound(x) + 1);
			}
		}

		[CompilerGenerated]
		private sealed class _003CCartesianProduct_003Ec__AnonStorey1<T>
		{
			internal IEnumerable<T> sequence;

			internal IEnumerable<T> _003C_003Em__0(IEnumerable<T> accseq)
			{
				return sequence;
			}
		}

		public static T Copy<T>(T source)
		{
			return DeepCopy(source, 0);
		}

		public static T DeepCopy<T>(T source, int depth = 10)
		{
			_003CDeepCopy_003Ec__AnonStorey0<T> _003CDeepCopy_003Ec__AnonStorey = new _003CDeepCopy_003Ec__AnonStorey0<T>();
			_003CDeepCopy_003Ec__AnonStorey.source = source;
			if (_003CDeepCopy_003Ec__AnonStorey.source == null || depth < 0)
			{
				return _003CDeepCopy_003Ec__AnonStorey.source;
			}
			Type type = _003CDeepCopy_003Ec__AnonStorey.source.GetType();
			if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal))
			{
				return _003CDeepCopy_003Ec__AnonStorey.source;
			}
			T val;
			if (_003CDeepCopy_003Ec__AnonStorey.source is Array)
			{
				object[] array = new object[type.GetArrayRank()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = (_003CDeepCopy_003Ec__AnonStorey.source as Array).GetLength(i);
				}
				val = (T)Activator.CreateInstance(type, array);
				IEnumerable<IEnumerable<int>> sequences = Enumerable.Range(0, (_003CDeepCopy_003Ec__AnonStorey.source as Array).Rank).Select(_003CDeepCopy_003Ec__AnonStorey._003C_003Em__0);
				{
					foreach (IEnumerable<int> item in sequences.CartesianProduct())
					{
						int[] indices = item.ToArray();
						(val as Array).SetValue(DeepCopy((_003CDeepCopy_003Ec__AnonStorey.source as Array).GetValue(indices), depth - 1), indices);
					}
					return val;
				}
			}
			val = (T)Activator.CreateInstance(type);
			if (_003CDeepCopy_003Ec__AnonStorey.source is IDictionary)
			{
				IEnumerator enumerator2 = (_003CDeepCopy_003Ec__AnonStorey.source as IDictionary).Keys.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						object current2 = enumerator2.Current;
						(val as IDictionary)[current2] = DeepCopy((_003CDeepCopy_003Ec__AnonStorey.source as IDictionary)[current2], depth - 1);
					}
					return val;
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = enumerator2 as IDisposable) != null)
					{
						disposable.Dispose();
					}
				}
			}
			if (type.IsGenericType && _003CDeepCopy_003Ec__AnonStorey.source is IEnumerable && type.GetGenericArguments().Length == 1)
			{
				MethodInfo method = type.GetMethod("Add");
				IEnumerator enumerator3 = (_003CDeepCopy_003Ec__AnonStorey.source as IEnumerable).GetEnumerator();
				try
				{
					while (enumerator3.MoveNext())
					{
						object current3 = enumerator3.Current;
						method.Invoke(val, new object[1] { DeepCopy(current3, depth - 1) });
					}
					return val;
				}
				finally
				{
					IDisposable disposable2;
					if ((disposable2 = enumerator3 as IDisposable) != null)
					{
						disposable2.Dispose();
					}
				}
			}
			FieldInfo[] fields = type.GetFields();
			FieldInfo[] array2 = fields;
			foreach (FieldInfo fieldInfo in array2)
			{
				object value = fieldInfo.GetValue(_003CDeepCopy_003Ec__AnonStorey.source);
				fieldInfo.SetValue(val, DeepCopy(value, depth - 1));
			}
			PropertyInfo[] properties = type.GetProperties();
			PropertyInfo[] array3 = properties;
			foreach (PropertyInfo propertyInfo in array3)
			{
				if (propertyInfo.CanRead && propertyInfo.CanWrite && propertyInfo.GetIndexParameters().Length == 0)
				{
					object value2 = propertyInfo.GetValue(_003CDeepCopy_003Ec__AnonStorey.source, null);
					propertyInfo.SetValue(val, DeepCopy(value2, depth - 1), null);
				}
			}
			return val;
		}

		public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
		{
			IEnumerable<IEnumerable<T>> seed = new IEnumerable<T>[1] { Enumerable.Empty<T>() };
			return sequences.Aggregate(seed, _003CCartesianProduct_00601_003Em__0);
		}

		[CompilerGenerated]
		private static IEnumerable<IEnumerable<T>> _003CCartesianProduct_00601_003Em__0<T>(IEnumerable<IEnumerable<T>> accumulator, IEnumerable<T> sequence)
		{
			_003CCartesianProduct_003Ec__AnonStorey1<T> _003CCartesianProduct_003Ec__AnonStorey = new _003CCartesianProduct_003Ec__AnonStorey1<T>();
			_003CCartesianProduct_003Ec__AnonStorey.sequence = sequence;
			return accumulator.SelectMany(_003CCartesianProduct_003Ec__AnonStorey._003C_003Em__0, _003CCartesianProduct_00601_003Em__1);
		}

		[CompilerGenerated]
		private static IEnumerable<T> _003CCartesianProduct_00601_003Em__1<T>(IEnumerable<T> accseq, T item)
		{
			return accseq.Concat(new T[1] { item });
		}
	}
}
