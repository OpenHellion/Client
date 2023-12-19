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
		private static Dictionary<string, string> properties = new Dictionary<string, string>();

		private static DateTime propertiesChangedTime;

		private static string fileName = "Properties.ini";

		public static string FileName
		{
			get { return fileName; }
			set
			{
				fileName = value;
				LoadProperties();
			}
		}

		private static void LoadProperties()
		{
			try
			{
				properties.Clear();
				string[] array = File.ReadAllLines(fileName);
				foreach (string text in array)
				{
					if (!text.TrimStart().StartsWith("#"))
					{
						try
						{
							string[] array2 = text.Split("=".ToCharArray(), 2);
							properties.Add(array2[0], array2[1]);
						}
						catch
						{
						}
					}
				}
			}
			catch
			{
			}
		}

		public static T GetProperty<T>(string propertyName, T defaultValue = default(T))
		{
			DateTime lastWriteTime = File.GetLastWriteTime(fileName);
			if (lastWriteTime != propertiesChangedTime)
			{
				propertiesChangedTime = lastWriteTime;
				LoadProperties();
			}

			TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
			try
			{
				return (T)converter.ConvertFrom(properties[propertyName]);
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
			filePath = Path.Combine((filePath == null) ? string.Empty : filePath, fileName);
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
