using UnityEngine;

namespace RootMotion.Demos
{
	[RequireComponent(typeof(Animator))]
	public class CharacterAnimationSimple : CharacterAnimationBase
	{
		[SerializeField]
		private CharacterThirdPerson characterController;

		[SerializeField]
		private float pivotOffset;

		[SerializeField]
		private AnimationCurve moveSpeed;

		private Animator animator;

		protected override void Start()
		{
			base.Start();
			animator = GetComponentInChildren<Animator>();
		}

		public override Vector3 GetPivotPoint()
		{
			if (pivotOffset == 0f)
			{
				return base.transform.position;
			}
			return base.transform.position + base.transform.forward * pivotOffset;
		}

		private void Update()
		{
			float num = moveSpeed.Evaluate(characterController.animState.moveDirection.z);
			animator.SetFloat("Speed", num);
			characterController.Move(characterController.transform.forward * Time.deltaTime * num, Quaternion.identity);
		}
	}
}
