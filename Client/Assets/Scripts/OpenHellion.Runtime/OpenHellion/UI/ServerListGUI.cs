// ServerListGUI.cs
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
using System.Threading;
using Cysharp.Threading.Tasks;
using OpenHellion.Net;
using OpenHellion.Net.Message;
using OpenHellion.Social;
using OpenHellion.Social.RichPresence;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ZeroGravity;
using ZeroGravity.Network;
using System.Security.Principal;
using System.Runtime.CompilerServices;

namespace OpenHellion.UI
{
	public class ServerListGUI : MonoBehaviour
	{
		private const int PollIntervalMs = 5000;

		private enum SortKey
		{
			Name,
			Favourites,
			Private,
			Character,
			Players,
			Ping
		}

		[Title("Screen")] public GameObject Screen;

		public MainMenuGUI MainMenu;

		[Title("List")] public ServerEntryUI EntryPrefab;

		public Transform EntryHolder;

		public InputField SearchInputField;

		[Title("Password")] public GameObject PasswordHolder;

		public InputField PasswordInputField;

		public Button EnterPassword;

		public Button CancelPassword;

		public TextMeshProUGUI EnterPasswordText;

		[Title("Add server")] public TMP_InputField AddAddress;

		public TMP_InputField AddGamePort;

		public TMP_InputField AddStatusPort;

		private readonly List<ServerEntryUI> _entries = new();

		private CancellationTokenSource _polling;

		private string _search = string.Empty;

		private SortKey _sort = SortKey.Name;

		private bool _sortDescending;

		public void Open()
		{
			Screen.SetActive(true);

			// Reopening with a stale search would look like an empty list.
			_search = string.Empty;
			if (SearchInputField is not null)
			{
				SearchInputField.text = string.Empty;
			}

			Rebuild().Forget();
		}

		public void Close()
		{
			if (PasswordHolder is not null)
			{
				PasswordHolder.SetActive(false);
			}

			StopStatusUpdate();
			Screen.SetActive(false);
		}

		private void OnDisable()
		{
			StopStatusUpdate();
		}

		// Clears all entries and build anew.
		private async UniTaskVoid Rebuild()
		{
			StopStatusUpdate();

			foreach (ServerEntryUI entry in _entries)
			{
				Destroy(entry.gameObject);
			}

			_entries.Clear();

			List<ServerConnectionInfo> servers = new(Profile.Servers);
			List<ServerConnectionInfo> found = RichPresenceManager.GetFriendServers();
			if (!Profile.OfflineMode)
			{
				found.AddRange(await NakamaClient.GetMatches());
			}

			foreach (ServerConnectionInfo server in found)
			{
				if (!servers.Exists(m => m.IpAddress == server.IpAddress && m.GamePort == server.GamePort))
				{
					servers.Add(server);
				}
			}

			foreach (ServerConnectionInfo server in servers)
			{
				ServerEntryUI entry = Instantiate(EntryPrefab, EntryHolder);
				entry.Initialise(server, m => RequestJoin(m.Server, null).Forget(), DeleteCharacter, Remove);
				_entries.Add(entry);
			}

			ApplyFilterAndSort();

			StartStatusUpdate().Forget();
		}

		private async UniTaskVoid StartStatusUpdate()
		{
			StopStatusUpdate();
			_polling = new CancellationTokenSource();
			CancellationToken token = _polling.Token;
			while (!token.IsCancellationRequested)
			{
				foreach (ServerEntryUI entry in _entries.ToArray())
				{
					if (token.IsCancellationRequested)
					{
						return;
					}

					if (entry == null)
					{
						continue;
					}

					(ServerStatusResponse status, int ping) = await FetchStatus(entry.Server);
					entry.Render(status, ping);
				}

				ApplyFilterAndSort();

				await UniTask.Delay(PollIntervalMs, cancellationToken: token).SuppressCancellationThrow();
			}
		}

		private void StopStatusUpdate()
		{
			_polling?.Cancel();
			_polling?.Dispose();
			_polling = null;
		}

		private static async UniTask<(ServerStatusResponse, int)> FetchStatus(ServerConnectionInfo server)
		{
			long start = System.Diagnostics.Stopwatch.GetTimestamp();

			ServerStatusResponse status = await NetworkController.SendTcp(new ServerStatusRequest
			{
				PlayerId = Profile.PlayerId
			}, server.IpAddress, server.StatusPort) as ServerStatusResponse;

			return (status, (int)((System.Diagnostics.Stopwatch.GetTimestamp() - start) * 1000L /
				System.Diagnostics.Stopwatch.Frequency));
		}

		/// <summary>
		/// 	Wired to the search field's value changed event.
		/// </summary>
		public void OnSearchChanged(string search)
		{
			_search = search ?? string.Empty;
			ApplyFilterAndSort();
		}

		public void SortByName()
		{
			SortBy(SortKey.Name);
		}

		public void SortByFavourites()
		{
			SortBy(SortKey.Favourites);
		}

		public void SortByPrivate()
		{
			SortBy(SortKey.Private);
		}

		public void SortByCharacter()
		{
			SortBy(SortKey.Character);
		}

		public void SortByPlayers()
		{
			SortBy(SortKey.Players);
		}

		public void SortByPing()
		{
			SortBy(SortKey.Ping);
		}

		private void SortBy(SortKey key)
		{
			// Clicking the same heading again reverses it, which is what a column header is expected to do.
			_sortDescending = _sort == key && !_sortDescending;
			_sort = key;
			ApplyFilterAndSort();
		}

