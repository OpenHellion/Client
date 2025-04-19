// GameStarter.cs
//
// Copyright (C) 2024, OpenHellion contributors
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
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using OpenHellion.Net;
using OpenHellion.Net.Message;
using OpenHellion.Social;
using OpenHellion.Social.NakamaRpc;
using OpenHellion.Social.RichPresence;
using UnityEngine;
using UnityEngine.SceneManagement;
using OpenHellion.UI;
using ZeroGravity;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;

namespace OpenHellion
{
	/// <summary>
	///		This class is to be instantiated when the we click the play button, and then destroyed when we finally connect.
	/// </summary>
	/// <remarks>
	///		Start a game by calling Create and FindAndConnectServer. It is expected that we are not calling this from the World scene.
	/// </remarks>
	public class GameStarter : MonoBehaviour
	{
		private InviteMessage _inviteMessage;

		private string _nakamaId;

		private World _world;

		/// <summary>
		///		Creates a GameStarter instance. If inviteId is provided the class automatically connects to it.
		/// </summary>
		/// <param name="lastConnectedServer">Last server we were connected to.</param>
		/// <param name="inviteMessage">The invite message we received.</param>
		/// <returns>An instance of GameStarter.</returns>
		public static GameStarter Create(InviteMessage inviteMessage = null)
		{
			var gameObject = new GameObject();
			var gameStarter = gameObject.AddComponent<GameStarter>();
			gameStarter._inviteMessage = inviteMessage;

			DontDestroyOnLoad(gameStarter);

			return gameStarter;
		}

		private async UniTaskVoid Awake()
		{
			_nakamaId = await NakamaClient.GetUserId();
		}

		/// <summary>
		/// 	Get server to connect to from the main server or invite, then start multiplayer game.
		/// </summary>
		public async UniTaskVoid FindServerAndConnect(bool reconnecting = false)
		{
			GlobalGUI.ShowLoadingScreen(GlobalGUI.LoadingScreenType.ConnectingToMain);

			try
			{
				// If signing in for the first time this session, get server we should connect to.
				ServerData connectingServerData = MainMenuGUI.LastConnectedServer;
				if (!reconnecting)
				{
					// Create socket if we are connecting to the game for the first time this session.
					if (MainMenuGUI.LastConnectedServer is null)
					{
						await NakamaClient.CreateSocket();
					}

					if (_inviteMessage is not null)
					{
						connectingServerData = await NakamaClient.GetMatchConnectionInfo(_inviteMessage.ServerId);
					}
					else
					{
						string[] result;
						try
						{
							Regex regex = new Regex("[^0-9.]");
							result = await NakamaClient.FindMatches(new FindMatchesRequest()
							{
								Version = regex.Replace(Application.version, string.Empty),
								Hash = Globals.CombinedHash,
								Location = RegionInfo.CurrentRegion.EnglishName
							});
						}
						catch (WebSocketException ex)
						{
							GlobalGUI.ShowMessageBox(Localization.ConnectionError, Localization.VersionError);
							Debug.LogError("Encountered server error with message: " + ex.Message);
							Destroy(gameObject);
							return;
						}


						// TODO: Add selection menu or something.
						connectingServerData = await NakamaClient.GetMatchConnectionInfo(result[0]);
					}
				}

				// Connect to server!
				MainMenuGUI.LastConnectedServer = connectingServerData;
				ConnectToServer(connectingServerData).Forget();
			}
			catch (Exception e)
			{
				GlobalGUI.ShowMessageBox(Localization.ConnectionError, Localization.NoServerConnection);
				Debug.LogException(e);
				Destroy(gameObject);
			}
		}

		/// <summary>
		/// 	Connect to a remote server.
		/// </summary>
		private async UniTaskVoid ConnectToServer(ServerData server)
		{
			GlobalGUI.ShowLoadingScreen(GlobalGUI.LoadingScreenType.ConnectingToGame);

			await SceneManager.LoadSceneAsync("WorldScene", LoadSceneMode.Single);

			_world = GameObject.Find("/World").GetComponent<World>();
			Debug.Assert(_world is not null);

			try {
				await NetworkController.ConnectToGame(server, _world.OnDisconnectedFromServer);

				Debug.Log("Successfully established connection with server.");

				LogInRequest logInRequest = new LogInRequest
				{
					ServerID = server.Id,
					ClientHash = Globals.CombinedHash,
					PlayerId = _nakamaId,
					CharacterData = await NakamaClient.GetCharacterData()
				};

				var response = await NetworkController.SendReceiveAsync(logInRequest) as LogInResponse;

				if (response != null && response.Status == NetworkData.MessageStatus.Success)
				{
					Debug.Log("Received log in response.");

					GlobalGUI.ShowLoadingScreen(GlobalGUI.LoadingScreenType.LoadWorld);

					bool wasLoginSuccessful = false;
					if (_inviteMessage is not null)
					{
						wasLoginSuccessful = await _world.OnLogin(response, _inviteMessage.SpawnPointId);
					}
					else
					{
						wasLoginSuccessful = await _world.OnLogin(response);
					}

					if (wasLoginSuccessful)
					{
						Debug.Log("ClearCanvasesAndStartGame");
						AkSoundEngine.SetRTPCValue(SoundManager.InGameVolume, 1f);
						MyPlayer.Instance.PlayerReady = true;
						RichPresenceManager.UpdateStatus();
						MyPlayer.Instance.InitializeCameraEffects();

						Globals.ToggleCursor(false);

						GlobalGUI.CloseLoadingScreen();
						_world.LoadingFinishedDelegate();
						FixPlayerInCryo().Forget();
						NetworkController.Send(new EnvironmentReadyMessage());
					}

					Destroy(gameObject);
					return;
				}
				/*else if (response.Status == NetworkData.MessageStatus.VersionError)
				{
					Debug.LogWarning("Version error.");
					GlobalGUI.ShowMessageBox(Localization.ConnectionError, Localization.VersionError);
				}*/
				else
				{
					Debug.LogWarning("Error in login data.");
					GlobalGUI.ShowErrorMessage(Localization.ConnectionError, Localization.NoServerConnection);
				}
			}
			catch (SocketException)
			{
				GlobalGUI.ShowErrorMessage(Localization.ConnectionError, Localization.NoServerConnection);
				Debug.LogWarning("Server refused connection.");
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				GlobalGUI.ShowErrorMessage(Localization.ConnectionError, Localization.NoServerConnection);
			}

			GlobalGUI.CloseLoadingScreen();
			SceneManager.LoadScene(1);
			Destroy(gameObject);
		}

		private async UniTaskVoid FixPlayerInCryo()
		{
			Debug.Log("FixPlayerInCryo");
			SceneTriggerExecutor exec = MyPlayer.Instance.Parent
				.GetComponentsInChildren<SceneTriggerExecutor>(includeInactive: true)
				.FirstOrDefault((SceneTriggerExecutor m) => m.IsMyPlayerInLockedState && m.CurrentState == "spawn");
			if (exec != null)
			{
				await UniTask.WaitUntil(() => MyPlayer.Instance.gameObject.activeInHierarchy);
				await UniTask.WaitForSeconds(0.5f);
				exec.ChangeStateImmediateForce("occupied");
			}
		}
	}
}
