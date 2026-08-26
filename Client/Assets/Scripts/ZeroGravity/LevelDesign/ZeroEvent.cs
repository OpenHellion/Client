using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ZeroGravity.LevelDesign
{
	[Serializable]
	public class ZeroEvent
	{
		[Serializable]
		public class EventObject
		{
			public GameObject Object;

			public string ScriptFunctionName;

			public int ScriptFunctionIndex;

			public object Value;

			public Delegate DelegateFunc;
		}

		private bool eventsCreated;

		public List<EventObject> EventObjects = new List<EventObject>();

		private void CreateEvents()
		{
			foreach (EventObject eventObject in EventObjects)
			{
				if (eventObject.Object == null || eventObject.ScriptFunctionName.IsNullOrEmpty())
				{
					continue;
				}

				string[] array = eventObject.ScriptFunctionName.Split('/');
				MonoBehaviour monoBehaviour = null;
				MonoBehaviour[] components = eventObject.Object.GetComponents<MonoBehaviour>();
				foreach (MonoBehaviour monoBehaviour2 in components)
				{
					if (monoBehaviour2.GetType().Name == array[0])
					{
						monoBehaviour = monoBehaviour2;
						break;
					}
				}

				if (!(monoBehaviour == null))
				{
					MethodInfo method = monoBehaviour.GetType()
						.GetMethod(array[1], BindingFlags.Instance | BindingFlags.Public);
					if (method != null)
					{
						eventObject.DelegateFunc = Delegate.CreateDelegate(method.GetType(), monoBehaviour, method);
						continue;
					}

					PropertyInfo property = monoBehaviour.GetType()
						.GetProperty(array[1], BindingFlags.Instance | BindingFlags.Public);
					eventObject.DelegateFunc = Delegate.CreateDelegate(property.GetGetMethod().GetType(), monoBehaviour,
						property.GetGetMethod());
				}
			}
		}

		public void Invoke()
		{
			if (EventObjects != null && EventObjects.Count != 0)
			{
				if (!eventsCreated)
				{
					CreateEvents();
				}

				for (int i = 0; i < EventObjects.Count; i++)
				{
					EventObjects[i].DelegateFunc.DynamicInvoke(EventObjects[i].Value);
				}
			}
		}
	}
}
