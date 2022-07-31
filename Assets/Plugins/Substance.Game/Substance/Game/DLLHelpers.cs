using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Substance.Game
{
	public class DLLHelpers
	{
		public static IntPtr DllHandle = IntPtr.Zero;

		private static object[] mParams = new object[0];

		[DllImport("kernel32.dll", SetLastError = true)]
		protected static extern IntPtr LoadLibrary(string filename);

		[DllImport("kernel32.dll", SetLastError = true)]
		protected static extern IntPtr GetProcAddress(IntPtr hModule, string procname);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool FreeLibrary(IntPtr hModule);

		[DllImport("libdl.dylib")]
		protected static extern IntPtr dlopen(string filename, int flags);

		[DllImport("libdl.dylib")]
		protected static extern IntPtr dlsym(IntPtr handle, string symbol);

		[DllImport("libdl.dylib")]
		protected static extern IntPtr dlerror();

		[DllImport("libdl.dylib")]
		protected static extern int dlclose(IntPtr handle);

		internal static object[] GetParams(int size)
		{
			Array.Resize(ref mParams, size);
			return mParams;
		}

		public static void LoadDLL(string dataPath)
		{
			if (DllHandle == IntPtr.Zero)
			{
				if (IsWindows())
				{
					DllHandle = LoadLibrary(Path.Combine(dataPath, "Plugins/Substance.Engine.dll"));
				}
				else if (IsMac())
				{
					DllHandle = dlopen(Path.Combine(dataPath, "Plugins/Substance.Engine.bundle/Contents/MacOS/Substance.Engine"), 3);
				}
				if (DllHandle == IntPtr.Zero)
				{
					Debug.LogError("Substance engine failed to load");
				}
			}
		}

		public static void UnloadDLL()
		{
			if (DllHandle != IntPtr.Zero)
			{
				if (IsWindows())
				{
					FreeLibrary(DllHandle);
				}
				else if (IsMac())
				{
					dlclose(DllHandle);
				}
				DllHandle = IntPtr.Zero;
			}
		}

		private static bool IsWindows()
		{
			if (Environment.OSVersion.Platform != PlatformID.Win32NT && Environment.OSVersion.Platform != 0 && Environment.OSVersion.Platform != PlatformID.Win32Windows)
			{
				return Environment.OSVersion.Platform == PlatformID.WinCE;
			}
			return true;
		}

		private static bool IsMac()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix && Directory.Exists("/Applications") && Directory.Exists("/System") && Directory.Exists("/Users") && Directory.Exists("/Volumes"))
			{
				return true;
			}
			return false;
		}

		private static bool IsLinux()
		{
			return Environment.OSVersion.Platform == PlatformID.Unix;
		}

		internal static Delegate GetFunction(string funcname, Type t)
		{
			IntPtr intPtr = IntPtr.Zero;
			if (DllHandle == IntPtr.Zero)
			{
				return null;
			}
			if (IsWindows())
			{
				intPtr = GetProcAddress(DllHandle, funcname);
			}
			else if (IsMac())
			{
				intPtr = dlsym(DllHandle, funcname);
			}
			if (intPtr == IntPtr.Zero)
			{
				return null;
			}
			return Marshal.GetDelegateForFunctionPointer(intPtr, t);
		}
	}
}
