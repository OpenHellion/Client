using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ZeroGravity
{
	public static class ClassHasher
	{
		public static uint GetClassHashCode(Type type, string nspace = null)
		{
			if (nspace == null)
			{
				nspace = type.Namespace;
			}
			HashSet<Type> hashSet = new HashSet<Type>();
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				Type[] types = assembly.GetTypes();
				foreach (Type type2 in types)
				{
					if ((type.IsClass && type2.IsSubclassOf(type)) || (type.IsInterface && type2.GetInterfaces().Contains(type)))
					{
						AddClass(type2, hashSet, nspace);
					}
				}
			}
			Type[] array = new Type[hashSet.Count];
			hashSet.CopyTo(array);
			Array.Sort(array, (Type x, Type y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
			string text = string.Empty;
			Type[] array2 = array;
			foreach (Type type3 in array2)
			{
				text = text + type3.Name + ":";
				AddHashingData(type3, ref text, nspace);
				text += "\r\n";
			}
			uint num = 744748791u;
			for (int l = 0; l < text.Length; l++)
			{
				num += text[l];
				num *= 3045351289u;
			}
			return num;
		}

		private static void AddClass(Type type, HashSet<Type> classes, string nspace)
		{
			if ((!type.IsClass && !type.IsInterface && !type.IsEnum) || type.IsNested || !(type.Namespace == nspace))
			{
				return;
			}
			if (type.IsArray)
			{
				type = type.GetElementType();
			}
			if (type.IsInterface)
			{
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (Assembly assembly in assemblies)
				{
					Type[] types = assembly.GetTypes();
					foreach (Type type2 in types)
					{
						if (type2.GetInterfaces().Contains(type))
						{
							AddClass(type2, classes, nspace);
						}
					}
				}
			}
			else if (type.IsEnum)
			{
				classes.Add(type);
			}
			else
			{
				if (!classes.Add(type))
				{
					return;
				}
				MemberInfo[] members = type.GetMembers();
				foreach (MemberInfo memberInfo in members)
				{
					if (memberInfo.MemberType == MemberTypes.Field)
					{
						AddClass((memberInfo as FieldInfo).FieldType, classes, nspace);
					}
					else if (memberInfo.MemberType == MemberTypes.Property)
					{
						AddClass((memberInfo as PropertyInfo).PropertyType, classes, nspace);
					}
					else if (memberInfo.MemberType == MemberTypes.Method)
					{
						ParameterInfo[] parameters = (memberInfo as MethodInfo).GetParameters();
						foreach (ParameterInfo parameterInfo in parameters)
						{
							AddClass(parameterInfo.ParameterType, classes, nspace);
						}
					}
					else if (memberInfo.MemberType == MemberTypes.Constructor)
					{
						ParameterInfo[] parameters2 = (memberInfo as ConstructorInfo).GetParameters();
						foreach (ParameterInfo parameterInfo2 in parameters2)
						{
							AddClass(parameterInfo2.ParameterType, classes, nspace);
						}
					}
				}
			}
		}

		private static void AddHashingData(Type type, ref string str, string nspace)
		{
			if (type.IsEnum)
			{
				string[] names = Enum.GetNames(type);
				foreach (string text in names)
				{
					str = str + text + "|";
				}
				return;
			}
			MemberInfo[] members = type.GetMembers();
			Array.Sort(members, (MemberInfo x, MemberInfo y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
			bool flag = true;
			MemberInfo[] array = members;
			foreach (MemberInfo memberInfo in array)
			{
				if (!(memberInfo.DeclaringType == type))
				{
					continue;
				}
				string name = memberInfo.Name;
				if (memberInfo.MemberType == MemberTypes.Field)
				{
					AddHashingDataMember((memberInfo as FieldInfo).FieldType, ref str, nspace);
				}
				else if (memberInfo.MemberType == MemberTypes.Property)
				{
					AddHashingDataMember((memberInfo as PropertyInfo).PropertyType, ref str, nspace);
				}
				else if (memberInfo.MemberType == MemberTypes.Method)
				{
					str = str + " " + (memberInfo as MethodInfo).ReturnType.ToString();
					ParameterInfo[] parameters = (memberInfo as MethodInfo).GetParameters();
					foreach (ParameterInfo parameterInfo in parameters)
					{
						str = str + " " + parameterInfo.Name;
						AddHashingDataMember(parameterInfo.ParameterType, ref str, nspace);
					}
				}
				else if (memberInfo.MemberType == MemberTypes.Constructor)
				{
					if ((memberInfo as ConstructorInfo).GetParameters().Length == 0)
					{
						continue;
					}
					ParameterInfo[] parameters2 = (memberInfo as ConstructorInfo).GetParameters();
					foreach (ParameterInfo parameterInfo2 in parameters2)
					{
						str = str + " " + parameterInfo2.Name;
						AddHashingDataMember(parameterInfo2.ParameterType, ref str, nspace);
					}
				}
				str = str + (flag ? " " : ", ") + name;
				flag = false;
			}
		}

		private static void AddHashingDataMember(Type t, ref string str, string nspace)
		{
			if (t.IsPrimitive)
			{
				str = str + " " + t.Name;
			}
			if (t.IsClass && !t.IsNested && t.Namespace == nspace)
			{
				AddHashingData(t, ref str, nspace);
			}
		}
	}
}
