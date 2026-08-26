using UnityEngine;

public class Decal : MonoBehaviour
{
	[SerializeField] private Material myMaterial;

	[SerializeField] private int mySortingLayer;

	public Material material
	{
		get { return myMaterial; }
		set { myMaterial = value; }
	}

	public int sortingLayer
	{
		get { return mySortingLayer; }
	}
}
