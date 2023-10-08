using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using OpenHellion;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;

[ExecuteInEditMode, RequireComponent(typeof(TextMeshPro))]
public class SceneNameTag : BaseSceneTrigger, ISceneObject
{
	[SerializeField, FormerlySerializedAs("_InSceneID"), FormerlySerializedAs("m_inSceneId")]
	private int _inSceneId;

	public string NameTagText;

	[NonSerialized] public SpaceObjectVessel ParentVessel;

	private TextMeshPro _textComponent;

	public bool Local;

	public override bool CameraMovementAllowed
	{
		get { return false; }
	}

	public override bool ExclusivePlayerLocking
	{
		get { return true; }
	}

	public int InSceneID
	{
		get { return _inSceneId; }
		set { _inSceneId = value; }
	}

	public override SceneTriggerType TriggerType
	{
		get { return SceneTriggerType.NameTag; }
	}

	public override bool IsNearTrigger
	{
		get { return true; }
	}

	public override bool IsInteractable
	{
		get { return true; }
	}

	public override PlayerHandsCheckType PlayerHandsCheck
	{
		get { return PlayerHandsCheckType.DontCheck; }
	}

	public override List<ItemType> PlayerHandsItemType
	{
		get { return null; }
	}

	private void Awake()
	{
		_textComponent = GetComponent<TextMeshPro>();
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

		World.InWorldPanels.NameTagUI.ToggleNameCanvas(this, true, ParentVessel, _inSceneId);
		return true;
	}

	public override void CancelInteract(MyPlayer player)
	{
		base.CancelInteract(player);
		World.InWorldPanels.NameTagUI.ToggleNameCanvas(this, false, ParentVessel, _inSceneId);
	}
}