		private void ApplyFilterAndSort()
		{
			List<ServerEntryUI> visible = new();

			foreach (ServerEntryUI entry in _entries)
			{
				bool matches = _search.IsNullOrEmpty()
					|| entry.DisplayName.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0
					|| (entry.Server.IpAddress is not null &&
					    entry.Server.IpAddress.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0);

				entry.gameObject.SetActive(matches);
				if (matches)
				{
					visible.Add(entry);
				}
			}

			visible.Sort(Compare);

			for (int i = 0; i < visible.Count; i++)
			{
				visible[i].transform.SetSiblingIndex(i);
			}
		}

		private int Compare(ServerEntryUI a, ServerEntryUI b)
		{
			int result = _sort switch
			{
				SortKey.Favourites => Profile.Favourites.Contains(b.Server).CompareTo(Profile.Favourites.Contains(a.Server)),
				SortKey.Private => a.IsPrivate.CompareTo(b.IsPrivate),
				SortKey.Character => string.Compare(a.Character, b.Character, StringComparison.OrdinalIgnoreCase),
				SortKey.Players => b.CurrentPlayers.CompareTo(a.CurrentPlayers),
				// A server we cannot reach has no ping, so it belongs at the bottom rather than at zero.
				SortKey.Ping => (a.IsReachable ? a.PingMs : int.MaxValue)
					.CompareTo(b.IsReachable ? b.PingMs : int.MaxValue),
				_ => 0
			};

			if (result == 0)
			{
				result = string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase);
			}

			return _sortDescending ? -result : result;
		}

		public void AddServerButton()
		{
			if (AddAddress.text.IsNullOrEmpty())
			{
				return;
			}

			if (!int.TryParse(AddGamePort.text, out int gamePort))
			{
				gamePort = 6004;
			}

			if (!int.TryParse(AddStatusPort.text, out int statusPort))
			{
				statusPort = gamePort + 1;
			}

			Profile.Servers.Add(new ServerConnectionInfo
			{
				IpAddress = AddAddress.text,
				GamePort = gamePort,
				StatusPort = statusPort
			});
			Profile.Save();

			AddAddress.text = string.Empty;

			Rebuild().Forget();
		}

		private void Remove(ServerEntryUI entry)
		{
			GlobalGUI.ShowConfirmMessageBox(Localization.RemoveServer, Localization.AreYouSureRemoveServer, Localization.Yes,
				Localization.No, delegate
				{
					Profile.Servers.Remove(entry.Server);
					Profile.Save();
					Rebuild().Forget();
				});
		}

		private void DeleteCharacter(ServerEntryUI entry)
		{
			GlobalGUI.ShowConfirmMessageBox(Localization.DeleteCharacter, Localization.AreYouSureDeleteCharacter,
				Localization.Yes, Localization.No, async delegate
				{
					await NetworkController.SendTcp(new DeleteCharacterRequest { PlayerId = Profile.PlayerId },
						entry.Server.IpAddress, entry.Server.StatusPort, getResponse: false);

					(ServerStatusResponse status, int ping) = await FetchStatus(entry.Server);
					if (entry != null)
					{
						entry.Render(status, ping);
					}
				});
		}

		private async UniTaskVoid RequestJoin(ServerConnectionInfo server, string password)
		{
			StopStatusUpdate();

			JoinInfoResponse joinInfo = await NetworkController.SendTcp(new JoinInfoRequest
			{
				PlayerId = Profile.PlayerId,
				Password = password
			}, server.IpAddress, server.StatusPort) as JoinInfoResponse;

			if (joinInfo is null)
			{
				PasswordHolder.SetActive(false);
				GlobalGUI.ShowMessageBox(Localization.ConnectionError, Localization.NoServerConnection);
				Open();
				return;
			}

			if (!joinInfo.PasswordAccepted)
			{
				AskForPassword(server, password is null ? Localization.EnterServerPassword : Localization.WrongServerPassword);
				return;
			}

			Close();

			if (joinInfo.CharacterData is null)
			{
				MainMenu.ShowCharacterCreation(Profile.Username,
					character => ChooseSpawn(server, joinInfo, character, password));
				return;
			}

			ChooseSpawn(server, joinInfo, joinInfo.CharacterData, password);
		}

		private void AskForPassword(ServerConnectionInfo server, string message)
		{
			PasswordInputField.text = string.Empty;

			if (EnterPasswordText is not null)
			{
				EnterPasswordText.text = message;
			}

			EnterPassword.onClick.RemoveAllListeners();
			EnterPassword.onClick.AddListener(delegate
			{
				if (!PasswordInputField.text.IsNullOrEmpty())
				{
					RequestJoin(server, PasswordInputField.text).Forget();
				}
			});

			if (CancelPassword is not null)
			{
				CancelPassword.onClick.RemoveAllListeners();
				CancelPassword.onClick.AddListener(ClosePasswordPrompt);
			}

			PasswordHolder.SetActive(true);
		}

		/// <summary>
		/// 	Back out of the password prompt and leave the player on the list they came from. Polling was
		/// 	stopped when they picked a server, so it has to be picked up again.
		/// </summary>
		public void ClosePasswordPrompt()
		{
			PasswordHolder.SetActive(false);
			StartStatusUpdate().Forget();
		}

		private void ChooseSpawn(ServerConnectionInfo server, JoinInfoResponse joinInfo, CharacterData character,
			string password)
		{
			// Already alive out there, so there is nothing to choose: the server puts us back where we were.
			if (joinInfo.IsAlive)
			{
				BeginGame(server, character, null, password);
				return;
			}

			MainMenu.ShowSpawnSelection(joinInfo.SpawnPointsList, joinInfo.CanContinue,
				spawn => BeginGame(server, character, spawn, password));
		}

		private void BeginGame(ServerConnectionInfo server, CharacterData character, SpawnPointDetails spawn,
			string password)
		{
			Close();
			GameStarter.Create(server, character, spawn, password).FindServerAndConnect().Forget();
		}
	}
}
