using System;
using UnityEngine;
using ZeroGravity.Math;
using ZeroGravity.UI;

namespace ZeroGravity.ShipComponents
{
	public class MapObjectFixedPosition : MapObject
	{
		private enum FadeMaterialFieldType
		{
			Float01 = 0,
			Color = 1
		}

		public GameObject FixedPositionObjectVisual;

		public double MinScale = 10000000.0;

		public double MaxScale = 1000000000.0;

		public float TimeToLive = 120f;

		public bool FadeOut = true;

		public string FadeMaterialField;

		[SerializeField] private FadeMaterialFieldType FadeFieldType;

		public bool RandomOffsetPosition;

		[NonSerialized] public Vector3D FixedPosition;

		[NonSerialized] public double MinMaxScale;

		[SerializeField] private Sprite _Icon;

		private Material fixedPositionObjectVisualMaterial;

		private object fadeIncrement;

		public override string Name { get; set; }

		public override string Description { get; set; }

		public override Sprite Icon
		{
			get { return (!(_Icon != null)) ? SpriteManager.Instance.DefaultRadarObject : _Icon; }
			set { }
		}

		protected double ObjectVisualScale
		{
			get { return MinScale + (MaxScale - MinScale) * MinMaxScale; }
		}

		public override Vector3D TruePosition
		{
			get { return FixedPosition; }
		}

		public override void CreateVisual()
		{
			Renderer component = FixedPositionObjectVisual.GetComponent<Renderer>();
			if (FadeOut && component != null)
			{
				fixedPositionObjectVisualMaterial = UnityEngine.Object.Instantiate(component.sharedMaterial);
				component.material = fixedPositionObjectVisualMaterial;
				if (FadeFieldType == FadeMaterialFieldType.Float01)
				{
					float @float = fixedPositionObjectVisualMaterial.GetFloat(FadeMaterialField);
					fadeIncrement = @float / TimeToLive;
				}

				if (FadeFieldType == FadeMaterialFieldType.Color)
				{
					Color color = fixedPositionObjectVisualMaterial.GetColor(FadeMaterialField);
					fadeIncrement = color / TimeToLive;
				}
			}

			base.gameObject.SetLayerRecursively("Map");
			if (RandomOffsetPosition)
			{
				FixedPosition +=
					new Vector3D(MathHelper.RandomRange(-1, 1), MathHelper.RandomRange(-1, 1),
						MathHelper.RandomRange(-1, 1)).Normalized *
					(float)MathHelper.RandomRange(ObjectVisualScale / 10.0, ObjectVisualScale) / 2.0;
			}

			if (TimeToLive > 0f)
			{
				UnityEngine.Object.Destroy(base.gameObject, TimeToLive);
			}

			UpdateObject();
		}

		public override void UpdateObject()
		{
			Position.position = base.ObjectPosition;
			FixedPositionObjectVisual.transform.localScale =
				Vector3.one * (float)(ObjectVisualScale * base.ObjectScale);
		}

		public override void UpdateVisibility()
		{
		}

		public override void SetOrbit()
		{
		}

		protected override void Update()
		{
			base.Update();
			if (fixedPositionObjectVisualMaterial != null)
			{
				if (FadeFieldType == FadeMaterialFieldType.Float01)
				{
					float @float = fixedPositionObjectVisualMaterial.GetFloat(FadeMaterialField);
					fixedPositionObjectVisualMaterial.SetFloat(FadeMaterialField,
						Mathf.Clamp01(@float - (float)fadeIncrement * Time.deltaTime));
				}

				if (FadeFieldType == FadeMaterialFieldType.Color)
				{
					Color color = fixedPositionObjectVisualMaterial.GetColor(FadeMaterialField);
					fixedPositionObjectVisualMaterial.SetColor(FadeMaterialField,
						color - (Color)fadeIncrement * Time.deltaTime);
				}
			}
		}
	}
}
