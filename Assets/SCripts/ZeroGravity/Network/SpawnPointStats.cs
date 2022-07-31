using ProtoBuf;
using ZeroGravity.Data;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class SpawnPointStats
	{
		public int InSceneID;

		public SpawnPointType? NewType;

		public SpawnPointState? NewState;

		public bool? HackUnlock;

		public long? PlayerGUID;

		public string PlayerName;

		public string PlayerSteamID;

		public bool? PlayerInvite;

		public string InvitedPlayerSteamID;

		public string InvitedPlayerName;
	}
}
