// Profile.cs
//
// Copyright (C) 2026, OpenHellion contributors
//
// SPDX-License-Identifier: GPL-3.0-or-later
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using OpenHellion.IO;
using UnityEngine;
using ZeroGravity;

namespace OpenHellion
{
	/// <summary>
	///		The player's local identity and their saved servers.
	/// </summary>
	/// <remarks>
	/// 	Essensially a abstraction for main server and offline mode to work together.
	/// 	TODO: Maybe this can be made an interface in front of the main server/rich-presence?
	/// </remarks>
	public static class Profile
	{
		private const string FileName = "Profile.json";

		private struct SavedServer
		{
			public string IpAddress;

			public int GamePort;

			public int StatusPort;
		}

		private class ProfileFile
		{
			public string Username;

			public List<SavedServer> Servers;

			public List<SavedServer> FavouriteServers;
		}

		public static string Username;

		public static readonly List<ServerConnectionInfo> Servers = new();

		public static readonly List<ServerConnectionInfo> Favourites = new();

		/// <summary>
		///		Raised during boot when offline mode needs the player to pick a username.
		/// </summary>
		public static Action OnRequireUsername;

		public static readonly bool OfflineMode = Properties.GetProperty("offline_mode", false);

		public static bool HasUsername => !Username.IsNullOrEmpty();

		public static bool HasIdentity => !PlayerId.IsNullOrEmpty();

		/// <summary>
		///		The id game servers know this player by. Resolved once during boot, from the username when
		///		offline and from the main server account when online, so every caller can read it
		///		synchronously.
		/// </summary>
		public static string PlayerId;

		/// <summary>
		///		Turn a username into a stable id, so that the same name always resolves to the same player
		///		on every server. Game servers only require the id to parse as a guid.
		/// </summary>
		public static string DeriveId(string username)
		{
			using MD5 md5 = MD5.Create();
			return new Guid(md5.ComputeHash(Encoding.UTF8.GetBytes(username ?? string.Empty))).ToString();
		}

		public static void ToggleFavourite(ServerConnectionInfo server)
		{
			if (!Favourites.Remove(server))
			{
				Favourites.Add(server);
			}

			Save();
		}

		public static void Load()
		{
			Username = null;
			Servers.Clear();
			Favourites.Clear();

			if (!File.Exists(Path.Combine(Application.persistentDataPath, FileName)))
			{
				return;
			}

			try
			{
				ProfileFile file = JsonSerialiser.LoadPersistent<ProfileFile>(FileName);
				if (file is null)
				{
					return;
				}

				Username = file.Username;
				if (file.Servers is not null)
				{
					Servers.AddRange(file.Servers.Select(static m => new ServerConnectionInfo
						{ IpAddress = m.IpAddress, GamePort = m.GamePort, StatusPort = m.StatusPort }));
				}

				if (file.FavouriteServers is not null)
				{
					Favourites.AddRange(file.FavouriteServers.Select(static m => new ServerConnectionInfo
						{ IpAddress = m.IpAddress, GamePort = m.GamePort, StatusPort = m.StatusPort }));
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void Save()
		{
			try
			{
				JsonSerialiser.SerializePersistent(new ProfileFile
				{
					Username = Username,
					Servers = Servers.Select(static m => new SavedServer
						{ IpAddress = m.IpAddress, GamePort = m.GamePort, StatusPort = m.StatusPort }).ToList(),
					FavouriteServers = Favourites.Select(static m => new SavedServer
						{ IpAddress = m.IpAddress, GamePort = m.GamePort, StatusPort = m.StatusPort }).ToList()
				}, FileName);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}
	}
}
