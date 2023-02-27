using System;
using System.Collections.Generic;
using System.Linq;
using OpenHellion.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using ZeroGravity;
using ZeroGravity.Math;
using ZeroGravity.Objects;
using ZeroGravity.UI;

public class HelmetRadar : MonoBehaviour
{
	public bool CanRadarWork;

	public GameObject RadarRoot;

	public GameObject listTransform;

	public GameObject UIPrefab;

	private List<HelmetRadarTargetElement> radarItems = new List<HelmetRadarTargetElement>();

	private HelmetRadarTargetElement currentTarget;

	public Transform listParent;

	private bool isTargettingListActive = true;

	private int currTargetIndex;

	[SerializeField]
	private GameObject HasTargetPanel;

	[SerializeField]
	private Text OnSpeedValText;

	[SerializeField]
	private Text OffSpeedValText;

	[SerializeField]
	private Text DistanceValText;

	[SerializeField]
	private Text TargetNameText;

	private HelmetRadarTargetElement hoveredTarget;

	public Transform TargetParent;

	public GameObject RadarTarget;

	public bool RadarActive;

	private Helmet Helmet => MyPlayer.Instance.CurrentHelmet;

	private float radarScanningRange => 10000f * Helmet.TierMultiplier;

	public int CurrTargetIndex
	{
		get
		{
			return currTargetIndex;
		}
		set
		{
			if (value <= radarItems.Count - 1 && value >= 0)
			{
				currTargetIndex = value;
			}
		}
	}

	private void Start()
	{
		RefreshRadar();
	}

	public void RefreshRadar()
	{
		if (MyPlayer.Instance.CurrentHelmet != null && MyPlayer.Instance.CurrentHelmet.IsVisorActive)
		{
			CanRadarWork = MyPlayer.Instance.Parent is Pivot || MyPlayer.Instance.Parent is Asteroid;
			ApplyFilter();
			ReloadRadarElements();
			ToggleTargetting(CanRadarWork);
		}
		else
		{
			ToggleTargetting(status: false);
		}
	}

	private void Update()
	{
		if (!Client.IsGameBuild || MyPlayer.Instance.Parent is Ship)
		{
			return;
		}
		if (!Client.Instance.IsChatOpened)
		{
			if (radarItems.Count > 0 && InputManager.GetButtonDown(InputManager.ConfigAction.HelmetRadar))
			{
				ToggleRadar();
			}
			if (!Client.Instance.CanvasManager.ConsoleIsUp)
			{
				if (hoveredTarget != null && Mouse.current.leftButton.isPressed)
				{
					currentTarget = hoveredTarget;
					currentTarget.IsSelected = true;
					GoToCurrentElement();
				}
				if (Mouse.current.scroll.y.ReadValue().IsNotEpsilonZero() && radarItems.Count > 0)
				{
					float axis = Mouse.current.scroll.y.ReadValue();
					if (axis > 0f)
					{
						if (currTargetIndex - 1 >= 0)
						{
							currTargetIndex--;
						}
						else
						{
							currTargetIndex = radarItems.Count - 1;
						}
						currentTarget = radarItems[currTargetIndex];
						currentTarget.IsSelected = true;
						GoToCurrentElement();
					}
					else if (axis < 0f)
					{
						if (radarItems.Count - 1 > currTargetIndex)
						{
							currTargetIndex++;
						}
						else
						{
							currTargetIndex = 0;
						}
						currentTarget = radarItems[currTargetIndex];
						currentTarget.IsSelected = true;
						GoToCurrentElement();
					}
				}
				if (radarItems.Count > 0 && InputManager.GetButtonDown(InputManager.ConfigAction.TargetDown))
				{
					if (radarItems.Count - 1 >= currTargetIndex + 1)
					{
						currTargetIndex++;
					}
					else
					{
						currTargetIndex = 0;
					}
					currentTarget = radarItems[currTargetIndex];
					currentTarget.IsSelected = true;
					GoToCurrentElement();
				}
				else if (radarItems.Count > 0 && InputManager.GetButtonDown(InputManager.ConfigAction.TargetUp))
				{
					if (currentTarget == null)
					{
						currentTarget = radarItems[0];
					}
					currentTarget.IsSelected = false;
					if (currTargetIndex - 1 >= 0)
					{
						currTargetIndex--;
					}
					else
					{
						currTargetIndex = radarItems.Count - 1;
					}
					currentTarget = radarItems[currTargetIndex];
					currentTarget.IsSelected = true;
					GoToCurrentElement();
				}
			}
		}
		if (!CanRadarWork)
		{
			return;
		}
		if (radarItems.Count > 0 && currentTarget != null && currentTarget.AB != null)
		{
			Vector3 vector = currentTarget.AB.transform.position - MyPlayer.Instance.transform.position;
			Vector3 vector2 = (currentTarget.AB.Velocity - MyPlayer.Instance.Parent.Velocity - MyPlayer.Instance.rigidBody.velocity.ToVector3D()).ToVector3();
			Vector3 vector3 = Vector3.Project(vector2, vector.normalized);
			Vector3 vector4 = Vector3.ProjectOnPlane(vector2, vector.normalized);
			OnSpeedValText.text = ((0f - vector3.magnitude) * (float)MathHelper.Sign(Vector3.Dot(vector.normalized, vector3.normalized))).ToString("f1");
			OffSpeedValText.text = vector4.magnitude.ToString("f1");
			if (currentTarget.Distance > 1000f)
			{
				DistanceValText.text = (currentTarget.Distance / 1000f).ToString("f1") + "km";
			}
			else
			{
				DistanceValText.text = currentTarget.Distance.ToString("f1") + "m";
			}
		}
		else
		{
			OnSpeedValText.text = string.Empty;
			OffSpeedValText.text = string.Empty;
			TargetNameText.text = string.Empty;
			DistanceValText.text = string.Empty;
		}
		ReloadRadarElements();
	}

