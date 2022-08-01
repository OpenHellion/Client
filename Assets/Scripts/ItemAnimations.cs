using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemAnimations", menuName = "Helpers/ItemAnimations")]
public class ItemAnimations : ScriptableObject
{
	[Serializable]
	public class FPSAnimations
	{
		public AnimationClip Passive_Idle;

		public AnimationClip Passive_WalkForward;

		public AnimationClip Passive_WalkBackward;

		public AnimationClip Passive_WalkRight;

		public AnimationClip Passive_WalkLeft;

		public AnimationClip Passive_Run;

		public AnimationClip Active_Idle;

		public AnimationClip Active_WalkForward;

		public AnimationClip Active_WalkBackward;

		public AnimationClip Active_WalkRight;

		public AnimationClip Active_WalkLeft;

		public AnimationClip Active_Run;

		public AnimationClip Special_Idle;

		public AnimationClip Special_WalkForward;

		public AnimationClip Stance1ToStance2;

		public AnimationClip Stance1ToStance3;

		public AnimationClip Stance2ToStance1;

		public AnimationClip Stance2ToStance3;

		public AnimationClip Stance3ToStance1;

		public AnimationClip Stance3ToStance2;

		public AnimationClip Passive_Jump;

		public AnimationClip Passive_Land;

		public AnimationClip Active_Jump;

		public AnimationClip Active_Land;

		public AnimationClip Special_Jump;

		public AnimationClip Special_Land;

		public AnimationClip Sway_Left;

		public AnimationClip Sway_Right;

		public AnimationClip Sway_Up;

		public AnimationClip Sway_Down;

		public AnimationClip Sway_Idle;

		public AnimationClip WeaponCheck_Idle;

		public AnimationClip Stance1_WeaponCheck_IdleToCheck;

		public AnimationClip Stance1_WeaponCheck_CheckToIdle;

		public AnimationClip Stance2_WeaponCheck_IdleToCheck;

		public AnimationClip Stance2_WeaponCheck_CheckToIdle;

		public AnimationClip Stance2_Shooting_Standard;

		public AnimationClip Stance2_Shooting_PowerShot;

		public AnimationClip Stance3_Shooting_Standard;

		public AnimationClip Stance3_Shooting_PowerShot;

		public AnimationClip Consumable_Use;

		public AnimationClip Stance1_Load;

		public AnimationClip Stance1_Reload;

		public AnimationClip Stance1_Unload;

		public AnimationClip Stance2_Load;

		public AnimationClip Stance2_Reload;

		public AnimationClip Stance2_Unload;

		public AnimationClip Pickup_ToHands;

		public AnimationClip Pickup_ToInventory;

		public AnimationClip Drop_Stance1;

		public AnimationClip Drop_Stance2;

		public AnimationClip Item_Equip;

		public AnimationClip Item_DeEquip;

		public AnimationClip Weapon_Activation;

		public AnimationClip Weapon_ActivatedIdle;

		public AnimationClip Weapon_ActivationCancel;

		public AnimationClip Melee_Passive;

		public AnimationClip Melee_Passive2;

		public AnimationClip Melee_Active;
	}

	[Serializable]
	public class TPSAnimations : FPSAnimations
	{
		public AnimationClip Passive_Walk_Forward_Right;

		public AnimationClip Passive_Walk_Forward_Left;

		public AnimationClip Passive_Walk_Backward_Right;

		public AnimationClip Passive_Walk_Backward_Left;

		public AnimationClip Passive_Run_Forward_Right;

		public AnimationClip Passive_Run_Forward_Left;

		public AnimationClip Passive_Run_Right;

		public AnimationClip Passive_Run_Left;

		public AnimationClip Passive_0G_Idle;

		public AnimationClip Passive_InAir;

		public AnimationClip Active_Walk_Forward_Right;

		public AnimationClip Active_Walk_Forward_Left;

		public AnimationClip Active_Walk_Backward_Right;

		public AnimationClip Active_Walk_Backward_Left;

		public AnimationClip Active_Run_Forward_Right;

		public AnimationClip Active_Run_Forward_Left;

		public AnimationClip Active_Run_Right;

		public AnimationClip Active_Run_Left;

		public AnimationClip Active_0G_Idle;

		public AnimationClip Active_InAir;

		public AnimationClip Special_Walk_Backward;

		public AnimationClip Special_Walk_Right;

		public AnimationClip Special_Walk_Left;

		public AnimationClip Special_Walk_Forward_Right;

		public AnimationClip Special_Walk_Forward_Left;

		public AnimationClip Special_Walk_Backward_Right;

		public AnimationClip Special_Walk_Backward_Left;

		public AnimationClip Special_0G_Idle;

		public AnimationClip Stance1_CrouchWalk_Forward;

		public AnimationClip Stance1_CrouchWalk_Backward;

		public AnimationClip Stance1_CrouchWalk_Right;

		public AnimationClip Stance1_CrouchWalk_Left;

		public AnimationClip Stance1_CrouchWalk_Forward_Right;

		public AnimationClip Stance1_CrouchWalk_Forward_Left;

		public AnimationClip Stance1_CrouchWalk_Backward_Right;

		public AnimationClip Stance1_CrouchWalk_Backward_Left;

		public AnimationClip Stance1_CrouchWalk_Idle;

		public AnimationClip Stance2_CrouchWalk_Forward;

		public AnimationClip Stance2_CrouchWalk_Backward;

		public AnimationClip Stance2_CrouchWalk_Right;

		public AnimationClip Stance2_CrouchWalk_Left;

		public AnimationClip Stance2_CrouchWalk_Forward_Right;

		public AnimationClip Stance2_CrouchWalk_Forward_Left;

		public AnimationClip Stance2_CrouchWalk_Backward_Right;

		public AnimationClip Stance2_CrouchWalk_Backward_Left;

		public AnimationClip Stance2_CrouchWalk_Idle;

		public AnimationClip Stance3_CrouchWalk_Forward;

		public AnimationClip Stance3_CrouchWalk_Backward;

		public AnimationClip Stance3_CrouchWalk_Right;

		public AnimationClip Stance3_CrouchWalk_Left;

		public AnimationClip Stance3_CrouchWalk_Forward_Right;

		public AnimationClip Stance3_CrouchWalk_Forward_Left;

		public AnimationClip Stance3_CrouchWalk_Backward_Right;

		public AnimationClip Stance3_CrouchWalk_Backward_Left;

		public AnimationClip Stance3_CrouchWalk_Idle;
	}

	public FPSAnimations FPS;

	public TPSAnimations TPS;
}
