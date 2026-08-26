using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneTag", menuName = "Helpers/SceneTag")]
public class SceneTagObject : ScriptableObject
{
	public static string TagsToString(List<SceneTagObject> tagObjects)
	{
		string text = string.Empty;
		for (int i = 0; i < tagObjects.Count; i++)
		{
			text += tagObjects[i].name;
			if (i + 1 != tagObjects.Count)
			{
				text += ";";
			}
		}

		return text;
	}
}