	private void ToggleRadar()
	{
		RadarRoot.SetActive(!RadarRoot.activeInHierarchy);
		ToggleTargetting(RadarRoot.activeInHierarchy);
	}

	private void GoToCurrentElement()
	{
		int num = radarItems.IndexOf(currentTarget);
	}

	private void ReloadRadarElements()
	{
		Vector3 position = MyPlayer.Instance.FpsController.MainCamera.transform.position;
		List<HelmetRadarTargetElement> list2 = radarItems.Where((HelmetRadarTargetElement m) => m.AB == null || !m.AB.gameObject.activeInHierarchy || !(m.AB is SpaceObjectVessel) || !(m.AB as SpaceObjectVessel).IsMainVessel || (MyPlayer.Instance.Parent.Position - m.AB.Position).Magnitude > (double)radarScanningRange).ToList();
		foreach (HelmetRadarTargetElement item in list2)
		{
			radarItems.Remove(item);
			Destroy(item.gameObject);
			Destroy(item.TargetMarker.gameObject);
		}
		if (!radarItems.Contains(currentTarget) && radarItems.Count > 0)
		{
			currTargetIndex = 0;
			currentTarget = radarItems[currTargetIndex];
			currentTarget.IsSelected = true;
		}
		List<ArtificialBody> list = radarItems.Select((HelmetRadarTargetElement m) => m.AB).ToList();
		foreach (ArtificialBody item2 in Client.Instance.SolarSystem.ArtificialBodies.Where((ArtificialBody m) => m is SpaceObjectVessel && (m as SpaceObjectVessel).IsMainVessel && (MyPlayer.Instance.Parent.Position - m.Position).Magnitude <= (double)radarScanningRange && !list.Contains(m)))
		{
			CreateUIElement(item2);
		}
		for (int i = 0; i < radarItems.Count; i++)
		{
			radarItems[i].transform.SetSiblingIndex(i);
			float distance = Vector3.Distance(MyPlayer.Instance.FpsController.MainCamera.transform.position, radarItems[i].AB.transform.position);
			radarItems[i].Distance = distance;
			if (radarItems[i].AB is Ship)
			{
				radarItems[i].Name = (radarItems[i].AB as Ship).CustomName.ToUpper();
			}
		}
		if (currentTarget == null && radarItems.Count != 0)
		{
			currentTarget = radarItems[0];
			currentTarget.IsSelected = true;
		}
		currTargetIndex = radarItems.IndexOf(currentTarget);
		if (currTargetIndex < 0)
		{
			currTargetIndex = 0;
		}
		if (radarItems.Count > 0)
		{
			SetTarget(radarItems[currTargetIndex]);
		}
		else
		{
			SetTarget();
		}
		foreach (HelmetRadarTargetElement radarItem in radarItems)
		{
			if (currentTarget != null && radarItem != currentTarget)
			{
				radarItem.IsSelected = false;
			}
			Vector3 position2 = (radarItem.AB.Position - MyPlayer.Instance.Parent.Position).ToVector3() - MyPlayer.Instance.FpsController.MainCamera.transform.position;
			if (position2.magnitude < 1000f)
			{
				position2 = radarItem.AB.transform.position;
				if (radarItem.AB is Pivot)
				{
					Pivot pivot = radarItem.AB as Pivot;
					if (pivot.ChildType == SpaceObjectType.Player)
					{
						Player player = Client.Instance.GetPlayer(pivot.GUID);
						if (player != null)
						{
							position2 += player.transform.position;
						}
					}
				}
			}
			Vector3 b = MyPlayer.Instance.FpsController.MainCamera.WorldToScreenPoint(position2);
			if (b.z > 0f)
			{
				radarItem.TargetMarker.transform.position = Vector3.Lerp(radarItem.TargetMarker.transform.position, b, 0.5f);
				radarItem.TargetMarker.transform.rotation = Quaternion.identity;
				radarItem.TargetMarker.SetActive(value: true);
			}
			else
			{
				radarItem.TargetMarker.SetActive(value: false);
			}
			hoveredTarget = radarItems.OrderBy((HelmetRadarTargetElement m) => m.DistanceFromCamera).FirstOrDefault((HelmetRadarTargetElement m) => m.AngleFromCameraForward < 5f);
			radarItem.TargetOnScreen.IsHovered.SetActive(hoveredTarget == radarItem);
			radarItem.TargetOnScreen.IsSelected.SetActive(radarItem.IsSelected);
			radarItem.TargetOnScreen.HoveredName.text = radarItem.Name;
			SetTargetSprite(radarItem);
		}
		GoToCurrentElement();
	}

