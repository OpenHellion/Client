using System;
using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Data;

namespace ZeroGravity.LevelDesign
{
	public class AsteroidScene : MonoBehaviour
	{
		[Tooltip("Unique ID for asteroid. ID will be populated automatially on save")]
		public long GUID;

		[Tooltip("Scene name used in game")]
		public string GameName;

		[Tooltip("Scene mass in tons")]
		public float Mass;

		[Tooltip("Object radar signature")]
		public float RadarSignature = 250000000f;

		private float radius;

		public GameObject ExteriorObject;

		public float RelativeDensity = 2.5f;

		public bool SaveSceneData(out string error, List<AsteroidSceneData> asteroids = null)
		{
			error = string.Empty;
			return true;
		}

		private void Awake()
		{
			if (Client.IsGameBuild)
			{
				base.gameObject.SetActive(false);
			}
		}

		public float GetMass()
		{
			float num = 0f;
			SceneTriggerRoom[] componentsInChildren = base.gameObject.GetComponentsInChildren<SceneTriggerRoom>();
			foreach (SceneTriggerRoom sceneTriggerRoom in componentsInChildren)
			{
				num += sceneTriggerRoom.Volume;
			}
			float num2 = 0f;
			if (ExteriorObject != null)
			{
				num2 = SceneHelper.VolumeOfGameObject(ExteriorObject);
				if (num2 <= 0f)
				{
					Dbg.Warning("Unable to calculate volume of '" + ExteriorObject.name + "'");
					ExteriorObject = null;
				}
			}
			if (num2 <= float.Epsilon || num2 < num)
			{
				SceneColliderShipCollision[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<SceneColliderShipCollision>();
				foreach (SceneColliderShipCollision sceneColliderShipCollision in componentsInChildren2)
				{
					num2 += SceneHelper.VolumeOfGameObject(sceneColliderShipCollision.gameObject);
				}
			}
			float num3 = (num2 - num) * RelativeDensity;
			if (num3 <= 0f)
			{
				throw new Exception("Error calculating structure mass (ExteriorObject field invalid OR room triggers summary volume too big OR ship colliders invalid)");
			}
			return num3;
		}
	}
}
