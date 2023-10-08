using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	[RequireComponent(typeof(Rigidbody))]
	public class SceneMoveablePlatform : MonoBehaviour
	{
		private List<SpaceObjectTransferable> objectOnPlatform = new List<SpaceObjectTransferable>();

		private Vector3 oldPosition;

		private bool isApplicationQuitting;

		private void Awake()
		{
			oldPosition = base.transform.position;
			GetComponent<Rigidbody>().isKinematic = true;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.GetComponent<SpaceObjectTransferable>() != null)
			{
				SpaceObjectTransferable component = other.GetComponent<SpaceObjectTransferable>();
				if (!objectOnPlatform.Contains(component))
				{
					component.OnPlatform = this;
					objectOnPlatform.Add(component);
				}
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (other.GetComponent<SpaceObjectTransferable>() != null)
			{
				SpaceObjectTransferable component = other.GetComponent<SpaceObjectTransferable>();
				if (objectOnPlatform.Contains(component))
				{
					component.OnPlatform = null;
					objectOnPlatform.Remove(component);
				}
			}
		}

		public void RemoveFromPlatform(SpaceObjectTransferable obj)
		{
			if (objectOnPlatform.Contains(obj))
			{
				obj.OnPlatform = null;
				objectOnPlatform.Remove(obj);
			}
		}

		private void OnDisable()
		{
			if (isApplicationQuitting || objectOnPlatform.Count <= 0)
			{
				return;
			}

			foreach (SpaceObjectTransferable item in objectOnPlatform)
			{
				item.OnPlatform = null;
				objectOnPlatform.Remove(item);
			}
		}

		private void OnApplicationQuit()
		{
			isApplicationQuitting = true;
		}

		private void LateUpdate()
		{
			if (!base.transform.hasChanged)
			{
				return;
			}

			if (objectOnPlatform.Count > 0)
			{
				Vector3 vector = base.transform.position - oldPosition;
				foreach (SpaceObjectTransferable item in objectOnPlatform)
				{
					if (item is OtherPlayer)
					{
						item.ModifyPositionAndRotation(vector);
					}
					else
					{
						item.transform.localPosition += vector;
					}
				}
			}

			oldPosition = base.transform.position;
		}
	}
}
