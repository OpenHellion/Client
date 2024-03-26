using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	public class GenericItem : Item
	{
		public GenericItemSubType SubType;

		[SerializeField] private GameObject FoldedItem;

		[SerializeField] private List<Collider> FoldedColliders;

		[Space(35f)] [SerializeField] private GameObject UnfoldedItem;

		[SerializeField] private List<Collider> UnfoldedColliders;

		public string Look;

		public string ItemName => SubType.ToLocalizedString();

		public string ItemDescription
		{
			get
			{
				string value;
				if (Localization.GenericItemsDescriptions.TryGetValue(SubType, out value))
				{
					return value;
				}

				return string.Empty;
			}
		}

		public float ProporcionalHealth => Health / MaxHealth;

		public override DynamicObjectAuxData GetAuxData()
		{
			GenericItemData baseAuxData = GetBaseAuxData<GenericItemData>();
			baseAuxData.SubType = SubType;
			baseAuxData.Look = Look;
			return baseAuxData;
		}

		private new void Start()
		{
			base.Start();
		}

		public override void ProcesStatsData(DynamicObjectStats dos)
		{
			base.ProcesStatsData(dos);
			GenericItemStats genericItemStats = dos as GenericItemStats;
			Look = genericItemStats.Look;
			if (!(Look != string.Empty))
			{
				return;
			}

			string path = "Materials/GenericItem/" + SubType + "/" + Look;
			Material material = Resources.Load<Material>(path);
			if (material != null && UnfoldedItem != null)
			{
				Renderer[] componentsInChildren = UnfoldedItem.GetComponentsInChildren<Renderer>();
				foreach (Renderer renderer in componentsInChildren)
				{
					renderer.material = material;
				}
			}
		}

		public override void ChangeEquip(EquipType type, Player pl)
		{
			if (!(UnfoldedItem == null) && !(FoldedItem == null) && type == EquipType.Hands)
			{
				FoldedItem.SetActive(true);
				UnfoldedItem.SetActive(false);
			}
		}

		public override void OnAttach(bool isAttached, bool isOnPlayer)
		{
			if (UnfoldedItem == null || FoldedItem == null)
			{
				return;
			}

			bool flag = isAttached || AttachPoint == null || !(AttachPoint is ActiveSceneAttachPoint);
			FoldedItem.SetActive(flag);
			UnfoldedItem.SetActive(!flag);
			foreach (Collider foldedCollider in FoldedColliders)
			{
				foldedCollider.enabled = !isOnPlayer && flag;
			}

			foreach (Collider unfoldedCollider in UnfoldedColliders)
			{
				unfoldedCollider.enabled = !isOnPlayer && !flag;
			}
		}
	}
}
