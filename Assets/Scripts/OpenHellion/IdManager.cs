// IdManager.cs
//
// Copyright (C) 2023, OpenHellion contributors
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

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
