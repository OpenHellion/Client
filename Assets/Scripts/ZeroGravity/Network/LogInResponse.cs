using System.Collections.Generic;
using OpenHellion.Networking.Message.MainServer;
using ProtoBuf;
using ZeroGravity.Data;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class LogInResponse : NetworkData
	{
		public ResponseResult Response = ResponseResult.Success;

		public long GUID;

		public CharacterData Data;

		public double ServerTime;

		public bool IsAlive;

		public bool CanContinue;

		public List<SpawnPointDetails> SpawnPointsList;

		public List<DebrisFieldDetails> DebrisFields;

		public List<ItemIngredientsData> ItemsIngredients;

		public List<QuestData> Quests;

		public double ExposureRange;

		public float[] VesselExposureValues;

		public float[] PlayerExposureValues;

		public double VesselDecayRateMultiplier;
	}
}
