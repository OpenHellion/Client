using UnityEngine;

public class NavigationPanelObject
{
	public enum Type
	{
		Planets = 1,
		Stations = 2,
		Asteroids = 3,
		Ships = 4
	}

	public long ObjectID;

	public Type ObjectType;

	public string Text;

	public TextAnchor Alignment;

	public float Height;
}
