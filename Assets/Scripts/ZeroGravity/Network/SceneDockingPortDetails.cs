using System.Collections.Generic;
using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class SceneDockingPortDetails
	{
		public VesselObjectID ID;

		public VesselObjectID DockedToID;

		public bool DockingStatus;

		public float[] RelativePosition;

		public float[] RelativeRotation;

		public float[] CollidersCenterOffset;

		public Dictionary<long, float[]> RelativePositionUpdate;

		public Dictionary<long, float[]> RelativeRotationUpdate;

		public List<ExecuterMergeDetails> ExecutersMerge;

		public List<PairedDoorsDetails> PairedDoors;

		public bool Locked;

		public float[] CollidersCenterOffsetOther;

		public OrbitData VesselOrbit;

		public OrbitData VesselOrbitOther;
	}
}
