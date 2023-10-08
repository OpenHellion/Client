using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public class GenericPoser : Poser
	{
		[Serializable]
		public class Map
		{
			public Transform bone;

			public Transform target;

			private Vector3 defaultLocalPosition;

			private Quaternion defaultLocalRotation;

			public Map(Transform bone, Transform target)
			{
				this.bone = bone;
				this.target = target;
				StoreDefaultState();
			}

			public void StoreDefaultState()
			{
				defaultLocalPosition = bone.localPosition;
				defaultLocalRotation = bone.localRotation;
			}

			public void FixTransform()
			{
				bone.localPosition = defaultLocalPosition;
				bone.localRotation = defaultLocalRotation;
			}

			public void Update(float localRotationWeight, float localPositionWeight)
			{
				bone.localRotation = Quaternion.Lerp(bone.localRotation, target.localRotation, localRotationWeight);
				bone.localPosition = Vector3.Lerp(bone.localPosition, target.localPosition, localPositionWeight);
			}
		}

		public Map[] maps;

		[ContextMenu("Auto-Mapping")]
		public override void AutoMapping()
		{
			if (poseRoot == null)
			{
				maps = new Map[0];
				return;
			}

			maps = new Map[0];
			Transform[] componentsInChildren = base.transform.GetComponentsInChildren<Transform>();
			Transform[] componentsInChildren2 = poseRoot.GetComponentsInChildren<Transform>();
			for (int i = 1; i < componentsInChildren.Length; i++)
			{
				Transform targetNamed = GetTargetNamed(componentsInChildren[i].name, componentsInChildren2);
				if (targetNamed != null)
				{
					Array.Resize(ref maps, maps.Length + 1);
					maps[maps.Length - 1] = new Map(componentsInChildren[i], targetNamed);
				}
			}

			StoreDefaultState();
		}

		public override void StoreDefaultState()
		{
			for (int i = 0; i < maps.Length; i++)
			{
				maps[i].StoreDefaultState();
			}
		}

		public override void FixTransforms()
		{
			for (int i = 0; i < maps.Length; i++)
			{
				maps[i].FixTransform();
			}
		}

		private Transform GetTargetNamed(string tName, Transform[] array)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].name == tName)
				{
					return array[i];
				}
			}

			return null;
		}

		private void LateUpdate()
		{
			if (!(weight <= 0f) && (!(localPositionWeight <= 0f) || !(localRotationWeight <= 0f)) &&
			    !(poseRoot == null))
			{
				float num = localRotationWeight * weight;
				float num2 = localPositionWeight * weight;
				for (int i = 0; i < maps.Length; i++)
				{
					maps[i].Update(num, num2);
				}
			}
		}
	}
}
