using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Objects;

namespace ZeroGravity.Effects
{
	public class RCSThrusters : MonoBehaviour
	{
		[SerializeField]
		private Vector3 moveVector = Vector3.zero;

		[SerializeField]
		private Vector3 rotateVector = Vector3.zero;

		public Transform CenterOfMass;

		public Camera MoveCamera;

		public List<ExhaustScript> Thrusters = new List<ExhaustScript>();

		public List<ParticleSystem> ParticleThrusters = new List<ParticleSystem>();

		[Tooltip("Sound effect objects need to be properly oriented so the right sound will trigger. Point the UP direction of the object as it is the thruster.")]
		public List<SoundEffect> SoundEffects = new List<SoundEffect>();

		private float moveEpsilon = 1E-06f;

		private float rotateEpsilon = 0.01f;

		public bool IsOn
		{
			get
			{
				return base.gameObject.activeInHierarchy;
			}
			set
			{
				base.gameObject.SetActive(value);
			}
		}

		private void Awake()
		{
			if (!Client.IsGameBuild)
			{
				return;
			}
			if (CenterOfMass == null)
			{
				CenterOfMass = base.transform;
			}
			if (MoveCamera == null)
			{
				MoveCamera = Camera.main;
			}
			if (MoveCamera == null && MyPlayer.Instance != null)
			{
				MoveCamera = MyPlayer.Instance.FpsController.MainCamera;
			}
			ExhaustScript[] componentsInChildren = GetComponentsInChildren<ExhaustScript>(true);
			foreach (ExhaustScript exhaustScript in componentsInChildren)
			{
				if (exhaustScript.transform.parent == base.transform)
				{
					Thrusters.Add(exhaustScript);
					exhaustScript.cameraToLookAt = MoveCamera;
					exhaustScript.gameObject.SetActive(false);
				}
			}
		}

		public void SetRotateVector(Vector3 rotation)
		{
			rotateVector.x = ((rotation.x > rotateEpsilon) ? 1 : ((rotation.x < 0f - rotateEpsilon) ? (-1) : 0));
			rotateVector.y = ((rotation.y > rotateEpsilon) ? 1 : ((rotation.y < 0f - rotateEpsilon) ? (-1) : 0));
			rotateVector.z = ((rotation.z > rotateEpsilon) ? 1 : ((rotation.z < 0f - rotateEpsilon) ? (-1) : 0));
		}

		public void SetMoveVector(Vector3 move)
		{
			moveVector.x = ((move.x > moveEpsilon) ? 1 : ((move.x < 0f - moveEpsilon) ? (-1) : 0));
			moveVector.y = ((move.y > moveEpsilon) ? 1 : ((move.y < 0f - moveEpsilon) ? (-1) : 0));
			moveVector.z = ((move.z > moveEpsilon) ? 1 : ((move.z < 0f - moveEpsilon) ? (-1) : 0));
		}

		public void UpdateThrusters()
		{
			foreach (ExhaustScript thruster in Thrusters)
			{
				if ((moveVector.IsNotEpsilonZero() && (double)Vector3.Dot(moveVector, Quaternion.Inverse(CenterOfMass.rotation) * thruster.transform.rotation * Vector3.up) < -0.7) || (rotateVector.x.IsNotEpsilonZero() && (double)Vector3.Dot(Quaternion.AngleAxis(90f * rotateVector.x, CenterOfMass.right) * (CenterOfMass.position - thruster.transform.position), thruster.transform.up) > 0.7) || (rotateVector.y.IsNotEpsilonZero() && (double)Vector3.Dot(Quaternion.AngleAxis(90f * rotateVector.y, CenterOfMass.up) * (CenterOfMass.position - thruster.transform.position), thruster.transform.up) > 0.7) || (rotateVector.z.IsNotEpsilonZero() && (double)Vector3.Dot(Quaternion.AngleAxis(90f * rotateVector.z, CenterOfMass.forward) * (CenterOfMass.position - thruster.transform.position), thruster.transform.up) > 0.7))
				{
					thruster.gameObject.SetActive(true);
				}
				else
				{
					thruster.gameObject.SetActive(false);
				}
			}
			foreach (ParticleSystem particleThruster in ParticleThrusters)
			{
				if ((moveVector.IsNotEpsilonZero() && (double)Vector3.Dot(moveVector, Quaternion.Inverse(CenterOfMass.rotation) * particleThruster.transform.rotation * Vector3.forward) < -0.7) || (rotateVector.x.IsNotEpsilonZero() && (double)Vector3.Dot(Quaternion.AngleAxis(90f * rotateVector.x, CenterOfMass.right) * (CenterOfMass.position - particleThruster.transform.position), particleThruster.transform.forward) > 0.7) || (rotateVector.y.IsNotEpsilonZero() && (double)Vector3.Dot(Quaternion.AngleAxis(90f * rotateVector.y, CenterOfMass.up) * (CenterOfMass.position - particleThruster.transform.position), particleThruster.transform.forward) > 0.7) || (rotateVector.z.IsNotEpsilonZero() && (double)Vector3.Dot(Quaternion.AngleAxis(90f * rotateVector.z, CenterOfMass.forward) * (CenterOfMass.position - particleThruster.transform.position), particleThruster.transform.forward) > 0.7))
				{
					particleThruster.Play();
				}
				else
				{
					particleThruster.Stop();
				}
			}
			foreach (SoundEffect soundEffect in SoundEffects)
			{
				if (moveVector.IsNotEpsilonZero() || rotateVector.IsNotEpsilonZero())
				{
					soundEffect.Play(0);
				}
				else
				{
					soundEffect.Play(1);
				}
			}
		}
	}
}
