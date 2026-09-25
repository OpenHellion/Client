using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace ZeroGravity
{
	public static class Properties
	{
		private static readonly Dictionary<string, string> _properties = new Dictionary<string, string>();

		private static DateTime _propertiesChangedTime;

		private static string _fileName = "Properties.ini";

		private static void LoadProperties()
		{
			_properties.Clear();
			if (!File.Exists(_fileName))
			{
				return;
			}

			foreach (string row in File.ReadAllLines(_fileName))
			{
				if (row.IsNullOrEmpty() || row.TrimStart().StartsWith("#")) continue;
				string[] parts = row.Split("=".ToCharArray(), 2);
				if (parts.Length < 2) continue;
				_properties[parts[0].Trim().ToLower()] = parts[1].Trim();
			}
		}

		public static T GetProperty<T>(string propertyName, T defaultValue = default)
		{
			DateTime lastWriteTime = File.GetLastWriteTime(_fileName);
			if (lastWriteTime != _propertiesChangedTime)
			{
				_propertiesChangedTime = lastWriteTime;
				LoadProperties();
			}

			TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
			try
			{
				return (T)converter.ConvertFrom(_properties[propertyName]);
			}
			catch
			{
				return defaultValue;
			}
		}
	}
}
