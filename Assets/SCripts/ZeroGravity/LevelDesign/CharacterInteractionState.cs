using UnityEngine;

namespace ZeroGravity.LevelDesign
{
	public class CharacterInteractionState : MonoBehaviour
	{
		public bool UseAnimationCamera;

		public bool AutoFreeLook = true;

		public Transform InteractPosition;

		public Transform InteractLookAt;

		public bool ImmediatePositionChange;

		public AnimatorHelper.InteractType InteractType;

		public AnimatorHelper.LockType LockType;

		public AnimatorHelper.LockType ImmediateLockType;

		public bool SetColliderToKinematic;

		[HideInInspector]
		public SceneTriggerExecuter Executer;
	}
}
