using UnityEngine;

namespace Substance.Game
{
	public class Log
	{
		public static bool bSomethingChanged = true;

		public static string logFilename = "";

		public static bool bLogImportSettings = true;

		public static string GetLogFilename()
		{
			string text = "";
			text = Application.dataPath;
			int num = text.LastIndexOf('/');
			if (num > 0)
			{
				text = text.Substring(0, num);
				text += "/Substance.log.txt";
			}
			return text;
		}

		public static void Initialize()
		{
			if (!(logFilename == ""))
			{
				string text = logFilename;
				if (NativeFunctions.cppSetLogPath(text) != 0)
				{
					Debug.LogError("Cannot set log file '" + text + "'!");
					logFilename = "";
				}
			}
		}

		public static void Message(string pString)
		{
			if (logFilename != "")
			{
				NativeFunctions.cppLog(pString);
			}
			else
			{
				Debug.Log(pString);
			}
		}

		public static void Error(string pString)
		{
			if (logFilename != "")
			{
				NativeFunctions.cppLogError(pString);
			}
			else
			{
				Debug.LogError(pString);
			}
		}
	}
}
