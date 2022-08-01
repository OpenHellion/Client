using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ZeroGravity.LevelDesign
{
	[Serializable]
	public class DependencyDelegate
	{
		public delegate bool DelegateFuncBool();

		public delegate int DelegateFuncInt();

		public delegate string DelegateFuncString();

		public enum LogicalConjunction
		{
			AND = 0,
			OR = 1
		}

		[Serializable]
		public class DelegateObject
		{
			public GameObject Object;

			public string ScriptFunctionName;

			public int ScriptFunctionIndex;

			public LogicalConjunction Operation = LogicalConjunction.OR;

			public bool ValueBool;

			public int ValueInt;

			public string ValueString = string.Empty;

			public DelegateFuncBool DelegateBool;

			public DelegateFuncInt DelegateInt;

			public DelegateFuncString DelegateString;
		}

		public List<DelegateObject> DelegateObjects = new List<DelegateObject>();

		public void CreateDelegates()
		{
			foreach (DelegateObject delegateObject in DelegateObjects)
			{
				if (!(delegateObject.Object != null) || delegateObject.ScriptFunctionName.IsNullOrEmpty())
				{
					continue;
				}
				string[] array = delegateObject.ScriptFunctionName.Split('/');
				MonoBehaviour monoBehaviour = null;
				MonoBehaviour[] components = delegateObject.Object.GetComponents<MonoBehaviour>();
				foreach (MonoBehaviour monoBehaviour2 in components)
				{
					if (monoBehaviour2.GetType().Name == array[0])
					{
						monoBehaviour = monoBehaviour2;
						break;
					}
				}
				MethodInfo method = monoBehaviour.GetType().GetMethod(array[1], BindingFlags.Instance | BindingFlags.Public);
				if (method != null)
				{
					if (method.ReturnType == typeof(bool))
					{
						delegateObject.DelegateBool = (DelegateFuncBool)Delegate.CreateDelegate(typeof(DelegateFuncBool), monoBehaviour, method);
					}
					else if (method.ReturnType == typeof(int))
					{
						delegateObject.DelegateInt = (DelegateFuncInt)Delegate.CreateDelegate(typeof(DelegateFuncInt), monoBehaviour, method);
					}
					else if (method.ReturnType == typeof(string))
					{
						delegateObject.DelegateString = (DelegateFuncString)Delegate.CreateDelegate(typeof(DelegateFuncString), monoBehaviour, method);
					}
					continue;
				}
				PropertyInfo property = monoBehaviour.GetType().GetProperty(array[1], BindingFlags.Instance | BindingFlags.Public);
				if (property.PropertyType == typeof(bool))
				{
					delegateObject.DelegateBool = (DelegateFuncBool)Delegate.CreateDelegate(typeof(DelegateFuncBool), monoBehaviour, property.GetGetMethod());
				}
				else if (property.PropertyType == typeof(int))
				{
					delegateObject.DelegateInt = (DelegateFuncInt)Delegate.CreateDelegate(typeof(DelegateFuncInt), monoBehaviour, property.GetGetMethod());
				}
				else if (property.PropertyType == typeof(string))
				{
					delegateObject.DelegateString = (DelegateFuncString)Delegate.CreateDelegate(typeof(DelegateFuncString), monoBehaviour, property.GetGetMethod());
				}
			}
		}

		public bool Invoke()
		{
			if (DelegateObjects == null || DelegateObjects.Count == 0)
			{
				return true;
			}
			bool flag = true;
			bool flag2 = true;
			for (int i = 0; i < DelegateObjects.Count; i++)
			{
				flag2 = true;
				if (DelegateObjects[i].DelegateBool != null)
				{
					if ((DelegateObjects[i].DelegateBool.GetInvocationList()[0] as DelegateFuncBool)() != DelegateObjects[i].ValueBool)
					{
						flag2 = false;
					}
				}
				else if (DelegateObjects[i].DelegateInt != null)
				{
					if ((DelegateObjects[i].DelegateInt.GetInvocationList()[0] as DelegateFuncInt)() != DelegateObjects[i].ValueInt)
					{
						flag2 = false;
					}
				}
				else if (DelegateObjects[i].DelegateString != null && (DelegateObjects[i].DelegateString.GetInvocationList()[0] as DelegateFuncString)() != DelegateObjects[i].ValueString)
				{
					flag2 = false;
				}
				flag = ((i != 0) ? ((DelegateObjects[i].Operation != 0) ? (flag || flag2) : (flag && flag2)) : flag2);
			}
			return flag;
		}
	}
}
