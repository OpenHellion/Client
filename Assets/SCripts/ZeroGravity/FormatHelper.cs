using System;
using System.Globalization;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Objects;

namespace ZeroGravity
{
	public static class FormatHelper
	{
		public static string FormatValue(float val, bool round = false)
		{
			string empty = string.Empty;
			if (val >= 1000f)
			{
				return (val / 1000f).ToString("0.##", NumberFormatInfo.InvariantInfo) + "k";
			}
			if (round)
			{
				return Mathf.Round(val).ToString("f0");
			}
			return val.ToString("0.#", NumberFormatInfo.InvariantInfo);
		}

		public static string Percentage(float val)
		{
			return FormatPercentage(val * 100f);
		}

		public static string FormatPercentage(float val)
		{
			string empty = string.Empty;
			string text = val.ToString("f0");
			if (val < 25f)
			{
				return "<color=#EA4141>" + ((!(text == "0") || !(val > float.Epsilon)) ? text : "<1") + " %</color>";
			}
			return text + " %";
		}

		public static string CurrentMax(float cur, float max)
		{
			string empty = string.Empty;
			if (cur / max < 0.25f)
			{
				return FormatValue(cur) + " / " + FormatValue(max);
			}
			return FormatValue(cur) + " / " + FormatValue(max);
		}

		public static string Timer(float val)
		{
			string empty = string.Empty;
			TimeSpan timeSpan = TimeSpan.FromSeconds(val);
			return string.Format("{0:D2} : {1:D2}", timeSpan.Minutes, timeSpan.Seconds);
		}

		public static string PerHour(float val)
		{
			string empty = string.Empty;
			return (val * 3600f).ToString("0.#", NumberFormatInfo.InvariantInfo) + " / h";
		}

		public static string PeriodFormat(double val)
		{
			string result = string.Empty;
			TimeSpan timeSpan = TimeSpan.FromSeconds(val);
			if (val < 60.0)
			{
				result = string.Format("{0}s", timeSpan.Seconds);
			}
			else if (val >= 60.0 && val < 3600.0)
			{
				result = string.Format("{0:D2}m {1:D2}s", timeSpan.Minutes, timeSpan.Seconds);
			}
			else if (val > 3600.0 && val < 86400.0)
			{
				result = string.Format("{0:D2}h {1:D2}m {2:D2}s", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
			}
			else if (val >= 86400.0)
			{
				result = string.Format("{0}d {1:D2}h {2:D2}m {3:D2}s", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
			}
			return result;
		}

		public static string PartTier(MachineryPart part)
		{
			if (part.PartType == MachineryPartType.WarpCell)
			{
				return string.Empty;
			}
			if (part.PartType == MachineryPartType.NaniteCore || part.PartType == MachineryPartType.MillitaryNaniteCore)
			{
				return "\n" + Localization.Armor + ": " + part.AuxValue;
			}
			string empty = string.Empty;
			if (part.TierMultiplier > 1f)
			{
				return " " + Percentage(part.TierMultiplier - 1f);
			}
			return empty + " " + Percentage(1f - part.TierMultiplier);
		}

		public static string DistanceFormat(float val)
		{
			string empty = string.Empty;
			if (val < 1000f)
			{
				return val.ToString("0.0", NumberFormatInfo.InvariantInfo) + " m";
			}
			return (val / 1000f).ToString("0.0", NumberFormatInfo.InvariantInfo) + " km";
		}

		public static string DistanceFormat(double val)
		{
			string empty = string.Empty;
			if (val < 1000.0)
			{
				return val.ToString("0.0", NumberFormatInfo.InvariantInfo) + " m";
			}
			return (val / 1000.0).ToString("##,0", NumberFormatInfo.InvariantInfo) + " km";
		}
	}
}
