using System.Collections.Generic;
using UnityEngine;
using OpenHellion.Net;

namespace ZeroGravity
{
	public class OcSector : MonoBehaviour
	{
		public List<OcSector> Neighbors;

		public List<GameObject> GroupsToHide;

		private bool _isActive;

		private readonly List<Renderer> _renderers = new List<Renderer>();

		private void Awake()
		{
			foreach (GameObject item in GroupsToHide)
			{
				_renderers.AddRange(item.GetComponentsInChildren<Renderer>(true));
			}

			foreach (Renderer renderer in _renderers)
			{
				renderer.enabled = false;
			}
		}

		public void ToggleVisibility(bool? isVisible = null)
		{
			if (!isVisible.HasValue)
			{
				isVisible = !_isActive;
			}

			foreach (Renderer renderer in _renderers)
			{
				renderer.enabled = isVisible.Value;
			}

			_isActive = isVisible.HasValue;
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
	}
}
