using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Substance.Game
{
	public static class NativeCallbacks
	{
		public class AbortException : Exception
		{
			private string mAbortMessage;

			public AbortException(string pAbortMessage)
			{
				mAbortMessage = pAbortMessage;
			}

			public string GetAbortMessage()
			{
				return mAbortMessage;
			}

			public bool IsError()
			{
				if (mAbortMessage == null)
				{
					return false;
				}
				return true;
			}
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void LogCallbackDelegate(int pLogLevel, string str);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void GameTextureCallbackDelegate(string assetPath, IntPtr preComputeCallback, uint outputUID, int format, int channelsOrder, int width, int height, int mipCount, IntPtr pixels);

		public delegate void GameTexturePreComputeCallbackDelegate(uint outputHash, int format, int channelsOrder, int width, int height, int mipCount, IntPtr sPixels, int numPixels);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void CreateSubstanceGraphCallbackGameDelegate(IntPtr substance, uint index, IntPtr nativeHandle);

		private static LogCallbackDelegate mLogCallbackDelegate;

		private static GameTextureCallbackDelegate mGameTextureCallbackDelegate;

		private static CreateSubstanceGraphCallbackGameDelegate mCreateSubstanceGraphCallbackGameDelegate;

		private const int LOG_INFO = 1;

		private const int LOG_CRUISE = 2;

		private const int LOG_WARNING = 3;

		private const int LOG_ERROR = 4;

		private static RuntimePlatform[] standaloneFields = new RuntimePlatform[9]
		{
			RuntimePlatform.OSXPlayer,
			RuntimePlatform.OSXEditor,
			RuntimePlatform.WindowsPlayer,
			RuntimePlatform.WindowsEditor,
			RuntimePlatform.LinuxPlayer,
			RuntimePlatform.LinuxEditor,
			RuntimePlatform.MetroPlayerX86,
			RuntimePlatform.MetroPlayerX64,
			RuntimePlatform.MetroPlayerARM
		};

		private static RuntimePlatform[] iOSFields = new RuntimePlatform[2]
		{
			RuntimePlatform.IPhonePlayer,
			RuntimePlatform.tvOS
		};

		private static void csharpLogCallback(int pLogLevel, string str)
		{
			switch (pLogLevel)
			{
			case 4:
				Debug.LogError(str);
				break;
			default:
				Debug.Log(str);
				break;
			}
		}

		public static void InitSubstance()
		{
			mLogCallbackDelegate = csharpLogCallback;
			mGameTextureCallbackDelegate = csGameTextureCallback;
			mCreateSubstanceGraphCallbackGameDelegate = csCreateSubstanceGraphCallbackGame;
			NativeFunctions.cppInitSubstance(Application.dataPath);
			NativeFunctions.cppSetCallbacks(Marshal.GetFunctionPointerForDelegate((Delegate)mLogCallbackDelegate), Marshal.GetFunctionPointerForDelegate((Delegate)mGameTextureCallbackDelegate), Marshal.GetFunctionPointerForDelegate((Delegate)mCreateSubstanceGraphCallbackGameDelegate));
			Log.Initialize();
		}

		public static void ShutdownSubstance()
		{
			NativeFunctions.cppShutdownSubstance();
		}

		private static void csCreateSubstanceGraphCallbackGame(IntPtr substancePtr, uint index, IntPtr nativeHandle)
		{
			(GCHandle.FromIntPtr(substancePtr).Target as Substance).graphs[(int)index].mNativeHandle = nativeHandle;
		}

		private static void csGameTextureCallback(string assetPath, IntPtr preComputeCallback, uint outputHash, int format, int channelsOrder, int width, int height, int mipCount, IntPtr sPixels)
		{
			Substance substance = Substance.Find(assetPath);
			int num = PixelFormat.GenerateImageSizeWithMipMaps(width, height, format, mipCount);
			if (substance == null)
			{
				Log.Message("Unable to find substance '" + assetPath + "'");
				return;
			}
			if (preComputeCallback != IntPtr.Zero)
			{
				((GameTexturePreComputeCallbackDelegate)Marshal.GetDelegateForFunctionPointer(preComputeCallback, typeof(GameTexturePreComputeCallbackDelegate)))(outputHash, format, channelsOrder, width, height, mipCount, sPixels, num);
				return;
			}
			Texture2D texture2D = substance.GetOutputTexture(outputHash);
			if (!(texture2D != null))
			{
				return;
			}
			if (texture2D.width != width || texture2D.height != height)
			{
				if (texture2D.format <= TextureFormat.ARGB32)
				{
					texture2D.Reinitialize(width, height);
				}
				else
				{
					string outputLabelFromOutputHash = substance.GetOutputLabelFromOutputHash(outputHash);
					int graphIndexFromOutputHash = Substance.GetGraphIndexFromOutputHash(outputHash);
					string graphLabel = substance.graphs[graphIndexFromOutputHash].graphLabel;
					Material material = substance.graphs[graphIndexFromOutputHash].material;
					IntPtr intPtr = NativeFunctions.cppGetOutputChannelStrFromHash(substance.assetPath, outputHash);
					string pOutputName = Marshal.PtrToStringAnsi(intPtr);
					NativeFunctions.cppFreeMemory(intPtr);
					bool linear = substance.graphs[graphIndexFromOutputHash].IsOutputLinear(outputLabelFromOutputHash);
					texture2D = new Texture2D(width, height, PixelFormat.GetUnityTextureFormat(format), mipCount > 1, linear);
					string shaderTextureName = NativeTypes.GetShaderTextureName(substance.graphs[graphIndexFromOutputHash].shaderName, pOutputName);
					material.SetTexture(shaderTextureName, texture2D);
					substance.AssignTextureToOutputHash(texture2D, outputHash);
				}
			}
			texture2D.LoadRawTextureData(sPixels, num);
			texture2D.Apply();
		}

		public static int GetTargetSettingIndex()
		{
			return 0;
		}
	}
}
