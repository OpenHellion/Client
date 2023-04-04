using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZeroGravity;
using ZeroGravity.Network;

public class SaveGameOptionUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	public TextMeshProUGUI Name;

	public GameObject Active;

	public Texture2D Screenshot;

	public string Description;

	[Title("Save")]
	public FileInfo SaveFile;

	public Button DeleteButton;

	private bool isSaveFile;

	private string m_SavedGameVersion;

	public void Start()
	{
		gameObject.transform.Reset();
		gameObject.SetActive(true);
	}

	/// <summary>
	/// 	Make the button create new worlds.
	/// </summary>
	public void CreateNewGameButton()
	{
		Name.text = Localization.NewGame.ToUpper();
		Screenshot = Client.Instance.SpriteManager.NewGameTexture;
		Description = Localization.NewGameDescription;
		GetComponent<Button>().onClick.AddListener(Client.Instance.PlayNewSPGame);
	}

	public void CreateSaveGameButton()
	{
		isSaveFile = true;
		string text = SaveFile.Name.Replace(".save", string.Empty);
		Name.text = SaveFile.LastWriteTime.ToString() + (text.ToLower() != "autosave" ? text : " (" + Localization.Autosave.ToUpper() + ")");
		JsonTextReader jsonTextReader = new JsonTextReader(File.OpenText(SaveFile.FullName));
		JToken jToken = null;
		while (jsonTextReader.Read())
		{
			if (jsonTextReader.Path == "AuxData")
			{
				jsonTextReader.Read();
				jToken = JToken.Load(jsonTextReader);
				break;
			}
		}
		jsonTextReader.Close();
		if (jToken != null)
		{
			SaveFileAuxData saveFileAuxData = jToken.ToObject<SaveFileAuxData>();
			Screenshot = new Texture2D(2, 2, TextureFormat.RGB24, false);
			Screenshot.LoadImage(saveFileAuxData.Screenshot);
			m_SavedGameVersion = new Regex("[^0-9.]").Replace(saveFileAuxData.ClientVersion, string.Empty);

			// If the version is the same.
			if (m_SavedGameVersion == new Regex("[^0-9.]").Replace(Application.version, string.Empty))
			{
				Description = $"<color=#D01D1D>{string.Format(Localization.ClientVersion, m_SavedGameVersion)}</color>\n"
						+ $"{Localization.OrbitingNear} : <color=#0F4F6F>{saveFileAuxData.CelestialBody}</color>\n"
						+ saveFileAuxData.ParentSceneID.ToLocalizedString();
			}
			else
			{
				Description = string.Format(Localization.ClientVersion, m_SavedGameVersion);
			}
		}
		DeleteButton.onClick.AddListener(DeleteAction);
		GetComponent<Button>().onClick.AddListener(LoadSavedGame);
	}

	private void LoadSavedGame()
	{
		if (!Client.Instance.AllowLoadingOldSaveGames && m_SavedGameVersion != new Regex("[^0-9.]").Replace(Application.version, string.Empty))
		{
			Client.Instance.ShowMessageBox(Localization.Warning, Localization.WrongSavegameVersion);
			return;
		}
		Client.Instance.CanvasManager.StartingPointScreen.SetActive(false);
		Client.Instance.CanvasManager.SaveAndSpawnPointScreen.SetActive(false);
		Client.Instance.StartCoroutine(Client.Instance.PlaySPCoroutine(SaveFile.Name));
	}

	public void DeleteAction()
	{
		Client.Instance.ShowConfirmMessageBox(Localization.DeleteSaveGame, Localization.AreYouSureDeleteSaveGame, Localization.Yes, Localization.No, Delete);
	}

	private void Delete()
	{
		SaveFile.Delete();
		Client.Instance.CanvasManager.ShowSingleplayerSaves();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Client.Instance.CanvasManager.CurrentSaveGameScreenshot.texture = Screenshot;
		Client.Instance.CanvasManager.CurrentSaveGameDescription.text = Description;
		if (isSaveFile)
		{
			DeleteButton.gameObject.SetActive(true);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Client.Instance.CanvasManager.CurrentSaveGameScreenshot.texture = Client.Instance.SpriteManager.DefaultSceneTexture;
		Client.Instance.CanvasManager.CurrentSaveGameDescription.text = Localization.ChooseSpawnPoint;
		if (DeleteButton.gameObject.activeInHierarchy)
		{
			DeleteButton.gameObject.SetActive(false);
		}
	}
}
