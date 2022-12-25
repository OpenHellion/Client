using System.IO;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZeroGravity;
using OpenHellion.ProviderSystem;


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
		if (SteamAPI.RestartAppIfNecessary((AppId_t)588210u) || !ProviderManager.AnyInitialised)
		{
			Dbg.Error("No external provider could be found. Exiting.");
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
