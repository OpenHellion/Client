using UnityEngine;
using OpenHellion;
using ZeroGravity;
using ZeroGravity.Objects;
using ZeroGravity.UI;

public class TargetObject
{
	public ArtificialBody ArtificialBody;

	public string Name
	{
		get
		{
			if (ArtificialBody is Asteroid)
			{
				return (ArtificialBody as Asteroid).Name;
			}

			if (ArtificialBody is Pivot)
			{
				return (ArtificialBody as Pivot).GetComponentInChildren<OtherPlayer>().PlayerName;
			}

			if (ArtificialBody is Ship)
			{
				return (ArtificialBody as Ship).CommandVesselName;
			}

			return "Unknown";
		}
	}

	public Sprite Icon
	{
		get
		{
			if (ArtificialBody is Asteroid)
			{
				return SpriteManager.Instance.GetSprite((ArtificialBody as Asteroid).Type);
			}

			if (ArtificialBody is Pivot)
			{
				return SpriteManager.Instance.GetSprite((ArtificialBody as Pivot).Type);
			}

			if (ArtificialBody is SpaceObjectVessel)
			{
				return SpriteManager.Instance.GetSprite(ArtificialBody as SpaceObjectVessel, true);
			}

			return SpriteManager.Instance.GetSprite(ArtificialBody.Type);
		}
	}

	public float Distance => (float)(MyPlayer.Instance.Parent.transform.position - ArtificialBody.transform.position).magnitude;

	public float AngleFromCameraForward
	{
		get
		{
			Vector3 to = MyPlayer.Instance.transform.rotation * MyPlayer.Instance.FpsController.MainCamera.transform.forward;
			return Vector3.Angle(
				(ArtificialBody.transform.position - MyPlayer.Instance.transform.rotation * MyPlayer.Instance.FpsController.MainCamera.transform.position).normalized, to);
		}
	}
}
