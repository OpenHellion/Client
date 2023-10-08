using UnityEngine;

namespace RootMotion.Demos
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CapsuleCollider))]
	public abstract class CharacterBase : MonoBehaviour
	{
		[Header("Base Parameters")] [Range(1f, 4f)] [SerializeField]
		protected float gravityMultiplier = 2f;

		[SerializeField] protected float airborneThreshold = 0.6f;

		[SerializeField] private float slopeStartAngle = 50f;

		[SerializeField] private float slopeEndAngle = 85f;

		[SerializeField] private float spherecastRadius = 0.1f;

		[SerializeField] private LayerMask groundLayers;

		private PhysicMaterial zeroFrictionMaterial;

		private PhysicMaterial highFrictionMaterial;

		protected Rigidbody r;

		protected const float half = 0.5f;

		protected float originalHeight;

		protected Vector3 originalCenter;

		protected CapsuleCollider capsule;

		public abstract void Move(Vector3 deltaPosition, Quaternion deltaRotation);

		protected virtual void Start()
		{
			capsule = GetComponent<Collider>() as CapsuleCollider;
			r = GetComponent<Rigidbody>();
			originalHeight = capsule.height;
			originalCenter = capsule.center;
			zeroFrictionMaterial = new PhysicMaterial();
			zeroFrictionMaterial.dynamicFriction = 0f;
			zeroFrictionMaterial.staticFriction = 0f;
			zeroFrictionMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
			zeroFrictionMaterial.bounciness = 0f;
			zeroFrictionMaterial.bounceCombine = PhysicMaterialCombine.Minimum;
			highFrictionMaterial = new PhysicMaterial();
			r.constraints = RigidbodyConstraints.FreezeRotation;
		}

		protected virtual RaycastHit GetSpherecastHit()
		{
			Vector3 up = base.transform.up;
			Ray ray = new Ray(r.position + up * airborneThreshold, -up);
			RaycastHit hitInfo = default(RaycastHit);
			Physics.SphereCast(ray, spherecastRadius, out hitInfo, airborneThreshold * 2f, groundLayers);
			return hitInfo;
		}

		public float GetAngleFromForward(Vector3 worldDirection)
		{
			Vector3 vector = base.transform.InverseTransformDirection(worldDirection);
			return Mathf.Atan2(vector.x, vector.z) * 57.29578f;
		}

		protected void RigidbodyRotateAround(Vector3 point, Vector3 axis, float angle)
		{
			Quaternion quaternion = Quaternion.AngleAxis(angle, axis);
			Vector3 vector = base.transform.position - point;
			r.MovePosition(point + quaternion * vector);
			r.MoveRotation(quaternion * base.transform.rotation);
		}

		protected void ScaleCapsule(float mlp)
		{
			if (capsule.height != originalHeight * mlp)
			{
				capsule.height = Mathf.MoveTowards(capsule.height, originalHeight * mlp, Time.deltaTime * 4f);
				capsule.center = Vector3.MoveTowards(capsule.center, originalCenter * mlp, Time.deltaTime * 2f);
			}
		}

		protected void HighFriction()
		{
			capsule.material = highFrictionMaterial;
		}

		protected void ZeroFriction()
		{
			capsule.material = zeroFrictionMaterial;
		}

		protected float GetSlopeDamper(Vector3 velocity, Vector3 groundNormal)
		{
			float num = 90f - Vector3.Angle(velocity, groundNormal);
			num -= slopeStartAngle;
			float num2 = slopeEndAngle - slopeStartAngle;
			return 1f - Mathf.Clamp(num / num2, 0f, 1f);
		}
	}
}
