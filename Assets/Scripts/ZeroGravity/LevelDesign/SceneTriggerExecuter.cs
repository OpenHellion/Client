using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using ZeroGravity.Data;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerExecuter : MonoBehaviour, ISceneObject
	{
		[Serializable]
		public class CustomEventActions
		{
			public int Type;

			public UnityEvent Actions;
		}

		public enum AnimatorActionType
		{
			ActivateStart = 0,
			ActivateEnd = 1,
			InactivateStart = 2,
			InactivateEnd = 3,
			FailStart = 4,
			FailEnd = 5,
			ActivateBeforeStart = 6,
			ActivateAfterEnd = 7,
			InactivateBeforeStart = 8,
			InactivateAfterEnd = 9,
			FailBeforeStart = 10,
			FailAfterEnd = 11
		}

		[Serializable]
		public class AnimatorActions
		{
			public SceneTriggerAnimation Animator;

			public AnimatorActionType Type;

			public UnityEvent Actions;
		}

		[Serializable]
		public class CharacterActions
		{
			public UnityEvent InteractStart;

			public UnityEvent InteractEnd;

			public UnityEvent LockStart;

			public UnityEvent LockEnd;
		}

		[Serializable]
		public class StateActions
		{
			[HideInInspector]
			public int StateID;

			public string StateName;

			public string OnCancelIteractGoToState;

			public string PlayerDisconnectReturnToState;

			public bool PlayerDisconnectToStateImmediate;

			public bool OnlyActivePlayerCanChangeState;

			public CharacterInteractionState CharacterPosition;

			[Space(5f)]
			public DependencyDelegate Dependencies;

			public UnityEvent PassActions;

			public UnityEvent FailActions;

			public UnityEvent InstantActions;

			public CharacterActions CharacterActions = new CharacterActions();

			public List<AnimatorActions> AnimatorActions;

			public List<CustomEventActions> CustomActions;

			[HideInInspector]
			public long TriggeredPlayerGUID;
		}

		[CompilerGenerated]
		private sealed class _003CReadStates_003Ec__AnonStorey0
		{
			internal AnimatorActions tmpAa;

			internal int stateID;

			internal SceneTriggerExecuter _0024this;

			internal void _003C_003Em__0(SceneTriggerAnimation anim, SceneTriggerAnimation.AnimationState state)
			{
				_0024this.OnAnimatorStateEnter(tmpAa, anim, state, stateID, false);
			}

			internal void _003C_003Em__1(SceneTriggerAnimation anim, SceneTriggerAnimation.AnimationState state)
			{
				_0024this.OnAnimatorStateExit(tmpAa, anim, state, stateID, false);
			}

			internal void _003C_003Em__2(SceneTriggerAnimation anim, SceneTriggerAnimation.AnimationState state)
			{
				_0024this.OnAnimatorStateEnter(tmpAa, anim, state, stateID, true);
			}

			internal void _003C_003Em__3(SceneTriggerAnimation anim, SceneTriggerAnimation.AnimationState state)
			{
				_0024this.OnAnimatorStateExit(tmpAa, anim, state, stateID, true);
			}
		}

		public TagAction TagAction;

		public string Tags;

		[SerializeField]
		private int _inSceneID;

		[SerializeField]
		private string _defaultState = string.Empty;

		private int _defaultStateID;

		private StateActions _newState;

		private StateActions _currentState;

		[SerializeField]
		private string _additionalData;

		[SerializeField]
		private Transform _executerPivot;

		[SerializeField]
		private List<StateActions> actionStates = new List<StateActions>();

		private Dictionary<int, StateActions> states = new Dictionary<int, StateActions>();

		private Dictionary<string, int> stateNameID = new Dictionary<string, int>();

		private long triggeredPlayerGUID;

		[HideInInspector]
		public SpaceObjectVessel ParentVessel;

		private SceneTriggerExecuter childExecuter;

		private SceneTriggerExecuter parentExecuter;

		private int? proximityTriggerID;

		private bool? proximityIsEnter;

		private CharacterInteractionState interactionToSetAfterTranslate;

		private long interactionToSetAfterTranslateGUID;

		public int InSceneID
		{
			get
			{
				return _inSceneID;
			}
			set
			{
				_inSceneID = value;
			}
		}

		public int DefaultStateID
		{
			get
			{
				return _defaultStateID;
			}
		}

		public StateActions CurrentStateActions
		{
			get
			{
				return _currentState;
			}
		}

		public int CurrentStateID
		{
			get
			{
				return (_currentState != null) ? _currentState.StateID : 0;
			}
		}

		public string CurrentState
		{
			get
			{
				return (_currentState == null) ? string.Empty : _currentState.StateName;
			}
		}

		public int NumberOfStates
		{
			get
			{
				return System.Math.Max(actionStates.Count, states.Count);
			}
		}

		public string AdditionalData
		{
			get
			{
				return _additionalData;
			}
			set
			{
				_additionalData = value;
			}
		}

		public SceneTriggerExecuter ChildExecuter
		{
			get
			{
				return childExecuter;
			}
		}

		public SceneTriggerExecuter ParentExecuter
		{
			get
			{
				return parentExecuter;
			}
		}

		public bool IsMyPlayerLockedToPanel
		{
			get
			{
				return MyPlayer.Instance != null && MyPlayer.Instance.IsLockedToTrigger;
			}
		}

		public bool IsMyPlayerInLockedState
		{
			get
			{
				return MyPlayer.Instance != null && MyPlayer.Instance.InLockState;
			}
		}

		public bool IsMyPlayerInInteractState
		{
			get
			{
				return MyPlayer.Instance != null && MyPlayer.Instance.InInteractState;
			}
		}

		public bool AreMyPlayerHandsEmpty
		{
			get
			{
				return MyPlayer.Instance != null && MyPlayer.Instance.Inventory.ItemInHands == null;
			}
		}

		public Transform ExecuterPivot
		{
			get
			{
				return (!(_executerPivot != null)) ? base.transform : _executerPivot;
			}
		}

		public bool IsMyPlayerTriggered
		{
			get
			{
				return MyPlayer.Instance != null && MyPlayer.Instance.GUID == triggeredPlayerGUID;
			}
		}

		private void Awake()
		{
			ReadStates();
		}

		private void Start()
		{
			if (Client.IsGameBuild && ParentVessel == null)
			{
				ParentVessel = GetComponentInParent<GeometryRoot>().MainObject as SpaceObjectVessel;
			}
		}

		public void ReadDefaultStates()
		{
			if (actionStates == null || actionStates.Count == 0)
			{
				return;
			}
			int num = 1;
			foreach (StateActions actionState in actionStates)
			{
				actionState.StateID = num++;
				if (actionState.StateName == _defaultState)
				{
					_currentState = actionState;
					_defaultStateID = actionState.StateID;
				}
			}
			if (_currentState == null)
			{
				_currentState = actionStates[0];
			}
		}

		public List<SceneTriggerExecuterStateData> GetExecuterStatesData()
		{
			List<SceneTriggerExecuterStateData> list = new List<SceneTriggerExecuterStateData>();
			if (actionStates.Count > 0)
			{
				foreach (StateActions actionState in actionStates)
				{
					list.Add(new SceneTriggerExecuterStateData
					{
						StateID = actionState.StateID,
						PlayerDisconnectToStateID = ((!actionState.PlayerDisconnectReturnToState.IsNullOrEmpty()) ? GetStateID(actionState.PlayerDisconnectReturnToState) : 0),
						PlayerDisconnectToStateImmediate = (!actionState.PlayerDisconnectReturnToState.IsNullOrEmpty() && actionState.PlayerDisconnectToStateImmediate)
					});
				}
				return list;
			}
			return list;
		}

		private void ReadStates()
		{
			if (actionStates == null || actionStates.Count == 0)
			{
				return;
			}
			int num = 1;
			foreach (StateActions actionState in actionStates)
			{
				actionState.StateID = num++;
				states.Add(actionState.StateID, actionState);
				stateNameID.Add(actionState.StateName, actionState.StateID);
				actionState.Dependencies.CreateDelegates();
				foreach (AnimatorActions animatorAction in actionState.AnimatorActions)
				{
					if (animatorAction.Animator != null)
					{
						_003CReadStates_003Ec__AnonStorey0 _003CReadStates_003Ec__AnonStorey = new _003CReadStates_003Ec__AnonStorey0();
						_003CReadStates_003Ec__AnonStorey._0024this = this;
						_003CReadStates_003Ec__AnonStorey.tmpAa = animatorAction;
						_003CReadStates_003Ec__AnonStorey.stateID = actionState.StateID;
						if (animatorAction.Type == AnimatorActionType.ActivateStart || animatorAction.Type == AnimatorActionType.FailStart || animatorAction.Type == AnimatorActionType.InactivateStart)
						{
							SceneTriggerAnimation animator = animatorAction.Animator;
							animator.OnStateEnter = (SceneTriggerAnimation.OnStateEnterExitDelegate)Delegate.Combine(animator.OnStateEnter, new SceneTriggerAnimation.OnStateEnterExitDelegate(_003CReadStates_003Ec__AnonStorey._003C_003Em__0));
						}
						else if (animatorAction.Type == AnimatorActionType.ActivateEnd || animatorAction.Type == AnimatorActionType.FailEnd || animatorAction.Type == AnimatorActionType.InactivateEnd)
						{
							SceneTriggerAnimation animator2 = animatorAction.Animator;
							animator2.OnStateExit = (SceneTriggerAnimation.OnStateEnterExitDelegate)Delegate.Combine(animator2.OnStateExit, new SceneTriggerAnimation.OnStateEnterExitDelegate(_003CReadStates_003Ec__AnonStorey._003C_003Em__1));
						}
						else if (animatorAction.Type == AnimatorActionType.ActivateBeforeStart || animatorAction.Type == AnimatorActionType.FailBeforeStart || animatorAction.Type == AnimatorActionType.InactivateBeforeStart)
						{
							SceneTriggerAnimation animator3 = animatorAction.Animator;
							animator3.OnStateBeforeEnter = (SceneTriggerAnimation.OnStateEnterExitDelegate)Delegate.Combine(animator3.OnStateBeforeEnter, new SceneTriggerAnimation.OnStateEnterExitDelegate(_003CReadStates_003Ec__AnonStorey._003C_003Em__2));
						}
						else if (animatorAction.Type == AnimatorActionType.ActivateAfterEnd || animatorAction.Type == AnimatorActionType.FailAfterEnd || animatorAction.Type == AnimatorActionType.InactivateAfterEnd)
						{
							SceneTriggerAnimation animator4 = animatorAction.Animator;
							animator4.OnStateAfterExit = (SceneTriggerAnimation.OnStateEnterExitDelegate)Delegate.Combine(animator4.OnStateAfterExit, new SceneTriggerAnimation.OnStateEnterExitDelegate(_003CReadStates_003Ec__AnonStorey._003C_003Em__3));
						}
					}
				}
				if (actionState.StateName == _defaultState)
				{
					_currentState = actionState;
					_defaultStateID = actionState.StateID;
				}
			}
			if (_currentState == null)
			{
				_currentState = actionStates[0];
			}
			actionStates.Clear();
		}

		private bool CheckDependencies(StateActions st)
		{
			if (st.Dependencies != null)
			{
				return st.Dependencies.Invoke();
			}
			return true;
		}

		private void CharacterInteractStarted()
		{
			if (MyPlayer.Instance.GUID == triggeredPlayerGUID)
			{
				MyPlayer.Instance.OnIteractStart = null;
				if (_currentState.CharacterActions.InteractStart != null)
				{
					_currentState.CharacterActions.InteractStart.Invoke();
				}
				return;
			}
			OtherPlayer player = Client.Instance.GetPlayer(triggeredPlayerGUID);
			if (player != null)
			{
				player.OnIteractStart = null;
				if (_currentState.CharacterActions.InteractStart != null)
				{
					_currentState.CharacterActions.InteractStart.Invoke();
				}
			}
		}

		private void CharacterInteractCompleted()
		{
			if (MyPlayer.Instance.GUID == triggeredPlayerGUID)
			{
				MyPlayer.Instance.OnIteractComplete = null;
				if (_currentState.CharacterActions.InteractEnd != null)
				{
					_currentState.CharacterActions.InteractEnd.Invoke();
				}
				return;
			}
			OtherPlayer player = Client.Instance.GetPlayer(triggeredPlayerGUID);
			if (player != null)
			{
				player.OnIteractComplete = null;
				if (_currentState.CharacterActions.InteractEnd != null)
				{
					_currentState.CharacterActions.InteractEnd.Invoke();
				}
			}
		}

		private void CharacterLockStarted()
		{
			if (MyPlayer.Instance.GUID == triggeredPlayerGUID)
			{
				MyPlayer.Instance.OnLockStart = null;
				if (_currentState.CharacterActions.LockStart != null)
				{
					_currentState.CharacterActions.LockStart.Invoke();
				}
				return;
			}
			OtherPlayer player = Client.Instance.GetPlayer(triggeredPlayerGUID);
			if (player != null)
			{
				player.OnLockStart = null;
				if (_currentState.CharacterActions.LockStart != null)
				{
					_currentState.CharacterActions.LockStart.Invoke();
				}
			}
		}

		private void CharacterLockCompleted()
		{
			if (MyPlayer.Instance.GUID == triggeredPlayerGUID)
			{
				MyPlayer.Instance.OnLockComplete = null;
				if (_currentState.CharacterActions.LockEnd != null)
				{
					_currentState.CharacterActions.LockEnd.Invoke();
				}
				return;
			}
			OtherPlayer player = Client.Instance.GetPlayer(triggeredPlayerGUID);
			if (player != null)
			{
				player.OnLockComplete = null;
				if (_currentState.CharacterActions.LockEnd != null)
				{
					_currentState.CharacterActions.LockEnd.Invoke();
				}
			}
		}

		private void SendPackageToServer()
		{
			SendPackageToServer(false);
		}

		private void SendPackageToServer(bool isImmediate, bool force = false)
		{
			if (Client.IsGameBuild)
			{
				SceneTriggerExecuterDetails sceneTriggerExecuterDetails = new SceneTriggerExecuterDetails();
				sceneTriggerExecuterDetails.InSceneID = InSceneID;
				sceneTriggerExecuterDetails.IsImmediate = isImmediate;
				sceneTriggerExecuterDetails.IsFail = !CheckDependencies(_newState) && !force;
				sceneTriggerExecuterDetails.CurrentStateID = CurrentStateID;
				sceneTriggerExecuterDetails.NewStateID = _newState.StateID;
				int? num = proximityTriggerID;
				if (num.HasValue)
				{
					sceneTriggerExecuterDetails.ProximityTriggerID = proximityTriggerID.Value;
					sceneTriggerExecuterDetails.ProximityIsEnter = proximityIsEnter.Value;
				}
				if (ParentVessel != null)
				{
					SpaceObjectVessel parentVessel = ParentVessel;
					SceneTriggerExecuterDetails sceneTriggerExecuter = sceneTriggerExecuterDetails;
					parentVessel.ChangeStats(null, null, null, null, null, null, null, null, sceneTriggerExecuter);
				}
			}
			else
			{
				bool isFail = !CheckDependencies(_newState) && !force;
				SetExecuterDetails(new SceneTriggerExecuterDetails
				{
					PlayerThatActivated = MyPlayer.Instance.GUID,
					InSceneID = InSceneID,
					IsImmediate = isImmediate,
					IsFail = isFail,
					CurrentStateID = CurrentStateID,
					NewStateID = _newState.StateID,
					ProximityTriggerID = proximityTriggerID,
					ProximityIsEnter = proximityIsEnter
				});
			}
			proximityTriggerID = null;
			proximityIsEnter = null;
		}

		private void RunActions(StateActions act, bool isFail, bool isInstant)
		{
			if (MyPlayer.Instance.GUID == triggeredPlayerGUID)
			{
				if (act.CharacterActions.InteractStart.GetPersistentEventCount() > 0)
				{
					MyPlayer.Instance.OnIteractStart = CharacterInteractStarted;
				}
				if (act.CharacterActions.InteractEnd.GetPersistentEventCount() > 0)
				{
					MyPlayer.Instance.OnIteractComplete = CharacterInteractCompleted;
				}
				if (act.CharacterActions.LockStart.GetPersistentEventCount() > 0)
				{
					MyPlayer.Instance.OnLockStart = CharacterLockStarted;
				}
				if (act.CharacterActions.LockEnd.GetPersistentEventCount() > 0)
				{
					MyPlayer.Instance.OnLockComplete = CharacterLockCompleted;
				}
			}
			else
			{
				OtherPlayer player = Client.Instance.GetPlayer(triggeredPlayerGUID);
				if (player != null)
				{
					player.OnIteractStart = CharacterInteractStarted;
					player.OnIteractComplete = CharacterInteractCompleted;
					player.OnLockStart = CharacterLockStarted;
					player.OnLockComplete = CharacterLockCompleted;
				}
			}
			if (isInstant)
			{
				act.InstantActions.Invoke();
			}
			else if (isFail)
			{
				act.FailActions.Invoke();
			}
			else
			{
				act.PassActions.Invoke();
			}
		}

		public int GetStateID(string stateName)
		{
			if (stateNameID.Count > 0)
			{
				if (stateNameID.ContainsKey(stateName))
				{
					return stateNameID[stateName];
				}
				return 0;
			}
			if (actionStates.Count > 0 && !Application.isPlaying)
			{
				int num = 1;
				foreach (StateActions actionState in actionStates)
				{
					num++;
					if (actionState.StateName == stateName)
					{
						return (actionState.StateID != 0) ? actionState.StateID : num;
					}
				}
			}
			return 0;
		}

		public void ChangeState(string newState)
		{
			if (stateNameID.ContainsKey(newState))
			{
				ChangeStateID(stateNameID[newState], false);
			}
		}

		public void ChangeStateImmediate(string newState)
		{
			if (stateNameID.ContainsKey(newState))
			{
				ChangeStateID(stateNameID[newState], true);
			}
		}

		public void ChangeStateImmediateForce(string newState)
		{
			if (stateNameID.ContainsKey(newState))
			{
				ChangeStateID(stateNameID[newState], true, true);
			}
		}

		public void ChangeStateID(int newState)
		{
			ChangeStateID(newState, false);
		}

		public void ChangeStateIDImmediate(int newState)
		{
			ChangeStateID(newState, true);
		}

		public void MergeStateID(int newState, bool isInstant)
		{
			if (newState != CurrentStateID)
			{
				SetExecuterDetails(new SceneTriggerExecuterDetails
				{
					PlayerThatActivated = 0L,
					InSceneID = InSceneID,
					IsImmediate = isInstant,
					IsFail = false,
					CurrentStateID = CurrentStateID,
					NewStateID = newState
				}, isInstant, parentExecuter);
			}
		}

		public void ChangeStateID(int newState, bool isInstantChange, bool force = false)
		{
			if (!states.ContainsKey(newState) || (_currentState.OnlyActivePlayerCanChangeState && _currentState.TriggeredPlayerGUID != 0 && _currentState.TriggeredPlayerGUID != MyPlayer.Instance.GUID))
			{
				return;
			}
			_newState = states[newState];
			if (_newState.CharacterPosition != null && _newState.CharacterPosition.InteractPosition != null)
			{
				if (_newState.CharacterPosition.SetColliderToKinematic)
				{
					MyPlayer.Instance.FpsController.ToggleKinematic(true);
				}
				triggeredPlayerGUID = MyPlayer.Instance.GUID;
				if (isInstantChange)
				{
					MyPlayer.Instance.transform.position = _newState.CharacterPosition.InteractPosition.position;
					MyPlayer.Instance.transform.rotation = _newState.CharacterPosition.InteractPosition.rotation;
					SendPackageToServer(isInstantChange, force);
				}
				else
				{
					StartCoroutine(MyPlayer.Instance.FpsController.TranslateAndLookAt(_newState.CharacterPosition.InteractPosition, _newState.CharacterPosition.InteractLookAt, SendPackageToServer));
				}
			}
			else
			{
				SendPackageToServer(isInstantChange, force);
			}
		}

		public void SetChild(SceneTriggerExecuter childExecuter, bool isInstant)
		{
			if (childExecuter != null)
			{
				this.childExecuter = childExecuter;
				this.childExecuter.parentExecuter = this;
				if (this.childExecuter.CurrentStateID != CurrentStateID)
				{
					this.childExecuter.MergeStateID(CurrentStateID, isInstant);
				}
			}
			else if (this.childExecuter != null)
			{
				this.childExecuter.parentExecuter = null;
				this.childExecuter = null;
			}
		}

		public string GetExecuterDebugString()
		{
			return string.Format("{6} = {0}, {1}, PEX = {2}, {3}, CH = {4}, {5}", ParentVessel.GUID, InSceneID, (!(parentExecuter != null)) ? 0 : parentExecuter.ParentVessel.GUID, (parentExecuter != null) ? parentExecuter.InSceneID : 0, (!(childExecuter != null)) ? 0 : childExecuter.ParentVessel.GUID, (childExecuter != null) ? childExecuter.InSceneID : 0, base.name);
		}

		public void SetExecuterDetails(SceneTriggerExecuterDetails details, bool isInstant = false, SceneTriggerExecuter fromExecuter = null, bool checkCurrentState = true)
		{
			if (parentExecuter != null && parentExecuter != fromExecuter)
			{
				parentExecuter.SetExecuterDetails(details, isInstant, this);
			}
			else
			{
				if (!states.ContainsKey(details.NewStateID) || (checkCurrentState && _currentState.StateID == details.NewStateID))
				{
					return;
				}
				if (details.IsImmediate.HasValue && details.IsImmediate.Value)
				{
					isInstant = true;
				}
				triggeredPlayerGUID = details.PlayerThatActivated;
				if (!details.IsFail)
				{
					if (MyPlayer.Instance.CancelInteractExecuter == this)
					{
						MyPlayer.Instance.CancelInteractExecuter = null;
					}
					_currentState = states[details.NewStateID];
					_currentState.TriggeredPlayerGUID = triggeredPlayerGUID;
					if (_currentState.TriggeredPlayerGUID == MyPlayer.Instance.GUID && !_currentState.OnCancelIteractGoToState.IsNullOrEmpty())
					{
						MyPlayer.Instance.CancelInteractExecuter = this;
					}
					RunActions(_currentState, details.IsFail, isInstant);
				}
				else
				{
					if (_currentState.TriggeredPlayerGUID == MyPlayer.Instance.GUID && !_currentState.OnCancelIteractGoToState.IsNullOrEmpty())
					{
						MyPlayer.Instance.CancelInteractExecuter = this;
					}
					RunActions(states[details.NewStateID], details.IsFail, isInstant);
				}
				if (childExecuter != null)
				{
					childExecuter.SetExecuterDetails(details, isInstant, this);
				}
			}
		}

		private void OnTranslateFinished()
		{
			if (interactionToSetAfterTranslate != null)
			{
				triggeredPlayerGUID = interactionToSetAfterTranslateGUID;
				CharacterInteractRunner(interactionToSetAfterTranslate, false);
				interactionToSetAfterTranslate = null;
			}
		}

		private void CharacterInteractRunner(CharacterInteractionState cis, bool isInstant)
		{
			if (MyPlayer.Instance.GUID == triggeredPlayerGUID)
			{
				if (!cis.ImmediatePositionChange && !isInstant && (!MyPlayer.Instance.transform.position.IsEpsilonEqual(cis.InteractPosition.position, 0.01f) || !MyPlayer.Instance.transform.rotation.IsEpsilonEqual(cis.InteractPosition.rotation, 0.01f)))
				{
					interactionToSetAfterTranslate = cis;
					interactionToSetAfterTranslateGUID = triggeredPlayerGUID;
					StartCoroutine(MyPlayer.Instance.FpsController.TranslateAndLookAt(cis.InteractPosition, cis.InteractLookAt, OnTranslateFinished));
					return;
				}
				if (cis.SetColliderToKinematic)
				{
					MyPlayer.Instance.FpsController.ToggleKinematic(true);
				}
				if (cis.InteractPosition != null)
				{
					MyPlayer.Instance.transform.position = cis.InteractPosition.position;
					MyPlayer.Instance.transform.rotation = cis.InteractPosition.rotation;
					if (cis.ImmediatePositionChange && cis.ImmediateLockType != 0)
					{
						AnimatorHelper animHelper = MyPlayer.Instance.animHelper;
						AnimatorHelper.LockType? lockType = cis.ImmediateLockType;
						animHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, lockType);
						MyPlayer.Instance.animHelper.ForceAnimationUpdate();
					}
					if (cis.InteractType != 0)
					{
						AnimatorHelper animHelper2 = MyPlayer.Instance.animHelper;
						AnimatorHelper.InteractType? interactType = cis.InteractType;
						animHelper2.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, interactType);
					}
				}
				if (cis.LockType != 0)
				{
					AnimatorHelper animHelper3 = MyPlayer.Instance.animHelper;
					AnimatorHelper.LockType? lockType = cis.LockType;
					animHelper3.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, lockType);
					if (!isInstant)
					{
						MyPlayer.Instance.animHelper.SetParameterTrigger(AnimatorHelper.Triggers.Lock);
					}
					else
					{
						MyPlayer.Instance.animHelper.SetParameterTrigger(AnimatorHelper.Triggers.LockImmediate);
						MyPlayer.Instance.animHelper.ForceAnimationUpdate();
					}
				}
				if ((!isInstant || cis.LockType == AnimatorHelper.LockType.None) && cis.InteractType != 0)
				{
					MyPlayer.Instance.animHelper.ResetParameterTrigger(AnimatorHelper.Triggers.UnlockImmediate);
					MyPlayer.Instance.animHelper.SetParameterTrigger(AnimatorHelper.Triggers.InteractTrigger);
				}
				if (cis.UseAnimationCamera)
				{
					MyPlayer.Instance.FpsController.ToggleCameraAttachToHeadBone(true);
				}
				if (cis.AutoFreeLook)
				{
					MyPlayer.Instance.FpsController.ToggleAutoFreeLook(true);
				}
				if (cis.LockType != 0 || cis.InteractType != 0)
				{
					MyPlayer.Instance.FpsController.ToggleMovement(false);
					MyPlayer.Instance.FpsController.ResetVelocity();
				}
				return;
			}
			OtherPlayer player = Client.Instance.GetPlayer(triggeredPlayerGUID);
			if (player != null)
			{
				if (!cis.ImmediatePositionChange && !isInstant && (!player.transform.position.IsEpsilonEqual(cis.InteractPosition.position, 0.01f) || !player.transform.rotation.IsEpsilonEqual(cis.InteractPosition.rotation, 0.01f)))
				{
					player.tpsController.UpdateMovementPosition = false;
					interactionToSetAfterTranslate = cis;
					interactionToSetAfterTranslateGUID = triggeredPlayerGUID;
					StartCoroutine(player.tpsController.TranslateTo(cis.InteractPosition, OnTranslateFinished));
					return;
				}
				if (cis.InteractPosition != null)
				{
					player.SetGlobalPositionAndRotation(cis.InteractPosition.position, cis.InteractPosition.rotation);
					if (cis.InteractType != 0)
					{
						AnimatorHelper animHelper4 = player.tpsController.animHelper;
						AnimatorHelper.InteractType? interactType = cis.InteractType;
						animHelper4.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, interactType);
					}
					if (cis.InteractType != 0 || cis.LockType != 0)
					{
						player.tpsController.UpdateMovementPosition = false;
					}
				}
				if ((!isInstant || cis.LockType == AnimatorHelper.LockType.None) && cis.InteractType != 0)
				{
					player.tpsController.animHelper.ResetParameterTrigger(AnimatorHelper.Triggers.UnlockImmediate);
					player.tpsController.animHelper.SetParameterTrigger(AnimatorHelper.Triggers.InteractTrigger);
				}
				if (cis.LockType != 0)
				{
					AnimatorHelper animHelper5 = player.tpsController.animHelper;
					AnimatorHelper.LockType? lockType = cis.LockType;
					animHelper5.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, lockType);
					if (!isInstant)
					{
						player.tpsController.animHelper.SetParameterTrigger(AnimatorHelper.Triggers.Lock);
						return;
					}
					player.tpsController.animHelper.SetParameterTrigger(AnimatorHelper.Triggers.LockImmediate);
					player.tpsController.animHelper.ForceAnimationUpdate();
				}
			}
			else if (isInstant && Client.IsGameBuild && cis.LockType != 0)
			{
				cis.Executer = this;
				Client.Instance.CharacterInteractionStatesQueue[triggeredPlayerGUID] = cis;
			}
		}

		public void LockPlayerToTrigger(GameObject trigger)
		{
			if (!(trigger == null) && triggeredPlayerGUID == MyPlayer.Instance.GUID)
			{
				if (!trigger.activeInHierarchy)
				{
					trigger.SetActive(true);
				}
				BaseSceneTrigger component = trigger.GetComponent<BaseSceneTrigger>();
				if (component != null)
				{
					component.Interact(MyPlayer.Instance);
				}
			}
		}

		public void UnlockPlayerFromTrigger(GameObject trigger)
		{
			if (MyPlayer.Instance.LockedToTrigger != null && trigger != null && MyPlayer.Instance.LockedToTrigger == trigger.GetComponent<BaseSceneTrigger>())
			{
				MyPlayer.Instance.LockedToTrigger.CancelInteract(MyPlayer.Instance);
				MyPlayer.Instance.DetachFromPanel();
			}
		}

		public void CharacterInteract(CharacterInteractionState cis)
		{
			CharacterInteractRunner(cis, false);
		}

		public void CharacterIteractInstant(CharacterInteractionState cis)
		{
			CharacterInteractRunner(cis, true);
		}

		public void CharacterInteractInstant(CharacterInteractionState cis, long playerGUID)
		{
			triggeredPlayerGUID = playerGUID;
			CharacterInteractRunner(cis, true);
		}

		public void CharacterUnlock()
		{
			if (MyPlayer.Instance.GUID == triggeredPlayerGUID)
			{
				MyPlayer.Instance.FpsController.ToggleKinematic(false);
				MyPlayer.Instance.animHelper.ResetParameterTrigger(AnimatorHelper.Triggers.LockImmediate);
				if (MyPlayer.Instance.animHelper.IsCurrentAnimState(AnimatorHelper.AnimatorLayers_FPS.InteractionLayer, "Locks"))
				{
					MyPlayer.Instance.animHelper.SetParameterTrigger(AnimatorHelper.Triggers.UnlockImmediate);
					MyPlayer.Instance.animHelper.ForceAnimationUpdate();
				}
				MyPlayer.Instance.FpsController.ToggleMovement(true);
				MyPlayer.Instance.FpsController.ToggleAttached(false);
				MyPlayer.Instance.FpsController.ResetLookAt(0.15f);
				MyPlayer.Instance.FpsController.ToggleCameraAttachToHeadBone(false);
				MyPlayer.Instance.FpsController.ToggleAutoFreeLook(false);
				return;
			}
			OtherPlayer player = Client.Instance.GetPlayer(triggeredPlayerGUID);
			if (player != null)
			{
				player.tpsController.UpdateMovementPosition = true;
				player.tpsController.animHelper.ResetParameterTrigger(AnimatorHelper.Triggers.LockImmediate);
				if (player.tpsController.animHelper.IsCurrentAnimState(AnimatorHelper.AnimatorLayers_TPS.InteractionLayer, "Locks"))
				{
					player.tpsController.animHelper.SetParameterTrigger(AnimatorHelper.Triggers.UnlockImmediate);
					player.tpsController.animHelper.ForceAnimationUpdate();
				}
				player.tpsController.animHelper.SetLayerWeight(AnimatorHelper.AnimatorLayers_TPS.MouseLookVertical, 1f);
			}
			else if (Client.IsGameBuild && Client.Instance.CharacterInteractionStatesQueue.ContainsKey(triggeredPlayerGUID))
			{
				Client.Instance.CharacterInteractionStatesQueue.Remove(triggeredPlayerGUID);
			}
		}

		public void LockCharacter()
		{
			if (MyPlayer.Instance.GUID == triggeredPlayerGUID)
			{
				MyPlayer.Instance.FpsController.ToggleMovement(false);
				MyPlayer.Instance.FpsController.ToggleKinematic(true);
				MyPlayer.Instance.FpsController.ToggleAutoFreeLook(true);
				MyPlayer.Instance.animHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, 0f, 0f);
			}
		}

		public void CharacterUnlockWithTransform(Transform transform)
		{
			if (MyPlayer.Instance.GUID == triggeredPlayerGUID)
			{
				MyPlayer.Instance.transform.position = transform.position;
				MyPlayer.Instance.transform.rotation = transform.rotation;
			}
			CharacterUnlock();
		}

		public void ExecuteCustomActions(int type)
		{
			if (MyPlayer.Instance.GUID != triggeredPlayerGUID || _currentState.CustomActions == null || _currentState.CustomActions.Count == 0)
			{
				return;
			}
			foreach (CustomEventActions customAction in _currentState.CustomActions)
			{
				if (customAction.Type == type)
				{
					customAction.Actions.Invoke();
					break;
				}
			}
		}

		public void OnAnimatorStateEnter(AnimatorActions animAction, SceneTriggerAnimation anim, SceneTriggerAnimation.AnimationState state, int stateID, bool isBefore)
		{
			if (animAction.Animator == anim && stateID == CurrentStateID && ((isBefore && ((animAction.Type == AnimatorActionType.ActivateBeforeStart && state == SceneTriggerAnimation.AnimationState.Active) || (animAction.Type == AnimatorActionType.InactivateBeforeStart && state == SceneTriggerAnimation.AnimationState.Inactive) || (animAction.Type == AnimatorActionType.FailBeforeStart && state == SceneTriggerAnimation.AnimationState.Fail))) || (!isBefore && ((animAction.Type == AnimatorActionType.ActivateStart && state == SceneTriggerAnimation.AnimationState.Active) || (animAction.Type == AnimatorActionType.InactivateStart && state == SceneTriggerAnimation.AnimationState.Inactive) || (animAction.Type == AnimatorActionType.FailStart && state == SceneTriggerAnimation.AnimationState.Fail)))))
			{
				animAction.Actions.Invoke();
			}
		}

		public void OnAnimatorStateExit(AnimatorActions animAction, SceneTriggerAnimation anim, SceneTriggerAnimation.AnimationState state, int stateID, bool isAfter)
		{
			if (animAction.Animator == anim && stateID == CurrentStateID && ((isAfter && ((animAction.Type == AnimatorActionType.ActivateAfterEnd && state == SceneTriggerAnimation.AnimationState.Active) || (animAction.Type == AnimatorActionType.InactivateAfterEnd && state == SceneTriggerAnimation.AnimationState.Inactive) || (animAction.Type == AnimatorActionType.FailAfterEnd && state == SceneTriggerAnimation.AnimationState.Fail))) || (!isAfter && ((animAction.Type == AnimatorActionType.ActivateEnd && state == SceneTriggerAnimation.AnimationState.Active) || (animAction.Type == AnimatorActionType.InactivateEnd && state == SceneTriggerAnimation.AnimationState.Inactive) || (animAction.Type == AnimatorActionType.FailEnd && state == SceneTriggerAnimation.AnimationState.Fail)))))
			{
				animAction.Actions.Invoke();
			}
		}

		public bool AreStatesEqual(SceneTriggerExecuter other)
		{
			if (states.Count != other.states.Count)
			{
				return false;
			}
			foreach (KeyValuePair<int, StateActions> state in states)
			{
				if (!other.states.ContainsKey(state.Key) || other.states[state.Key].StateName != state.Value.StateName)
				{
					return false;
				}
			}
			return true;
		}

		public void PlayerEnterTrigger(SceneTrigger trigg, MyPlayer player, int newStateID)
		{
			proximityTriggerID = trigg.TriggerID;
			proximityIsEnter = true;
			ChangeStateID(newStateID);
		}

		public void PlayerExitTrigger(SceneTrigger trigg, MyPlayer player, int newStateID)
		{
			proximityTriggerID = trigg.TriggerID;
			proximityIsEnter = false;
			ChangeStateID(newStateID);
		}

		public void SetCancelInteractExecuter(SceneTriggerExecuter exec)
		{
			if (exec != null)
			{
				MyPlayer.Instance.CancelInteractExecuter = exec;
			}
		}

		public void CancelInteract()
		{
			ChangeStateID(GetStateID(_currentState.OnCancelIteractGoToState), false);
		}

		public void CallMyPlayerItemInHandsSpecial()
		{
			if (triggeredPlayerGUID == MyPlayer.Instance.GUID && MyPlayer.Instance.Inventory.ItemInHands != null)
			{
				MyPlayer.Instance.Inventory.ItemInHands.Special();
			}
		}

		public void StartExitCryoChamberCountdown(string stateName)
		{
			if (triggeredPlayerGUID == MyPlayer.Instance.GUID)
			{
				MyPlayer.Instance.StartExitCryoChamberCountdown(this, stateName);
			}
		}

		public void MyPlayerInteractWithPilotChair()
		{
			if (CurrentStateActions.TriggeredPlayerGUID == MyPlayer.Instance.GUID)
			{
				MyPlayer.Instance.SittingOnPilotSeat = CurrentStateID != DefaultStateID;
			}
		}
	}
}
