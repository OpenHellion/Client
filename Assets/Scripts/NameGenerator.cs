using System;
using System.Collections;
using System.Collections.Generic;
using ZeroGravity.Data;

public static class NameGenerator
{
	private static DateTime lastClearDate = DateTime.UtcNow;

	private static Dictionary<GameScenes.SceneID, int> dailySpawnCount = new Dictionary<GameScenes.SceneID, int>();

	private static List<char> monthCodes = new List<char>
	{
		'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
		'K', 'L'
	};

	private static Dictionary<GameScenes.SceneID, string> shipNaming = new Dictionary<GameScenes.SceneID, string>
	{
		{
			GameScenes.SceneID.AltCorp_LifeSupportModule,
			"LSM-AC:HE3/"
		},
		{
			GameScenes.SceneID.ALtCorp_PowerSupply_Module,
			"PSM-AC:HE1/"
		},
		{
			GameScenes.SceneID.AltCorp_AirLock,
			"AM-AC:HE1/"
		},
		{
			GameScenes.SceneID.AltCorp_Cargo_Module,
			"CBM-AC:HE1/"
		},
		{
			GameScenes.SceneID.AltCorp_Command_Module,
			"CM-AC:HE3/"
		},
		{
			GameScenes.SceneID.AltCorp_DockableContainer,
			"IC-AC:HE2/"
		},
		{
			GameScenes.SceneID.AltCorp_CorridorIntersectionModule,
			"CTM-AC:HE1/"
		},
		{
			GameScenes.SceneID.AltCorp_Corridor45TurnModule,
			"CLM-AC:HE1/"
		},
		{
			GameScenes.SceneID.AltCorp_Corridor45TurnRightModule,
			"CRM-AC:HE1/"
		},
		{
			GameScenes.SceneID.AltCorp_CorridorVertical,
			"CSM-AC:HE1/"
		},
		{
			GameScenes.SceneID.AltCorp_CorridorModule,
			"CIM-AC:HE1/"
		},
		{
			GameScenes.SceneID.AltCorp_StartingModule,
			"OUTPOST "
		},
		{
			GameScenes.SceneID.AltCorp_Shuttle_SARA,
			"AC-ARG HE1/"
		},
		{
			GameScenes.SceneID.AltCorp_CrewQuarters_Module,
			"CQM-AC:HE1/"
		},
		{
			GameScenes.SceneID.AltCorp_SolarPowerModule,
			"SPM-AC:HE1/"
		},
		{
			GameScenes.SceneID.AltCorp_FabricatorModule,
			"FM-AC:HE1/"
		},
		{
			GameScenes.SceneID.AltCorp_Shuttle_CECA,
			"Steropes:HE1/"
		},
		{
			GameScenes.SceneID.Generic_Debris_Spawn1,
			"Small Debris:HE1/"
		},
		{
			GameScenes.SceneID.Generic_Debris_Spawn2,
			"Large Debris:HE1/"
		},
		{
			GameScenes.SceneID.Generic_Debris_Spawn3,
			"Medium Debris:HE1/"
		},
		{
			GameScenes.SceneID.Generic_Debris_Outpost001,
			"Doomed outpost:HE1/"
		},
		{
			GameScenes.SceneID.AltCorp_PatchModule,
			"Patch Module:HE1/"
		}
	};

	private static List<GameScenes.SceneID> derelicts = new List<GameScenes.SceneID>
	{
		GameScenes.SceneID.Generic_Debris_Corridor001,
		GameScenes.SceneID.Generic_Debris_Corridor002,
		GameScenes.SceneID.Generic_Debris_JuncRoom001,
		GameScenes.SceneID.Generic_Debris_JuncRoom002
	};

	public static string GenerateStationRegistration()
	{
		return "STATION";
	}

	public static GameScenes.SceneID GetSceneID(string text)
	{
		IEnumerator enumerator = Enum.GetValues(typeof(GameScenes.SceneID)).GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				GameScenes.SceneID result = (GameScenes.SceneID)enumerator.Current;
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

		foreach (KeyValuePair<GameScenes.SceneID, string> item in shipNaming)
		{
			if (item.Value.ToLower().StartsWith(text.ToLower()))
			{
				return item.Key;
			}
		}

		foreach (KeyValuePair<GameScenes.SceneID, string> item2 in shipNaming)
		{
			if (item2.Value.ToLower().Contains(text.ToLower()))
			{
				return item2.Key;
			}
		}

		return GameScenes.SceneID.None;
	}
}
