using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class ExecuterMergeDetails
	{
		public VesselObjectID ParentTriggerID;

		public VesselObjectID ChildTriggerID;
	}
}
