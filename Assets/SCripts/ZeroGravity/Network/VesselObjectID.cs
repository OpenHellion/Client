using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class VesselObjectID
	{
		public long VesselGUID;

		public int InSceneID;

		public VesselObjectID()
		{
		}

		public VesselObjectID(long vesselGUID, int inSceneID)
		{
			VesselGUID = vesselGUID;
			InSceneID = inSceneID;
		}

		public override string ToString()
		{
			return "[" + VesselGUID + ", " + InSceneID + "]";
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is VesselObjectID))
			{
				return false;
			}
			VesselObjectID vesselObjectID = obj as VesselObjectID;
			return VesselGUID == vesselObjectID.VesselGUID && InSceneID == vesselObjectID.InSceneID;
		}

		public override int GetHashCode()
		{
			long num = 17L;
			num = num * 23 + VesselGUID;
			num = num * 23 + InSceneID;
			return (int)(num & 0xFFFFFFF);
		}
	}
}
