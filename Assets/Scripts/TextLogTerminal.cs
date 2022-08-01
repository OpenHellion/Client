using UnityEngine;
using UnityEngine.UI;
using ZeroGravity;

public class TextLogTerminal : MonoBehaviour
{
	public Text LogTitle;

	public Image CharacterImage;

	public Text CharacterInfo;

	public Text LogText;

	public Text LogDate;

	public LogObject LogObject;

	public void OnInteract()
	{
		if (LogObject == null)
		{
			Dbg.Error("Log Object not set!");
			return;
		}
		LogTitle.text = LogObject.LogTitle;
		LogText.text = LogObject.LogText;
		LogDate.text = LogObject.LogDate;
		if (LogObject.Character != null)
		{
			CharacterImage.sprite = LogObject.Character.CharacterImage;
			CharacterInfo.text = LogObject.Character.CharacterInfo;
		}
		base.gameObject.SetActive(true);
	}

	public void OnDetach()
	{
		Client.Instance.CanvasManager.IsInputFieldIsActive = false;
		base.gameObject.SetActive(false);
	}
}
