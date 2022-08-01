using System.Collections.Generic;
using ProtoBuf;
using ZeroGravity.Objects;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class ObjectTransform
	{
		public long GUID;

		public SpaceObjectType Type;

		public float[] Forward;

		public float[] Up;

		public float[] AngularVelocity;

		public float[] RotationVec;

		public OrbitData Orbit;

		public RealtimeData Realtime;

		public ManeuverData Maneuver;

		public List<CharacterMovementMessage> CharactersMovement;

		public List<DynamicObectMovementMessage> DynamicObjectsMovement;

		public List<CorpseMovementMessage> CorpsesMovement;

		public long? StabilizeToTargetGUID;

		public double[] StabilizeToTargetRelPosition;
	}
}
