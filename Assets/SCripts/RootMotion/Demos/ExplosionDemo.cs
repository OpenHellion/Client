using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	public class ExplosionDemo : MonoBehaviour
	{
		public SimpleLocomotion character;

		public float forceMlp = 1f;

		public float upForce = 1f;

		public float weightFalloffSpeed = 1f;

		public AnimationCurve weightFalloff;

		public AnimationCurve explosionForceByDistance;

		public AnimationCurve scale;

		private float weight;

		private Vector3 defaultScale = Vector3.one;

		private Rigidbody r;

		private FullBodyBipedIK ik;

		private void Start()
		{
			defaultScale = base.transform.localScale;
			r = character.GetComponent<Rigidbody>();
			ik = character.GetComponent<FullBodyBipedIK>();
		}

		private void Update()
		{
			weight = Mathf.Clamp(weight - Time.deltaTime * weightFalloffSpeed, 0f, 1f);
			if (Input.GetKeyDown(KeyCode.E) && character.isGrounded)
			{
				ik.solver.IKPositionWeight = 1f;
				ik.solver.leftHandEffector.position = ik.solver.leftHandEffector.bone.position;
				ik.solver.rightHandEffector.position = ik.solver.rightHandEffector.bone.position;
				ik.solver.leftFootEffector.position = ik.solver.leftFootEffector.bone.position;
				ik.solver.rightFootEffector.position = ik.solver.rightFootEffector.bone.position;
				weight = 1f;
				Vector3 vector = r.position - base.transform.position;
				vector.y = 0f;
				float num = explosionForceByDistance.Evaluate(vector.magnitude);
				r.velocity = (vector.normalized + Vector3.up * upForce) * num * forceMlp;
			}
			if (weight < 0.5f && character.isGrounded)
			{
				weight = Mathf.Clamp(weight - Time.deltaTime * 3f, 0f, 1f);
			}
			SetEffectorWeights(weightFalloff.Evaluate(weight));
			base.transform.localScale = scale.Evaluate(weight) * defaultScale;
		}

		private void SetEffectorWeights(float w)
		{
			ik.solver.leftHandEffector.positionWeight = w;
			ik.solver.rightHandEffector.positionWeight = w;
			ik.solver.leftFootEffector.positionWeight = w;
			ik.solver.rightFootEffector.positionWeight = w;
		}
	}
}
