// ServerEntryUI.cs
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
using OpenHellion.Net.Message;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity;

namespace OpenHellion.UI
{
	public class ServerEntryUI : MonoBehaviour
	{
		public Text Name;

		public Text Description;

		public Text CharacterName;

		public Text Ping;

		public Text PlayerCount;

		public Text AlivePlayers;

		public Button Connect;

		public Button DeleteCharacter;

		public Button Remove;

		public Button Favourite;

		public GameObject FavouriteActive;

		public GameObject Private;

		public GameObject CurrentServer;

		public GameObject Details;

		public ServerConnectionInfo Server { get; private set; }

		public string DisplayName { get; private set; }

		public string Character { get; private set; }

		public bool IsReachable { get; private set; }

		public bool IsPrivate { get; private set; }

		public int PingMs { get; private set; }

		public short CurrentPlayers { get; private set; }

		public void Initialise(ServerConnectionInfo server, Action<ServerEntryUI> onConnect,
			Action<ServerEntryUI> onDeleteCharacter, Action<ServerEntryUI> onRemove)
		{
			Server = server;

			Connect.onClick.AddListener(delegate { onConnect(this); });

			if (DeleteCharacter is not null)
			{
				DeleteCharacter.onClick.AddListener(delegate { onDeleteCharacter(this); });
			}

			if (Remove is not null)
			{
				Remove.onClick.AddListener(delegate { onRemove(this); });
				Remove.gameObject.SetActive(Profile.Servers.Contains(server));
			}

			if (Favourite is not null)
			{
				Favourite.onClick.AddListener(delegate
				{
					Profile.ToggleFavourite(Server);
					if (FavouriteActive is not null)
					{
						FavouriteActive.SetActive(Profile.Favourites.Contains(Server));
					}
				});
			}

			Render(null, 0);
		}

		public void Render(ServerStatusResponse status, int ping)
		{
			DisplayName = status is null || status.Name.IsNullOrEmpty()
				? Server.IpAddress + ":" + Server.GamePort
				: status.Name;
			Character = status?.CharacterName;
			IsReachable = status is not null;
			IsPrivate = status is { IsPrivate: true };
			PingMs = ping;
			CurrentPlayers = status?.CurrentPlayers ?? 0;

			Name.text = DisplayName;

			if (Description is not null)
			{
				Description.text = status?.Description;
			}

			if (CharacterName is not null)
			{
				CharacterName.text = Character ?? string.Empty;
			}

			if (PlayerCount is not null)
			{
				PlayerCount.text = IsReachable ? status.CurrentPlayers + " / " + status.MaxPlayers : string.Empty;
			}

			if (AlivePlayers is not null)
			{
				AlivePlayers.text = IsReachable ? status.AlivePlayers.ToString() : string.Empty;
			}

			if (CurrentServer is not null)
			{
				CurrentServer.SetActive(Globals.LastConnectedServer.Equals(Server));
			}

			if (!IsReachable)
			{
				Ping.text = Localization.Offline.ToUpper();
				Ping.color = Colors.Red;
			}
			else if (status.Hash != Globals.CombinedHash)
			{
				Ping.text = Localization.Disabled.ToUpper();
				Ping.color = Colors.Gray;
			}
			else if (status.IsOffline != Profile.OfflineMode)
			{
				Ping.text = (Localization.WrongMode ?? Localization.Disabled)?.ToUpper();
				Ping.color = Colors.Gray;
			}
			else
			{
				Ping.text = ping + " ms";
				Ping.color = Colors.White;
			}

			Connect.interactable = IsReachable && status.Hash == Globals.CombinedHash
				&& status.IsOffline == Profile.OfflineMode;

			if (DeleteCharacter is not null)
			{
				DeleteCharacter.gameObject.SetActive(Character is not null);
			}

			if (FavouriteActive is not null)
			{
				FavouriteActive.SetActive(Profile.Favourites.Contains(Server));
			}

			if (Private is not null)
			{
				Private.SetActive(IsPrivate);
			}
		}

		public void ToggleDetails()
		{
			if (Details is not null)
			{
				Details.SetActive(!Details.activeSelf);
			}
		}
	}
}
