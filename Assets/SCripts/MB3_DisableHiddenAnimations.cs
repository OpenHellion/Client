using System.Collections.Generic;
using UnityEngine;

public class MB3_DisableHiddenAnimations : MonoBehaviour
{
	public List<Animation> animationsToCull = new List<Animation>();

	private void Start()
	{
		if (GetComponent<SkinnedMeshRenderer>() == null)
		{
			Debug.LogError("The MB3_CullHiddenAnimations script was placed on and object " + base.name + " which has no SkinnedMeshRenderer attached");
		}
	}

	private void OnBecameVisible()
	{
		for (int i = 0; i < animationsToCull.Count; i++)
		{
			if (animationsToCull[i] != null)
			{
				animationsToCull[i].enabled = true;
			}
		}
	}

	private void OnBecameInvisible()
	{
		for (int i = 0; i < animationsToCull.Count; i++)
		{
			if (animationsToCull[i] != null)
			{
				animationsToCull[i].enabled = false;
			}
		}
	}
}
