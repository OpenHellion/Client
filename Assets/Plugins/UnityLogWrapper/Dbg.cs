#define DEBUG
using System;
using System.Diagnostics;

public static class Dbg
{
	public static bool AddTimestamp = true;

	public static string TimestampFormat = "yyyy/MM/dd HH:mm:ss.ffff";

	private static string ObjectParamsToString(params object[] values)
	{
		string text = "";
		checked
		{
			for (int i = 0; i < values.Length; i++)
			{
				text = text + ((i > 0) ? ", " : "") + GetString(values[i]);
			}
			return text;
		}
	}

	private static string GetString(object value)
	{
		return (value != null) ? value.ToString() : "NULL";
	}

	[Conditional("DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public static void Log(string message)
	{
		UnityEngine.Debug.Log(message);
	}

	[Conditional("DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public static void Log(params object[] values)
	{
		if (values.Length == 1)
		{
			UnityEngine.Debug.Log(GetString(values[0]));
		}
		else
		{
			UnityEngine.Debug.Log(ObjectParamsToString(values));
		}
	}

	[Conditional("DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public static void LogIf(bool condition, string message)
	{
		if (condition)
		{
			UnityEngine.Debug.Log(message);
		}
	}

	[Conditional("DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public static void LogIf(bool condition, params object[] values)
	{
		if (condition)
		{
			Log(values);
		}
	}

	[Conditional("DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public static void LogArray(object[] values, int printLimit = 10)
	{
		string text = "";
		checked
		{
			for (int i = 0; i < values.Length && i < printLimit; i++)
			{
				text = text + ((i > 0) ? ", " : "") + GetString(values[i]);
			}
			Log(text);
		}
	}

	[Conditional("DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public static void LogArray(short[] values, int printLimit = 10)
	{
		string text = "";
		checked
		{
			for (int i = 0; i < values.Length && i < printLimit; i++)
			{
				text = text + ((i > 0) ? ", " : "") + GetString(values[i]);
			}
			Log(text);
		}
	}

	[Conditional("DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public static void LogArray(int[] values, int printLimit = 10)
	{
		string text = "";
		checked
		{
			for (int i = 0; i < values.Length && i < printLimit; i++)
			{
				text = text + ((i > 0) ? ", " : "") + GetString(values[i]);
			}
			Log(text);
		}
	}

	[Conditional("DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public static void LogArray(float[] values, int printLimit = 10)
	{
		string text = "";
		checked
		{
			for (int i = 0; i < values.Length && i < printLimit; i++)
			{
				text = text + ((i > 0) ? ", " : "") + GetString(values[i]);
			}
			Log(text);
		}
	}

	[Conditional("DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public static void LogArray(double[] values, int printLimit = 10)
	{
		string text = "";
		checked
		{
			for (int i = 0; i < values.Length && i < printLimit; i++)
			{
				text = text + ((i > 0) ? ", " : "") + GetString(values[i]);
			}
			Log(text);
		}
	}

	public static void Info(string message)
	{
		UnityEngine.Debug.Log((AddTimestamp ? DateTime.UtcNow.ToString(TimestampFormat + " ") : "") + message);
	}

	public static void Info(params object[] values)
	{
		if (values.Length == 1)
		{
			UnityEngine.Debug.Log((AddTimestamp ? DateTime.UtcNow.ToString(TimestampFormat + " ") : "") + GetString(values[0]));
		}
		else
		{
			UnityEngine.Debug.Log((AddTimestamp ? DateTime.UtcNow.ToString(TimestampFormat + " ") : "") + ObjectParamsToString(values));
		}
	}

	public static void Warning(string message)
	{
		UnityEngine.Debug.LogWarning((AddTimestamp ? DateTime.UtcNow.ToString(TimestampFormat + " ") : "") + message);
	}

	public static void Warning(params object[] values)
	{
		if (values.Length == 1)
		{
			UnityEngine.Debug.LogWarning((AddTimestamp ? DateTime.UtcNow.ToString(TimestampFormat + " ") : "") + GetString(values[0]));
		}
		else
		{
			UnityEngine.Debug.LogWarning((AddTimestamp ? DateTime.UtcNow.ToString(TimestampFormat + " ") : "") + ObjectParamsToString(values));
		}
	}

	public static void Error(string message)
	{
		UnityEngine.Debug.LogError(DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss.ffff ") + message);
	}

	public static void Error(params object[] values)
	{
		if (values.Length == 1)
		{
			UnityEngine.Debug.LogError((AddTimestamp ? DateTime.UtcNow.ToString(TimestampFormat + " ") : "") + GetString(values[0]));
		}
		else
		{
			UnityEngine.Debug.LogError((AddTimestamp ? DateTime.UtcNow.ToString(TimestampFormat + " ") : "") + ObjectParamsToString(values));
		}
	}
}
