using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ProBuilder2.Common
{
	public static class pb_Log
	{
		public const string DEFAULT_LOG_PATH = "ProBuilderLog.txt";

		private static Stack<pb_LogLevel> m_logStack = new Stack<pb_LogLevel>();

		private static pb_LogLevel m_LogLevel = pb_LogLevel.All;

		private static pb_LogOutput m_Output = pb_LogOutput.Console;

		private static string m_LogFilePath = "ProBuilderLog.txt";

		public static void PushLogLevel(pb_LogLevel level)
		{
			m_logStack.Push(m_LogLevel);
			m_LogLevel = level;
		}

		public static void PopLogLevel()
		{
			m_LogLevel = m_logStack.Pop();
		}

		public static void SetLogLevel(pb_LogLevel level)
		{
			m_LogLevel = level;
		}

		public static void SetOutput(pb_LogOutput output)
		{
			m_Output = output;
		}

		public static void SetLogFile(string path)
		{
			m_LogFilePath = path;
		}

		public static void Debug<T>(T value)
		{
			Debug(value.ToString());
		}

		public static void Debug(string message)
		{
			DoPrint(message, LogType.Log);
		}

		public static void Debug(string format, params object[] values)
		{
			Debug(string.Format(format, values));
		}

		public static void Info(string format, params object[] values)
		{
			Info(string.Format(format, values));
		}

		public static void Info(string message)
		{
			if ((m_LogLevel & pb_LogLevel.Info) > pb_LogLevel.None)
			{
				DoPrint(message, LogType.Log);
			}
		}

		public static void Warning(string format, params object[] values)
		{
			Warning(string.Format(format, values));
		}

		public static void Warning(string message)
		{
			if ((m_LogLevel & pb_LogLevel.Warning) > pb_LogLevel.None)
			{
				DoPrint(message, LogType.Warning);
			}
		}

		public static void Error(string format, params object[] values)
		{
			Error(string.Format(format, values));
		}

		public static void Error(string message)
		{
			if ((m_LogLevel & pb_LogLevel.Error) > pb_LogLevel.None)
			{
				DoPrint(message, LogType.Error);
			}
		}

		public static void Watch<T, K>(T key, K value)
		{
			UnityEngine.Debug.Log(string.Format("{0} : {1}\nCPAPI:{{\"cmd\":\"Watch\" \"name\":\"{0}\"}}", key.ToString(), value.ToString()));
		}

		private static void DoPrint(string message, LogType type)
		{
			if ((m_Output & pb_LogOutput.Console) > pb_LogOutput.None)
			{
				PrintToConsole(message, type);
			}
			if ((m_Output & pb_LogOutput.File) > pb_LogOutput.None)
			{
				PrintToFile(message, m_LogFilePath);
			}
		}

		public static void PrintToFile(string message, string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return;
			}
			string fullPath = Path.GetFullPath(path);
			if (string.IsNullOrEmpty(fullPath))
			{
				PrintToConsole("m_LogFilePath bad: " + fullPath);
				return;
			}
			if (!File.Exists(fullPath))
			{
				string directoryName = Path.GetDirectoryName(fullPath);
				if (string.IsNullOrEmpty(directoryName))
				{
					PrintToConsole("m_LogFilePath bad: " + fullPath);
					return;
				}
				Directory.CreateDirectory(directoryName);
				using StreamWriter streamWriter = File.CreateText(fullPath);
				streamWriter.WriteLine(message);
				return;
			}
			using StreamWriter streamWriter2 = File.AppendText(fullPath);
			streamWriter2.WriteLine(message);
		}

		public static void ClearLogFile()
		{
			if (File.Exists(m_LogFilePath))
			{
				File.Delete(m_LogFilePath);
			}
		}

		public static void PrintToConsole(string message, LogType type = LogType.Log)
		{
			switch (type)
			{
			case LogType.Log:
				UnityEngine.Debug.Log(message);
				break;
			case LogType.Warning:
				UnityEngine.Debug.LogWarning(message);
				break;
			case LogType.Error:
				UnityEngine.Debug.LogError(message);
				break;
			case LogType.Assert:
				break;
			default:
				UnityEngine.Debug.Log(message);
				break;
			}
		}
	}
}
