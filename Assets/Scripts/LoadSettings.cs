using OpenHellion;
using UnityEngine;
using ZeroGravity.UI;

public class LoadSettings : MonoBehaviour
{
	public GameObject InGameMenu;

	private void Start()
	{
		InGameMenu.GetComponent<Settings>().LoadSettings(Settings.SettingsType.All);
	}
}
