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

	public float Distance => (float)(MyPlayer.Instance.Parent.Position - ArtificialBody.Position).Magnitude;

	public float DistanceFromCamera => ((ArtificialBody.Position - MyPlayer.Instance.Parent.Position).ToVector3() -
	                                    Quaternion.LookRotation(MyPlayer.Instance.Parent.Forward,
		                                    MyPlayer.Instance.Parent.Up) *
	                                    MyPlayer.Instance.FpsController.MainCamera.transform.position).magnitude;

	public float AngleFromCameraForward
	{
		get
		{
			Quaternion quaternion =
				Quaternion.LookRotation(MyPlayer.Instance.Parent.Forward, MyPlayer.Instance.Parent.Up);
			Vector3 to = quaternion * MyPlayer.Instance.FpsController.MainCamera.transform.forward;
			return Vector3.Angle(
				((ArtificialBody.Position - MyPlayer.Instance.Parent.Position).ToVector3() -
				 quaternion * MyPlayer.Instance.FpsController.MainCamera.transform.position).normalized, to);
		}
	}
}
