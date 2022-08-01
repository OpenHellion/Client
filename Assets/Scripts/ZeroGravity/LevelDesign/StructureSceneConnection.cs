using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.LevelDesign
{
	public class StructureSceneConnection : MonoBehaviour, ISceneObject
	{
		[Tooltip("InSceneID will be assigned automatically")]
		[SerializeField]
		private int _inSceneID;

		public StructureConnection.ConnectionType Type;

		[Tooltip("If true connection point is output (other structure connect to that point)")]
		public bool IsOut;

		public List<GameObject> EnabledObjects;

		public List<GameObject> DisabledObjects;

		public SceneTriggerRoom ConnectionRoom;

		public int InSceneID
		{
			get
			{
				return _inSceneID;
			}
			set
			{
				_inSceneID = value;
			}
		}

		public void ToggleObjects(bool showEnabled)
		{
			if (DisabledObjects != null && DisabledObjects.Count > 0)
			{
				foreach (GameObject disabledObject in DisabledObjects)
				{
					if (disabledObject != null)
					{
						disabledObject.SetActive(!showEnabled);
					}
				}
			}
			if (EnabledObjects == null || EnabledObjects.Count <= 0)
			{
				return;
			}
			foreach (GameObject enabledObject in EnabledObjects)
			{
				if (enabledObject != null)
				{
					enabledObject.SetActive(showEnabled);
				}
			}
		}
	}
}
