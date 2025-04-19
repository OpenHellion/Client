using System;
using OpenHellion.IO;
using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	[ProtoInclude(100, typeof(LogInRequest))]
	[ProtoInclude(101, typeof(LogOutRequest))]
	[ProtoInclude(103, typeof(PlayerSpawnRequest))]
	[ProtoInclude(104, typeof(PlayerRespawnRequest))]
	[ProtoInclude(105, typeof(SpawnObjectsRequest))]
	[ProtoInclude(106, typeof(SubscribeToObjectsRequest))]
	[ProtoInclude(107, typeof(UnsubscribeFromObjectsRequest))]
	[ProtoInclude(108, typeof(ManeuverCourseRequest))]
	[ProtoInclude(109, typeof(DistressCallRequest))]
	[ProtoInclude(110, typeof(SuicideRequest))]
	[ProtoInclude(111, typeof(PlayersOnServerRequest))]
	[ProtoInclude(112, typeof(AvailableSpawnPointsRequest))]
	[ProtoInclude(113, typeof(VesselSecurityRequest))]
	[ProtoInclude(114, typeof(VesselRequest))]
	[ProtoInclude(115, typeof(AuthorizedVesselsRequest))]
	[ProtoInclude(200, typeof(LogInResponse))]
	[ProtoInclude(201, typeof(LogOutResponse))]
	[ProtoInclude(203, typeof(PlayerSpawnResponse))]
	[ProtoInclude(204, typeof(PlayerRespawnResponse))]
	[ProtoInclude(205, typeof(SpawnObjectsResponse))]
	[ProtoInclude(206, typeof(ManeuverCourseResponse))]
	[ProtoInclude(209, typeof(PlayersOnServerResponse))]
	[ProtoInclude(210, typeof(AvailableSpawnPointsResponse))]
	[ProtoInclude(211, typeof(VesselSecurityResponse))]
	[ProtoInclude(212, typeof(VesselRequestResponse))]
	[ProtoInclude(213, typeof(AuthorizedVesselsResponse))]
	[ProtoInclude(300, typeof(CheckConnectionMessage))]
	[ProtoInclude(301, typeof(EnvironmentReadyMessage))]
	[ProtoInclude(302, typeof(CharacterMovementMessage))]
	[ProtoInclude(303, typeof(DestroyObjectMessage))]
	[ProtoInclude(304, typeof(PlayerStatsMessage))]
	[ProtoInclude(305, typeof(PlayerShootingMessage))]
	[ProtoInclude(306, typeof(PlayerHitMessage))]
	[ProtoInclude(307, typeof(DynamicObjectMovementMessage))]
	[ProtoInclude(308, typeof(MovementMessage))]
	[ProtoInclude(309, typeof(ShipStatsMessage))]
	[ProtoInclude(311, typeof(KillPlayerMessage))]
	[ProtoInclude(312, typeof(CorpseStatsMessage))]
	[ProtoInclude(313, typeof(PlayerRoomMessage))]
	[ProtoInclude(314, typeof(CorpseMovementMessage))]
	[ProtoInclude(316, typeof(DynamicObjectStatsMessage))]
	[ProtoInclude(317, typeof(TurretShootingMessage))]
	[ProtoInclude(318, typeof(DestroyVesselMessage))]
	[ProtoInclude(319, typeof(ResetServer))]
	[ProtoInclude(320, typeof(TextChatMessage))]
	[ProtoInclude(321, typeof(PlayerDrillingMessage))]
	[ProtoInclude(322, typeof(InitializeSpaceObjectMessage))]
	[ProtoInclude(324, typeof(DynamicObjectsInfoMessage))]
	[ProtoInclude(325, typeof(TransferResourceMessage))]
	[ProtoInclude(326, typeof(RefineResourceMessage))]
	[ProtoInclude(327, typeof(ShipCollisionMessage))]
	[ProtoInclude(328, typeof(ResetBaseBuilding))]
	[ProtoInclude(330, typeof(VoiceCommDataMessage))]
	[ProtoInclude(332, typeof(ServerShutDownMessage))]
	[ProtoInclude(333, typeof(LatencyTestMessage))]
	[ProtoInclude(334, typeof(ServerUpdateMessage))]
	[ProtoInclude(335, typeof(NameTagMessage))]
	[ProtoInclude(336, typeof(PortableTurretShootingMessage))]
	[ProtoInclude(337, typeof(ExplosionMessage))]
	[ProtoInclude(338, typeof(FabricateItemMessage))]
	[ProtoInclude(339, typeof(RepairItemMessage))]
	[ProtoInclude(340, typeof(RepairVesselMessage))]
	[ProtoInclude(341, typeof(HurtPlayerMessage))]
	[ProtoInclude(343, typeof(RoomPressureMessage))]
	[ProtoInclude(344, typeof(RecycleItemMessage))]
	[ProtoInclude(345, typeof(CancelFabricationMessage))]
	[ProtoInclude(346, typeof(LockToTriggerMessage))]
	[ProtoInclude(347, typeof(ConsoleMessage))]
	[ProtoInclude(348, typeof(QuestTriggerMessage))]
	[ProtoInclude(349, typeof(QuestStatsMessage))]
	[ProtoInclude(350, typeof(SkipQuestMessage))]
	[ProtoInclude(351, typeof(MiningPointStatsMessage))]
	[ProtoInclude(352, typeof(UpdateBlueprintsMessage))]
	[ProtoInclude(353, typeof(NavigationMapDetailsMessage))]
	[ProtoInclude(354, typeof(UpdateVesselDataMessage))]
	[ProtoInclude(406, typeof(DeleteCharacterRequest))]
	[ProtoInclude(506, typeof(DeleteCharacterResponse))]
	public abstract class NetworkData
	{
		public enum MessageStatus : byte
		{
			Normal = 0,
			Success = 1,
			Failure = 2,
			Removed = 7,
			Shutdown = 8,
			Heartbeat = 9,
			Timeout = 10,
		}

		public long Sender;

		public MessageStatus Status;

		public Guid ConversationGuid { get; set; } = Guid.NewGuid();

		public bool SyncRequest;

		public bool SyncResponse;

		public DateTime ExpirationUtc;

		public override string ToString()
		{
			return JsonSerialiser.Serialize(this);
		}
	}
}
