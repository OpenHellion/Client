using UnityEngine;
using ZeroGravity;
using ZeroGravity.Objects;

public class TargetObject
{
	public ArtificialBody AB;

	public string Name
	{
		get
		{
			if (AB is Asteroid)
			{
				return (AB as Asteroid).Name;
			}
			if (AB is Pivot)
			{
				return (AB as Pivot).GetComponentInChildren<OtherPlayer>().PlayerName;
			}
			if (AB is Ship)
			{
				return (AB as Ship).CommandVesselName;
			}
			return "Unknown";
		}
	}

	public Sprite Icon
	{
		get
		{
			if (AB is Asteroid)
			{
				return Client.Instance.SpriteManager.GetSprite((AB as Asteroid).Type);
			}
			if (AB is Pivot)
			{
				return Client.Instance.SpriteManager.GetSprite((AB as Pivot).Type);
			}
			if (AB is SpaceObjectVessel)
			{
				return Client.Instance.SpriteManager.GetSprite(AB as SpaceObjectVessel, true);
			}
			return Client.Instance.SpriteManager.GetSprite(AB.Type);
		}
	}

	public float Distance
	{
		get
		{
			return (float)(MyPlayer.Instance.Parent.Position - AB.Position).Magnitude;
		}
	}

	public float DistanceFromCamera
	{
		get
		{
			return ((AB.Position - MyPlayer.Instance.Parent.Position).ToVector3() - Quaternion.LookRotation(MyPlayer.Instance.Parent.Forward, MyPlayer.Instance.Parent.Up) * MyPlayer.Instance.FpsController.MainCamera.transform.position).magnitude;
		}
	}

	public float AngleFromCameraForward
	{
		get
		{
			Quaternion quaternion = Quaternion.LookRotation(MyPlayer.Instance.Parent.Forward, MyPlayer.Instance.Parent.Up);
			Vector3 to = quaternion * MyPlayer.Instance.FpsController.MainCamera.transform.forward;
			return Vector3.Angle(((AB.Position - MyPlayer.Instance.Parent.Position).ToVector3() - quaternion * MyPlayer.Instance.FpsController.MainCamera.transform.position).normalized, to);
		}
	}
}
