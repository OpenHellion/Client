using System;
using UnityEngine;

namespace OpenHellion
{
	public static class IdManager
	{
		private static string s_playerId;
		public static string PlayerId
		{
			get {
				if (s_playerId == null)
				{
					s_playerId = PlayerPrefs.GetString("player_id", null);

					if (s_playerId == null)
					{
						Dbg.Error("Player id could not be found.");
					}
				}

				return s_playerId;
			}
		}
	}
}
