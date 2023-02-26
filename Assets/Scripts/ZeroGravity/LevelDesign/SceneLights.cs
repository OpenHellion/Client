using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.LevelDesign
{
	public class SceneLights : MonoBehaviour, IVesselSystemAccessory
	{
		[Serializable]
		public class LightItemObject
		{
			public Light Light;

			[HideInInspector]
			public float DefaultIntensity;
		}

		[Serializable]
		public enum EmissionPropertyType
		{
			_None = 0,
			_EmissionAmount = 1,
			_EmissionColor = 2
		}

		[Serializable]
		public class RendererItemObject
		{
			public Renderer Renderer;

			public int MaterialIndex;

			[HideInInspector]
			public Color DefaultEmissionColor;

			[HideInInspector]
			public float DefaultEmissionAmount;

			[HideInInspector]
			public EmissionPropertyType EmissionProperty = EmissionPropertyType._EmissionAmount;
		}

		[Serializable]
		public class SceneLightsItem
		{
			public List<LightItemObject> Lights;

			public List<RendererItemObject> Renderers;

			public void SetMultiplier(float multiplier)
			{
				multiplier = Mathf.Clamp01(multiplier);
				for (int i = 0; i < Lights.Count; i++)
				{
					if (Lights[i] != null && Lights[i].Light != null)
					{
						Lights[i].Light.intensity = Lights[i].DefaultIntensity * multiplier;
					}
				}
				for (int j = 0; j < Renderers.Count; j++)
				{
					if (Renderers[j] != null && Renderers[j].Renderer != null)
					{
						if (Renderers[j].EmissionProperty == EmissionPropertyType._EmissionAmount)
						{
							Renderers[j].Renderer.materials[Renderers[j].MaterialIndex].SetFloat("_EmissionAmount", Renderers[j].DefaultEmissionAmount * multiplier);
						}
						else if (Renderers[j].EmissionProperty == EmissionPropertyType._EmissionColor)
						{
							Renderers[j].Renderer.materials[Renderers[j].MaterialIndex].SetColor("_EmissionColor", Renderers[j].DefaultEmissionColor * multiplier);
						}
					}
				}
			}

			public void ReadDefaultData()
			{
				for (int i = 0; i < Lights.Count; i++)
				{
					if (Lights[i] != null && Lights[i].Light != null)
					{
						Lights[i].DefaultIntensity = Lights[i].Light.intensity;
					}
				}
				for (int j = 0; j < Renderers.Count; j++)
				{
					if (Renderers[j] != null && Renderers[j].Renderer != null)
					{
						if (Renderers[j].Renderer.materials[Renderers[j].MaterialIndex].HasProperty("_EmissionAmount"))
						{
							Renderers[j].DefaultEmissionAmount = Renderers[j].Renderer.materials[Renderers[j].MaterialIndex].GetFloat("_EmissionAmount");
							Renderers[j].EmissionProperty = EmissionPropertyType._EmissionAmount;
						}
						else if (Renderers[j].Renderer.materials[Renderers[j].MaterialIndex].HasProperty("_EmissionColor"))
						{
							Renderers[j].DefaultEmissionColor = Renderers[j].Renderer.materials[Renderers[j].MaterialIndex].GetColor("_EmissionColor");
							Renderers[j].EmissionProperty = EmissionPropertyType._EmissionColor;
						}
					}
				}
			}
		}

		private enum SwitchOrder
		{
			Altogether = 0,
			Forward = 1,
			Backward = 2,
			Random = 3
		}

		[Serializable]
		private class SwitchOnOffData
		{
			public AnimationCurve Curve;

			[Tooltip("Time in seconds")]
			public float Time = 1f;

			[Tooltip("Time in seconds")]
			public float TransitionTime;

			public SwitchOrder Order = SwitchOrder.Forward;

			public int RandomOrderSeed = 1;
		}

		[SerializeField]
		[FormerlySerializedAs("triggerAnimtion")]
		protected SceneTriggerAnimation triggerAnimation;

		public bool FixedLightsIntensity;

		public bool FixedLightsColor;

		[SerializeField]
		public List<SceneLightsItem> lightObjects;

		[SerializeField]
		private SwitchOnOffData switchOnData;

		[SerializeField]
		private SwitchOnOffData switchOffData;

		private List<int> switchOnOrderIndex;

		private List<int> switchOffOrderIndex;

		private bool doAnimationCurve;

		private SwitchOnOffData currentSwitchData;

		private float currentCurveTime;

		private int currentIndex;

		private List<int> currentOrderIndexList;

		[SerializeField]
		private VesselSystem _BaseVesselSystem;

		[HideInInspector]
		public SpaceObjectVessel ParentVessel;

		private bool initialize = true;

		public List<SceneLightController> SceneLightCotrollers;

		public VesselSystem BaseVesselSystem
		{
			get
			{
				return _BaseVesselSystem;
			}
			set
			{
				_BaseVesselSystem = value;
			}
		}

		private void FillSwitchIndexOrder(ref List<int> list, SwitchOrder order, int randomSeed)
		{
			list = new List<int>(new int[lightObjects.Count]);
			switch (order)
			{
			case SwitchOrder.Forward:
			case SwitchOrder.Random:
			{
				for (int i = 0; i < lightObjects.Count; i++)
				{
					list[i] = i;
				}
				break;
			}
			case SwitchOrder.Backward:
			{
				int num = 0;
				for (int num2 = lightObjects.Count - 1; num2 >= 0; num2--)
				{
					list[num++] = num2;
				}
				break;
			}
			}
			if (order == SwitchOrder.Random)
			{
				System.Random random = new System.Random(randomSeed);
				for (int num3 = lightObjects.Count - 1; num3 >= 0; num3--)
				{
					int index = random.Next(0, lightObjects.Count);
					int value = list[index];
					list[index] = list[num3];
					list[num3] = value;
				}
			}
		}

		private void Awake()
		{
			if (ParentVessel == null && Client.IsGameBuild)
			{
				ParentVessel = GetComponentInParent<GeometryRoot>().MainObject as SpaceObjectVessel;
			}
			try
			{
				if (triggerAnimation == null)
				{
					triggerAnimation = GetComponent<SceneTriggerAnimation>();
				}
				if (!(triggerAnimation == null) || lightObjects == null || lightObjects.Count <= 0)
				{
					return;
				}
				foreach (SceneLightsItem lightObject in lightObjects)
				{
					lightObject.ReadDefaultData();
				}
				FillSwitchIndexOrder(ref switchOnOrderIndex, switchOnData.Order, switchOnData.RandomOrderSeed);
				FillSwitchIndexOrder(ref switchOffOrderIndex, switchOffData.Order, switchOffData.RandomOrderSeed);
			}
			catch (Exception ex)
			{
				Dbg.Error(ex.Message, ex.StackTrace);
			}
		}

		private void Start()
		{
			_BaseVesselSystem.Accessories.Add(this);
			BaseVesselSystemUpdated();
		}

		public void BaseVesselSystemUpdated()
		{
			try
			{
				bool flag = initialize;
				initialize = false;
				if (triggerAnimation != null)
				{
					triggerAnimation.ChangeState(BaseVesselSystem.Status == SystemStatus.Online, flag);
				}
				else if (lightObjects.Count > 0)
				{
					SwitchOnOffData switchOnOffData = ((BaseVesselSystem.Status != SystemStatus.Online) ? switchOffData : switchOnData);
					if (switchOnOffData == currentSwitchData)
					{
						return;
					}
					currentSwitchData = switchOnOffData;
					if (flag)
					{
						float multiplier = currentSwitchData.Curve.Evaluate(1f);
						foreach (SceneLightsItem lightObject in lightObjects)
						{
							lightObject.SetMultiplier(multiplier);
						}
					}
					else
					{
						doAnimationCurve = true;
						currentCurveTime = 0f;
						currentIndex = 0;
						currentOrderIndexList = ((BaseVesselSystem.Status != SystemStatus.Online) ? switchOffOrderIndex : switchOnOrderIndex);
						UpdateAnimationCurveData(true);
					}
				}
			}
			catch (Exception ex)
			{
				doAnimationCurve = false;
				Dbg.Error(ex.Message, ex.StackTrace);
			}
			foreach (SceneLightController sceneLightCotroller in SceneLightCotrollers)
			{
				if (sceneLightCotroller != null)
				{
					if (BaseVesselSystem.Status == SystemStatus.Online)
					{
						sceneLightCotroller.SwitchOnOff(true);
					}
					else
					{
						sceneLightCotroller.SwitchOnOff(false);
					}
				}
			}
		}

		public bool IsEventFinished()
		{
			if (triggerAnimation != null)
			{
				return triggerAnimation.IsEventFinished;
			}
			return !doAnimationCurve;
		}

		private void UpdateAnimationCurveData(bool isReset)
		{
			if (!isReset)
			{
				currentCurveTime += 1f / currentSwitchData.Time * Time.deltaTime;
			}
			float multiplier = currentSwitchData.Curve.Evaluate(Mathf.Clamp01(currentCurveTime));
			if (currentSwitchData.Order == SwitchOrder.Altogether)
			{
				foreach (SceneLightsItem lightObject in lightObjects)
				{
					lightObject.SetMultiplier(multiplier);
				}
			}
			else
			{
				lightObjects[currentOrderIndexList[currentIndex]].SetMultiplier(multiplier);
				if (currentSwitchData.TransitionTime > 0.0001f && currentIndex < lightObjects.Count - 1)
				{
					float num = currentSwitchData.TransitionTime / currentSwitchData.Time;
					float num2 = currentCurveTime - (1f - num);
					if (num2 >= 0f)
					{
						int num3 = currentIndex + 1;
						while (true)
						{
							float multiplier2 = currentSwitchData.Curve.Evaluate(num2);
							lightObjects[currentOrderIndexList[num3]].SetMultiplier(multiplier2);
							num2 -= 1f - num;
							if (num3 < lightObjects.Count - 1 && num2 >= 0f)
							{
								num3++;
								continue;
							}
							break;
						}
					}
				}
			}
			if (currentCurveTime >= 1f)
			{
				currentCurveTime = currentSwitchData.TransitionTime / currentSwitchData.Time + currentCurveTime % 1f;
				currentIndex++;
				if (currentSwitchData.Order == SwitchOrder.Altogether || currentIndex == lightObjects.Count)
				{
					doAnimationCurve = false;
				}
			}
		}

		private void Update()
		{
			if (doAnimationCurve)
			{
				try
				{
					UpdateAnimationCurveData(false);
				}
				catch (Exception ex)
				{
					doAnimationCurve = false;
					Dbg.Error(ex.Message, ex.StackTrace);
				}
			}
		}

		private void OnDestroy()
		{
			if (BaseVesselSystem != null)
			{
				BaseVesselSystem.Accessories.Remove(this);
			}
		}
	}
}
