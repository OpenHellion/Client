using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	[RequireComponent(typeof(FullBodyBipedIK))]
	public class PendulumExample : MonoBehaviour
	{
		[Tooltip("The master weight of this script.")]
		[Range(0f, 1f)]
		public float weight = 1f;

		[Tooltip("Multiplier for the distance of the root to the target.")]
		public float hangingDistanceMlp = 1.3f;

		[Tooltip("Where does the root of the character land when weight is blended out?")]
		[HideInInspector]
		public Vector3 rootTargetPosition;

		[Tooltip("How is the root of the character rotated when weight is blended out?")]
		[HideInInspector]
		public Quaternion rootTargetRotation;

		[SerializeField]
		private Transform target;

		[SerializeField]
		private Transform leftHandTarget;

		[SerializeField]
		private Transform rightHandTarget;

		[SerializeField]
		private Transform leftFootTarget;

		[SerializeField]
		private Transform rightFootTarget;

		[SerializeField]
		private Transform pelvisTarget;

		[SerializeField]
		private Transform bodyTarget;

		[SerializeField]
		private Transform headTarget;

		[SerializeField]
		private Vector3 pelvisDownAxis = Vector3.right;

		private FullBodyBipedIK ik;

		private Quaternion rootRelativeToPelvis;

		private Vector3 pelvisToRoot;

		private float lastWeight;

		private void Start()
		{
			ik = GetComponent<FullBodyBipedIK>();
			Quaternion rotation = target.rotation;
			target.rotation = leftHandTarget.rotation;
			FixedJoint fixedJoint = target.gameObject.AddComponent<FixedJoint>();
			fixedJoint.connectedBody = leftHandTarget.GetComponent<Rigidbody>();
			target.GetComponent<Rigidbody>().MoveRotation(rotation);
			rootRelativeToPelvis = Quaternion.Inverse(pelvisTarget.rotation) * base.transform.rotation;
			pelvisToRoot = Quaternion.Inverse(ik.references.pelvis.rotation) * (base.transform.position - ik.references.pelvis.position);
			rootTargetPosition = base.transform.position;
			rootTargetRotation = base.transform.rotation;
			lastWeight = weight;
		}

		private void LateUpdate()
		{
			if (weight > 0f)
			{
				ik.solver.leftHandEffector.positionWeight = weight;
				ik.solver.leftHandEffector.rotationWeight = weight;
			}
			else
			{
				rootTargetPosition = base.transform.position;
				rootTargetRotation = base.transform.rotation;
				if (lastWeight > 0f)
				{
					ik.solver.leftHandEffector.positionWeight = 0f;
					ik.solver.leftHandEffector.rotationWeight = 0f;
				}
			}
			lastWeight = weight;
			if (!(weight <= 0f))
			{
				base.transform.position = Vector3.Lerp(rootTargetPosition, pelvisTarget.position + pelvisTarget.rotation * pelvisToRoot * hangingDistanceMlp, weight);
				base.transform.rotation = Quaternion.Lerp(rootTargetRotation, pelvisTarget.rotation * rootRelativeToPelvis, weight);
				ik.solver.leftHandEffector.position = leftHandTarget.position;
				ik.solver.leftHandEffector.rotation = leftHandTarget.rotation;
				Vector3 fromDirection = ik.references.pelvis.rotation * pelvisDownAxis;
				Quaternion b = Quaternion.FromToRotation(fromDirection, rightHandTarget.position - headTarget.position);
				ik.references.rightUpperArm.rotation = Quaternion.Lerp(Quaternion.identity, b, weight) * ik.references.rightUpperArm.rotation;
				Quaternion b2 = Quaternion.FromToRotation(fromDirection, leftFootTarget.position - bodyTarget.position);
				ik.references.leftThigh.rotation = Quaternion.Lerp(Quaternion.identity, b2, weight) * ik.references.leftThigh.rotation;
				Quaternion b3 = Quaternion.FromToRotation(fromDirection, rightFootTarget.position - bodyTarget.position);
				ik.references.rightThigh.rotation = Quaternion.Lerp(Quaternion.identity, b3, weight) * ik.references.rightThigh.rotation;
			}
		}
	}
}
