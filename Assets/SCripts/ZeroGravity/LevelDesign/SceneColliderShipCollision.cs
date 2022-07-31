using UnityEngine;

namespace ZeroGravity.LevelDesign
{
	public class SceneColliderShipCollision : MonoBehaviour, ISceneCollider
	{
		public bool AffectingCenterOfMass = true;

		public SceneColliderType Type
		{
			get
			{
				return SceneColliderType.ShipCollision;
			}
		}
	}
}
