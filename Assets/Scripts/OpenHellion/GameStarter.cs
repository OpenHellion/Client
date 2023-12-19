// GameStarter.cs
//
// Copyright (C) 2023, OpenHellion contributors
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

namespace OpenHellion
{
	/// <summary>
	///		This class is to be instantiated when the game starts, and then destroyed when we finally connect.
	///		Its point is to handle the mid-level code from clicking the button to closing the loading screen.
	/// </summary>
	/// <remarks>
	///		This class has two methods of starting the game. 1. calling PlayMultiplayer and 2. assigning an id to inviteId when calling Create.
	///		This class is temporary, so nothing here will be kept.
	/// </remarks>
	public class GameStarter : MonoBehaviour
	{
		private InviteMessage _inviteMessage;

		private string _nakamaId;

		private ServerData _lastConnectedServer;

		private World _world;

		/// <summary>
		///		Creates a GameStarter instance. If inviteId is provided the class automatically connects to it.
		/// </summary>
		/// <param name="lastConnectedServer">Last server we were connected to.</param>
		/// <param name="inviteMessage">The invite message we received.</param>
		/// <returns>An instance of GameStarter.</returns>
		public static GameStarter Create(ref ServerData lastConnectedServer,
			InviteMessage inviteMessage = null)
		{
			var gameObject = new GameObject();
			var gameStarter = gameObject.AddComponent<GameStarter>();
			gameStarter._lastConnectedServer = lastConnectedServer;
			gameStarter._inviteMessage = inviteMessage;

			DontDestroyOnLoad(gameStarter);

			return gameStarter;
		}

		private async void Awake()
		{
			_nakamaId = await NakamaClient.GetUserId();

			EventSystem.AddListener(typeof(LogInResponse), LogInResponseListener);
		}

		private void OnDestroy()
		{
			EventSystem.RemoveListener(typeof(LogInResponse), LogInResponseListener);
		}

		/// <summary>
		/// 	Get server to connect to from the main server or invite, then start multiplayer game.
		/// </summary>
		public async void FindServerAndConnect(bool reconnecting = false)
		{
			GlobalGUI.ShowLoadingScreen(GlobalGUI.LoadingScreenType.ConnectingToMain);

			try
			{
				// If signing in for the first time this session, get server we should connect to.
				ServerData connectingServerData = _lastConnectedServer;
				if (!reconnecting)
				{
					// Create socket if we are connecting to the game for the first time this session.
					if (_lastConnectedServer is null)
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
							return;
						}


						// TODO: Add selection menu or something.
						connectingServerData = await NakamaClient.GetMatchConnectionInfo(result[0]);
					}

					// TODO: Move this to InitialisingScene.
					//if (signInResponse.Result is ResponseResult.ServerNotFound)
					//{
					//	CanvasManager.SelectScreen(CanvasManager.Screen.MainMenu);
					//	ShowMessageBox(Localization.ConnectionError, Localization.NoServerConnection);
					//}
					//else if (signInResponse.Result is ResponseResult.ClientVersionError)
					//{
					//	CanvasManager.SelectScreen(CanvasManager.Screen.MainMenu);
					//	ShowMessageBox(Localization.VersionError, Localization.VersionErrorMessage);
					//}
				}

