using System.Text;

namespace DigitalOpus.MB.Core
{
	public class ObjectLog
	{
		private int pos;

		private string[] logMessages;

		public ObjectLog(short bufferSize)
		{
			logMessages = new string[bufferSize];
		}

		private void _CacheLogMessage(string msg)
		{
			if (logMessages.Length != 0)
			{
				logMessages[pos] = msg;
				pos++;
				if (pos >= logMessages.Length)
				{
					pos = 0;
				}
			}
		}

		public void Log(MB2_LogLevel l, string msg, MB2_LogLevel currentThreshold)
		{
			MB2_Log.Log(l, msg, currentThreshold);
			_CacheLogMessage(msg);
		}

		public void Error(string msg, params object[] args)
		{
			_CacheLogMessage(MB2_Log.Error(msg, args));
		}

		public void Warn(string msg, params object[] args)
		{
			_CacheLogMessage(MB2_Log.Warn(msg, args));
		}

		public void Info(string msg, params object[] args)
		{
			_CacheLogMessage(MB2_Log.Info(msg, args));
		}

		public void LogDebug(string msg, params object[] args)
		{
			_CacheLogMessage(MB2_Log.LogDebug(msg, args));
		}

		public void Trace(string msg, params object[] args)
		{
			_CacheLogMessage(MB2_Log.Trace(msg, args));
		}

		public string Dump()
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			if (logMessages[logMessages.Length - 1] != null)
			{
				num = pos;
			}

			for (int i = 0; i < logMessages.Length; i++)
			{
				int num2 = (num + i) % logMessages.Length;
				if (logMessages[num2] == null)
				{
					break;
				}

				stringBuilder.AppendLine(logMessages[num2]);
			}

			return stringBuilder.ToString();
		}
	}
}
