using System;
using System.Collections;
using System.Collections.Generic;
using OpenHellion;
using OpenHellion.Net;
using OpenHellion.UI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
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

		private List<string> _reportInfo;

		private void Awake()
		{
			_reportInfo = new List<string>
			{
				Localization.LatencyProblems,
				Localization.Rubberbanding,
				Localization.ServerStuck,
				Localization.DisconnectedFromServer,
				Localization.Other
			};
			foreach (string item in _reportInfo)
			{
				ReportReason.options.Add(new Dropdown.OptionData(item));
			}

			ReportReason.RefreshShownValue();
			ReportReason.value = 0;
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
		}

		public void ActivateBox()
		{
			gameObject.SetActive(true);
			ReportServerHeading.text = Localization.ReportServer;
		}

		public void DeactivateBox()
		{
			gameObject.SetActive(false);
			CharactersCount.text = " ";
			OtherText.text = string.Empty;
			ReportReason.value = 0;
			ReportReason.RefreshShownValue();
		}

		/// <summary>
		/// 	TODO: The server is down, so this has to be rewritten eventually.
		/// </summary>
		public void SendReport()
		{
			/*string text = "http://api.playhellion.com/add-report.php?";
			text = text + "reporter=" + Uri.EscapeUriString(NetworkController.PlayerId);
			text = text + "&server=" + Uri.EscapeUriString(MainMenuGUI.LastConnectedServer.Id);
			text = text + "&reason=" + Uri.EscapeUriString(reportInfo[ReportReason.value].ToString());
			text = text + "&other=" + Uri.EscapeUriString(OtherText.textComponent.text);
			text = text.Replace("#", "%23");*/

			DeactivateBox();
		}
	}
}
