using System;

namespace ZeroGravity
{
	public static class EnumExtensions
	{
		public static string ToLocalizedString(this Enum code)
		{
			try
			{
				return Localization.Enums[code];
			}
			catch
			{
				return Enum.GetName(code.GetType(), code);
			}
		}
	}
}
