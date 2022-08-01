using System;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Objects;

[Serializable]
public class PreviewCharacterSlots
{
	public InventorySlot.Group SlotGroup;

	public ItemType ItemType;

	public Transform Point;

	public GameObject AttachedObject;
}
