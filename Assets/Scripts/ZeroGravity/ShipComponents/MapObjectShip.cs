using System;
using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Math;
using ZeroGravity.Objects;

namespace ZeroGravity.ShipComponents
{
	public class MapObjectShip : MapObjectVessel
	{
		public GameObject ScanningCone;

		public float ReferentRadarSignature = 50f;

		public GameObject ScanningEffectCone;

		public float ScanningEffectConeScaleMultiplier;

		public GameObject Pitch;

		public GameObject Yaw;

		public float PitchYawScale;

		public MeshRenderer IconRenderer;

		private bool scanningConeActive;

		[NonSerialized]
		public float ScanningConeAngle = 45f;

		public override string Name
		{
			get
			{
				return (MainObject as Ship).CommandVesselName;
			}
		}

		public override Sprite Icon
		{
			get
			{
				if (base.Map != null && this == base.Map.MyShip)
				{
					return Client.Instance.SpriteManager.GetSprite(MainObject as SpaceObjectVessel);
				}
				return Client.Instance.SpriteManager.GetSprite(MainObject as SpaceObjectVessel, true);
			}
			set
			{
			}
		}

		public override string Description
		{
			get
			{
				return (MainObject as SpaceObjectVessel).GetDescription();
			}
			set
			{
			}
		}

		public override OrbitParameters Orbit
		{
			get
			{
				if ((MainObject as SpaceObjectVessel).LastKnownMapOrbit != null)
				{
					return (MainObject as SpaceObjectVessel).LastKnownMapOrbit;
				}
				return base.Orbit;
			}
		}

		public override void CreateVisual()
		{
			if (MainObject != null)
			{
				base.gameObject.SetLayerRecursively("Map");
				SetOrbit();
				UpdateOrbitPlane();
				SetIcon();
				if ((MainObject as Ship).RadarSystem == null)
				{
					UnityEngine.Object.Destroy(ScanningCone);
					UnityEngine.Object.Destroy(ScanningEffectCone);
					UnityEngine.Object.Destroy(Yaw);
				}
			}
			int renderQueue = MathHelper.RandomRange(0, 2000);
			if ((((object)IconRenderer != null) ? IconRenderer.material : null) != null)
			{
				IconRenderer.material.renderQueue = renderQueue;
			}
			if ((((object)ObjectVisibilityBackground != null) ? ObjectVisibilityBackground.material : null) != null)
			{
				ObjectVisibilityBackground.material.renderQueue = renderQueue;
			}
		}

		public override void UpdateObject()
		{
			Position.position = base.ObjectPosition;
			Orbits.localScale = Vector3.one * (float)base.ObjectScale;
			DistressVisual.SetActive(base.RadarVisibilityType == RadarVisibilityType.Distress);
			if (base.Map == null || base.Map.SelectedObject != this)
			{
				ToggleCone(false);
			}
			if (!(ScanningCone != null))
			{
				return;
			}
			ScanningCone.Activate(scanningConeActive && !ScanningEffectCone.activeSelf);
			if (ScanningCone.activeSelf && (MainObject as Ship).RadarSystem != null)
			{
				if (ScanningConeAngle < 1f)
				{
					ScanningConeAngle = 1f;
				}
				double num = (double)(MainObject as Ship).RadarSystem.SignalAmplification * (MainObject as Ship).RadarSystem.GetCelestialSensivityModifier();
				if (MyPlayer.Instance.InDebrisField != null)
				{
					num *= (double)MyPlayer.Instance.InDebrisField.ScanningSensitivityMultiplier;
				}
				float num2 = (float)((MainObject as Ship).RadarSystem.ActiveScanFuzzySensitivity * 1000.0 / (double)ScanningConeAngle * (double)ReferentRadarSignature * num);
				float num3 = (float)(2.0 * System.Math.Tan((double)ScanningConeAngle * (System.Math.PI / 180.0) / 2.0));
				ScanningCone.transform.localScale = new Vector3(num3 * num2, num3 * num2, num2) * (float)base.ObjectScale;
				ScanningEffectCone.transform.localScale = ScanningCone.transform.localScale * ScanningEffectConeScaleMultiplier;
				Yaw.transform.localScale = new Vector3(PitchYawScale, PitchYawScale, PitchYawScale) * (float)base.ObjectScale;
			}
		}

		public override void SetOrbit()
		{
			if (Orbit == null)
			{
				return;
			}
			List<Vector3D> list = null;
			bool parentChanged = false;
			list = Orbit.GetFlightPathPositions(NumberOfOrbitPositions, 60.0, out parentChanged);
			MyOrbitRenderer.positionCount = list.Count;
			if (list != null && list.Count > 0)
			{
				for (int i = 0; i < list.Count; i++)
				{
					MyOrbitRenderer.SetPosition(i, list[i].ToVector3());
				}
			}
			MyOrbitRenderer.transform.SetParent(Orbits);
			MyOrbitRenderer.startColor = OrbitColor;
			MyOrbitRenderer.endColor = OrbitColor;
			bool loop = false;
			if (Orbit.Eccentricity < 1.0 && Orbit.Eccentricity > 0.0 && !parentChanged)
			{
				loop = true;
			}
			MyOrbitRenderer.loop = loop;
			if (base.Map != null && base.Map.MyShip == this)
			{
				if (Orbit.PeriapsisDistance < Orbit.Parent.Radius)
				{
					base.Map.Panel.ShowWarning(Localization.GetLocalizedField("UnstableOrbit").ToUpper());
				}
				else
				{
					base.Map.Panel.HideWarning();
				}
			}
		}

		public void SetIcon()
		{
			if (Icon != null)
			{
				IconRenderer.material.SetTexture("_MainTex", Icon.texture);
				float x = Icon.rect.width / (float)Icon.texture.width;
				float y = Icon.rect.height / (float)Icon.texture.height;
				float x2 = 1f - (Icon.rect.x + Icon.rect.width) / (float)Icon.texture.width;
				float y2 = 1f - (Icon.rect.y + Icon.rect.height) / (float)Icon.texture.height;
				IconRenderer.material.SetTextureScale("_MainTex", new Vector2(x, y));
				IconRenderer.material.SetTextureOffset("_MainTex", new Vector2(x2, y2));
			}
		}

		public void ToggleCone(bool val)
		{
			if (ScanningCone != null)
			{
				scanningConeActive = val;
				Pitch.Activate(val);
				Yaw.Activate(val);
			}
		}
	}
}
