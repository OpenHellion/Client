using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class ReportServerUI : MonoBehaviour
	{
		public Text ReportServerHeading;

		public Dropdown ReportReason;

		public InputField OtherText;

		public GameObject OtherField;

		public Text CharactersCount;

		private List<string> reportInfo;

		private string eMailAddress;

		private bool sendFile;

		private void Awake()
		{
			reportInfo = new List<string>
			{
				Localization.ServerOffline,
				Localization.LatencyProblems,
				Localization.Rubberbanding,
				Localization.ServerStuck,
				Localization.DisconnectedFromServer,
				Localization.Other
			};
			foreach (string item in reportInfo)
			{
				ReportReason.options.Add(new Dropdown.OptionData(item));
			}
			ReportReason.RefreshShownValue();
			ReportReason.value = 0;
		}

		private void Start()
		{
		}

		private void Update()
		{
			if (ReportReason.value == 5)
			{
				OtherField.SetActive(true);
				CharactersCount.text = "( " + OtherText.text.Length + " / " + OtherText.characterLimit + " )";
			}
			else
			{
				OtherField.SetActive(false);
				OtherText.text = string.Empty;
				CharactersCount.text = string.Empty;
			}
			if (sendFile)
			{
				StartCoroutine("PostFileOnServer");
				sendFile = false;
			}
		}

		public void ActivateBox(string nameOfServer)
		{
			if (nameOfServer == null)
			{
				nameOfServer = NetworkController.NameOfCurrentServer;
			}
			base.gameObject.SetActive(true);
			Client.Instance.CanvasManager.IsInputFieldIsActive = true;
			ReportServerHeading.text = Localization.ReportServer + " - " + nameOfServer;
			sendFile = true;
		}

		public void DeactivateBox()
		{
			base.gameObject.SetActive(false);
			Client.Instance.CanvasManager.IsInputFieldIsActive = false;
			CharactersCount.text = " ";
			OtherText.text = string.Empty;
			ReportReason.value = 0;
			ReportReason.RefreshShownValue();
		}

		public void SendReport()
		{
			try
			{
				string text = "http://api.playhellion.com/add-report.php?";
				text = text + "reporter=" + Uri.EscapeUriString(SteamUser.GetSteamID().ToString());
				text = text + "&server=" + Uri.EscapeUriString(Client.LastConnectedServer.Name);
				text = text + "&reason=" + Uri.EscapeUriString(reportInfo[ReportReason.value].ToString());
				text = text + "&other=" + Uri.EscapeUriString(OtherText.textComponent.text);
				text = text + "&ping=" + Uri.EscapeUriString(((!(Client.Instance.CanvasManager.currentlySelectedServer != null)) ? Client.LastConnectedServer : Client.Instance.CanvasManager.currentlySelectedServer).PingText.text);
				text = text.Replace("#", "%23");
				UnityWebRequest request = new UnityWebRequest(text);
				StartCoroutine(WaitForRequest(request));
				Client.Instance.ShowMessageBox(Localization.SendReport, Localization.ReportSent);
			}
			catch
			{
				Client.Instance.ShowMessageBox(Localization.SendReport, Localization.ReportFailed);
			}
			DeactivateBox();
		}

		private IEnumerator WaitForRequest(UnityWebRequest request)
		{
			yield return request;
		}

		public IEnumerator PostFileOnServer()
		{
			WWWForm form = new WWWForm();
			string time = DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss");
			string user = MyPlayer.Instance.PlayerName;
			form.AddField("name", Client.LastConnectedServer.Name + "-" + time);
			string stats = Client.Instance.GetNetworkDataLogs();
			form.AddField("data", Client.LastConnectedServer.Name + " - " + user + "\n" + stats + "\nPOWERED BY JUNKRAT");
			UnityWebRequest req = UnityWebRequest.Post("http://api.playhellion.com/upload-log.php", form);
			yield return req.SendWebRequest();
			if (req.result == UnityWebRequest.Result.ConnectionError)
			{
				Dbg.Log(req.error);
			}
			else
			{
				Dbg.Log("Uploaded");
			}
			Dbg.Log(req.downloadHandler.text);
		}
	}
}
