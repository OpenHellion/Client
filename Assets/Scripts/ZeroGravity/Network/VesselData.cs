using ProtoBuf;
using ZeroGravity.Data;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class VesselData
	{
		public long Id;

		public string VesselRegistration;

		public string VesselName;

		public string Tag;

		public long SpawnRuleID;

		public GameScenes.SceneID SceneID;

		public float[] CollidersCenterOffset;

		public bool IsDebrisFragment;

		public double CreationSolarSystemTime;

		public double RadarSignature;

		public bool IsDistressSignalActive;

		public bool IsAlwaysVisible;
	}
}
