using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;
using OpenHellion.UI;

public class News : MonoBehaviour
{
	private string url = "http://www.playhellion.com/blog/onenews.php";

	private string buttonUrl = "www.playhellion.com/blog";

	public Text HeadingText;

	public Text PostTime;

	public RawImage Image;

	private string imageUrl;

	public Text Body;

	private void Update()
	{
		if (base.gameObject.activeInHierarchy && Keyboard.current.escapeKey.wasPressedThisFrame)
		{
			ToggleNews(false);
		}
	}

	public void ToggleNews(bool val)
	{
		if (val)
		{
			base.gameObject.SetActive(true);
			if (CheckForInternetConnection())
			{
				StartCoroutine(Connect());
			}
			else
			{
				FailedConnection();
			}
		}
		else
		{
			base.gameObject.SetActive(false);
		}
	}

	public IEnumerator Connect()
	{
		UnityWebRequest con = UnityWebRequest.Get(url);
		yield return con.SendWebRequest();
		if (con.result != UnityWebRequest.Result.Success)
		{
			MonoBehaviour.print("Error downloading: " + con.error);
			yield break;
		}

		string separator = "<br>";
		string[] res = Regex.Split(con.downloadHandler.text, separator);
		HeadingText.text = res[1].ToUpper();
		PostTime.text = res[2];
		Body.text = res[3];
		buttonUrl = res[5];
		imageUrl = res[6];
		Texture2D tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
		UnityWebRequest img = UnityWebRequestTexture.GetTexture(imageUrl);
		yield return img.SendWebRequest();

		if (img.result != UnityWebRequest.Result.Success)
		{
			Debug.Log(img.error);
		}
		else
		{
			tex = DownloadHandlerTexture.GetContent(img);
			Image.texture = tex;
		}
	}

	public void FailedConnection()
	{
		HeadingText.text = "CONNECTION FAILED";
		PostTime.text = string.Empty;
		Body.text = string.Empty;
		buttonUrl = "#";
	}

	public void ShowMe()
	{
		Application.OpenURL(buttonUrl);
	}

	public static bool CheckForInternetConnection()
	{
		// Website is down, so don't try to connect.
		return false;
	}
}
