using System;
using UnityEngine;
using ZeroGravity.CharacterMovement;
using ZeroGravity.LevelDesign;

namespace ZeroGravity.Objects
{
	public abstract class SpaceObjectTransferable : SpaceObject, ISoundOccludable
	{
		[HideInInspector]
		public SceneMoveablePlatform OnPlatform;

		[SerializeField]
		protected TransitionTriggerHelper TransitionTrigger;

		private Vector3 _Gravity;

		[NonSerialized]
		public float ImpactVelocity;

		private SceneTriggerRoom _currentRoomTrigger;

		public SceneTriggerRoom EnterVesselRoomTrigger;

		private bool gravitySet;

		protected bool IsDestroying;

		public Vector3 Gravity
		{
			get
			{
				return (!(Parent != null)) ? _Gravity : (Parent.transform.rotation * _Gravity);
			}
		}

		public Vector3 GravityDirection
		{
			get
			{
				return (!(Parent != null)) ? _Gravity.normalized : (Parent.transform.rotation * _Gravity.normalized);
			}
		}

		public SceneTriggerRoom CurrentRoomTrigger
		{
			get
			{
				return _currentRoomTrigger;
			}
			set
			{
				_currentRoomTrigger = value;
				UpdateGravity();
				SetSoundEnvirontment(value);
				if (this is MyPlayer)
				{
					if (value != null)
					{
						value.ExecuteBehaviourScripts();
						(this as MyPlayer).Pressure = value.AirPressure;
					}
					else
					{
						(this as MyPlayer).Pressure = 0f;
					}
					if (value != null)
					{
						Client.Instance.AmbientSounds.SwitchAmbience(value.Ambience);
						Client.Instance.AmbientSounds.SetEnvironment(value.EnvironmentReverb);
					}
					else
					{
						Client.Instance.AmbientSounds.SwitchAmbience(SoundManager.Instance.SpaceAmbience);
						Client.Instance.AmbientSounds.SetEnvironment(SoundManager.Instance.SpaceEnvironment);
					}
					Client.Instance.CanvasManager.CanvasUI.HelmetHud.WarningsUpdate();
				}
			}
		}

		public bool IsInsideSpaceObject
		{
			get
			{
				return Parent != null && Parent is SpaceObjectVessel;
			}
		}

		public void SetSoundEnvirontment(SceneTriggerRoom roomTrigger)
		{
			SoundEffect[] componentsInChildren = GetComponentsInChildren<SoundEffect>();
			foreach (SoundEffect soundEffect in componentsInChildren)
			{
				if (roomTrigger != null)
				{
					soundEffect.SetEnvironment(roomTrigger.EnvironmentReverb);
				}
				else
				{
					soundEffect.SetEnvironment(string.Empty);
				}
			}
		}

		private void UpdateGravity()
		{
			Vector3 gravity = ((!(_currentRoomTrigger == null) && _currentRoomTrigger.UseGravity) ? _currentRoomTrigger.GravityForce : Vector3.zero);
			SetGravity(gravity);
		}

		public void SetGravity(Vector3 newGravity)
		{
			if (!gravitySet || !Gravity.IsEpsilonEqual(newGravity, 0.0001f))
			{
				gravitySet = true;
				Vector3 gravity = _Gravity;
				_Gravity = newGravity;
				OnGravityChanged(gravity);
			}
		}

		public abstract void EnterVessel(SpaceObjectVessel vessel);

		public abstract void ExitVessel(bool forceExit);

		public abstract void DockedVesselParentChanged(SpaceObjectVessel vessel);

		public abstract void OnGravityChanged(Vector3 oldGravity);

		public virtual void RoomChanged(SceneTriggerRoom prevRoomTrigger)
		{
			UpdateGravity();
		}

		public void CheckRoomTrigger(SceneTriggerRoom trigger)
		{
			if (CurrentRoomTrigger != null && (trigger == null || CurrentRoomTrigger == trigger))
			{
				UpdateGravity();
			}
			else if (trigger == null && CurrentRoomTrigger == null)
			{
				UpdateGravity();
			}
		}

		protected virtual void OnDestroy()
		{
			if (OnPlatform != null)
			{
				OnPlatform.RemoveFromPlatform(this);
			}
			IsDestroying = true;
		}

		public virtual void SetParentTransferableObjectsRoot()
		{
			base.transform.parent = Parent.TransferableObjectsRoot.transform;
		}
	}
}
