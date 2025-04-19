using System.Collections.Generic;
using ProtoBuf;
using ZeroGravity.Data;
using ZeroGravity.Objects;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class PlayerSpawnResponse : NetworkData
	{
		public long ParentID;

		public SpaceObjectType ParentType;

		public ObjectTransform ParentTransform;

		public long MainVesselID;

		public List<GameScenes.SceneId> Scenes;

		public VesselData VesselData;

		public VesselObjects VesselObjects;

		public List<DockedVesselData> DockedVessels;

		public List<AsteroidMiningPointDetails> MiningPoints;

		public int Health;

		public bool IsAdmin;

		public List<DynamicObjectDetails> DynamicObjects;

		public int SpawnPointID;

		public CharacterTransformData CharacterTransform;

		public long? HomeGUID;

		public double? TimeUntilServerRestart;

		public List<QuestDetails> Quests;

		public List<ItemCompoundType> Blueprints;

		public NavigationMapDetails NavMapDetails;
	}
}
