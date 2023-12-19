using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ZeroGravity.Data
{
	public static class ObjectCopier
	{
		public static T Copy<T>(T source)
		{
			return DeepCopy(source, 0);
		}

		public static T DeepCopy<T>(T source, int depth = 10)
		{
			if (source == null || depth < 0)
			{
				return source;
			}
			Type type = source.GetType();
			if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal))
			{
				return source;
			}
			T val;
			if (source is Array)
			{
				object[] array = new object[type.GetArrayRank()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = (source as Array).GetLength(i);
				}
				val = (T)Activator.CreateInstance(type, array);
				IEnumerable<IEnumerable<int>> sequences = from x in Enumerable.Range(0, (source as Array).Rank)
					select Enumerable.Range((source as Array).GetLowerBound(x), (source as Array).GetUpperBound(x) - (source as Array).GetLowerBound(x) + 1);
				foreach (IEnumerable<int> item in CartesianProduct(sequences))
				{
					int[] indices = item.ToArray();
					(val as Array).SetValue(DeepCopy((source as Array).GetValue(indices), depth - 1), indices);
				}
			}
			else
			{
				val = (T)Activator.CreateInstance(type);
				if (source is IDictionary)
				{
					foreach (object key in (source as IDictionary).Keys)
					{
						(val as IDictionary)[key] = DeepCopy((source as IDictionary)[key], depth - 1);
					}
				}
				else if (type.IsGenericType && source is IEnumerable && type.GetGenericArguments().Length == 1)
				{
					MethodInfo method = type.GetMethod("Add");
					foreach (object item2 in source as IEnumerable)
					{
						method.Invoke(val, new object[1] { DeepCopy(item2, depth - 1) });
					}
				}
				else
				{
					FieldInfo[] fields = type.GetFields();
					FieldInfo[] array2 = fields;
					foreach (FieldInfo fieldInfo in array2)
					{
						object value = fieldInfo.GetValue(source);
						fieldInfo.SetValue(val, DeepCopy(value, depth - 1));
					}
					PropertyInfo[] properties = type.GetProperties();
					PropertyInfo[] array3 = properties;
					foreach (PropertyInfo propertyInfo in array3)
					{
						if (propertyInfo.CanRead && propertyInfo.CanWrite && propertyInfo.GetIndexParameters().Length == 0)
						{
							object value2 = propertyInfo.GetValue(source, null);
							propertyInfo.SetValue(val, DeepCopy(value2, depth - 1), null);
						}
					}
				}
			}
			return val;
		}

		public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
		{
			IEnumerable<IEnumerable<T>> seed = new IEnumerable<T>[1] { Enumerable.Empty<T>() };
			return sequences.Aggregate(seed, (IEnumerable<IEnumerable<T>> accumulator, IEnumerable<T> sequence) => from accseq in accumulator
				from item in sequence
				select accseq.Concat(new T[1] { item }));
		}
	}
}
