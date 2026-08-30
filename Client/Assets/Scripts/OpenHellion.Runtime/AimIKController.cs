using System.Collections;
using RootMotion.FinalIK;
using UnityEngine;
using ZeroGravity.Objects;

public class AimIKController : MonoBehaviour
{
	public AimIK aimIK;

	[SerializeField] private AnimatorHelper animHelper;

	public float weightLerpSpeed;

	public bool IKEnabled()
	{
		return aimIK.enabled;
	}

	public void UpdateIKBones()
	{
		aimIK.solver.bones[0].transform = animHelper.GetBone(AnimatorHelper.HumanBones.Spine1);
		aimIK.solver.bones[1].transform = animHelper.GetBone(AnimatorHelper.HumanBones.Spine2);
	}

	public void ToggleIK(bool enabled, bool overrideBehaviour = false)
	{
		if (IsItemUsingIK() || overrideBehaviour)
		{
			if (enabled && !overrideBehaviour)
			{
				aimIK.enabled = enabled;
			}
			else if (!enabled && overrideBehaviour)
			{
				aimIK.enabled = enabled;
				animHelper.SetLayerWeight(AnimatorHelper.AnimatorLayers_TPS.MouseLookVertical, 1f);
			}

			StartCoroutine(LerpIKWeight(enabled));
		}
	}

	public bool IsItemUsingIK()
	{
		if (GetComponentInParent<OtherPlayer>().Inventory.ItemInHands != null)
		{
			return GetComponentInParent<OtherPlayer>().Inventory.ItemInHands.useIkForTargeting;
		}

		return false;
	}

	private IEnumerator LerpIKWeight(bool enabled)
	{
		float animWeightFrom = animHelper.GetLayerWeight(AnimatorHelper.AnimatorLayers_TPS.MouseLookVertical);
		float animWeightTo = ((!enabled) ? 1 : 0);
		float lerpFrom = aimIK.solver.IKPositionWeight;
		float lerpTo = ((!enabled) ? 0f : 1f);
		float lerpHelper = 0f;
		while (lerpHelper < 1f)
		{
			lerpHelper += weightLerpSpeed * Time.deltaTime;
			aimIK.solver.IKPositionWeight = Mathf.Lerp(lerpFrom, lerpTo, lerpHelper);
			animHelper.SetLayerWeight(AnimatorHelper.AnimatorLayers_TPS.MouseLookVertical,
				Mathf.Lerp(animWeightFrom, animWeightTo, lerpHelper));
			yield return null;
		}

		if (!enabled)
		{
			aimIK.enabled = false;
		}
	}
}
