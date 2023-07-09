// IProvider.cs
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

namespace OpenHellion.RichPresence
{
	/// <summary>
	/// 	Wrapper around APIs like Steamworks and Discord Game API.
	/// </summary>
	/// <seealso cref="DiscordProvider"/>
	/// <seealso cref="SteamProvider"/>
	/// <seealso cref="PresenceManager"/>
	internal interface IPresenceProvider
	{
		internal bool Initialise();
		internal void Enable();
		internal void Destroy();
		internal void Update();

		/// <summary>
		/// 	Used to update rich presence.
		/// </summary>
		void UpdateStatus();

		/// <summary>
		/// 	Get the username of our local player.
		/// </summary>
		public string GetUsername();
	}
}
