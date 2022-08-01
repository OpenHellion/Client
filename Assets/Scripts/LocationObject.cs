using UnityEngine;

[CreateAssetMenu(fileName = "Location", menuName = "Helpers/Location")]
public class LocationObject : ScriptableObject
{
	public string LocationName;

	public string Name
	{
		get
		{
			if (LocationName != string.Empty)
			{
				return LocationName;
			}
			return base.name;
		}
	}

	public static string GetNameFromObject(LocationObject location)
	{
		string empty = string.Empty;
		if (location != null)
		{
			empty = location.Name;
		}
		return empty;
	}
}
