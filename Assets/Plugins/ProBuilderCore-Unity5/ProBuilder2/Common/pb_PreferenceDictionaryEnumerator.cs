using System;
using System.Collections;

namespace ProBuilder2.Common
{
	public class pb_PreferenceDictionaryEnumerator : IEnumerator
	{
		private int m_Position = -1;

		private pb_PreferenceDictionary m_Preferences;

		object IEnumerator.Current => Current;

		public IEnumerable Current
		{
			get
			{
				if (m_Position == 0)
				{
					return m_Preferences.GetBoolDictionary();
				}
				if (m_Position == 1)
				{
					return m_Preferences.GetIntDictionary();
				}
				if (m_Position == 2)
				{
					return m_Preferences.GetFloatDictionary();
				}
				if (m_Position == 3)
				{
					return m_Preferences.GetStringDictionary();
				}
				if (m_Position == 4)
				{
					return m_Preferences.GetColorDictionary();
				}
				if (m_Position == 5)
				{
					return m_Preferences.GetMaterialDictionary();
				}
				throw new InvalidOperationException();
			}
		}

		public pb_PreferenceDictionaryEnumerator(pb_PreferenceDictionary dictionary)
		{
			m_Preferences = dictionary;
		}

		public bool MoveNext()
		{
			m_Position++;
			return m_Position < m_Preferences.Length;
		}

		public void Reset()
		{
			m_Position = -1;
		}
	}
}
