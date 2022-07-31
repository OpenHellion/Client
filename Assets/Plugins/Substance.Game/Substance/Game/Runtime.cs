using System;
using UnityEngine;

namespace Substance.Game
{
	public sealed class Runtime : MonoBehaviour
	{
		private static Runtime mInstance;

		private void Awake()
		{
			if (mInstance == null)
			{
				mInstance = this;
			}
			else if (mInstance != this)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}

		private void OnDestroy()
		{
			if (mInstance == this)
			{
				mInstance = null;
			}
		}

		public static void TryLoadRuntime()
		{
			if (!Application.isEditor && !(mInstance != null))
			{
				mInstance = new GameObject
				{
					name = "Substance.Runtime"
				}.AddComponent<Runtime>();
			}
		}

		private void LateUpdate()
		{
			NativeFunctions.cppProcessOutputQueue();
			NativeFunctions.cppRenderSubstances(true, IntPtr.Zero);
		}
	}
}
