using ProtoBuf;
using ZeroGravity.Math;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class CharacterAnimationData
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

		public CharacterAnimationData()
		{
		}

		public CharacterAnimationData(float velocityForward, float velocityRight, float zeroGForward, float zeroGRight,
			float interactType, int playerStance, int turningDirection, float equipOrDeEquip, float equipItemId,
			float emotetype, float reloadItemType, float meleeAttackType, float ladderDirection,
			float playerStanceFloat, float getUpType, float fireMode, float airTime)
		{
			VelocityForward = (byte)MathHelper.ProportionalValue(velocityForward, -1f, 1f, 0f, 255f);
			VelocityRight = (byte)MathHelper.ProportionalValue(velocityRight, -1f, 1f, 0f, 255f);
			ZeroGForward = (byte)MathHelper.ProportionalValue(zeroGForward, -1f, 1f, 0f, 255f);
			ZeroGRight = (byte)MathHelper.ProportionalValue(zeroGRight, -1f, 1f, 0f, 255f);
			InteractType = (byte)interactType;
			PlayerStance = (byte)playerStance;
			TurningDirection = (byte)turningDirection;
			EquipOrDeEquip = (byte)equipOrDeEquip;
			EquipItemId = (byte)equipItemId;
			EmoteType = (byte)emotetype;
			ReloadItemType = (byte)reloadItemType;
			MeleeAttackType = (byte)meleeAttackType;
			LadderDirection = (sbyte)ladderDirection;
			PlayerStanceFloat = (byte)playerStanceFloat;
			GetUpType = (byte)getUpType;
			FireMode = (byte)fireMode;
			AirTime = airTime;
		}
	}
}
