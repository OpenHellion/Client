using System;
using System.Collections;
using System.Collections.Generic;
using OpenHellion;
using UnityEngine;
using ZeroGravity.Math;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.CharacterMovement
{
	public class OtherCharacterController : MonoBehaviour
	{
		private static readonly int ZeroGHandStateHash = Animator.StringToHash("ZeroGHandState");

		private static readonly int CanTouchWallHash = Animator.StringToHash("CanTouchWall");

		private static readonly int MouseLookUpHash = Animator.StringToHash("MouseLookUp");

		private static readonly int FreeLookRightHash = Animator.StringToHash("FreeLookRight");

		private static readonly int FreeLookUpHash = Animator.StringToHash("FreeLookUp");

		private float _freeLookUpPos;

		private float _freeLookRightPos;

		private float _mouseLookUpPos;

		private float _ikLookPos;

		public float TargetFreeLookUpPos;

		public float TargetFreeLookRightPos;

		public float TargetMouseLookUpPos;

		private string _playerName;

		public Animator TPSAnimator;

		public bool inPlayerColliders;

		public Outfit CurrentOutfit;

		private Transform _playerNameTransform;

		private Vector3 _velocity = Vector3.zero;

		public Transform Outfit;

		public Transform BasicOutfitHolder;

		public SkinnedMeshRenderer HeadSkin;

		public SkinnedMeshRenderer ReferenceHead;

		public float LerpFreeLookUpAnimationRate = 5f;

		public float LerpFreeLookRightAnimationRate = 5f;

		public float LerpMouseLookUpAnimationRate = 16f;

		public GameObject TransitionHelperGO;

		public RagdollHelper RagdollComponent;

		public AnimatorHelper animHelper;

		private Transform myPlayerCameraTransform;

		public Transform targetHelperParent;

		public Transform hips;

		public Transform spine2;

		public OtherPlayer player;

		public bool IsZeroG => player.Gravity.IsEpsilonEqual(Vector3.zero);

		[SerializeField] private CapsuleCollider collider1G;

		[SerializeField] private SphereCollider collider0G;

		public SoundEffect ImpactSounds;

		public SoundEffect HealthSounds;

		private Dictionary<byte, RagdollItemData> _ragdollTargetData;

		private readonly Dictionary<byte, RagdollItemData> _ragdollStartData = new Dictionary<byte, RagdollItemData>();

		private bool _lerpRagdollData;

		private float _lerpRagdollTimer;

		private MyPlayer.HandAnimationStates _handsAnimState;

		private float _translateLerpHelper;

		public string PlayerName
		{
			get { return _playerName; }
			set
			{
				transform.Find("Name").GetComponent<TextMesh>().text = value; // TODO move this to OtherPlayer and make textmeshpro
				_playerName = value;
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
			_playerNameTransform = transform.Find("Name");
			myPlayerCameraTransform = MyPlayer.Instance.FpsController.MainCamera.transform;
			if (player == null)
			{
				player = GetComponent<OtherPlayer>();
			}
		}

		private void UpdateRagdollData()
		{
			if (!_lerpRagdollData)
			{
				return;
			}

			foreach (KeyValuePair<AnimatorHelper.HumanBones, Transform> bone in animHelper.GetBones())
			{
				if (_ragdollTargetData.ContainsKey((byte)bone.Key))
				{
					bone.Value.SetLocalPositionAndRotation(Vector3.Lerp(_ragdollStartData[(byte)bone.Key].Position.ToVector3(),
						_ragdollTargetData[(byte)bone.Key].Position.ToVector3(), _lerpRagdollTimer * 10f),
Quaternion.Lerp(_ragdollStartData[(byte)bone.Key].LocalRotation.ToQuaternion(),
							_ragdollTargetData[(byte)bone.Key].LocalRotation.ToQuaternion(), _lerpRagdollTimer * 10f));
				}
			}

			_lerpRagdollTimer += Time.deltaTime;
			if (_lerpRagdollTimer >= 1f)
			{
				_lerpRagdollData = false;
				_ragdollStartData.Clear();
				_lerpRagdollTimer = 0f;
			}
		}

		private void Update()
		{
			UpdateAnimator();
			UpdateRagdollData();
			_ikLookPos = MathHelper.LerpValue(_ikLookPos, TargetMouseLookUpPos,
				Time.deltaTime * LerpMouseLookUpAnimationRate);
			targetHelperParent.transform.localRotation = Quaternion.Euler(new Vector3(0f - _ikLookPos,
				targetHelperParent.transform.localRotation.eulerAngles.y,
				targetHelperParent.transform.localRotation.eulerAngles.z));
			if (MyPlayer.Instance.ShowGUIElements)
			{
				if (!_playerNameTransform.gameObject.activeInHierarchy)
				{
					_playerNameTransform.gameObject.SetActive(true);
				}

				_playerNameTransform.rotation = Quaternion.LookRotation(
					_playerNameTransform.position - myPlayerCameraTransform.position, myPlayerCameraTransform.up);
			}
			else if (_playerNameTransform.gameObject.activeInHierarchy)
			{
				_playerNameTransform.gameObject.SetActive(false);
			}

			player.UpdateMovement();
		}

		public void UpdateAnimatorOneFrame()
		{
			animHelper.UpdateTPSAnimatorOneFrame();
		}

		public void UpdateAnimator()
		{
			if (gameObject.activeInHierarchy)
			{
				animHelper.UpdateTPSAnimatorConstant();
				if (!(TPSAnimator == null))
				{
					_freeLookUpPos = MathHelper.LerpValue(_freeLookUpPos, TargetFreeLookUpPos,
						Time.deltaTime * LerpFreeLookUpAnimationRate);
					_freeLookRightPos = MathHelper.LerpValue(_freeLookRightPos, TargetFreeLookRightPos,
						Time.deltaTime * LerpFreeLookRightAnimationRate);
					_mouseLookUpPos = MathHelper.LerpValue(_mouseLookUpPos, TargetMouseLookUpPos / 85f,
						Time.deltaTime * LerpMouseLookUpAnimationRate);
					TPSAnimator.SetFloat(FreeLookUpHash, _freeLookUpPos);
					TPSAnimator.SetFloat(FreeLookRightHash, _freeLookRightPos);
					TPSAnimator.SetFloat(MouseLookUpHash, _mouseLookUpPos);
				}
			}
		}

		public void SetRagdollData(Dictionary<byte, RagdollItemData> data)
		{
			_ragdollTargetData = data;
			_ragdollStartData.Clear();
			_lerpRagdollData = true;
			_lerpRagdollTimer = 0f;
			foreach (KeyValuePair<AnimatorHelper.HumanBones, Transform> bone in animHelper.GetBones())
			{
				if (data.ContainsKey((byte)bone.Key))
				{
					_ragdollStartData.Add((byte)bone.Key, new RagdollItemData
					{
						Position = bone.Value.localPosition.ToArray(),
						LocalRotation = bone.Value.localRotation.ToArray()
					});
				}
			}
		}

		private void CalculateIdleAnimation()
		{
			if (inPlayerColliders && _velocity.magnitude < 0.45f)
			{
				RaycastHit hitInfo;
				if (Physics.Raycast(transform.position, transform.forward, out hitInfo, 0.8f,
					    World.DefaultLayerMask))
				{
					Debug.DrawRay(transform.position, transform.forward * 0.8f, Color.blue);
					_handsAnimState = MyPlayer.HandAnimationStates.Forward;
				}
				else if (Physics.Raycast(transform.position, -transform.forward, out hitInfo, 0.8f,
					         World.DefaultLayerMask))
				{
					Debug.DrawRay(transform.position, -transform.forward * 0.8f, Color.red);
					_handsAnimState = MyPlayer.HandAnimationStates.Back;
				}
				else if (Physics.Raycast(transform.position, transform.up, out hitInfo, 0.9f,
					         World.DefaultLayerMask))
				{
					_handsAnimState = MyPlayer.HandAnimationStates.Top;
				}
				else if (Physics.Raycast(transform.position, -transform.up, out hitInfo, 0.9f,
					         World.DefaultLayerMask))
				{
					_handsAnimState = MyPlayer.HandAnimationStates.Bottom;
				}
				else if (Physics.Raycast(transform.position, transform.right, out hitInfo, 0.9f,
					         World.DefaultLayerMask))
				{
					Debug.DrawRay(transform.position, transform.right * 0.8f, Color.green);
					_handsAnimState = MyPlayer.HandAnimationStates.Right;
				}
				else if (Physics.Raycast(transform.position, -transform.right, out hitInfo, 0.9f,
					         World.DefaultLayerMask))
				{
					Debug.DrawRay(transform.position, -transform.right * 0.8f, Color.yellow);
					_handsAnimState = MyPlayer.HandAnimationStates.Left;
				}
				else
				{
					_handsAnimState = MyPlayer.HandAnimationStates.Clear;
				}
			}
			else
			{
				_handsAnimState = MyPlayer.HandAnimationStates.Clear;
			}

			TPSAnimator.SetBool(CanTouchWallHash, _handsAnimState != MyPlayer.HandAnimationStates.Clear);
			TPSAnimator.SetInteger(ZeroGHandStateHash, (int)_handsAnimState);
		}

		public void PlayerColliderToggle(bool isInTrigger)
		{
			inPlayerColliders = isInTrigger;
		}

		public void ToggleRagdoll(bool enabled, Corpse corpse)
		{
			if (enabled)
			{
				RagdollComponent.ToggleRagdoll(enabled, corpse, _velocity);
				player.AnimHelper.aimIKController.ToggleIK(false, true);
				return;
			}

			RaycastHit hitInfo;
			if (animHelper.animationData.IsZeroG)
			{
				transform.position = TPSAnimator.GetBoneTransform(HumanBodyBones.Hips).position;
			}
			else if (Physics.Raycast(TPSAnimator.GetBoneTransform(HumanBodyBones.Hips).position,
				         player.GravityDirection, out hitInfo, 2f, World.DefaultLayerMask))
			{
				transform.position = hitInfo.point - player.GravityDirection * 1.34f;
			}

			RagdollComponent.ToggleRagdoll(enabled, null);
		}

		public void ToggleKinematic(bool? isKinematic = null)
		{
			bool flag = isKinematic.HasValue && isKinematic.Value;
			collider0G.enabled = !flag && IsZeroG;
			collider1G.enabled = !flag && !IsZeroG;
		}

		public void ModifyPositionAndRotation(Vector3? position = null, Quaternion? rotation = null)
		{
			if (position.HasValue)
			{
				transform.localPosition += position.Value;
			}

			if (rotation.HasValue)
			{
				transform.localRotation *= rotation.Value;
			}
		}

		public IEnumerator TranslateTo(Transform position, Action actionToCall)
		{
			transform.GetPositionAndRotation(out Vector3 startingPosition, out Quaternion startingRotation);
			_translateLerpHelper = 0f;
			while (_translateLerpHelper < 1f)
			{
				transform.SetPositionAndRotation(Vector3.Lerp(startingPosition, position.position,
					Mathf.SmoothStep(0f, 1f, _translateLerpHelper)), Quaternion.Lerp(startingRotation, position.rotation,
					Mathf.SmoothStep(0f, 1f, _translateLerpHelper)));
				_translateLerpHelper += Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}

			transform.position = position.position;
			transform.rotation = position.rotation;
			actionToCall();
		}

		private void OnCollisionEnter(Collision collision)
		{
			float velocity = collision.relativeVelocity.magnitude;
			if (ImpactSounds != null && IsZeroG && velocity > 0.4f)
			{
				PlayImpactSound(velocity);
			}
		}

		public void PlayImpactSound(float velocity)
		{
			ImpactSounds.SetRTPCValue(SoundManager.ImpactVelocity, velocity);
			ImpactSounds.Play(0);
		}
	}
}
