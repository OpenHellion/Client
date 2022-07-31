using System.Collections.Generic;
using UnityEngine;

public class MB_SwapShirts : MonoBehaviour
{
	public MB3_MeshBaker meshBaker;

	public Renderer[] clothingAndBodyPartsBareTorso;

	public Renderer[] clothingAndBodyPartsBareTorsoDamagedArm;

	public Renderer[] clothingAndBodyPartsHoodie;

	private void OnGUI()
	{
		if (GUILayout.Button("Wear Hoodie"))
		{
			ChangeOutfit(clothingAndBodyPartsHoodie);
		}
		if (GUILayout.Button("Bare Torso"))
		{
			ChangeOutfit(clothingAndBodyPartsBareTorso);
		}
		if (GUILayout.Button("Damaged Arm"))
		{
			ChangeOutfit(clothingAndBodyPartsBareTorsoDamagedArm);
		}
	}

	private void ChangeOutfit(Renderer[] outfit)
	{
		List<GameObject> list = new List<GameObject>();
		foreach (GameObject item in meshBaker.meshCombiner.GetObjectsInCombined())
		{
			Renderer component = item.GetComponent<Renderer>();
			bool flag = false;
			for (int i = 0; i < outfit.Length; i++)
			{
				if (component == outfit[i])
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(component.gameObject);
				Debug.Log("Removing " + component.gameObject);
			}
		}
		List<GameObject> list2 = new List<GameObject>();
		for (int j = 0; j < outfit.Length; j++)
		{
			if (!meshBaker.meshCombiner.GetObjectsInCombined().Contains(outfit[j].gameObject))
			{
				list2.Add(outfit[j].gameObject);
				Debug.Log("Adding " + outfit[j].gameObject);
			}
		}
		meshBaker.AddDeleteGameObjects(list2.ToArray(), list.ToArray(), true);
		meshBaker.Apply();
	}
}
