using System;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity;
using ZeroGravity.UI;

public class EmblemObjectUI : MonoBehaviour
{
	[NonSerialized] public SecurityScreen Panel;

	[NonSerialized] public string EmblemId;

	[NonSerialized] public Texture2D Texture;

	public RawImage Image;

	public GameObject IsSelected;

	public Text SelectedText;

	private void Start()
	{
		SelectedText.text = Localization.Selected.ToUpper();
	}

	public void SelectEmblem()
	{
		Panel.ChangeEmblem(EmblemId);
	}
}
