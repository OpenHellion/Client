using System;
using OpenHellion;
using UnityEngine;
using ZeroGravity.CharacterMovement;
using ZeroGravity.LevelDesign;

namespace ZeroGravity.Objects
{
	public abstract class SpaceObjectTransferable : SpaceObject, ISoundOccludable
	{
		[HideInInspector] public SceneMoveablePlatform OnPlatform;

		[SerializeField] protected TransitionTriggerHelper TransitionTrigger;

		private Vector3 _gravity;

		[NonSerialized] public float ImpactVelocity;

		private SceneTriggerRoom _currentRoomTrigger;

		public SceneTriggerRoom EnterVesselRoomTrigger;

		private bool _gravitySet;

		protected bool IsDestroying;

		public Vector3 Gravity => Parent is null ? _gravity : Parent.transform.rotation * _gravity;

		public Vector3 GravityDirection =>
			Parent is null ? _gravity.normalized : Parent.transform.rotation * _gravity.normalized;

		public SceneTriggerRoom CurrentRoomTrigger
		{
			get => _currentRoomTrigger;
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
						Globals.SoundEffect.SwitchAmbience(value.Ambience);
						Globals.SoundEffect.SetEnvironment(value.EnvironmentReverb);
					}
					else
					{
						Globals.SoundEffect.SwitchAmbience(SoundManager.SpaceAmbience);
						Globals.SoundEffect.SetEnvironment(SoundManager.SpaceEnvironment);
					}

					World.InGameGUI.HelmetHud.WarningsUpdate();
				}
			}
		}

		public bool IsInsideSpaceObject => Parent is not null && Parent is SpaceObjectVessel;

		public void SetSoundEnvirontment(SceneTriggerRoom roomTrigger)
		{
			SoundEffect[] componentsInChildren = GetComponentsInChildren<SoundEffect>();
			foreach (SoundEffect soundEffect in componentsInChildren)
			{
				soundEffect.SetEnvironment(roomTrigger != null ? roomTrigger.EnvironmentReverb : string.Empty);
			}
		}

		private void UpdateGravity()
		{
			Vector3 gravity = _currentRoomTrigger is not null && _currentRoomTrigger.UseGravity
				? _currentRoomTrigger.GravityForce
				: Vector3.zero;
			SetGravity(gravity);
		}

		public void SetGravity(Vector3 newGravity)
		{
			if (!_gravitySet || !Gravity.IsEpsilonEqual(newGravity, 0.0001f))
			{
				_gravitySet = true;
				Vector3 gravity = _gravity;
				_gravity = newGravity;
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
			if (CurrentRoomTrigger is not null && (trigger is null || CurrentRoomTrigger == trigger))
			{
				UpdateGravity();
			}
			else if (trigger is null && CurrentRoomTrigger is null)
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
			transform.parent = Parent.TransferableObjectsRoot.transform;
		}
	}
}
