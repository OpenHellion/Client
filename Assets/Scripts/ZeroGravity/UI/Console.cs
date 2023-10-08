using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class Console : MonoBehaviour
	{
		[Tooltip("Text in this field will be displayed when user requests help in console")] [TextArea(10, 20)]
		public string HelpText;

		public InputField inputField;

		public Text ConsoleText;

		public GameObject MainCanvas;

		public ConsoleComand[] comandHelper;

		private Dictionary<string, string> comands = new Dictionary<string, string>();

		private void Start()
		{
			MainCanvas.SetActive(false);
			ConsoleComand[] array = comandHelper;
			for (int i = 0; i < array.Length; i++)
			{
				ConsoleComand consoleComand = array[i];
				comands.Add(consoleComand.shortcut, consoleComand.command);
			}
		}

		private void Update()
		{
			if (Keyboard.current.backquoteKey.isPressed)
			{
				MainCanvas.SetActive(!MainCanvas.activeSelf);
				inputField.ActivateInputField();
				inputField.Select();
				MyPlayer.Instance.FpsController.ToggleMovement(!MainCanvas.activeSelf);
				MyPlayer.Instance.FpsController.ToggleCameraMovement(!MainCanvas.activeSelf);
			}

			if (Keyboard.current.escapeKey.isPressed)
			{
				MainCanvas.SetActive(false);
				if (MyPlayer.Instance != null)
				{
					MyPlayer.Instance.FpsController.ToggleMovement(false);
					MyPlayer.Instance.FpsController.ToggleCameraMovement(false);
				}
			}
		}

		public void OnTextChange()
		{
			if (inputField.text.Length <= 1)
			{
				inputField.text = string.Empty;
				return;
			}

			string[] array = inputField.text.Split(" ".ToCharArray(), 2);
			string command = inputField.text;
			if (array.Length == 2)
			{
				string[] array2 = array[1].Split(new char[1] { '=' }, 2);
				if (!comands.ContainsKey(array2[0]))
				{
					ConsoleText.text = "Wrong command format, enter \"help\" for more info";
					inputField.text = string.Empty;
					return;
				}

				command = array[0] + " " + comands[array2[0]] +
				          ((array2.Length != 2) ? string.Empty : ("=" + array2[1]));
			}

			try
			{
				ConsoleText.text = ExecuteCommand(command);
			}
			catch (Exception ex)
			{
				ConsoleText.text = ex.Message;
			}

			inputField.text = string.Empty;
		}

		public string GetHelpText()
		{
			return
				"Available command types are: set, get, execute, save, load.\nSet command format: set FieldName=newValue\nGet command format: get FieldName\nExec command format: exec FunctionName\n\n" +
				HelpText;
		}

		public string ExecuteCommand(string command)
		{
			string[] array = command.Split(new char[1] { ' ' }, 2);
			if (array.Length == 1 && array[0] == "help")
			{
				return GetHelpText();
			}

			if (array.Length == 1 && array[0] == "save")
			{
				saveSettings();
				return "Settings saved.";
			}

			if (array.Length == 1 && array[0] == "load")
			{
				loadSettings();
				return "Settings loadad.";
			}

			if (array.Length == 1 && array[0] == "list")
			{
				return listSettings();
			}

			if (array.Length != 2)
			{
				throw new Exception("Wrong command format, enter \"help\" for more info");
			}

			string[] array2 = array[1].Split(new string[1] { "->" }, StringSplitOptions.None);
			if (array2.Length != 3)
			{
				throw new Exception("Wrong command format, enter \"help\" for more info");
			}

			string[] array3 = array2[2].Split(new char[1] { '=' }, 2);
			if (array3[0].Length == 0 || (array[0] == "set" && array3.Length != 2))
			{
				throw new Exception("Wrong command format, enter \"help\" for more info");
			}

			GameObject gameObject = GameObject.Find(array2[0]);
			if (gameObject == null)
			{
				throw new Exception("Cannot find game object \"" + array2[0] + "\"");
			}

			Component component = gameObject.GetComponent(array2[1]);
			if (component == null)
			{
				throw new Exception("Cannot find component \"" + array2[1] + "\"");
			}

			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
			                           BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
			string[] array4 = array3[0].Split('/');
			MemberInfo[] member = component.GetType().GetMember(array4[0], bindingAttr);
			if (member == null || member.Length == 0)
			{
				throw new Exception("Cannot find component member \"" + array4[0] + "\"");
			}

			object obj = component;
			object parent = null;
			MemberInfo parentMemberInfo = null;
			if (array4.Length > 1)
			{
				if (array[0] == "exec")
				{
					throw new Exception("Only root methods can be executed.");
				}

				for (int i = 1; i < array4.Length; i++)
				{
					parent = obj;
					parentMemberInfo = member[0];
					if (member[0].MemberType == MemberTypes.Field)
					{
						obj = ((FieldInfo)member[0]).GetValue(obj);
						member = ((FieldInfo)member[0]).FieldType.GetMember(array4[i], bindingAttr);
					}
					else
					{
						if (member[0].MemberType != MemberTypes.Property)
						{
							throw new Exception("Cannot find child member \"" + array4[i] + "\"");
						}

						obj = ((PropertyInfo)member[0]).GetValue(obj, null);
						member = ((PropertyInfo)member[0]).PropertyType.GetMember(array4[i], bindingAttr);
					}

					if (member == null || member.Length == 0)
					{
						throw new Exception("Cannot find child member \"" + array4[i] + "\"");
					}
				}
			}

			MemberInfo memberInfo = member[0];
			if ((array[0] == "set" || array[0] == "get") && memberInfo.MemberType != MemberTypes.Field &&
			    memberInfo.MemberType != MemberTypes.Property)
			{
				throw new Exception("Member type for get and set commands must be field or property");
			}

			if (array[0] == "exec" && memberInfo.MemberType != MemberTypes.Method)
			{
				throw new Exception("Member type for exec command must be method without parameters");
			}

			if (array[0] == "set")
			{
				if (obj.GetType().IsValueType)
				{
					if (!SetMemberValue(parent, parentMemberInfo, obj, memberInfo, array3[1]))
					{
						throw new Exception("Failed to set value \"" + array3[1] + "\" to member \"" + array3[0] +
						                    "\"");
					}
				}
				else if (!SetMemberValue(obj, memberInfo, array3[1]))
				{
					throw new Exception("Failed to set value \"" + array3[1] + "\" to member \"" + array3[0] + "\"");
				}
			}
			else if (array[0] == "get")
			{
				if (memberInfo.MemberType == MemberTypes.Field)
				{
					return ((FieldInfo)memberInfo).GetValue(obj).ToString();
				}

				if (memberInfo.MemberType == MemberTypes.Property)
				{
					return ((PropertyInfo)memberInfo).GetValue(obj, null).ToString();
				}
			}
			else if (array[0] == "exec")
			{
				((MethodInfo)memberInfo).Invoke(obj, null);
			}

			return "OK";
		}

		private bool SetMemberValue(object parent, MemberInfo parentMemberInfo, object obj, MemberInfo membInfo,
			string value)
		{
			bool result = SetMemberValue(obj, membInfo, value);
			try
			{
				if (parentMemberInfo.MemberType == MemberTypes.Field)
				{
					((FieldInfo)parentMemberInfo).SetValue(parent, obj);
					return result;
				}

				if (parentMemberInfo.MemberType == MemberTypes.Property)
				{
					((PropertyInfo)parentMemberInfo).SetValue(parent, obj, null);
					return result;
				}

				return result;
			}
			catch
			{
				return false;
			}
		}

		private bool SetMemberValue(object obj, MemberInfo membInfo, string value)
		{
			try
			{
				if (membInfo.MemberType == MemberTypes.Field)
				{
					FieldInfo fieldInfo = (FieldInfo)membInfo;
					if (fieldInfo.FieldType == typeof(float))
					{
						fieldInfo.SetValue(obj, float.Parse(value));
					}
					else if (fieldInfo.FieldType == typeof(int))
					{
						fieldInfo.SetValue(obj, int.Parse(value));
					}
					else
					{
						if (!(fieldInfo.FieldType == typeof(string)))
						{
							return false;
						}

						fieldInfo.SetValue(obj, value);
					}
				}
				else if (membInfo.MemberType == MemberTypes.Property)
				{
					PropertyInfo propertyInfo = (PropertyInfo)membInfo;
					if (propertyInfo.PropertyType == typeof(float))
					{
						propertyInfo.SetValue(obj, float.Parse(value), null);
					}
					else if (propertyInfo.PropertyType == typeof(int))
					{
						propertyInfo.SetValue(obj, int.Parse(value), null);
					}
					else
					{
						if (!(propertyInfo.PropertyType == typeof(string)))
						{
							return false;
						}

						propertyInfo.SetValue(obj, value, null);
					}
				}
			}
			catch (Exception)
			{
				return false;
			}

			return true;
		}

		private string listSettings()
		{
			StringBuilder stringBuilder = new StringBuilder();
			ConsoleComand[] array = comandHelper;
			for (int i = 0; i < array.Length; i++)
			{
				ConsoleComand consoleComand = array[i];
				try
				{
					stringBuilder.Append(consoleComand.shortcut).Append("=")
						.Append(ExecuteCommand("get " + consoleComand.command))
						.Append("\r\n");
				}
				catch
				{
				}
			}

			return stringBuilder.ToString();
		}

		private void saveSettings()
		{
			StringBuilder stringBuilder = new StringBuilder();
			ConsoleComand[] array = comandHelper;
			for (int i = 0; i < array.Length; i++)
			{
				ConsoleComand consoleComand = array[i];
				try
				{
					stringBuilder.Append(consoleComand.shortcut).Append("=")
						.Append(ExecuteCommand("get " + consoleComand.command))
						.Append("\r\n");
				}
				catch
				{
				}
			}

			File.WriteAllText("console_settings.dat", stringBuilder.ToString());
		}

		private void loadSettings()
		{
			string[] array = File.ReadAllLines("console_settings.dat");
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string[] array2 = array;
			foreach (string text in array2)
			{
				try
				{
					string[] array3 = text.Split("=".ToCharArray(), 2);
					dictionary.Add(array3[0], array3[1]);
				}
				catch
				{
				}
			}

			ConsoleComand[] array4 = comandHelper;
			for (int j = 0; j < array4.Length; j++)
			{
				ConsoleComand consoleComand = array4[j];
				try
				{
					ExecuteCommand("set " + consoleComand.command + "=" + dictionary[consoleComand.shortcut]);
				}
				catch
				{
				}
			}
		}
	}
}