	private void CreateUIElement(ArtificialBody ab)
	{
		if ((!(ab is Ship) || (ab as SpaceObjectVessel).IsMainVessel) && !(ab as SpaceObjectVessel).IsDebrisFragment)
		{
			GameObject gameObject = Instantiate(UIPrefab, listParent);
			gameObject.transform.localScale = Vector3.one;
			gameObject.SetActive(value: true);
			HelmetRadarTargetElement component = gameObject.GetComponent<HelmetRadarTargetElement>();
			component.IsSelected = false;
			component.AB = ab;
			if (currentTarget == null)
			{
				currentTarget = component;
				currentTarget.IsSelected = true;
			}
			radarItems.Add(component);
			if (ab is Asteroid)
			{
				component.Name = (ab as Asteroid).Name.ToUpper();
				component.Icon.sprite = Client.Instance.SpriteManager.GetSprite((ab as Asteroid).Type);
			}
			else if (ab is Pivot)
			{
				component.Name = (ab as Pivot).GetComponentInChildren<OtherPlayer>().PlayerName.ToUpper();
				component.Icon.sprite = Client.Instance.SpriteManager.GetSprite((ab as Pivot).Type);
			}
			else if (ab is SpaceObjectVessel)
			{
				component.Name = (ab as SpaceObjectVessel).CustomName.ToUpper();
				component.Icon.sprite = Client.Instance.SpriteManager.GetSprite(ab as SpaceObjectVessel, checkDocked: true);
			}
			GameObject gameObject2 = Instantiate(RadarTarget, TargetParent);
			gameObject2.transform.localScale = Vector3.one;
			gameObject2.transform.Rotate(Vector3.zero);
			component.TargetMarker = gameObject2;
			component.TargetOnScreen = gameObject2.GetComponent<HelmetTargetOnScreen>();
		}
	}

	private void SetTargetSprite(HelmetRadarTargetElement target)
	{
		if (target.AB is Asteroid)
		{
			target.Icon.sprite = Client.Instance.SpriteManager.GetSprite((target.AB as Asteroid).Type);
		}
		else if (target.AB is Pivot)
		{
			target.Icon.sprite = Client.Instance.SpriteManager.GetSprite((target.AB as Pivot).Type);
		}
		else if (target.AB is SpaceObjectVessel)
		{
			target.Icon.sprite = Client.Instance.SpriteManager.GetSprite(target.AB as SpaceObjectVessel, checkDocked: true);
		}
	}

	private void ApplyFilter()
	{
		if (currentTarget != null)
		{
			currentTarget.IsSelected = false;
		}
		if (radarItems.Count == 0)
		{
			SetTarget();
		}
		else if (radarItems.Contains(currentTarget))
		{
			currTargetIndex = radarItems.IndexOf(currentTarget);
			if (currTargetIndex < 0)
			{
				currTargetIndex = 0;
			}
			SetTarget(radarItems[currTargetIndex]);
		}
		else
		{
			currTargetIndex = 0;
			SetTarget(radarItems[0]);
			currentTarget = radarItems[0];
		}
		if (currentTarget != null)
		{
			currentTarget.IsSelected = true;
		}
		GoToCurrentElement();
	}

	private void SetTarget(HelmetRadarTargetElement target = null)
	{
		if (target != null && target.AB != null)
		{
			TargetNameText.text = target.Name;
			HasTargetPanel.SetActive(value: true);
			ToggleStarDust(value: true);
		}
		else
		{
			HasTargetPanel.SetActive(value: false);
			ToggleTargettingList(status: false);
			RadarTarget.SetActive(value: false);
		}
	}

	public void ToggleTargetting(bool status)
	{
		if (!status || RadarRoot.activeInHierarchy)
		{
			CanRadarWork = status;
			ToggleTargettingList(status);
			if (!status)
			{
				SetTarget();
			}
		}
	}

	public void ToggleTargettingList(bool status)
	{
		if (!status || RadarRoot.activeInHierarchy)
		{
			RadarActive = status;
			TargetParent.gameObject.SetActive(status);
			ToggleStarDust(status);
			isTargettingListActive = status;
		}
	}

	public void ToggleStarDust(bool value)
	{
		if (value && Helmet.IsVisorActive)
		{
			if (RadarActive && radarItems.Count > 0)
			{
				ParticleSystem.MainModule main = MyPlayer.Instance.FpsController.StarDustParticle.main;
				main.customSimulationSpace = radarItems[currTargetIndex].AB.transform;
				MyPlayer.Instance.FpsController.StarDustParticle.gameObject.SetActive(value: true);
				MyPlayer.Instance.FpsController.StarDustParticle.Play();
			}
		}
		else
		{
			MyPlayer.Instance.FpsController.StarDustParticle.gameObject.SetActive(value: false);
			MyPlayer.Instance.FpsController.StarDustParticle.Stop();
		}
	}
}
