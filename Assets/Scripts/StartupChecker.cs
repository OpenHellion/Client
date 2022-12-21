using System.IO;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZeroGravity;


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
		/*
		if (!SteamManager.Initialized || File.Exists("steam_appid.txt"))
		{
			try
			{
				File.Delete("steam_appid.txt");
			}
			catch { }

			// Never actually do this
			//Application.OpenURL("steam://run/588210");
			Application.Quit();
		}*/
		if (SteamAPI.RestartAppIfNecessary((AppId_t)588210u))
		{
			// Ensure that we launced this through steam.
			// Some anti-piracy thing it seems.
			Application.Quit();
		}
		else
		{
			// This is if we sucessfully started the game.
			Client.LogCustomEvent("application_start", true);
			StartClient();
		}
	}

	/// <summary>
	/// 	Starts the game.
	/// </summary>
	private void StartClient()
	{
		SceneManager.LoadScene("Client", LoadSceneMode.Single);
	}
}
