using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ProBuilder2.Common
{
	public class pb_ObjectPool<T> where T : UnityEngine.Object, new()
	{
		public int desiredSize;

		public Func<T> constructor;

		public Action<T> destructor;

		private Queue pool = new Queue();

		[CompilerGenerated]
		private static Action<T> _003C_003Ef__mg_0024cache0;

		public pb_ObjectPool(int initialSize, int desiredSize, Func<T> constructor, Action<T> destructor)
		{
			this.constructor = constructor;
			Action<T> action;
			if (destructor == null)
			{
				if (_003C_003Ef__mg_0024cache0 == null)
				{
					_003C_003Ef__mg_0024cache0 = DestroyObject;
				}
				action = _003C_003Ef__mg_0024cache0;
			}
			else
			{
				action = destructor;
			}
			this.destructor = action;
			this.desiredSize = desiredSize;
			for (int i = 0; i < initialSize && i < desiredSize; i++)
			{
				pool.Enqueue((constructor == null) ? new T() : constructor());
			}
		}

		public T Get()
		{
			T val = ((pool.Count <= 0) ? ((T)null) : ((T)pool.Dequeue()));
			if ((UnityEngine.Object)val == (UnityEngine.Object)null)
			{
				val = ((constructor != null) ? constructor() : new T());
			}
			return val;
		}

		public void Put(T obj)
		{
			if (pool.Count < desiredSize)
			{
				pool.Enqueue(obj);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(obj);
			}
		}

		public void Empty()
		{
			int count = pool.Count;
			for (int i = 0; i < count; i++)
			{
				if (destructor != null)
				{
					destructor((T)pool.Dequeue());
				}
				else
				{
					DestroyObject((T)pool.Dequeue());
				}
			}
		}

		private static void DestroyObject(T obj)
		{
			UnityEngine.Object.DestroyImmediate(obj);
		}

		private void OnDestroy()
		{
			Empty();
		}
	}
}
