using UnityEngine;
using UnityEngine.SceneManagement;
using ZeroGravity;
using OpenHellion;
using OpenHellion.ProviderSystem;
using OpenHellion.Networking;
using OpenHellion.Networking.Message.MainServer;
using System.Collections;

/// <summary>
/// 	Checks if the program has all required specifications and dependencies it needs to run.
/// </summary>
public class StartupChecker : MonoBehaviour
{
	private void Start()
	{
		HiResTime.Start();
		if (!RichPresence.AnyInitialised)
		{
			Dbg.Error("No external provider could be found. Exiting...");
			Client.ShowMessageBox(Localization.SystemError, Localization.NoProvider, gameObject, Application.Quit);
			HiResTime.Stop();
		}
		else if (SystemInfo.systemMemorySize < 4000 || SystemInfo.processorFrequency < 2000)
		{
			Dbg.Error("System has invalid specifcations. Exiting...");
			Client.ShowMessageBox(Localization.SystemError, Localization.InvalidSystemSpesifications, gameObject, Application.Quit);
			HiResTime.Stop();
		}
		else
		{
			StartCoroutine(StartClient());
		}
	}

	public IEnumerator StartClient()	
	{
		yield return !RichPresence.HasStarted;

		GetPlayerIdRequest req = new();
		req.Ids.Add(new GetPlayerIdRequest.Entry
		{
			SteamId = RichPresence.SteamId,
			DiscordId = RichPresence.DiscordId
		});

		// Get remote player id.
		MSConnection.Post<PlayerIdResponse>(req, (res) =>
		{
			if (res?.Result == ResponseResult.Success)
			{
				NetworkController.PlayerId = res.PlayerIds[0];
			}
		});

		SceneManager.LoadScene("Client", LoadSceneMode.Single);
		yield break;
	}
}
