using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Math;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.CharacterMovement
{
	public class OtherCharacterController : MonoBehaviour
	{
		public delegate void TranslateDelegate();

		public volatile bool UpdateMovementPosition = true;

		private float freeLookUpPos;

		private float freeLookRightPos;

		private float mouseLookUpPos;

		private float ikLookPos;

		private float targetFreeLookUpPos;

		private float targetFreeLookRightPos;

		private float targetMouseLookUpPos;

		private float targetIkLookPos;

		private string playerName;

		private bool isMouseLook;

		public Animator TPSAnimator;

		public bool inPlayerColliders;

		public Outfit CurrentOutfit;

		private Transform playerNameTransform;

		private Vector3 velocity = Vector3.zero;

		public Transform Outfit;

		public Transform BasicOutfitHolder;

		public SkinnedMeshRenderer HeadSkin;

		public SkinnedMeshRenderer ReferenceHead;

		public float LerpPositionRate = 1f;

		[SerializeField]
		private Vector3 posSyncStart = Vector3.zero;

		private Vector3 posSyncEnd = Vector3.zero;

		private Quaternion rotationSyncStart = Quaternion.identity;

		private Quaternion rotationSyncEnd = Quaternion.identity;

		private float syncTime;

		private float syncLastTime = -1f;

		public float LerpFreeLookUpAnimationRate = 5f;

		public float LerpFreeLookRightAnimationRate = 5f;

		public float LerpMouseLookUpAnimationRate = 16f;

		public GameObject TransitionHelperGO;

		private float maxUpAngle;

		private float maxDownAngle;

		private float maxRightAngle;

		public RagdollHelper RagdollComponent;

		[SerializeField]
		public AnimatorHelper animHelper;

		private Transform myPlayerCameraTransform;

		public Transform targetHelperParent;

		[SerializeField]
		public Transform hips;

		[SerializeField]
		public Transform spine2;

		public OtherPlayer player;

		private bool isZeroG;

		[SerializeField]
		private CapsuleCollider collider1G;

		[SerializeField]
		private SphereCollider collider0G;

		public SoundEffect ImpactSounds;

		public SoundEffect HealthSounds;

		private float lastImpactTime;

		private Dictionary<byte, RagdollItemData> ragdollTargetData;

		private Dictionary<byte, RagdollItemData> ragdollStartData = new Dictionary<byte, RagdollItemData>();

		private bool lerpRagdollData;

		private float lerpRagdollTimer;

		private MyPlayer.HandAnimationStates handsAnimState;

		private bool areCollidersEnabled = true;

		private float lastInstantPositionSetTime;

		private float translateLerpHelper;

		public string PlayerName
		{
			get
			{
				return playerName;
			}
			set
			{
				base.transform.Find("Name").GetComponent<TextMesh>().text = value;
				playerName = value;
			}
		}

		public void SetPlayer(OtherPlayer pl)
		{
			player = pl;
		}

		public void RecreateRig()
		{
			if (animHelper.GetBones().Count == 0)
			{
				animHelper.CreateRig();
			}
		}

		private void Start()
		{
			playerNameTransform = base.transform.Find("Name");
			maxUpAngle = MyPlayer.Instance.FpsController.CameraMaxUpAngle;
			maxDownAngle = MyPlayer.Instance.FpsController.CameraMaxDownAngle;
			maxRightAngle = MyPlayer.Instance.FpsController.CameraMaxRightAngle;
			posSyncStart = base.transform.localPosition;
			posSyncEnd = base.transform.localPosition;
			rotationSyncStart = base.transform.localRotation;
			rotationSyncEnd = base.transform.localRotation;
			myPlayerCameraTransform = MyPlayer.Instance.FpsController.MainCamera.transform;
			if (player == null)
			{
				player = GetComponent<OtherPlayer>();
			}
			player.AttachDynamicObjectsOnPlayer();
		}

		private void FixedUpdate()
		{
			if (UpdateMovementPosition)
			{
				UpdateMovement();
			}
		}

		private void UpdateRagdollData()
		{
			if (!lerpRagdollData)
			{
				return;
			}
			foreach (KeyValuePair<AnimatorHelper.HumanBones, Transform> bone in animHelper.GetBones())
			{
				if (ragdollTargetData.ContainsKey((byte)bone.Key))
				{
					bone.Value.localPosition = Vector3.Lerp(ragdollStartData[(byte)bone.Key].Position.ToVector3(), ragdollTargetData[(byte)bone.Key].Position.ToVector3(), lerpRagdollTimer * 10f);
					bone.Value.localRotation = Quaternion.Lerp(ragdollStartData[(byte)bone.Key].LocalRotation.ToQuaternion(), ragdollTargetData[(byte)bone.Key].LocalRotation.ToQuaternion(), lerpRagdollTimer * 10f);
				}
			}
			lerpRagdollTimer += Time.deltaTime;
			if (lerpRagdollTimer >= 1f)
			{
				lerpRagdollData = false;
				ragdollStartData.Clear();
				lerpRagdollTimer = 0f;
			}
		}

		private void Update()
		{
			UpdateAnimator();
			UpdateRagdollData();
			ikLookPos = MathHelper.LerpValue(ikLookPos, targetMouseLookUpPos, Time.deltaTime * LerpMouseLookUpAnimationRate);
			targetHelperParent.transform.localRotation = Quaternion.Euler(new Vector3(0f - ikLookPos, targetHelperParent.transform.localRotation.eulerAngles.y, targetHelperParent.transform.localRotation.eulerAngles.z));
			if (MyPlayer.Instance.ShowGUIElements)
			{
				if (!playerNameTransform.gameObject.activeInHierarchy)
				{
					playerNameTransform.gameObject.SetActive(true);
				}
				playerNameTransform.rotation = Quaternion.LookRotation(playerNameTransform.position - myPlayerCameraTransform.position, myPlayerCameraTransform.up);
			}
			else if (playerNameTransform.gameObject.activeInHierarchy)
			{
				playerNameTransform.gameObject.SetActive(false);
			}
			player.UpdateMovement();
		}

		private void LateUpdate()
		{
		}

		public void UpdateAnimatorOneFrame()
		{
			animHelper.UpdateTPSAnimatorOneFrame();
		}

		public void UpdateAnimator()
		{
			if (base.gameObject.activeInHierarchy)
			{
				animHelper.UpdateTPSAnimatorConstant();
				if (!(TPSAnimator == null))
				{
					freeLookUpPos = MathHelper.LerpValue(freeLookUpPos, targetFreeLookUpPos, Time.deltaTime * LerpFreeLookUpAnimationRate);
					freeLookRightPos = MathHelper.LerpValue(freeLookRightPos, targetFreeLookRightPos, Time.deltaTime * LerpFreeLookRightAnimationRate);
					mouseLookUpPos = MathHelper.LerpValue(mouseLookUpPos, targetMouseLookUpPos / 85f, Time.deltaTime * LerpMouseLookUpAnimationRate);
					TPSAnimator.SetFloat("FreeLookUp", freeLookUpPos);
					TPSAnimator.SetFloat("FreeLookRight", freeLookRightPos);
					TPSAnimator.SetFloat("MouseLookUp", mouseLookUpPos);
				}
			}
		}

		private void UpdateMovement()
		{
			syncTime += Time.fixedDeltaTime;
		}

		private void CalculateTurning()
		{
			float num = rotationSyncStart.eulerAngles.y - base.transform.localRotation.eulerAngles.y;
			string text = ((!animHelper.animationData.IsZeroG) ? "TurningDirection" : "HorizontalRollDirection");
			if (Mathf.Abs(num) > 0.45f)
			{
				if (!animHelper.animationData.IsZeroG)
				{
					TPSAnimator.SetBool("Turning", true);
				}
				if (num < -5f)
				{
					TPSAnimator.SetInteger(text, -1);
					return;
				}
				if (num > 5f)
				{
					TPSAnimator.SetInteger(text, 1);
					return;
				}
				if (!animHelper.animationData.IsZeroG)
				{
					TPSAnimator.SetBool("Turning", false);
				}
				TPSAnimator.SetInteger(text, 0);
			}
			else
			{
				if (!animHelper.animationData.IsZeroG)
				{
					TPSAnimator.SetBool("Turning", false);
				}
				TPSAnimator.SetInteger(text, 0);
			}
		}

		public void ModifyTransformDataList(Vector3 positionCorrection)
		{
		}

		public void MovementMessageReceived(CharacterMovementMessage cmm)
		{
			if (player == null)
			{
				return;
			}
			Vector3 value = cmm.Gravity.ToVector3();
			isZeroG = !value.IsNotEpsilonZero();
			if (cmm.ImpactVelocity.HasValue && isZeroG && cmm.ImpactVelocity.Value > 0.4f && Time.realtimeSinceStartup - lastImpactTime > 0.3f)
			{
				lastImpactTime = Time.realtimeSinceStartup;
				PlayImpactSound(cmm.ImpactVelocity.Value);
			}
			CharacterTransformData transformData = cmm.TransformData;
			animHelper.ParseData(cmm.AnimationData);
			syncTime = 0f;

			syncLastTime = Time.realtimeSinceStartup;
			if (syncLastTime < lastInstantPositionSetTime)
			{
				return;
			}
			if (transformData.PlatformRelativePos != null && player.OnPlatform != null)
			{
				SetTargetPositionAndRotation(null, transformData.LocalRotation.ToQuaternion());
			}
			else if (cmm.RagdollData == null)
			{
				SetTargetPositionAndRotation(null, transformData.LocalRotation.ToQuaternion());
			}
			else
			{
				SetTargetPositionAndRotation(null, transformData.LocalRotation.ToQuaternion(), true);
			}
			player.SetMovementData(transformData.LocalPosition.ToVector3(), transformData.LocalRotation.ToQuaternion(), transformData.LocalVelocity.ToVector3(), transformData.Timestamp);
			player.SetGravity(cmm.Gravity.ToVector3());
			targetMouseLookUpPos = transformData.MouseLook;
			targetFreeLookUpPos = transformData.FreeLookX;
			targetFreeLookRightPos = transformData.FreeLookY;
			if (cmm.RagdollData != null)
			{
				animHelper.ToggleMainAnimator(false);
				SetRagdollData(cmm.RagdollData);
			}
			else
			{
				animHelper.ToggleMainAnimator(true);
			}
			if (cmm != null && player.CurrentJetpack != null)
			{
				if (cmm.JetpackDirection != null)
				{
					player.CurrentJetpack.StartNozzles(new Vector4(cmm.JetpackDirection[0], cmm.JetpackDirection[1], cmm.JetpackDirection[2], cmm.JetpackDirection[3]));
				}
				else if (!player.Gravity.IsEpsilonEqual(Vector3.zero))
				{
					player.CurrentJetpack.StartNozzles(Vector4.zero);
				}
			}
		}

		private void SetRagdollData(Dictionary<byte, RagdollItemData> data)
		{
			ragdollTargetData = data;
			ragdollStartData.Clear();
			lerpRagdollData = true;
			lerpRagdollTimer = 0f;
			foreach (KeyValuePair<AnimatorHelper.HumanBones, Transform> bone in animHelper.GetBones())
			{
				if (data.ContainsKey((byte)bone.Key))
				{
					ragdollStartData.Add((byte)bone.Key, new RagdollItemData
					{
						Position = bone.Value.localPosition.ToArray(),
						LocalRotation = bone.Value.localRotation.ToArray()
					});
				}
			}
		}

		private void CalculateIdleAnimation()
		{
			if (inPlayerColliders && velocity.magnitude < 0.45f)
			{
				RaycastHit hitInfo;
				if (Physics.Raycast(base.transform.position, base.transform.forward, out hitInfo, 0.8f, Client.DefaultLayerMask))
				{
					Debug.DrawRay(base.transform.position, base.transform.forward * 0.8f, Color.blue);
					handsAnimState = MyPlayer.HandAnimationStates.Forward;
				}
				else if (Physics.Raycast(base.transform.position, -base.transform.forward, out hitInfo, 0.8f, Client.DefaultLayerMask))
				{
					Debug.DrawRay(base.transform.position, -base.transform.forward * 0.8f, Color.red);
					handsAnimState = MyPlayer.HandAnimationStates.Back;
				}
				else if (Physics.Raycast(base.transform.position, base.transform.up, out hitInfo, 0.9f, Client.DefaultLayerMask))
				{
					handsAnimState = MyPlayer.HandAnimationStates.Top;
				}
				else if (Physics.Raycast(base.transform.position, -base.transform.up, out hitInfo, 0.9f, Client.DefaultLayerMask))
				{
					handsAnimState = MyPlayer.HandAnimationStates.Bottom;
				}
				else if (Physics.Raycast(base.transform.position, base.transform.right, out hitInfo, 0.9f, Client.DefaultLayerMask))
				{
					Debug.DrawRay(base.transform.position, base.transform.right * 0.8f, Color.green);
					handsAnimState = MyPlayer.HandAnimationStates.Right;
				}
				else if (Physics.Raycast(base.transform.position, -base.transform.right, out hitInfo, 0.9f, Client.DefaultLayerMask))
				{
					Debug.DrawRay(base.transform.position, -base.transform.right * 0.8f, Color.yellow);
					handsAnimState = MyPlayer.HandAnimationStates.Left;
				}
				else
				{
					handsAnimState = MyPlayer.HandAnimationStates.Clear;
				}
			}
			else
			{
				handsAnimState = MyPlayer.HandAnimationStates.Clear;
			}
			TPSAnimator.SetBool("CanTouchWall", handsAnimState != MyPlayer.HandAnimationStates.Clear);
			TPSAnimator.SetInteger("ZeroGHandState", (int)handsAnimState);
		}

		public void PlayerColliderToggle(bool isInTrigger)
		{
			inPlayerColliders = isInTrigger;
		}

		public void ToggleRagdoll(bool enabled, Corpse corpse)
		{
			if (enabled)
			{
				RagdollComponent.ToggleRagdoll(enabled, corpse, velocity);
				player.AnimHelper.aimIKController.ToggleIK(false, true);
				return;
			}
			RaycastHit hitInfo;
			if (animHelper.animationData.IsZeroG)
			{
				base.transform.position = TPSAnimator.GetBoneTransform(HumanBodyBones.Hips).position;
			}
			else if (Physics.Raycast(TPSAnimator.GetBoneTransform(HumanBodyBones.Hips).position, player.GravityDirection, out hitInfo, 2f, Client.DefaultLayerMask))
			{
				base.transform.position = hitInfo.point - player.GravityDirection * 1.34f;
			}
			RagdollComponent.ToggleRagdoll(enabled, null);
		}

		public void ToggleKinematic(bool? isKinematic = null)
		{
			bool flag = ((!isKinematic.HasValue) ? (!areCollidersEnabled) : isKinematic.Value);
			collider0G.enabled = !flag && isZeroG;
			collider1G.enabled = !flag && !isZeroG;
		}

		public void ModifyPositionAndRotation(Vector3? position = null, Quaternion? rotation = null)
		{
			if (position.HasValue)
			{
				base.transform.localPosition += position.Value;
				posSyncStart += position.Value;
				posSyncEnd += position.Value;
			}
			if (rotation.HasValue)
			{
				base.transform.localRotation *= rotation.Value;
				rotationSyncStart *= rotation.Value;
				rotationSyncEnd *= rotation.Value;
			}
		}

		public void SetTargetPositionAndRotation(Vector3? localPosition, Quaternion? localRotation, bool instant = false)
		{
			if (instant && (localPosition.HasValue || localRotation.HasValue))
			{
				lastInstantPositionSetTime = Time.time;
			}
		}

		public IEnumerator TranslateTo(Transform position, TranslateDelegate actionToCall)
		{
			Vector3 startingPosition = base.transform.position;
			Quaternion startingRotation = base.transform.rotation;
			translateLerpHelper = 0f;
			while (translateLerpHelper < 1f)
			{
				base.transform.position = Vector3.Lerp(startingPosition, position.position, Mathf.SmoothStep(0f, 1f, translateLerpHelper));
				base.transform.rotation = Quaternion.Lerp(startingRotation, position.rotation, Mathf.SmoothStep(0f, 1f, translateLerpHelper));
				translateLerpHelper += Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}
			base.transform.position = position.position;
			base.transform.rotation = position.rotation;
			actionToCall();
		}

		private void PlayImpactSound(float velocity)
		{
			ImpactSounds.SetRTPCValue(SoundManager.Instance.ImpactVelocity, velocity);
			ImpactSounds.Play(0);
		}
	}
}
