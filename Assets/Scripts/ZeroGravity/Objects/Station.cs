using System;
using UnityEngine;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	public class Station : SpaceObjectVessel
	{
		public override void ChangeStats(Vector3? thrust = null, Vector3? rotation = null, Vector3? autoStabilize = null, float? engineThrustPercentage = null, SubSystemDetails subSystem = null, GeneratorDetails generator = null, RoomDetails roomTrigger = null, DoorDetails door = null, SceneTriggerExecuterDetails sceneTriggerExecuter = null, SceneDockingPortDetails dockingPort = null, AttachPointDetails attachPoint = null, long? stabilizationTarget = null, SpawnPointStats spawnPoint = null, float? selfDestructTime = null, string emblemId = null)
		{
			throw new NotImplementedException();
		}
	}
}
