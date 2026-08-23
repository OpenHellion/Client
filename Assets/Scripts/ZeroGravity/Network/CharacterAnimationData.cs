using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public struct CharacterAnimationData
	{
		public byte VelocityForward;

		public byte VelocityRight;

		public byte ZeroGForward;

		public byte ZeroGRight;

		public byte InteractType;

		public byte PlayerStance;

		public byte TurningDirection;

		public byte EquipOrDeEquip;

		public byte EquipItemId;

		public byte EmoteType;

		public byte ReloadItemType;

		public byte MeleeAttackType;

		public sbyte LadderDirection;

		public byte PlayerStanceFloat;

		public byte GetUpType;

		public byte FireMode;

		public float AirTime;
	}
}
