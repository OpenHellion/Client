using UnityEngine;
using UnityEngine.UI;
using ZeroGravity;
using ZeroGravity.Objects;
using ZeroGravity.UI;

public class HelmetRadarTargetElement : MonoBehaviour
{
	[SerializeField]
	private new Text name;

	[SerializeField]
	private Text DistanceText;

	public GameObject NameSelected;

	public ArtificialBody AB;

	public GameObject TargetMarker;

	public HelmetTargetOnScreen TargetOnScreen;

	public Image Icon;

	private bool isSelected;

	private float distance;

	public bool IsSelected
	{
		get
		{
			return isSelected;
		}
		set
		{
			isSelected = value;
			NameSelected.SetActive(value);
		}
	}

	public string Name
	{
		get
		{
			return name.text;
		}
		set
		{
			name.text = value;
		}
	}

	public float Distance
	{
		get
		{
			return distance;
		}
		set
		{
			distance = value;
			if (value < 1000f)
			{
				DistanceText.text = value.ToString("f1") + "m";
			}
			else
			{
				DistanceText.text = (distance / 1000f).ToString("f1") + "km";
			}
		}
	}

	public float DistanceFromCamera
	{
		get
		{
			return ((AB.Position - MyPlayer.Instance.Parent.Position).ToVector3() - MyPlayer.Instance.transform.position).magnitude;
		}
	}

	public float AngleFromCameraForward
	{
		get
		{
			Vector3 forward = MyPlayer.Instance.FpsController.MainCamera.transform.forward;
			Vector3 from = (AB.Position - MyPlayer.Instance.Parent.Position).ToVector3() - MyPlayer.Instance.transform.position;
			return Vector3.Angle(from, forward);
		}
	}
}
