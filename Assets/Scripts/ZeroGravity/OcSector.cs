using System.Collections.Generic;
using UnityEngine;
using OpenHellion.Net;

namespace ZeroGravity
{
	public class OcSector : MonoBehaviour
	{
		public List<OcSector> Neighbors;

		public bool IsConnectedToExterior;

		public List<GameObject> GroupsToHide;

		private bool isActive;

		private List<Renderer> renderers = new List<Renderer>();

		private void Awake()
		{
			EventSystem.AddListener(EventSystem.InternalEventType.OcExteriorStatus, OcExteriorStatusListener);
			foreach (GameObject item in GroupsToHide)
			{
				renderers.AddRange(item.GetComponentsInChildren<Renderer>(true));
			}
			foreach (Renderer renderer in renderers)
			{
				renderer.enabled = false;
			}
		}

		public void ToggleVisibility(bool? isVisible = null)
		{
			if (!isVisible.HasValue)
			{
				isVisible = !isActive;
			}
			foreach (Renderer renderer in renderers)
			{
				renderer.enabled = isVisible.Value;
			}
			isActive = isVisible.HasValue;
		}

		private void ToggleAll(bool isVisible)
		{
			foreach (OcSector neighbor in Neighbors)
			{
				neighbor.ToggleVisibility(isVisible);
			}
			ToggleVisibility(isVisible);
		}

		public void ToggleNeighbours(bool isVisible, OcSector nextRoom = null)
		{
			foreach (OcSector neighbor in Neighbors)
			{
				if (nextRoom == null || neighbor != nextRoom)
				{
					neighbor.ToggleVisibility(isVisible);
				}
			}
		}

		public void OcExteriorStatusListener(EventSystem.InternalEventData data)
		{
			ToggleVisibility(IsConnectedToExterior && (bool)data.Objects[0]);
		}

		private void OnDestroy()
		{
			EventSystem.RemoveListener(EventSystem.InternalEventType.ShowMessageBox, OcExteriorStatusListener);
		}
	}
}
