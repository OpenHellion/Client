using DigitalOpus.MB.Core;
using UnityEngine;

public class MB2_UpdateSkinnedMeshBoundsFromBones : MonoBehaviour
{
	private SkinnedMeshRenderer smr;

	private Transform[] bones;

	private void Start()
	{
		smr = GetComponent<SkinnedMeshRenderer>();
		if (smr == null)
		{
			Debug.LogError(
				"Need to attach MB2_UpdateSkinnedMeshBoundsFromBones script to an object with a SkinnedMeshRenderer component attached.");
			return;
		}

		bones = smr.bones;
		smr.updateWhenOffscreen = true;
		smr.updateWhenOffscreen = false;
	}

	private void Update()
	{
		if (smr != null)
		{
			MB3_MeshCombiner.UpdateSkinnedMeshApproximateBoundsFromBonesStatic(bones, smr);
		}
	}
}
