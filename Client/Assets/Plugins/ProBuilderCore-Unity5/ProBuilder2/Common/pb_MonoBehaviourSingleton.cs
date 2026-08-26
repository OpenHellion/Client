using UnityEngine;

namespace ProBuilder2.Common
{
	public class pb_MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static MonoBehaviour _instance;

		public static T instance
		{
			get
			{
				if ((Object)nullableInstance == (Object)null)
				{
					GameObject gameObject = new GameObject();
					gameObject.name = typeof(T).ToString();
					_instance = gameObject.AddComponent<T>();
				}
				return (T)_instance;
			}
		}

		public static T nullableInstance
		{
			get
			{
				if (_instance == null)
				{
					T[] array = Resources.FindObjectsOfTypeAll<T>();
					if (array != null && array.Length > 0)
					{
						_instance = array[0];
						for (int i = 1; i < array.Length; i++)
						{
							Object.DestroyImmediate(array[i]);
						}
					}
				}
				return (T)_instance;
			}
		}

		public static bool Valid()
		{
			return (Object)nullableInstance != (Object)null;
		}

		public virtual void Awake()
		{
			if (_instance == null)
			{
				_instance = this;
			}
			else
			{
				Object.Destroy(this);
			}
		}

		public virtual void OnEnable()
		{
			_instance = this;
		}
	}
}
