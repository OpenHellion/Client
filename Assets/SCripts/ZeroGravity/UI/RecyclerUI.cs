using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class RecyclerUI : MonoBehaviour
	{
		private SpaceObjectVessel ParentVessel;

		private SceneCargoBay Cargo;

		private float CargoCapacity;

		public SceneItemRecycler Recycler;

		public GameObject NoPower;

		public Transform ResultsTransform;

		public GameObject ResultItem;

		public Text Status;

		public Text ItemName;

		[CompilerGenerated]
		private static Func<ItemSlot, bool> _003C_003Ef__am_0024cache0;

		private void Start()
		{
			Recycler.RecyclerUI = this;
			ParentVessel = GetComponentInParent<GeometryRoot>().MainObject as SpaceObjectVessel;
			Cargo = ParentVessel.CargoBay;
		}

		private void Update()
		{
		}

		public void UpdateUI()
		{
			NoPower.SetActive(Recycler.BaseVesselSystem.Status != SystemStatus.OnLine);
			if (Recycler.Item != null)
			{
				ShowResults();
				return;
			}
			ResultsTransform.DestroyAll<RecycleResultUI>();
			Status.color = Colors.GreenText;
			Status.text = Localization.Ready.ToUpper();
			ItemName.text = Localization.InsertItem;
			ItemName.color = Colors.White;
		}

		public void ShowResults(Item item = null)
		{
			bool flag = false;
			if (item == null)
			{
				item = Recycler.Item;
			}
			if (item != null)
			{
				Dictionary<short, ItemSlot>.ValueCollection values = item.Slots.Values;
				if (_003C_003Ef__am_0024cache0 == null)
				{
					_003C_003Ef__am_0024cache0 = _003CShowResults_003Em__0;
				}
				flag = values.FirstOrDefault(_003C_003Ef__am_0024cache0);
			}
			ResultsTransform.DestroyAll<RecycleResultUI>();
			if (Recycler.AutoRecycle)
			{
				if (!(item != null))
				{
					return;
				}
				Status.text = string.Empty;
				ItemName.text = item.Name;
				Dictionary<ResourceType, float> recycleResources = Item.GetRecycleResources(item);
				if (item is Grenade)
				{
					Status.color = Colors.Red;
					Status.text = Localization.Danger.ToUpper();
				}
				else if (recycleResources != null)
				{
					float num = 0f;
					foreach (ResourceType key in recycleResources.Keys)
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(ResultItem, ResultsTransform);
						gameObject.SetActive(true);
						gameObject.transform.Reset();
						RecycleResultUI component = gameObject.GetComponent<RecycleResultUI>();
						component.Resource = key;
						component.Value.text = FormatHelper.FormatValue(recycleResources[key]);
						num += recycleResources[key];
					}
					if (num > CheckCargo())
					{
						ResultsTransform.gameObject.Activate(false);
						Status.color = Colors.Red;
						Status.text = Localization.CargoFull.ToUpper();
					}
					else if (flag)
					{
						ResultsTransform.gameObject.Activate(false);
						Status.color = Colors.GreenText;
						Status.text = Localization.Recycling.ToUpper();
						ItemName.text = Localization.MultipleItems.ToUpper();
					}
					else
					{
						ResultsTransform.gameObject.Activate(true);
					}
				}
				else
				{
					Status.color = Colors.BlueLight;
					Status.text = Localization.Jettison.ToUpper();
				}
				CancelInvoke("UpdateUI");
				Invoke("UpdateUI", 5f);
			}
			else
			{
				Status.color = Colors.White;
				ItemName.color = Colors.BlueLight;
				ItemName.text = Localization.Researching.ToUpper();
				if (flag)
				{
					Status.text = Localization.MultipleItems.ToUpper();
				}
				else
				{
					Status.text = item.Name;
				}
			}
		}

		public float CheckCargo()
		{
			CargoCapacity = 0f;
			if (Cargo != null)
			{
				foreach (ICargoCompartment compartment in Cargo.Compartments)
				{
					CargoCapacity += compartment.AvailableCapacity;
				}
			}
			return CargoCapacity;
		}

		[CompilerGenerated]
		private static bool _003CShowResults_003Em__0(ItemSlot m)
		{
			return m.Item != null;
		}
	}
}
