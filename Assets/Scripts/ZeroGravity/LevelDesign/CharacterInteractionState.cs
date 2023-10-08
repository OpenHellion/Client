using UnityEngine;
using UnityEngine.Serialization;

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

		[FormerlySerializedAs("Executer")] [HideInInspector]
		public SceneTriggerExecutor Executor;
	}
}
