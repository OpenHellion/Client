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
				if (delegateObject.Object == null || delegateObject.ScriptFunctionName.IsNullOrEmpty())
				{
					continue;
				}

				string[] scriptFunctionNames = delegateObject.ScriptFunctionName.Split('/');
				MonoBehaviour script = null;
				MonoBehaviour[] components = delegateObject.Object.GetComponents<MonoBehaviour>();
				foreach (MonoBehaviour component in components)
				{
					if (component.GetType().Name == scriptFunctionNames[0])
					{
						script = component;
						break;
					}
				}

				if (2 > scriptFunctionNames.Length || scriptFunctionNames[0].IsNullOrEmpty() ||
				    scriptFunctionNames[1].IsNullOrEmpty())
				{
					Debug.LogWarningFormat(delegateObject.Object, "Dependency delegate did not find any class or function specified on object {0}.", delegateObject.Object.name);
					continue;
				}

				if (script == null)
				{
					Debug.LogErrorFormat(delegateObject.Object, "Could not find MonoBehaviour with name {0} not found on object {1}.", scriptFunctionNames[0], delegateObject.Object.name);
					continue;
				}

				MethodInfo method = script.GetType().GetMethod(scriptFunctionNames[1], BindingFlags.Instance | BindingFlags.Public);

				if (method != null)
				{
					if (method.ReturnType == typeof(bool))
					{
						delegateObject.DelegateBool =
							(DelegateFuncBool)Delegate.CreateDelegate(typeof(DelegateFuncBool), script, method);
					}
					else if (method.ReturnType == typeof(int))
					{
						delegateObject.DelegateInt =
							(DelegateFuncInt)Delegate.CreateDelegate(typeof(DelegateFuncInt), script, method);
					}
					else if (method.ReturnType == typeof(string))
					{
						delegateObject.DelegateString =
							(DelegateFuncString)Delegate.CreateDelegate(typeof(DelegateFuncString), script,
								method);
					}

					continue;
				}


				PropertyInfo property = script.GetType()
					.GetProperty(scriptFunctionNames[1], BindingFlags.Instance | BindingFlags.Public);

				if (property != null && property.GetGetMethod() != null)
				{
					if (property.PropertyType == typeof(bool))
					{
						delegateObject.DelegateBool = (DelegateFuncBool)Delegate.CreateDelegate(typeof(DelegateFuncBool),
							script, property.GetGetMethod());
					}
					else if (property.PropertyType == typeof(int))
					{
						delegateObject.DelegateInt = (DelegateFuncInt)Delegate.CreateDelegate(typeof(DelegateFuncInt),
							script, property.GetGetMethod());
					}
					else if (property.PropertyType == typeof(string))
					{
						delegateObject.DelegateString =
							(DelegateFuncString)Delegate.CreateDelegate(typeof(DelegateFuncString), script,
								property.GetGetMethod());
					}
				}
				else
				{
					Debug.LogErrorFormat(delegateObject.Object, "Property or method with name {0} could not be found on object {1}.", scriptFunctionNames[1], delegateObject.Object.name);
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
			for (int i = 0; i < DelegateObjects.Count; i++)
			{
				var flag2 = true;
				if (DelegateObjects[i].DelegateBool != null)
				{
					if ((DelegateObjects[i].DelegateBool.GetInvocationList()[0] as DelegateFuncBool)() !=
					    DelegateObjects[i].ValueBool)
					{
						flag2 = false;
					}
				}
				else if (DelegateObjects[i].DelegateInt != null)
				{
					if ((DelegateObjects[i].DelegateInt.GetInvocationList()[0] as DelegateFuncInt)() !=
					    DelegateObjects[i].ValueInt)
					{
						flag2 = false;
					}
				}
				else if (DelegateObjects[i].DelegateString != null &&
				         (DelegateObjects[i].DelegateString.GetInvocationList()[0] as DelegateFuncString)() !=
				         DelegateObjects[i].ValueString)
				{
					flag2 = false;
				}

				flag = i != 0 ? DelegateObjects[i].Operation != 0 ? flag || flag2 : flag && flag2 : flag2;
			}

			return flag;
		}
	}
}
