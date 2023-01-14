using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TriInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZeroGravity;
using ZeroGravity.Network;

public class SpawnPointOptionUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	public Text Name;

	public GameObject Active;

	public Texture2D Screenshot;

	public string Description;

	[Title("Save")]
	public FileInfo SaveFile;

	public Button DeleteButton;

	private bool isSaveFile;

	private string SavedGameVersion;

	public void Start()
	{
		base.gameObject.transform.Reset();
		base.gameObject.SetActive(true);
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
		Name.text = SaveFile.LastWriteTime.ToString() + ((!(text.ToLower() == "autosave")) ? string.Empty : (" (" + Localization.Autosave.ToUpper() + ")"));
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
			SavedGameVersion = new Regex("[^0-9.]").Replace(saveFileAuxData.ClientVersion, string.Empty);
			Description = ((!(SavedGameVersion != new Regex("[^0-9.]").Replace(Application.version, string.Empty))) ? string.Format(Localization.ClientVersion, SavedGameVersion) : ("<color=#D01D1D>" + string.Format(Localization.ClientVersion, SavedGameVersion) + "</color>")) + "\n" + Localization.OrbitingNear + " : <color=#0F4F6F>" + saveFileAuxData.CelestialBody + "</color>\n" + saveFileAuxData.ParentSceneID.ToLocalizedString();
		}
		DeleteButton.onClick.AddListener(DeleteAction);
		GetComponent<Button>().onClick.AddListener(LoadSavedGame);
	}

	private void LoadSavedGame()
	{
		if (!Client.Instance.AllowLoadingOldSaveGames && SavedGameVersion != new Regex("[^0-9.]").Replace(Application.version, string.Empty))
		{
			Client.Instance.ShowMessageBox(Localization.Warning, Localization.WrongSavegameVersion);
			return;
		}
		Client.Instance.CanvasManager.StartingPointScreen.SetActive(false);
		Client.Instance.CanvasManager.SelectSpawnPointScreen.SetActive(false);
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
		Client.Instance.CanvasManager.CurrentSpawnPointScreenshot.texture = Screenshot;
		Client.Instance.CanvasManager.CurrentSpawnPointDescription.text = Description;
		if (isSaveFile)
		{
			DeleteButton.gameObject.SetActive(true);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Client.Instance.CanvasManager.CurrentSpawnPointScreenshot.texture = Client.Instance.SpriteManager.DefaultSceneTexture;
		Client.Instance.CanvasManager.CurrentSpawnPointDescription.text = Localization.ChooseSpawnPoint;
		if (DeleteButton.gameObject.activeInHierarchy)
		{
			DeleteButton.gameObject.SetActive(false);
		}
	}
}
