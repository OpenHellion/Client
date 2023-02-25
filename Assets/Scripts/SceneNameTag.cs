using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using ZeroGravity;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;

[ExecuteInEditMode, RequireComponent(typeof(TextMeshPro))]
public class SceneNameTag : BaseSceneTrigger, ISceneObject
{
	[SerializeField, FormerlySerializedAs("_InSceneID")]
	private int m_inSceneId;

	public string NameTagText;

	[NonSerialized]
	public SpaceObjectVessel ParentVessel;

	[SerializeField]
	private TMP_FontAsset m_font;

	[SerializeField]
	private Material m_sourceMat;

	private TextMeshPro m_textComponent;

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
			return m_inSceneId;
		}
		set
		{
			m_inSceneId = value;
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
		m_textComponent = GetComponent<TextMeshPro>();
	}

	protected override void Start()
	{
		base.Start();
		SetNameTagText(NameTagText);
	}

	public void SetNameTagText(string text)
	{
		NameTagText = text;
		m_textComponent.text = NameTagText;
	}

	public void SetNameTagTextEditor(string text)
	{
		NameTagText = text;
		m_textComponent.text = NameTagText;
	}

	public override bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
	{
		if (!base.Interact(player, interactWithOverlappingTriggers))
		{
			return false;
		}
		Client.Instance.InGamePanels.NameTagUI.ToggleNameCanvas(this, true, ParentVessel, m_inSceneId);
		return true;
	}

	public override void CancelInteract(MyPlayer player)
	{
		base.CancelInteract(player);
		Client.Instance.InGamePanels.NameTagUI.ToggleNameCanvas(this, false, ParentVessel, m_inSceneId);
	}
}
