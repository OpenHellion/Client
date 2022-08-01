using System.Collections;
using TeamUtility.IO;
using UnityEngine;

namespace ZeroGravity.CharacterMovement
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CapsuleCollider))]
	public class RigidbodyFirstPersonControllerMod : MonoBehaviour
	{
		public float ForwardSpeed = 8f;

		public float BackwardSpeed = 4f;

		public float StrafeSpeed = 4f;

		public float RunMultiplier = 2f;

		public KeyCode RunKey = KeyCode.LeftShift;

		private float JumpForce = 30f;

		public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90f, 1f), new Keyframe(0f, 1f), new Keyframe(90f, 0f));

		public float forwardCone;

		public float leprCone;

		public float CurrentTargetSpeed = 8f;

		public float crouchMultiplier = 2f;

		public float groundCheckDistance = 0.01f;

		public float stickToGroundHelperDistance = 0.5f;

		public Camera cam;

		private Rigidbody m_RigidBody;

		private CapsuleCollider m_Capsule;

		private Vector3 m_GroundContactNormal;

		private bool m_Jump;

		private bool m_PreviouslyGrounded;

		private bool m_Jumping;

		private bool m_IsGrounded;

		private bool flying = true;

		private float timerForJump;

		public float maxJumpTime = 3f;

		public float minJumpTime = 0.5f;

		public float lookSpeed = 2f;

		public float moveSpeed = 0.3f;

		public float rotationX;

		public float rotationY;

		public float maxJump;

		public float minJump;

		public float normalHeight;

		public float crouchHeight;

		private bool crouch;

		private bool sprint;

		public float airMultiplier;

		public float jumpBlockTime;

		private float normalFwdSpeed;

		private float strafeNormalSpeed;

		private float backwardNormalSpeed;

		private float airSpeed = 7f;

		private float strafeAirSpeed = 5f;

		private float backwardAirSpeed = 5f;

		private bool gotGrounded = true;

		private bool speedLerpingDone;

		public Vector3 Velocity
		{
			get
			{
				return m_RigidBody.velocity;
			}
		}

		public bool Grounded
		{
			get
			{
				return m_IsGrounded;
			}
		}

		public bool Jumping
		{
			get
			{
				return m_Jumping;
			}
		}

		public bool Running
		{
			get
			{
				return false;
			}
		}

		private void Start()
		{
			Object.DontDestroyOnLoad(base.gameObject);
			m_RigidBody = GetComponent<Rigidbody>();
			m_Capsule = GetComponent<CapsuleCollider>();
			m_Capsule.height = normalHeight;
			normalFwdSpeed = ForwardSpeed;
			strafeNormalSpeed = StrafeSpeed;
			backwardNormalSpeed = BackwardSpeed;
			if (crouchHeight / 2f <= m_Capsule.radius)
			{
				Debug.Log("Cucanj?!.... Nece da moze!");
			}
			setFlying(true);
		}

		public void UpdateDesiredTargetSpeed(Vector2 input)
		{
			if (input == Vector2.zero)
			{
				return;
			}
			if (m_IsGrounded)
			{
				if (speedLerpingDone)
				{
					SetNormalSpeed();
				}
				if (sprint)
				{
					SpeedIncrease(RunMultiplier);
				}
				if (crouch)
				{
					SetNormalSpeed();
					SpeedDecrease(crouchMultiplier);
				}
			}
			else
			{
				ForwardSpeed = airSpeed;
				StrafeSpeed = strafeAirSpeed;
				BackwardSpeed = backwardAirSpeed;
			}
			if (input.y <= 0f)
			{
				CurrentTargetSpeed = BackwardSpeed;
				return;
			}
			float num = Mathf.Abs(Mathf.Atan2(input.x, input.y) * 57.29578f);
			if (num > forwardCone && num < leprCone)
			{
				CurrentTargetSpeed = Mathf.Lerp(ForwardSpeed, StrafeSpeed, (num - forwardCone) / (leprCone - forwardCone));
			}
			else if (num >= leprCone)
			{
				CurrentTargetSpeed = StrafeSpeed;
			}
			else
			{
				CurrentTargetSpeed = ForwardSpeed;
			}
		}

		public void setFlying(bool isOn)
		{
			flying = isOn;
			m_Capsule.enabled = !isOn;
			m_RigidBody.isKinematic = isOn;
			Camera.main.GetComponent<MouseLookModded>().enabled = !isOn;
		}

		private IEnumerator AntiJumpSpam()
		{
			yield return new WaitForSeconds(jumpBlockTime);
			gotGrounded = true;
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				GameObject.Find("MotusMan_v2_mod").transform.position = GameObject.Find("SpawnPoint").transform.position;
				setFlying(true);
			}
			if (Input.GetKeyDown(KeyCode.F))
			{
				setFlying(!flying);
			}
			if (!crouch && speedLerpingDone)
			{
				if (InputManager.GetButton("Jump") && !m_Jump)
				{
					timerForJump += Time.deltaTime;
				}
				if (InputManager.GetButtonUp("Jump") && !m_Jump)
				{
					if (!(timerForJump > minJumpTime))
					{
						timerForJump = 0f;
						return;
					}
					if (timerForJump > maxJumpTime)
					{
						timerForJump = maxJumpTime;
					}
					JumpForce = minJump + (maxJump - minJump) * ((timerForJump - minJumpTime) / (maxJumpTime - minJumpTime));
					m_Jump = true;
					timerForJump = 0f;
				}
				if (InputManager.GetButtonDown("Sprint"))
				{
					sprint = true;
				}
				if (InputManager.GetButtonUp("Sprint"))
				{
					sprint = false;
				}
			}
			if (InputManager.GetButtonDown("Crouch"))
			{
				SetNormalSpeed();
				SpeedDecrease(crouchMultiplier);
				crouch = true;
			}
			if (InputManager.GetButtonUp("Crouch"))
			{
				speedLerpingDone = false;
				crouch = false;
			}
			if (crouch)
			{
				m_Capsule.center = new Vector3(0f, Mathf.Lerp(m_Capsule.center.y, 0.38f, Time.deltaTime * 4f), 0f);
				m_Capsule.height = Mathf.Lerp(m_Capsule.height, 0.81f, Time.deltaTime * 4f);
				return;
			}
			RaycastHit hitInfo;
			Physics.SphereCast(base.transform.position, m_Capsule.radius - 0.02f, Vector3.up, out hitInfo, normalHeight - m_Capsule.radius);
			if (hitInfo.transform == null)
			{
				ForwardSpeed = Mathf.Lerp(ForwardSpeed, normalFwdSpeed, Time.deltaTime * 4f);
				BackwardSpeed = (StrafeSpeed = Mathf.Lerp(BackwardSpeed, backwardNormalSpeed, Time.deltaTime * 4f));
				m_Capsule.center = new Vector3(0f, Mathf.Lerp(m_Capsule.center.y, 0.89f, Time.deltaTime * 4f), 0f);
				m_Capsule.height = Mathf.Lerp(m_Capsule.height, 1.8f, Time.deltaTime * 4f);
			}
			if ((double)ForwardSpeed > (double)normalFwdSpeed - 0.5)
			{
				speedLerpingDone = true;
			}
		}

		private void SpeedIncrease(float incr)
		{
			ForwardSpeed *= incr;
			StrafeSpeed *= incr;
			BackwardSpeed *= incr;
		}

		private void SpeedDecrease(float decr)
		{
			ForwardSpeed /= decr;
			StrafeSpeed /= decr;
			BackwardSpeed /= decr;
		}

		public void SetAirSpeed()
		{
			airSpeed /= airMultiplier;
			strafeAirSpeed /= airMultiplier;
			backwardAirSpeed /= airMultiplier;
			MonoBehaviour.print("Obrisati kad QA zavrsi testiranje");
		}

		public void SetNormalSpeed()
		{
			ForwardSpeed = normalFwdSpeed;
			StrafeSpeed = strafeNormalSpeed;
			BackwardSpeed = backwardNormalSpeed;
		}

		private void OnApplicationFocus(bool focus)
		{
			if (!focus)
			{
				m_Capsule.height = normalHeight;
				ForwardSpeed = normalFwdSpeed;
				StrafeSpeed = strafeNormalSpeed;
				BackwardSpeed = backwardNormalSpeed;
				timerForJump = 0f;
				crouch = false;
			}
		}

		private void FixedUpdate()
		{
			if (flying)
			{
				Vector2 input = GetInput();
				rotationX += InputManager.GetAxis("LookHorizontal") * lookSpeed;
				rotationY += InputManager.GetAxis("LookVertical") * lookSpeed;
				base.transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
				base.transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);
				base.transform.position += base.transform.forward * moveSpeed * input.y;
				base.transform.position += base.transform.right * moveSpeed * input.x;
			}
			GroundCheck();
			Vector2 input2 = GetInput();
			if (Mathf.Abs(input2.x) > float.Epsilon || Mathf.Abs(input2.y) > float.Epsilon)
			{
				Vector3 vector = base.transform.forward * input2.y + base.transform.right * input2.x;
				vector = Vector3.ProjectOnPlane(vector, m_GroundContactNormal).normalized;
				vector.x *= CurrentTargetSpeed;
				vector.z *= CurrentTargetSpeed;
				vector.y *= CurrentTargetSpeed;
				if (m_RigidBody.velocity.sqrMagnitude < CurrentTargetSpeed * CurrentTargetSpeed)
				{
					m_RigidBody.AddForce(vector * SlopeMultiplier(), ForceMode.Impulse);
				}
			}
			if (m_IsGrounded)
			{
				m_RigidBody.drag = 8f;
				if (m_Jump && gotGrounded)
				{
					m_RigidBody.drag = 0f;
					m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x, 0f, m_RigidBody.velocity.z);
					m_RigidBody.AddForce(new Vector3(0f, JumpForce, 0f), ForceMode.Impulse);
					m_Jumping = true;
				}
				if (!m_Jumping && Mathf.Abs(input2.x) < float.Epsilon && Mathf.Abs(input2.y) < float.Epsilon && m_RigidBody.velocity.magnitude < 1f)
				{
					m_RigidBody.Sleep();
				}
			}
			else
			{
				if (m_Jumping)
				{
					gotGrounded = false;
				}
				m_RigidBody.drag = 0f;
				if (m_PreviouslyGrounded && !m_Jumping)
				{
					StickToGroundHelper();
				}
			}
			m_Jump = false;
		}

		private float SlopeMultiplier()
		{
			float time = Vector3.Angle(m_GroundContactNormal, Vector3.up);
			return SlopeCurveModifier.Evaluate(time);
		}

		private Vector2 GetInput()
		{
			Vector2 vector = default(Vector2);
			vector.x = InputManager.GetAxis("Horizontal");
			vector.y = InputManager.GetAxis("Vertical");
			Vector2 vector2 = vector;
			UpdateDesiredTargetSpeed(vector2);
			return vector2;
		}

		private void GroundCheck()
		{
			m_PreviouslyGrounded = m_IsGrounded;
			RaycastHit hitInfo;
			if (Physics.SphereCast(base.transform.position + base.transform.up * (m_Capsule.radius - 0.02f + groundCheckDistance / 2f), m_Capsule.radius - 0.02f, Vector3.down, out hitInfo, groundCheckDistance + 0.1f))
			{
				m_IsGrounded = true;
				m_GroundContactNormal = hitInfo.normal;
			}
			else
			{
				m_IsGrounded = false;
				m_GroundContactNormal = Vector3.up;
			}
			if (!m_PreviouslyGrounded && m_IsGrounded && m_Jumping)
			{
				timerForJump = 0f;
				m_Jumping = false;
				StartCoroutine(AntiJumpSpam());
			}
		}

		private void StickToGroundHelper()
		{
			RaycastHit hitInfo;
			if (Physics.SphereCast(base.transform.position, m_Capsule.radius, Vector3.down, out hitInfo, m_Capsule.height / 2f - m_Capsule.radius + stickToGroundHelperDistance) && Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
			{
				m_RigidBody.velocity = Vector3.ProjectOnPlane(m_RigidBody.velocity, hitInfo.normal);
			}
		}
	}
}
