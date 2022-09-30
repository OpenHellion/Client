using System;

namespace ProBuilder2.Common
{
	[Flags]
	public enum pb_LogLevel
	{
		None = 0,
		Error = 1,
		Warning = 2,
		Info = 4,
		All = 0xFF
	}
}
