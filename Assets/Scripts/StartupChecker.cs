using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZeroGravity;
using OpenHellion.ProviderSystem;
using OpenHellion.Networking;
using OpenHellion.Networking.Message.MainServer;
using System.Collections;
using System;

/// <summary>
/// 	This is the first class that executes. Ensures that this program was launced properly.
/// 	Will probably not be needed.
/// </summary>
public class StartupChecker : MonoBehaviour
{
	/// <summary>
	/// 	Executed on startup.
	/// </summary>
	private void Start()
	{
		// Safety check.
		if (!ProviderManager.AnyInitialised)
		{
			Dbg.Error("No external provider could be found. Exiting.");
			Client.ShowMessageBox(Localization.ConnectionError, Localization.ServerUnreachable, gameObject, Application.Quit);
		}
		else
		{
			StartClient();
		}
	}

	/// <summary>
	/// 	Starts the game.
	/// </summary>
	private void StartClient()
	{
		// Get our player id.
		GetPlayerIdRequest req = new();
		req.Ids.Add(new GetPlayerIdRequest.Entry {
			SteamId = ProviderManager.SteamId,
			DiscordId = ProviderManager.DiscordId
		});

		ConnectionMain.Get<PlayerIdResponse>(req, (res) =>
		{
			// Use the remote id instead.
			if (res != default && res.Result == ResponseResult.Success)
			{
				NetworkController.PlayerId = res.PlayerIds[0];
			}
		});

		// This is if we sucessfully started the game.
		SceneManager.LoadScene("Client", LoadSceneMode.Single);
	}
}
