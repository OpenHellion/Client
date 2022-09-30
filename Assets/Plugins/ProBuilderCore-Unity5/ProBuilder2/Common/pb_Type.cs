using System;
using UnityEngine;

namespace ProBuilder2.Common
{
	[Serializable]
	public class pb_Type : ISerializationCallbackReceiver
	{
		[SerializeField]
		private string assemblyQualifiedName;

		public Type type;

		public pb_Type(Type t)
		{
			type = t;
		}

		public void OnBeforeSerialize()
		{
			assemblyQualifiedName = type.AssemblyQualifiedName;
		}

		public void OnAfterDeserialize()
		{
			type = Type.GetType(assemblyQualifiedName);
		}

		public static implicit operator Type(pb_Type t)
		{
			return t.type;
		}

		public static implicit operator pb_Type(Type t)
		{
			return new pb_Type(t);
		}
	}
}
