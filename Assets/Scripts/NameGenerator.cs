using System;
using System.Collections;
using System.Collections.Generic;
using ZeroGravity.Data;

public static class NameGenerator
{
	private static DateTime lastClearDate = DateTime.UtcNow;

	private static Dictionary<GameScenes.SceneId, int> dailySpawnCount = new Dictionary<GameScenes.SceneId, int>();

	private static List<char> monthCodes = new List<char>
	{
		'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
		'K', 'L'
	};

	private static Dictionary<GameScenes.SceneId, string> shipNaming = new Dictionary<GameScenes.SceneId, string>
	{
		{
			GameScenes.SceneId.AltCorp_LifeSupportModule,
			"LSM-AC:HE3/"
		},
		{
			GameScenes.SceneId.ALtCorp_PowerSupply_Module,
			"PSM-AC:HE1/"
		},
		{
			GameScenes.SceneId.AltCorp_AirLock,
			"AM-AC:HE1/"
		},
		{
			GameScenes.SceneId.AltCorp_Cargo_Module,
			"CBM-AC:HE1/"
		},
		{
			GameScenes.SceneId.AltCorp_Command_Module,
			"CM-AC:HE3/"
		},
		{
			GameScenes.SceneId.AltCorp_DockableContainer,
			"IC-AC:HE2/"
		},
		{
			GameScenes.SceneId.AltCorp_CorridorIntersectionModule,
			"CTM-AC:HE1/"
		},
		{
			GameScenes.SceneId.AltCorp_Corridor45TurnModule,
			"CLM-AC:HE1/"
		},
		{
			GameScenes.SceneId.AltCorp_Corridor45TurnRightModule,
			"CRM-AC:HE1/"
		},
		{
			GameScenes.SceneId.AltCorp_CorridorVertical,
			"CSM-AC:HE1/"
		},
		{
			GameScenes.SceneId.AltCorp_CorridorModule,
			"CIM-AC:HE1/"
		},
		{
			GameScenes.SceneId.AltCorp_StartingModule,
			"OUTPOST "
		},
		{
			GameScenes.SceneId.AltCorp_Shuttle_SARA,
			"AC-ARG HE1/"
		},
		{
			GameScenes.SceneId.AltCorp_CrewQuarters_Module,
			"CQM-AC:HE1/"
		},
		{
			GameScenes.SceneId.AltCorp_SolarPowerModule,
			"SPM-AC:HE1/"
		},
		{
			GameScenes.SceneId.AltCorp_FabricatorModule,
			"FM-AC:HE1/"
		},
		{
			GameScenes.SceneId.AltCorp_Shuttle_CECA,
			"Steropes:HE1/"
		},
		{
			GameScenes.SceneId.Generic_Debris_Spawn1,
			"Small Debris:HE1/"
		},
		{
			GameScenes.SceneId.Generic_Debris_Spawn2,
			"Large Debris:HE1/"
		},
		{
			GameScenes.SceneId.Generic_Debris_Spawn3,
			"Medium Debris:HE1/"
		},
		{
			GameScenes.SceneId.Generic_Debris_Outpost001,
			"Doomed outpost:HE1/"
		},
		{
			GameScenes.SceneId.AltCorp_PatchModule,
			"Patch Module:HE1/"
		}
	};

	private static List<GameScenes.SceneId> derelicts = new List<GameScenes.SceneId>
	{
		GameScenes.SceneId.Generic_Debris_Corridor001,
		GameScenes.SceneId.Generic_Debris_Corridor002,
		GameScenes.SceneId.Generic_Debris_JuncRoom001,
		GameScenes.SceneId.Generic_Debris_JuncRoom002
	};

	public static string GenerateStationRegistration()
	{
		return "STATION";
	}

	public static GameScenes.SceneId GetSceneID(string text)
	{
		IEnumerator enumerator = Enum.GetValues(typeof(GameScenes.SceneId)).GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				GameScenes.SceneId result = (GameScenes.SceneId)enumerator.Current;
				if (result.ToString().ToLower().StartsWith(text.ToLower()))
				{
					return result;
				}
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}

		foreach (KeyValuePair<GameScenes.SceneId, string> item in shipNaming)
		{
			if (item.Value.ToLower().StartsWith(text.ToLower()))
			{
				return item.Key;
			}
		}

		foreach (KeyValuePair<GameScenes.SceneId, string> item2 in shipNaming)
		{
			if (item2.Value.ToLower().Contains(text.ToLower()))
			{
				return item2.Key;
			}
		}

		return GameScenes.SceneId.None;
	}
}
