using System;
using System.Collections;
using OpenHellion.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using ZeroGravity;

public class MessageTerminal : MonoBehaviour
{
	public GameObject SendMessageScreen;

	public InputField Input;

	public void OnInteract()
	{
		SendMessageScreen.SetActive(false);
		base.gameObject.SetActive(true);
	}

	public void OnDetach()
	{
		Client.Instance.CanvasManager.IsInputFieldIsActive = false;
		base.gameObject.SetActive(false);
	}

	public void SendMessageAction()
	{
		SendMessageScreen.SetActive(true);
		Input.Select();
		Input.text = string.Empty;
		Client.Instance.CanvasManager.IsInputFieldIsActive = true;
	}

	public void CancelSending()
	{
		SendMessageScreen.SetActive(false);
		Client.Instance.CanvasManager.IsInputFieldIsActive = false;
	}

	public void SendMessageToEarth()
	{
		try
		{
			string text = "http://api.playhellion.com/add-message.php?";
			text = text + "reporter=" + Uri.EscapeUriString(NetworkController.PlayerId);
			text = text + "&msg=" + Uri.EscapeUriString(Input.textComponent.text);
			text = text.Replace("#", "%23");
			UnityWebRequest req = new UnityWebRequest(text);
			StartCoroutine(WaitForRequest(req));
		}
		catch
		{
		}
		SendMessageScreen.SetActive(false);
		Client.Instance.CanvasManager.IsInputFieldIsActive = false;
	}

	private IEnumerator WaitForRequest(UnityWebRequest www)
	{
		yield return www;
	}
}
