using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;

namespace ZeroGravity.Objects
{
	public class HandheldAsteroidScanner : Item
	{
		[Serializable]
		public class HandheldAsteroidScannerUI
		{
			public Canvas Canvas;

			public Text Range;

			public GameObject Active;

			public Transform ResourcesHolder;

			public Text ScanStatus;

			public GameObject Resource;
		}

		[CompilerGenerated]
		private sealed class _003CPrimaryFunction_003Ec__AnonStorey0
		{
			private sealed class _003CPrimaryFunction_003Ec__AnonStorey1
			{
				internal ResourceType k;

				internal _003CPrimaryFunction_003Ec__AnonStorey0 _003C_003Ef__ref_00240;

				internal bool _003C_003Em__0(AsteroidMiningPoint n)
				{
					return n.ResourceType == k;
				}
			}

			internal Asteroid ast;

			private static Func<AsteroidMiningPoint, float> _003C_003Ef__am_0024cache0;

			internal KeyValuePair<ResourceType, float> _003C_003Em__0(ResourceType k)
			{
				_003CPrimaryFunction_003Ec__AnonStorey1 _003CPrimaryFunction_003Ec__AnonStorey = new _003CPrimaryFunction_003Ec__AnonStorey1();
				_003CPrimaryFunction_003Ec__AnonStorey._003C_003Ef__ref_00240 = this;
				_003CPrimaryFunction_003Ec__AnonStorey.k = k;
				ResourceType k2 = _003CPrimaryFunction_003Ec__AnonStorey.k;
				IEnumerable<AsteroidMiningPoint> source = ast.MiningPoints.Values.Where(_003CPrimaryFunction_003Ec__AnonStorey._003C_003Em__0);
				if (_003C_003Ef__am_0024cache0 == null)
				{
					_003C_003Ef__am_0024cache0 = _003C_003Em__1;
				}
				return new KeyValuePair<ResourceType, float>(k2, source.Sum(_003C_003Ef__am_0024cache0));
			}

			private static float _003C_003Em__1(AsteroidMiningPoint m)
			{
				return m.Quantity;
			}
		}

		private float timeToScan;

		private int penetrationLevel;

		private ResourceType lasResScanned;

		private float amountOfRes;

		public int MaxRange;

		private float lastScannedTime;

		private const float rateOfScanning = 0.5f;

		private Transform cameraX;

		public HandheldAsteroidScannerUI UI;

		private bool isScanning;

		[CompilerGenerated]
		private static Func<KeyValuePair<ResourceType, float>, bool> _003C_003Ef__am_0024cache0;

		public float Range
		{
			get
			{
				return (float)MaxRange * base.TierMultiplier;
			}
		}

		private new void Start()
		{
			base.Start();
			UI.Range.text = Localization.ScanningRange.ToUpper() + " - " + Range.ToString("0.0") + " m";
			PrimaryReleased();
		}

		public override void PrimaryReleased()
		{
			isScanning = false;
			UI.ScanStatus.text = Localization.Ready.ToUpper();
			UI.ScanStatus.color = Colors.Green;
			UI.Active.Activate(false);
			UI.ResourcesHolder.DestroyAll<Transform>(true);
		}

		public override bool PrimaryFunction()
		{
			if (Time.time - lastScannedTime > 0.5f)
			{
				lastScannedTime = Time.time;
				RaycastHit hitInfo;
				if (Physics.Raycast(cameraX.position, cameraX.forward, out hitInfo, Range))
				{
					GeometryRoot componentInParent = hitInfo.collider.gameObject.GetComponentInParent<GeometryRoot>();
					if (componentInParent != null && componentInParent.MainObject is Asteroid)
					{
						_003CPrimaryFunction_003Ec__AnonStorey0 _003CPrimaryFunction_003Ec__AnonStorey = new _003CPrimaryFunction_003Ec__AnonStorey0();
						_003CPrimaryFunction_003Ec__AnonStorey.ast = componentInParent.MainObject as Asteroid;
						if (_003CPrimaryFunction_003Ec__AnonStorey.ast != null && !isScanning)
						{
							UI.ScanStatus.text = Localization.Scanning.ToUpper();
							UI.ScanStatus.color = Colors.Red;
							isScanning = true;
							IEnumerable<KeyValuePair<ResourceType, float>> source = Enum.GetValues(typeof(ResourceType)).Cast<ResourceType>().Select(_003CPrimaryFunction_003Ec__AnonStorey._003C_003Em__0);
							if (_003C_003Ef__am_0024cache0 == null)
							{
								_003C_003Ef__am_0024cache0 = _003CPrimaryFunction_003Em__0;
							}
							List<KeyValuePair<ResourceType, float>> list = source.Where(_003C_003Ef__am_0024cache0).ToList();
							foreach (KeyValuePair<ResourceType, float> item in list)
							{
								InstantiateResources(item.Key, item.Value);
							}
							UI.Active.Activate(true);
						}
					}
					else
					{
						UI.ScanStatus.text = Localization.NotScannable.ToUpper();
						UI.ScanStatus.color = Colors.Gray;
						UI.Active.Activate(false);
					}
				}
				else
				{
					UI.ScanStatus.text = Localization.OutOfRange.ToUpper();
					UI.ScanStatus.color = Colors.Red;
					UI.Active.Activate(false);
				}
			}
			return false;
		}

		public override DynamicObjectAuxData GetAuxData()
		{
			HandheldAsteroidScannerData baseAuxData = GetBaseAuxData<HandheldAsteroidScannerData>();
			baseAuxData.timeToScan = timeToScan;
			baseAuxData.penetrationLevel = penetrationLevel;
			return baseAuxData;
		}

		public override void ChangeEquip(EquipType type, Player pl)
		{
			if (type == EquipTo && pl is MyPlayer)
			{
				cameraX = (pl as MyPlayer).FpsController.CameraController.MouseLookXTransform;
				UI.Canvas.GetComponent<CanvasGroup>().alpha = 1f;
			}
			else
			{
				UI.Canvas.GetComponent<CanvasGroup>().alpha = 0f;
			}
		}

		public void InstantiateResources(ResourceType res, float quantity)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(UI.Resource, UI.ResourcesHolder);
			gameObject.GetComponentInChildren<Image>().sprite = Client.Instance.SpriteManager.GetSprite(res);
			if (base.Tier == 2)
			{
				if (quantity < 500f)
				{
					gameObject.GetComponentInChildren<Text>().text = Localization.Low.ToUpper();
					gameObject.GetComponentInChildren<Text>().color = Colors.GrayText;
				}
				else if (quantity > 500f && quantity < 1500f)
				{
					gameObject.GetComponentInChildren<Text>().text = Localization.Medium.ToUpper();
					gameObject.GetComponentInChildren<Text>().color = Colors.Yellow;
				}
				else
				{
					gameObject.GetComponentInChildren<Text>().text = Localization.High.ToUpper();
					gameObject.GetComponentInChildren<Text>().color = Colors.GreenText;
				}
			}
			else if (base.Tier == 3)
			{
				gameObject.GetComponentInChildren<Text>().text = quantity.ToString();
				gameObject.GetComponentInChildren<Text>().color = Colors.White;
			}
			else
			{
				gameObject.GetComponentInChildren<Text>().text = Localization.Unknown.ToUpper();
				gameObject.GetComponentInChildren<Text>().color = Colors.FormatedRed;
			}
			gameObject.Activate(true);
		}

		[CompilerGenerated]
		private static bool _003CPrimaryFunction_003Em__0(KeyValuePair<ResourceType, float> a)
		{
			return a.Value > 0f;
		}
	}
}
