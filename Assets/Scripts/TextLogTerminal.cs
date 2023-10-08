using UnityEngine;
using UnityEngine.UI;
using OpenHellion;
using UnityEngine.Serialization;

public class TextLogTerminal : MonoBehaviour
{
	public Text LogTitle;

	public Image CharacterImage;

	public Text CharacterInfo;

	public Text LogText;

	public Text LogDate;

	public LogObject LogObject;

	[FormerlySerializedAs("_worldState")] [SerializeField] private World _world;

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

		gameObject.SetActive(true);
	}

	public void OnDetach()
	{
		_world.InGameGUI.IsInputFieldIsActive = false;
		gameObject.SetActive(false);
	}
}
