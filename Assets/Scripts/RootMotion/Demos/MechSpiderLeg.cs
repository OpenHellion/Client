using System.Collections;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	public class MechSpiderLeg : MonoBehaviour
	{
		public MechSpider mechSpider;

		public MechSpiderLeg unSync;

		public Vector3 offset;

		public float minDelay = 0.2f;

		public float maxOffset = 1f;

		public float stepSpeed = 5f;

		public float footHeight = 0.15f;

		public float velocityPrediction = 0.2f;

		public float raycastFocus = 0.1f;

		public AnimationCurve yOffset;

		public ParticleSystem sand;

		private IK ik;

		private float stepProgress = 1f;

		private float lastStepTime;

		private Vector3 defaultPosition;

		private RaycastHit hit = default(RaycastHit);

		public bool isStepping
		{
			get { return stepProgress < 1f; }
		}

		public Vector3 position
		{
			get { return ik.GetIKSolver().GetIKPosition(); }
			set { ik.GetIKSolver().SetIKPosition(value); }
		}

		private void Start()
		{
			ik = GetComponent<IK>();
			stepProgress = 1f;
			hit = default(RaycastHit);
			IKSolver.Point[] points = ik.GetIKSolver().GetPoints();
			position = points[points.Length - 1].transform.position;
			hit.point = position;
			defaultPosition = mechSpider.transform.InverseTransformPoint(position + offset * mechSpider.scale);
		}

		private Vector3 GetStepTarget(out bool stepFound, float focus, float distance)
		{
			stepFound = false;
			Vector3 vector = mechSpider.transform.TransformPoint(defaultPosition);
			vector += (hit.point - position) * velocityPrediction;
			Vector3 up = mechSpider.transform.up;
			Vector3 rhs = mechSpider.body.position - position;
			Vector3 axis = Vector3.Cross(up, rhs);
			up = Quaternion.AngleAxis(focus, axis) * up;
			if (Physics.Raycast(vector + up * mechSpider.raycastHeight * mechSpider.scale, -up, out hit,
				    mechSpider.raycastHeight * mechSpider.scale + distance, mechSpider.raycastLayers))
			{
				stepFound = true;
			}

			return hit.point + mechSpider.transform.up * footHeight * mechSpider.scale;
		}

		private void Update()
		{
			if (!isStepping && !(Time.time < lastStepTime + minDelay) && (!(unSync != null) || !unSync.isStepping))
			{
				bool stepFound = false;
				Vector3 stepTarget = GetStepTarget(out stepFound, raycastFocus,
					mechSpider.raycastDistance * mechSpider.scale);
				if (!stepFound)
				{
					stepTarget = GetStepTarget(out stepFound, 0f - raycastFocus,
						mechSpider.raycastDistance * 3f * mechSpider.scale);
				}

				if (stepFound && !(Vector3.Distance(position, stepTarget) <
				                   maxOffset * mechSpider.scale * Random.Range(0.9f, 1.2f)))
				{
					StopAllCoroutines();
					StartCoroutine(Step(position, stepTarget));
				}
			}
		}

		private IEnumerator Step(Vector3 stepStartPosition, Vector3 targetPosition)
		{
			stepProgress = 0f;
			while (stepProgress < 1f)
			{
				stepProgress += Time.deltaTime * stepSpeed;
				position = Vector3.Lerp(stepStartPosition, targetPosition, stepProgress);
				position += mechSpider.transform.up * yOffset.Evaluate(stepProgress) * mechSpider.scale;
				yield return null;
			}

			position = targetPosition;
			if (sand != null)
			{
				sand.transform.position = position - mechSpider.transform.up * footHeight * mechSpider.scale;
				sand.Emit(20);
			}

			lastStepTime = Time.time;
		}
	}
}
