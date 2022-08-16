using System;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity;
using ZeroGravity.Network;

public class GameServerUI : MonoBehaviour
{
	public GameObject Panel;

	public bool IsFavourite;

	private CharacterData _CharacterData;

	public DateTime LastUpdateTime;

	public long Id;

	private string _Name;

	public string IPAddress;

	public int GamePort;

	public int StatusPort;

	public string AltIPAddress;

	public int AltGamePort;

	public int AltStatusPort;

	public bool UseAltIPAddress;

	public bool Locked;

	public ServerTag Tag;

	private uint _Hash;

	public Client.ServerCategories FilterType;

	public bool IsVisible;

	private int _CurrentPlayers;

	private int _AlivePlayers;

	private int _MaxPlayers;

	private bool _OnLine;

	private int _PingTime;

	private string _Description;

	public bool Disabled;

	public Text NameText;

	public Text PingText;

	public Text PlayerCount;

	public Text CharacterNameText;

	public GameObject Private;

	public Button FavouriteButton;

	public GameObject IsFavoriteServer;

	public bool isSelectedServer;

	public GameObject CurrentServer;

	private bool dataChanged;

	public GameObject Details;

	public Text ConnectLabel;

	public Text DescriptionField;

	public Button DeleteCharacter;

	public Button ReportServer;

	public Text AlivePlayersValue;

	public string Name
	{
		get
		{
			return _Name;
		}
		set
		{
			_Name = value;
			dataChanged = true;
		}
	}

	public uint Hash
	{
		get
		{
			return _Hash;
		}
		set
		{
			_Hash = value;
			dataChanged = true;
		}
	}

	public int PingTime
	{
		get
		{
			return _PingTime;
		}
		set
		{
			_PingTime = value;
			dataChanged = true;
		}
	}

	public string Description
	{
		get
		{
			return _Description;
		}
		set
		{
			_Description = value;
		}
	}

	public bool OnLine
	{
		get
		{
			return _OnLine;
		}
		set
		{
			_OnLine = value;
			dataChanged = true;
		}
	}

	public int MaxPlayers
	{
		get
		{
			return _MaxPlayers;
		}
		set
		{
			_MaxPlayers = value;
			dataChanged = true;
		}
	}

	public int CurrentPlayers
	{
		get
		{
			return _CurrentPlayers;
		}
		set
		{
			_CurrentPlayers = value;
			dataChanged = true;
		}
	}

	public int AlivePlayers
	{
		get
		{
			return _AlivePlayers;
		}
		set
		{
			_AlivePlayers = value;
			dataChanged = true;
		}
	}

	public CharacterData CharacterData
	{
		get
		{
			return _CharacterData;
		}
		set
		{
			_CharacterData = value;
			dataChanged = true;
		}
	}

	public void UpdateUI()
	{
		CharacterNameText.text = ((CharacterData == null) ? string.Empty : CharacterData.Name);
		PlayerCount.text = _CurrentPlayers + " / " + _MaxPlayers;
		AlivePlayersValue.text = AlivePlayers.ToString();
		NameText.text = _Name;
		DescriptionField.text = Description;
		Disabled = _Hash != Client.CombinedHash;
		if (Disabled)
		{
			PingText.color = Colors.Gray;
			PingText.text = Localization.Disabled.ToUpper();
		}
		else if (OnLine)
		{
			PingText.text = _PingTime + " ms";
			PingText.color = Colors.White;
		}
		else
		{
			PingText.text = Localization.Offline.ToUpper();
			PingText.color = Colors.Red;
			PlayerCount.text = string.Empty;
		}
		dataChanged = false;
	}

	private void Start()
	{
		ConnectLabel.text = Localization.Connect.ToUpper();
	}

	private void Update()
	{
		if (dataChanged)
		{
			UpdateUI();
		}
	}

	public void ToggleDetails()
	{
		Details.SetActive(!Details.activeInHierarchy);
		if (Details.activeInHierarchy)
		{
			Vector2 sizeDelta = GetComponent<RectTransform>().sizeDelta;
			GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta.x, sizeDelta.y + 100f);
			Canvas.ForceUpdateCanvases();
			if (CharacterNameText.text == string.Empty)
			{
				DeleteCharacter.gameObject.SetActive(false);
			}
			else
			{
				DeleteCharacter.gameObject.SetActive(true);
			}
			if (FilterType == Client.ServerCategories.Official)
			{
				ReportServer.gameObject.SetActive(true);
			}
			else
			{
				ReportServer.gameObject.SetActive(false);
			}
		}
		else
		{
			Vector2 sizeDelta2 = GetComponent<RectTransform>().sizeDelta;
			GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta2.x, sizeDelta2.y - 100f);
			Canvas.ForceUpdateCanvases();
		}
	}

	public void ConnectToServer()
	{
		Client.Instance.ConnectToServer(this);
		Client.Instance.CanvasManager.currentlySelectedServer = this;
	}

	public void DeleteCharacterAction()
	{
		Client.Instance.DeleteCharacterRequest(this);
	}

	public void ReportServerAction()
	{
		Client.Instance.CanvasManager.ReportServerBox.ActivateBox(Name);
	}
}
