using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using UnityEngine;

namespace ZeroGravity
{
	public static class Properties
	{
		private static readonly Dictionary<string, string> _properties = new Dictionary<string, string>();

		private static DateTime _propertiesChangedTime;

		private static string _fileName = "Properties.ini";

		private static void LoadProperties()
		{
			_propertiesChangedTime = File.GetLastWriteTime(_fileName);
			_properties.Clear();
			string[] file = File.ReadAllLines(_fileName);
			foreach (string row in file)
			{
				if (row.IsNullOrEmpty() || row.TrimStart().StartsWith("#")) continue;
				string[] parts = row.Split("=".ToCharArray(), 2);
				_properties.Add(parts[0].ToLower(), parts[1]);
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

		public static void GetProperty<T>(string propertyName, ref T value)
		{
			try
			{
				value = GetProperty<T>(propertyName);
			}
			catch
			{
			}
		}

		public static void SetProperty<T>(string propertyName, T propertyValue, string filePath = null)
		{
			filePath = Path.Combine((filePath == null) ? string.Empty : filePath, _fileName);
			if (!File.Exists(filePath))
			{
				File.Create(filePath);
			}

			int num = 0;
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			try
			{
				bool flag = false;
				string[] array = File.ReadAllLines(filePath);
				foreach (string text in array)
				{
					if (text.TrimStart().StartsWith("#"))
					{
						dictionary.Add("#" + num++, text);
						continue;
					}

					try
					{
						string[] array2 = text.Split("=".ToCharArray(), 2);
						if (array2[0] == propertyName)
						{
							flag = true;
							dictionary.Add(array2[0], propertyValue.ToString());
						}
						else
						{
							dictionary.Add(array2[0], array2[1]);
						}
					}
					catch
					{
					}
				}

				if (!flag)
				{
					dictionary.Add(propertyName, propertyValue.ToString());
				}

				StringBuilder stringBuilder = new StringBuilder();
				foreach (KeyValuePair<string, string> item in dictionary)
				{
					if (item.Key.StartsWith("#"))
					{
						stringBuilder.AppendLine(item.Value);
					}
					else
					{
						stringBuilder.AppendLine(item.Key + "=" + item.Value);
					}
				}

				using (StreamWriter streamWriter = new StreamWriter(filePath))
				{
					streamWriter.WriteLine(stringBuilder.ToString());
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}
	}
}
