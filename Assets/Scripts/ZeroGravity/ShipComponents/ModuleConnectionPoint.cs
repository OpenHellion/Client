using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZeroGravity.ShipComponents
{
	[Serializable]
	public class ModuleConnectionPoint
	{
		public enum PointType
		{
			Front = 0,
			Back = 1,
			Left = 2,
			Right = 3,
			Top = 4,
			Bottom = 5
		}

		public PointType Type;

		public List<GameObject> EnabledObjects;

		public List<GameObject> DisabledObjects;

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
