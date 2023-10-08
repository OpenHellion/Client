using System.Collections.Generic;
using UnityEngine;

public class LightManager<T> : MonoBehaviour
{
	private static LightManager<T> s_Instance;

	private HashSet<T> m_Container = new HashSet<T>();

	private static LightManager<T> Instance
	{
		get
		{
			if (s_Instance != null)
			{
				return s_Instance;
			}

			s_Instance = (LightManager<T>)Object.FindObjectOfType(typeof(LightManager<T>));
			return s_Instance;
		}
	}

	public static HashSet<T> Get()
	{
		LightManager<T> instance = Instance;
		return (!(instance == null)) ? instance.m_Container : new HashSet<T>();
	}

	public static bool Add(T t)
	{
		LightManager<T> instance = Instance;
		if (instance == null)
		{
			return false;
		}

		instance.m_Container.Add(t);
		return true;
	}

	public static void Remove(T t)
	{
		LightManager<T> instance = Instance;
		if (!(instance == null))
		{
			instance.m_Container.Remove(t);
		}
	}
}
