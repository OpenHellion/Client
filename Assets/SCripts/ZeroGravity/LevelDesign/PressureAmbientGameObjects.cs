using System;
using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	[Serializable]
	public class PressureAmbientGameObjects
	{
		public GameObject PressureUnder0_4bar;

		public GameObject PressureAbove0_4barAndAirBreathable;

		public GameObject PressureAbove0_4barAndAirNotBreathable;

		public GameObject PressureAbove0_4barAndAirCritical;

		public List<SceneLightController> SceneLightControllers;

		private HxVolumetricCamera hvc;

		public void Initialize()
		{
			if (MyPlayer.Instance != null)
			{
				hvc = MyPlayer.Instance.FpsController.MainCamera.GetComponent<HxVolumetricCamera>();
			}
		}

		public void CheckPressureForAmbient(float pressure, bool isAirOk, bool isAirCrit)
		{
			if (hvc != null)
			{
				hvc.Density = pressure;
			}
			if (PressureUnder0_4bar != null)
			{
				PressureUnder0_4bar.SetActive(pressure < 0.4f);
			}
			if (PressureAbove0_4barAndAirCritical != null)
			{
				PressureAbove0_4barAndAirCritical.SetActive(pressure >= 0.4f && isAirCrit);
			}
			if (PressureAbove0_4barAndAirBreathable != null)
			{
				PressureAbove0_4barAndAirBreathable.SetActive(pressure >= 0.4f && !isAirCrit && isAirOk);
			}
			if (PressureAbove0_4barAndAirNotBreathable != null)
			{
				PressureAbove0_4barAndAirNotBreathable.SetActive(pressure >= 0.4f && !isAirCrit && !isAirOk);
			}
			foreach (SceneLightController sceneLightController in SceneLightControllers)
			{
				if (!(sceneLightController == null))
				{
					if (pressure < 0.4f)
					{
						sceneLightController.SwitchStateTo(SceneLightController.LightState.LowPressure);
					}
					if (pressure >= 0.4f && isAirOk)
					{
						sceneLightController.SwitchStateTo(SceneLightController.LightState.Normal);
					}
					if (pressure >= 0.4f && !isAirOk)
					{
						sceneLightController.SwitchStateTo(SceneLightController.LightState.ToxicAtmosphere);
					}
				}
			}
		}
	}
}
