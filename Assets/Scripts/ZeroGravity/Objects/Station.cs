using System;
using UnityEngine;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	/// <summary>
	///		Unfinished class from the original developers. Probably intended split station code from ship code.
	///		Might be cool to implement later.
	/// </summary>
	public class Station : SpaceObjectVessel
	{
		public override void ChangeStats(Vector3? thrust = null, Vector3? rotation = null,
			Vector3? autoStabilize = null, float? engineThrustPercentage = null, SubSystemDetails subSystem = null,
			GeneratorDetails generator = null, RoomDetails roomTrigger = null, DoorDetails door = null,
			SceneTriggerExecutorDetails sceneTriggerExecutor = null, SceneDockingPortDetails dockingPort = null,
			AttachPointDetails attachPoint = null, long? stabilizationTarget = null, SpawnPointStats spawnPoint = null,
			float? selfDestructTime = null, string emblemId = null)
		{
			throw new NotImplementedException();
		}
	}
}
