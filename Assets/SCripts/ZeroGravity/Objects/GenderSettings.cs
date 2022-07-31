using System;
using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	public class GenderSettings : MonoBehaviour
	{
		[Serializable]
		public class GenderItem
		{
			public Gender Gender;

			public Transform Outfit;

			public Transform HeadCameraParent;

			public List<SkinnedMeshRenderer> ArmSkins;
		}

		public List<GenderItem> settings;
	}
}
