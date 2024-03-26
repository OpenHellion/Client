using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZeroGravity;
using ZeroGravity.Network;
using ZeroGravity.Objects;

public class InventoryCharacterPreview : MonoBehaviour
{
	public static InventoryCharacterPreview Instance;
	public Transform CharacterTransform;
	public Transform HandSlotTransform;
	public GameObject ItemInHands;
	public GameObject OutfitItem;
	public List<GameObject> BodyObjects;
	public List<PreviewCharacterSlots> Slots = new List<PreviewCharacterSlots>();
	public Animator Animator;
	public bool CanRotate;

	[ContextMenuItem("ChangeGender", "ChangeGender")]
	public Gender PlayerGender;

	public GameObject Male;
	public GameObject Female;
	public GameObject FemaleHair;
	public Transform RootBone;
	public Transform CameraNear;
	public Transform CameraFar;
	public Camera Camera;
	[Range(0f, 1f)] public float Zoom;
	private AnimatorOverrideController animOverride;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}

		animOverride = new AnimatorOverrideController();
		animOverride.runtimeAnimatorController = Animator.runtimeAnimatorController;
	}

	public void ZoomCamera(float zoomFraction)
	{
		Zoom = Mathf.Clamp01(Zoom + zoomFraction);
		Camera.transform.position =
			Vector3.Lerp(CameraFar.position, CameraNear.position, Mathf.SmoothStep(0f, 1f, Zoom));
	}

	public void ResetPosition()
	{
		Zoom = 0f;
		Camera.transform.position = CameraFar.position;
		CharacterTransform.rotation = Quaternion.identity;
	}

	public void ChangeGender(Gender gen)
	{
		PlayerGender = gen;
		if (PlayerGender == Gender.Male)
		{
			Male.SetActive(value: true);
			Female.SetActive(value: false);
			FemaleHair.SetActive(value: false);
		}
		else
		{
			Male.SetActive(value: false);
			Female.SetActive(value: true);
			FemaleHair.SetActive(value: true);
		}
	}

	public void ChangeGravity(bool gravity)
	{
		Animator.SetBool("Gravity", !gravity);
	}

	public void RotateCharacter(float rotateValue)
	{
		CharacterTransform.Rotate(0f, 0f - rotateValue, 0f);
	}

	public void SetHandAnimation(AnimationClip animationClip)
	{
		animOverride.AddIfExists("HandSlot", animationClip);
		Animator.runtimeAnimatorController = animOverride;
	}

	public void InstantiateInventoryItem(Item item, InventorySlot.Group inventorySlotGroup)
	{
		if (inventorySlotGroup == InventorySlot.Group.Outfit)
		{
			GameObject gameObject = new GameObject();
			gameObject.name = item.gameObject.name + "_Inventory";
			gameObject.transform.position = item.transform.position;
			gameObject.transform.rotation = item.transform.rotation;
			if (OutfitItem != null)
			{
				RemoveOutfit();
			}

			RemoveItemInHands();
			List<Transform> list = RootBone.GetComponentsInChildren<Transform>().ToList();
			SkinnedMeshRenderer[] componentsInChildren =
				MyPlayer.Instance.Outfit.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
			{
				List<Transform> list2 = new List<Transform>();
				Transform[] bones = skinnedMeshRenderer.bones;
				foreach (Transform transform in bones)
				{
					foreach (Transform item2 in list)
					{
						if (transform.name == item2.name && !list2.Contains(item2))
						{
							list2.Add(item2);
						}
					}
				}

				GameObject gameObject2 = new GameObject();
				gameObject2.name = skinnedMeshRenderer.name;
				gameObject2.transform.SetParent(gameObject.transform);
				SkinnedMeshRenderer skinnedMeshRenderer2 = gameObject2.AddComponent<SkinnedMeshRenderer>();
				skinnedMeshRenderer2.sharedMesh = skinnedMeshRenderer.sharedMesh;
				skinnedMeshRenderer2.materials = skinnedMeshRenderer.materials;
				skinnedMeshRenderer2.bones = list2.ToArray();
			}

			gameObject.transform.SetParent(CharacterTransform);
			OutfitItem = gameObject;
			foreach (GameObject bodyObject in BodyObjects)
			{
				bodyObject.SetActive(value: false);
			}

			Outfit outfit = (Outfit)item;
			foreach (Outfit.SlotGroup slotGroup in outfit.slotGroups)
			{
				foreach (Outfit.SlotInfo slot in slotGroup.Slots)
				{
					foreach (InventorySlot.AttachData itemToAttach in slot.ItemsToAttach)
					{
						if (itemToAttach.Point is not null || !(itemToAttach.Point != outfit.OutfitTrans.gameObject))
						{
							continue;
						}

						GameObject gameObject3;
						if (Slots.FirstOrDefault((PreviewCharacterSlots m) =>
							    m.Point.name == itemToAttach.Point.name) != null)
						{
							gameObject3 = Slots
								.FirstOrDefault((PreviewCharacterSlots m) => m.Point.name == itemToAttach.Point.name)
								.Point.gameObject;
						}
						else
						{
							gameObject3 = new GameObject();
							gameObject3.name = itemToAttach.Point.name;
							gameObject3.transform.SetParent(list.FirstOrDefault((Transform m) =>
								m.name == itemToAttach.Point.transform.parent.name));
							gameObject3.transform.localPosition = itemToAttach.Point.transform.localPosition;
							gameObject3.transform.localRotation = itemToAttach.Point.transform.localRotation;
							gameObject3.transform.localScale = itemToAttach.Point.transform.localScale;
						}

						PreviewCharacterSlots previewCharacterSlots = new PreviewCharacterSlots();
						previewCharacterSlots.SlotGroup = slotGroup.Group;
						previewCharacterSlots.ItemType = itemToAttach.ItemType;
						previewCharacterSlots.Point = gameObject3.transform;
						Slots.Add(previewCharacterSlots);
					}
				}
			}

			Animator.SetLayerWeight(1, 0f);
			gameObject.transform.Reset();
			gameObject.SetLayerRecursively(23);
			return;
		}

		if (inventorySlotGroup == InventorySlot.Group.Hands)
		{
			GameObject gameObject4 = new GameObject();
			gameObject4.name = item.gameObject.name + "_Inventory";
			gameObject4.transform.position = item.transform.position;
			gameObject4.transform.rotation = item.transform.rotation;
			if (item as Outfit != null)
			{
				RecreateHierarchy((item as Outfit).FoldedOutfitTrans, gameObject4.transform);
			}
			else
			{
				RecreateHierarchy(item.transform, gameObject4.transform);
			}

			RemoveItemInHands();
			gameObject4.transform.SetParent(HandSlotTransform);
			SetHandAnimation(item.tpsAnimations.Passive_Idle);
			Animator.SetLayerWeight(1, 1f);
			ItemInHands = gameObject4;
			gameObject4.transform.Reset();
			gameObject4.SetLayerRecursively(23);
			return;
		}

		PreviewCharacterSlots previewCharacterSlots2 = Slots.FirstOrDefault((PreviewCharacterSlots m) =>
			m.SlotGroup == inventorySlotGroup && m.ItemType == item.Type);
		if (previewCharacterSlots2 == null)
		{
			return;
		}

		GameObject gameObject5 = new GameObject();
		gameObject5.name = item.gameObject.name + "_Inventory";
		gameObject5.transform.position = item.transform.position;
		gameObject5.transform.rotation = item.transform.rotation;
		foreach (PreviewCharacterSlots item3 in Slots.Where((PreviewCharacterSlots m) =>
			         m.SlotGroup == inventorySlotGroup && m.AttachedObject != null))
		{
			GameObject.Destroy(item3.AttachedObject);
		}

		RecreateHierarchy(item.transform, gameObject5.transform);
		previewCharacterSlots2.AttachedObject = gameObject5;
		gameObject5.transform.SetParent(previewCharacterSlots2.Point);
		gameObject5.transform.Reset();
		gameObject5.SetLayerRecursively(23);
	}

	public void RemoveOutfit()
	{
		foreach (PreviewCharacterSlots slot in Slots)
		{
			if (slot.Point != null)
			{
				GameObject.Destroy(slot.Point.gameObject);
			}

			if (slot.AttachedObject != null)
			{
				GameObject.Destroy(slot.AttachedObject);
			}
		}

		RemoveItemInHands();
		foreach (GameObject bodyObject in BodyObjects)
		{
			bodyObject.SetActive(value: true);
		}

		Slots.Clear();
		GameObject.Destroy(OutfitItem);
	}

	public void RemoveItemInHands()
	{
		if (ItemInHands != null)
		{
			GameObject.Destroy(ItemInHands);
		}

		Animator.SetLayerWeight(1, 0f);
	}

	public void RecreateHierarchy(Transform from, Transform to)
	{
		if (!from.gameObject.activeInHierarchy)
		{
			return;
		}

		if (from.gameObject.GetComponent<MeshRenderer>() != null && from.gameObject.activeSelf)
		{
			MeshRenderer meshRenderer = to.gameObject.AddComponent<MeshRenderer>();
			MeshFilter meshFilter = to.gameObject.AddComponent<MeshFilter>();
			meshFilter.mesh = from.GetComponent<MeshFilter>().mesh;
			meshRenderer.materials = from.GetComponent<MeshRenderer>().materials;
		}

		foreach (Transform item in from)
		{
			GameObject gameObject = new GameObject();
			gameObject.name = item.name;
			gameObject.transform.SetParent(to);
			gameObject.transform.localPosition = item.localPosition;
			gameObject.transform.localRotation = item.localRotation;
			gameObject.transform.localScale = item.localScale;
			RecreateHierarchy(item, gameObject.transform);
		}
	}

	public void RefreshPreviewCharacter(Inventory inventory)
	{
		StartCoroutine(RefreshPreviewCharacterCoroutine(inventory));
	}

	public IEnumerator RefreshPreviewCharacterCoroutine(Inventory inventory)
	{
		yield return null;
		if (!Camera.gameObject.activeInHierarchy)
		{
			yield break;
		}

		RemoveOutfit();
		if (inventory == null)
		{
			yield break;
		}

		if (inventory.OutfitSlot?.Item != null)
		{
			InstantiateInventoryItem(inventory.OutfitSlot.Item, InventorySlot.Group.Outfit);
		}

		foreach (InventorySlot.Group group in inventory.GetAllSlots().Values.Select((InventorySlot m) => m.SlotGroup)
			         .Distinct())
		{
			if (group != InventorySlot.Group.Outfit && inventory.GetSlotsByGroup(group).First().Value.Item != null)
			{
				InstantiateInventoryItem(inventory.GetSlotsByGroup(group).First().Value.Item, group);
			}

			yield return null;
		}
	}
}
