using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using ZeroGravity;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;

[ExecuteInEditMode]
public class SceneNameTag : BaseSceneTrigger, ISceneObject
{
	[SerializeField]
	private int _InSceneID;

	public string NameTagText;

	[NonSerialized]
	public SpaceObjectVessel ParentVessel;

	private TextMeshPro _textComponent;

	[SerializeField]
	public Material sourceMat;

	public string fontName;

	public bool Local;

	public override bool CameraMovementAllowed
	{
		get
		{
			return false;
		}
	}

	public override bool ExclusivePlayerLocking
	{
		get
		{
			return true;
		}
	}

	public int InSceneID
	{
		get
		{
			return _InSceneID;
		}
		set
		{
			_InSceneID = value;
		}
	}

	public override SceneTriggerType TriggerType
	{
		get
		{
			return SceneTriggerType.NameTag;
		}
	}

	public override bool IsNearTrigger
	{
		get
		{
			return true;
		}
	}

	public override bool IsInteractable
	{
		get
		{
			return true;
		}
	}

	public override PlayerHandsCheckType PlayerHandsCheck
	{
		get
		{
			return PlayerHandsCheckType.DontCheck;
		}
	}

	public override List<ItemType> PlayerHandsItemType
	{
		get
		{
			return null;
		}
	}

	private void Awake()
	{
		_textComponent = GetComponent<TextMeshPro>();
		if (_textComponent == null)
		{
			_textComponent = gameObject.AddComponent<TextMeshPro>() as TextMeshPro;
		}
	}

	protected override void Start()
	{
		base.Start();
		SetNameTagText(NameTagText);
	}

	public void SetNameTagText(string text)
	{
		NameTagText = text;
		_textComponent.text = NameTagText;
	}

	public void SetNameTagTextEditor(string text)
	{
		NameTagText = text;
		_textComponent.text = NameTagText;
	}

	public override bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
	{
		if (!base.Interact(player, interactWithOverlappingTriggers))
		{
			return false;
		}
		Client.Instance.InGamePanels.NameTagUI.ToggleNameCanvas(this, true, ParentVessel, _InSceneID);
		return true;
	}

	public override void CancelInteract(MyPlayer player)
	{
		base.CancelInteract(player);
		Client.Instance.InGamePanels.NameTagUI.ToggleNameCanvas(this, false, ParentVessel, _InSceneID);
	}
}
