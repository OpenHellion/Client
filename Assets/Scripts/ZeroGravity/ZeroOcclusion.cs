using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZeroGravity.Objects;

namespace ZeroGravity
{
	public static class ZeroOcclusion
	{
		private class ZeroOcclusionObject
		{
			public List<ZeroOccluder> Distance = new List<ZeroOccluder>();

			public ZeroOccluder PlayerInside;

			public ZeroOccluder PlayerOutside;
		}

		private static bool useOcclusion = true;

		private static Dictionary<SpaceObjectVessel, ZeroOcclusionObject> zeroOccluders =
			new Dictionary<SpaceObjectVessel, ZeroOcclusionObject>();

		public static GameObject BlackHole = null;

		public static bool UseOcclusion
		{
			get { return useOcclusion; }
		}

		public static void ToggleOcclusion(bool use)
		{
			useOcclusion = use;
			foreach (KeyValuePair<SpaceObjectVessel, ZeroOcclusionObject> zeroOccluder2 in zeroOccluders)
			{
				if (!use)
				{
					foreach (ZeroOccluder item in zeroOccluder2.Value.Distance)
					{
						item.ShowOccludedObjects(true);
					}

					if (zeroOccluder2.Value.PlayerOutside != null)
					{
						zeroOccluder2.Value.PlayerOutside.ShowOccludedObjects(true);
					}

					if (zeroOccluder2.Value.PlayerInside != null)
					{
						zeroOccluder2.Value.PlayerInside.ShowOccludedObjects(false);
					}
				}
				else
				{
					CheckOcclusionFor(zeroOccluder2.Key, false);
				}
			}
		}

		public static void AddOccludersFrom(SpaceObjectVessel ves)
		{
			ZeroOccluder[] componentsInChildren = ves.GeometryRoot.GetComponentsInChildren<ZeroOccluder>(true);
			foreach (ZeroOccluder zeroOccluder in componentsInChildren)
			{
				if (!zeroOccluders.ContainsKey(ves))
				{
					zeroOccluders.Add(ves, new ZeroOcclusionObject());
				}

				if (zeroOccluder.OccluderType == ZeroOccluder.Type.PlayerInside)
				{
					zeroOccluders[ves].PlayerInside = zeroOccluder;
				}
				else if (zeroOccluder.OccluderType == ZeroOccluder.Type.PlayerOutside)
				{
					zeroOccluders[ves].PlayerOutside = zeroOccluder;
				}
				else if (zeroOccluder.OccluderType == ZeroOccluder.Type.Distance)
				{
					zeroOccluders[ves].Distance.Add(zeroOccluder);
				}
			}
		}

		public static void RemoveOccludersFrom(SpaceObjectVessel ves)
		{
			if (zeroOccluders.ContainsKey(ves))
			{
				zeroOccluders.Remove(ves);
			}
		}

		public static void CheckOcclusionFor(SpaceObjectVessel ves, bool onlyCheckDistance)
		{
			if (ves == null || MyPlayer.Instance == null || !zeroOccluders.ContainsKey(ves))
			{
				return;
			}

			if (onlyCheckDistance)
			{
				foreach (ZeroOccluder item in zeroOccluders[ves].Distance)
				{
					item.ShowOccludedObjects(MyPlayer.Instance.Parent == ves ||
					                         MyPlayer.Instance.transform.position.DistanceSquared(
						                         ves.transform.position) < (double)item.OcclusionDistanceSquared);
				}

				return;
			}

			if (MyPlayer.Instance.Parent is SpaceObjectVessel &&
			    (MyPlayer.Instance.Parent as SpaceObjectVessel).MainVessel == ves.MainVessel)
			{
				foreach (ZeroOccluder item2 in zeroOccluders[ves].Distance)
				{
					item2.ShowOccludedObjects(true);
				}

				if (zeroOccluders[ves].PlayerOutside != null)
				{
					zeroOccluders[ves].PlayerOutside.ShowOccludedObjects(true);
				}

				if (zeroOccluders[ves].PlayerInside != null &&
				    (MyPlayer.Instance.CurrentRoomTrigger == null ||
				     !MyPlayer.Instance.CurrentRoomTrigger.DisablePlayerInsideOccluder) &&
				    (MyPlayer.Instance.EnterVesselRoomTrigger == null ||
				     !MyPlayer.Instance.EnterVesselRoomTrigger.DisablePlayerInsideOccluder))
				{
					zeroOccluders[ves].PlayerInside.ShowOccludedObjects(false);
				}

				return;
			}

			if (zeroOccluders[ves].PlayerInside != null)
			{
				zeroOccluders[ves].PlayerInside.ShowOccludedObjects(true);
			}

			foreach (ZeroOccluder item3 in zeroOccluders[ves].Distance)
			{
				item3.ShowOccludedObjects(MyPlayer.Instance.transform.position.DistanceSquared(ves.transform.position) <
				                          (double)item3.OcclusionDistanceSquared);
			}

			if (zeroOccluders[ves].PlayerOutside != null)
			{
				zeroOccluders[ves].PlayerOutside.ShowOccludedObjects(false);
			}
		}

		public static void DestroyOcclusionObjectsFor(SpaceObjectVessel ves)
		{
			if (!zeroOccluders.ContainsKey(ves))
			{
				return;
			}

			foreach (ZeroOccluder item in zeroOccluders[ves].Distance)
			{
				item.DestroyOcclusionObjects();
			}

			if (zeroOccluders[ves].PlayerInside != null)
			{
				zeroOccluders[ves].PlayerInside.DestroyOcclusionObjects();
			}

			if (zeroOccluders[ves].PlayerOutside != null)
			{
				zeroOccluders[ves].PlayerOutside.DestroyOcclusionObjects();
			}

			RemoveOccludersFrom(ves);
		}
	}
}
