using System.Collections.Generic;
using ProtoBuf;
using ZeroGravity.Objects;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class SpawnShipResponseData : SpawnObjectResponseData
	{
		public VesselData Data;

		public VesselObjects VesselObjects;

		public bool IsDummy;

		public bool DockingControlsDisabled;

		public bool SecurityPanelsLocked;

		public List<DockedVesselData> DockedVessels;

		public override SpaceObjectType Type
		{
			get
			{
				return SpaceObjectType.Ship;
			}
			set
			{
			}
		}
	}
}