				// Connect to server!
				ConnectToServer(connectingServerData);
				_lastConnectedServer = connectingServerData;
			}
			catch (Exception e)
			{
				GlobalGUI.ShowMessageBox(Localization.ConnectionError, Localization.NoServerConnection);
				Debug.LogException(e);
			}
		}

		/// <summary>
		/// 	Connect to a remote server.
		/// </summary>
		private void ConnectToServer(ServerData server)
		{
			_lastConnectedServer = server;
			GlobalGUI.ShowLoadingScreen(GlobalGUI.LoadingScreenType.ConnectingToGame);

			SceneManager.LoadScene("WorldScene", LoadSceneMode.Single);
			NetworkController.ConnectToGame(server, OnConnected);

			_world = GameObject.Find("/World").GetComponent<World>();
			Debug.Assert(_world != null);

			InvokeRepeating(nameof(CheckLoadingComplete), 3f, 1f);
		}

		private async void OnConnected()
		{
			try
			{
				Debug.Log("Successfully established connection with server.");

				LogInRequest logInRequest = new LogInRequest
				{
					ServerID = _lastConnectedServer.Id,
					ClientHash = Globals.CombinedHash,
					PlayerId = _nakamaId,
					CharacterData = await NakamaClient.GetCharacterData()
				};

				NetworkController.SendToGameServer(logInRequest);
			}
			catch (Exception ex)
			{
				Debug.LogErrorFormat("Server dropped connection. Exception: {0}\nStack trace: {1}", ex.Message, ex.StackTrace);
				GlobalGUI.ShowMessageBox(Localization.ConnectionError, Localization.NoServerConnection);
				CancelInvoke(nameof(CheckLoadingComplete));
				SceneManager.LoadScene(1);
			}
		}

		private void LogInResponseListener(NetworkData data)
		{
			LogInResponse logInResponse = data as LogInResponse;
			if (logInResponse.Response == ResponseResult.Success)
			{
				GlobalGUI.ShowLoadingScreen(GlobalGUI.LoadingScreenType.LoadWorld);
				Debug.Log("Successfully logged into game.");

				if (_inviteMessage is not null)
				{
					_world.OnLogin(logInResponse, ref _inviteMessage.SpawnPointId);
				}
				else
				{
					_world.OnLogin(logInResponse, ref new InviteMessage().SpawnPointId);
				}

				return;
			}

			if (logInResponse.Response == ResponseResult.ClientVersionError)
			{
				Debug.LogError("Version error.");

				GlobalGUI.ShowMessageBox(Localization.ConnectionError, Localization.VersionError);
			}
			else
			{
				Debug.LogError("Server dropped connection.");
				GlobalGUI.ShowMessageBox(Localization.ConnectionError, Localization.NoServerConnection);
			}

			CancelInvoke(nameof(CheckLoadingComplete));
			SceneManager.LoadScene(1);
		}

		private void CheckLoadingComplete()
		{
			Debug.Log("CheckLoadingComplete");
			if (_world.LoadingFinishedTask != null)
			{
				Invoke(nameof(AfterLoadingFinishedTask), 1f);
			}
		}

		public void AfterLoadingFinishedTask()
		{
			this.Invoke(ClearCanvasesAndStartGame, MyPlayer.Instance.Parent is Pivot ? 10 : 0);
		}

		private void ClearCanvasesAndStartGame()
		{
			Debug.Log("ClearCanvasesAndStartGame");
			this.CancelInvoke(CheckLoadingComplete);
			AkSoundEngine.SetRTPCValue(SoundManager.InGameVolume, 1f);
			MyPlayer.Instance.PlayerReady = true;
			RichPresenceManager.UpdateStatus();
			MyPlayer.Instance.InitializeCameraEffects();

			Globals.ToggleCursor(false);

			GlobalGUI.CloseLoadingScreen();
			StartCoroutine(FixPlayerInCryo());
			_world.LoadingFinishedTask.RunSynchronously();
			NetworkController.SendToGameServer(new EnvironmentReadyMessage());
		}

		private IEnumerator FixPlayerInCryo()
		{
			Debug.Log("FixPlayerInCryo");
			SceneTriggerExecutor exec = MyPlayer.Instance.Parent
				.GetComponentsInChildren<SceneTriggerExecutor>(includeInactive: true)
				.FirstOrDefault((SceneTriggerExecutor m) => m.IsMyPlayerInLockedState && m.CurrentState == "spawn");
			if (exec is not null)
			{
				yield return new WaitUntil(() => MyPlayer.Instance.gameObject.activeInHierarchy);
				yield return new WaitForSecondsRealtime(0.5f);
				exec.ChangeStateImmediateForce("occupied");
			}

			Destroy(gameObject);
		}
	}
}
