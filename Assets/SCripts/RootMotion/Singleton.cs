using UnityEngine;

namespace RootMotion
{
	public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
	{
		private static T sInstance;

		public static T instance
		{
			get
			{
				return sInstance;
			}
		}

		protected virtual void Awake()
		{
			if ((Object)sInstance != (Object)null)
			{
				Debug.LogError(base.name + "error: already initialized", this);
			}
			sInstance = (T)this;
		}
	}
}
