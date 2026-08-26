using System.Collections.Generic;
using UnityEngine;

namespace ProBuilder2.Common
{
	public class pb_Shortcut
	{
		public string action;

		public string description;

		public KeyCode key;

		public EventModifiers eventModifiers;

		public pb_Shortcut(string a, string d, KeyCode k, EventModifiers e)
		{
			action = a;
			description = d;
			key = k;
			eventModifiers = e;
		}

		public pb_Shortcut(string str)
		{
			try
			{
				string[] array = str.Split('-');
				action = array[0];
				description = array[1];
				if (int.TryParse(array[2], out var result))
				{
					key = (KeyCode)result;
				}
				if (int.TryParse(array[3], out result))
				{
					eventModifiers = (EventModifiers)result;
				}
			}
			catch
			{
				Debug.LogWarning("Failed parsing shortcut: " + str);
			}
		}

		public bool Matches(KeyCode key, EventModifiers modifiers)
		{
			return this.key == key && eventModifiers == modifiers;
		}

		public static int IndexOf(pb_Shortcut[] shortcuts, KeyCode k, EventModifiers e)
		{
			for (int i = 0; i < shortcuts.Length; i++)
			{
				if (shortcuts[i].key == k && shortcuts[i].eventModifiers == e)
				{
					return i;
				}
			}
			return -1;
		}

		public static IEnumerable<pb_Shortcut> DefaultShortcuts()
		{
			List<pb_Shortcut> list = new List<pb_Shortcut>();
			list.Add(new pb_Shortcut("Escape", "Top Level", KeyCode.Escape, EventModifiers.None));
			list.Add(new pb_Shortcut("Toggle Geometry Mode", "Geometry Level", KeyCode.G, EventModifiers.None));
			list.Add(new pb_Shortcut("Toggle Selection Mode", "Toggle Selection Mode.  If Toggle Mode Shortcuts is disabled, this shortcut does not apply.", KeyCode.H, EventModifiers.None));
			list.Add(new pb_Shortcut("Set Trigger", "Sets all selected objects to entity type Trigger.", KeyCode.T, EventModifiers.None));
			list.Add(new pb_Shortcut("Set Occluder", "Sets all selected objects to entity type Occluder.", KeyCode.O, EventModifiers.None));
			list.Add(new pb_Shortcut("Set Collider", "Sets all selected objects to entity type Collider.", KeyCode.C, EventModifiers.None));
			list.Add(new pb_Shortcut("Set Mover", "Sets all selected objects to entity type Mover.", KeyCode.M, EventModifiers.None));
			list.Add(new pb_Shortcut("Set Detail", "Sets all selected objects to entity type Brush.", KeyCode.B, EventModifiers.None));
			list.Add(new pb_Shortcut("Toggle Handle Pivot", "Toggles the orientation of the ProBuilder selection handle.", KeyCode.P, EventModifiers.None));
			list.Add(new pb_Shortcut("Set Pivot", "Center pivot around current selection.", KeyCode.J, EventModifiers.Command));
			list.Add(new pb_Shortcut("Delete Face", "Deletes all selected faces.", KeyCode.Backspace, EventModifiers.FunctionKey));
			list.Add(new pb_Shortcut("Vertex Mode", "Enter Vertex editing mode.  Automatically swaps to Element level editing.", KeyCode.H, EventModifiers.None));
			list.Add(new pb_Shortcut("Edge Mode", "Enter Edge editing mode.  Automatically swaps to Element level editing.", KeyCode.J, EventModifiers.None));
			list.Add(new pb_Shortcut("Face Mode", "Enter Face editing mode.  Automatically swaps to Element level editing.", KeyCode.K, EventModifiers.None));
			return list;
		}

		public static IEnumerable<pb_Shortcut> ParseShortcuts(string str)
		{
			if (str == null || str.Length < 3)
			{
				return DefaultShortcuts();
			}
			string[] array = str.Split('*');
			pb_Shortcut[] array2 = new pb_Shortcut[array.Length];
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] = new pb_Shortcut(array[i]);
			}
			return array2;
		}

		public override string ToString()
		{
			return $"{action}: {key.ToString()}, {eventModifiers.ToString()} ({(int)eventModifiers})";
		}

		public string Serialize()
		{
			action = action.Replace("-", " ").Replace("*", string.Empty);
			description = description.Replace("-", " ").Replace("*", string.Empty);
			return action + "-" + description + "-" + (int)key + "-" + (int)eventModifiers;
		}

		public static string ShortcutsToString(pb_Shortcut[] shortcuts)
		{
			string text = string.Empty;
			for (int i = 0; i < shortcuts.Length; i++)
			{
				text += shortcuts[i].Serialize();
				if (i != shortcuts.Length - 1)
				{
					text += "*";
				}
			}
			return text;
		}
	}
}
