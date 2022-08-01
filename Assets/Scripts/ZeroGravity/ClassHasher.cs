using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ZeroGravity
{
	public static class ClassHasher
	{
		[CompilerGenerated]
		private static Comparison<Type> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Comparison<MemberInfo> _003C_003Ef__am_0024cache1;

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
						addClass(type2, hashSet, nspace);
					}
				}
			}
			Type[] array = new Type[hashSet.Count];
			hashSet.CopyTo(array);
			if (_003C_003Ef__am_0024cache0 == null)
			{
				_003C_003Ef__am_0024cache0 = _003CGetClassHashCode_003Em__0;
			}
			Array.Sort(array, _003C_003Ef__am_0024cache0);
			string text = string.Empty;
			Type[] array2 = array;
			foreach (Type type3 in array2)
			{
				text = text + type3.Name + ":";
				addHashingData(type3, ref text, nspace);
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

		private static void addClass(Type type, HashSet<Type> classes, string nspace)
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
							addClass(type2, classes, nspace);
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
						addClass((memberInfo as FieldInfo).FieldType, classes, nspace);
					}
					else if (memberInfo.MemberType == MemberTypes.Property)
					{
						addClass((memberInfo as PropertyInfo).PropertyType, classes, nspace);
					}
					else if (memberInfo.MemberType == MemberTypes.Method)
					{
						ParameterInfo[] parameters = (memberInfo as MethodInfo).GetParameters();
						foreach (ParameterInfo parameterInfo in parameters)
						{
							addClass(parameterInfo.ParameterType, classes, nspace);
						}
					}
					else if (memberInfo.MemberType == MemberTypes.Constructor)
					{
						ParameterInfo[] parameters2 = (memberInfo as ConstructorInfo).GetParameters();
						foreach (ParameterInfo parameterInfo2 in parameters2)
						{
							addClass(parameterInfo2.ParameterType, classes, nspace);
						}
					}
				}
			}
		}

		private static void addHashingData(Type type, ref string str, string nspace)
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
			if (_003C_003Ef__am_0024cache1 == null)
			{
				_003C_003Ef__am_0024cache1 = _003CaddHashingData_003Em__1;
			}
			Array.Sort(members, _003C_003Ef__am_0024cache1);
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
					addHashingDataMember((memberInfo as FieldInfo).FieldType, ref str, nspace);
				}
				else if (memberInfo.MemberType == MemberTypes.Property)
				{
					addHashingDataMember((memberInfo as PropertyInfo).PropertyType, ref str, nspace);
				}
				else if (memberInfo.MemberType == MemberTypes.Method)
				{
					str = str + " " + (memberInfo as MethodInfo).ReturnType.ToString();
					ParameterInfo[] parameters = (memberInfo as MethodInfo).GetParameters();
					foreach (ParameterInfo parameterInfo in parameters)
					{
						str = str + " " + parameterInfo.Name;
						addHashingDataMember(parameterInfo.ParameterType, ref str, nspace);
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
						addHashingDataMember(parameterInfo2.ParameterType, ref str, nspace);
					}
				}
				str = str + (flag ? " " : ", ") + name;
				flag = false;
			}
		}

		private static void addHashingDataMember(Type t, ref string str, string nspace)
		{
			if (t.IsPrimitive)
			{
				str = str + " " + t.Name;
			}
			if (t.IsClass && !t.IsNested && t.Namespace == nspace)
			{
				addHashingData(t, ref str, nspace);
			}
		}

		[CompilerGenerated]
		private static int _003CGetClassHashCode_003Em__0(Type x, Type y)
		{
			return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
		}

		[CompilerGenerated]
		private static int _003CaddHashingData_003Em__1(MemberInfo x, MemberInfo y)
		{
			return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
		}
	}
}
