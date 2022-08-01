using ProtoBuf;
using ZeroGravity.Objects;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class DockedVesselData
	{
		public long GUID;

		public SpaceObjectType Type;

		public VesselData Data;

		public VesselObjects VesselObjects;

		public bool DockingControlsDisabled;

		public bool SecurityPanelsLocked;
	}
}
