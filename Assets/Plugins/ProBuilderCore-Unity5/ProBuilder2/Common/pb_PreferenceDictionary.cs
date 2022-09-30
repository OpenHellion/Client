using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProBuilder2.Common
{
	public class pb_PreferenceDictionary : ScriptableObject, IEnumerable, ISerializationCallbackReceiver, pb_IHasDefault
	{
		private Dictionary<string, bool> m_Bool = new Dictionary<string, bool>();

		private Dictionary<string, int> m_Int = new Dictionary<string, int>();

		private Dictionary<string, float> m_Float = new Dictionary<string, float>();

		private Dictionary<string, string> m_String = new Dictionary<string, string>();

		private Dictionary<string, Color> m_Color = new Dictionary<string, Color>();

		private Dictionary<string, Material> m_Material = new Dictionary<string, Material>();

		[SerializeField]
		private List<string> m_Bool_keys;

		[SerializeField]
		private List<string> m_Int_keys;

		[SerializeField]
		private List<string> m_Float_keys;

		[SerializeField]
		private List<string> m_String_keys;

		[SerializeField]
		private List<string> m_Color_keys;

		[SerializeField]
		private List<string> m_Material_keys;

		[SerializeField]
		private List<bool> m_Bool_values;

		[SerializeField]
		private List<int> m_Int_values;

		[SerializeField]
		private List<float> m_Float_values;

		[SerializeField]
		private List<string> m_String_values;

		[SerializeField]
		private List<Color> m_Color_values;

		[SerializeField]
		private List<Material> m_Material_values;

		public int Length => 6;

		public void OnBeforeSerialize()
		{
			m_Bool_keys = m_Bool.Keys.ToList();
			m_Int_keys = m_Int.Keys.ToList();
			m_Float_keys = m_Float.Keys.ToList();
			m_String_keys = m_String.Keys.ToList();
			m_Color_keys = m_Color.Keys.ToList();
			m_Material_keys = m_Material.Keys.ToList();
			m_Bool_values = m_Bool.Values.ToList();
			m_Int_values = m_Int.Values.ToList();
			m_Float_values = m_Float.Values.ToList();
			m_String_values = m_String.Values.ToList();
			m_Color_values = m_Color.Values.ToList();
			m_Material_values = m_Material.Values.ToList();
		}

		public void OnAfterDeserialize()
		{
			for (int i = 0; i < m_Bool_keys.Count; i++)
			{
				m_Bool.Add(m_Bool_keys[i], m_Bool_values[i]);
			}
			for (int j = 0; j < m_Int_keys.Count; j++)
			{
				m_Int.Add(m_Int_keys[j], m_Int_values[j]);
			}
			for (int k = 0; k < m_Float_keys.Count; k++)
			{
				m_Float.Add(m_Float_keys[k], m_Float_values[k]);
			}
			for (int l = 0; l < m_String_keys.Count; l++)
			{
				m_String.Add(m_String_keys[l], m_String_values[l]);
			}
			for (int m = 0; m < m_Color_keys.Count; m++)
			{
				m_Color.Add(m_Color_keys[m], m_Color_values[m]);
			}
			for (int n = 0; n < m_Material_keys.Count; n++)
			{
				m_Material.Add(m_Material_keys[n], m_Material_values[n]);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public pb_PreferenceDictionaryEnumerator GetEnumerator()
		{
			return new pb_PreferenceDictionaryEnumerator(this);
		}

		public void SetDefaultValues()
		{
			m_Bool.Clear();
			m_Int.Clear();
			m_Float.Clear();
			m_String.Clear();
			m_Color.Clear();
			m_Material.Clear();
		}

		public bool HasKey(string key)
		{
			return m_Bool.ContainsKey(key) || m_Int.ContainsKey(key) || m_Float.ContainsKey(key) || m_String.ContainsKey(key) || m_Color.ContainsKey(key) || m_Material.ContainsKey(key);
		}

		public bool HasKey<T>(string key)
		{
			Type typeFromHandle = typeof(T);
			if ((object)typeFromHandle == typeof(int))
			{
				return m_Int.ContainsKey(key);
			}
			if ((object)typeFromHandle == typeof(float))
			{
				return m_Float.ContainsKey(key);
			}
			if ((object)typeFromHandle == typeof(bool))
			{
				return m_Bool.ContainsKey(key);
			}
			if ((object)typeFromHandle == typeof(string))
			{
				return m_String.ContainsKey(key);
			}
			if ((object)typeFromHandle == typeof(Color))
			{
				return m_Color.ContainsKey(key);
			}
			if ((object)typeFromHandle == typeof(Material))
			{
				return m_Material.ContainsKey(key);
			}
			Debug.LogWarning($"HasKey<{typeof(T).ToString()}>({key}) not valid preference type.");
			return false;
		}

		public void DeleteKey(string key)
		{
			if (m_Bool.ContainsKey(key))
			{
				m_Bool.Remove(key);
			}
			if (m_Int.ContainsKey(key))
			{
				m_Int.Remove(key);
			}
			if (m_Float.ContainsKey(key))
			{
				m_Float.Remove(key);
			}
			if (m_String.ContainsKey(key))
			{
				m_String.Remove(key);
			}
			if (m_Color.ContainsKey(key))
			{
				m_Color.Remove(key);
			}
			if (m_Material.ContainsKey(key))
			{
				m_Material.Remove(key);
			}
		}

		public T Get<T>(string key, T fallback = default(T))
		{
			Type typeFromHandle = typeof(T);
			if ((object)typeFromHandle == typeof(int))
			{
				if (m_Int.ContainsKey(key))
				{
					return (T)(object)GetInt(key);
				}
			}
			else if ((object)typeFromHandle == typeof(float))
			{
				if (m_Float.ContainsKey(key))
				{
					return (T)(object)GetFloat(key);
				}
			}
			else if ((object)typeFromHandle == typeof(bool))
			{
				if (m_Bool.ContainsKey(key))
				{
					return (T)(object)GetBool(key);
				}
			}
			else if ((object)typeFromHandle == typeof(string))
			{
				if (m_String.ContainsKey(key))
				{
					return (T)(object)GetString(key);
				}
			}
			else if ((object)typeFromHandle == typeof(Color))
			{
				if (m_Color.ContainsKey(key))
				{
					return (T)(object)GetColor(key);
				}
			}
			else if ((object)typeFromHandle == typeof(Material))
			{
				if (m_Material.ContainsKey(key))
				{
					return (T)(object)GetMaterial(key);
				}
			}
			else
			{
				Debug.LogWarning($"Get<{typeof(T).ToString()}>({key}) not valid preference type.");
			}
			return fallback;
		}

		public void Set<T>(string key, T value)
		{
			object obj = value;
			if (value is int)
			{
				SetInt(key, (int)obj);
			}
			else if (value is float)
			{
				SetFloat(key, (float)obj);
			}
			else if (value is bool)
			{
				SetBool(key, (bool)obj);
			}
			else if (value is string)
			{
				SetString(key, (string)obj);
			}
			else if (value is Color)
			{
				SetColor(key, (Color)obj);
			}
			else if (value is Material)
			{
				SetMaterial(key, (Material)obj);
			}
			else
			{
				Debug.LogWarning($"Set<{typeof(T).ToString()}>({key}, {value.ToString()}) not valid preference type.");
			}
		}

		public bool GetBool(string key, bool fallback = false)
		{
			if (m_Bool.TryGetValue(key, out var value))
			{
				return value;
			}
			return fallback;
		}

		public int GetInt(string key, int fallback = 0)
		{
			if (m_Int.TryGetValue(key, out var value))
			{
				return value;
			}
			return fallback;
		}

		public float GetFloat(string key, float fallback = 0f)
		{
			if (m_Float.TryGetValue(key, out var value))
			{
				return value;
			}
			return fallback;
		}

		public string GetString(string key, string fallback = null)
		{
			if (m_String.TryGetValue(key, out var value))
			{
				return value;
			}
			return fallback;
		}

		public Color GetColor(string key, Color fallback = default(Color))
		{
			if (m_Color.TryGetValue(key, out var value))
			{
				return value;
			}
			return fallback;
		}

		public Material GetMaterial(string key, Material fallback = null)
		{
			if (m_Material.TryGetValue(key, out var value))
			{
				return value;
			}
			return fallback;
		}

		public void SetBool(string key, bool value)
		{
			if (m_Bool.ContainsKey(key))
			{
				m_Bool[key] = value;
			}
			else
			{
				m_Bool.Add(key, value);
			}
		}

		public void SetInt(string key, int value)
		{
			if (m_Int.ContainsKey(key))
			{
				m_Int[key] = value;
			}
			else
			{
				m_Int.Add(key, value);
			}
		}

		public void SetFloat(string key, float value)
		{
			if (m_Float.ContainsKey(key))
			{
				m_Float[key] = value;
			}
			else
			{
				m_Float.Add(key, value);
			}
		}

		public void SetString(string key, string value)
		{
			if (m_String.ContainsKey(key))
			{
				m_String[key] = value;
			}
			else
			{
				m_String.Add(key, value);
			}
		}

		public void SetColor(string key, Color value)
		{
			if (m_Color.ContainsKey(key))
			{
				m_Color[key] = value;
			}
			else
			{
				m_Color.Add(key, value);
			}
		}

		public void SetMaterial(string key, Material value)
		{
			if (m_Material.ContainsKey(key))
			{
				m_Material[key] = value;
			}
			else
			{
				m_Material.Add(key, value);
			}
		}

		public Dictionary<string, bool> GetBoolDictionary()
		{
			return m_Bool;
		}

		public Dictionary<string, int> GetIntDictionary()
		{
			return m_Int;
		}

		public Dictionary<string, float> GetFloatDictionary()
		{
			return m_Float;
		}

		public Dictionary<string, string> GetStringDictionary()
		{
			return m_String;
		}

		public Dictionary<string, Color> GetColorDictionary()
		{
			return m_Color;
		}

		public Dictionary<string, Material> GetMaterialDictionary()
		{
			return m_Material;
		}

		public void Clear()
		{
			m_Bool.Clear();
			m_Int.Clear();
			m_Float.Clear();
			m_String.Clear();
			m_Color.Clear();
		}
	}
}
