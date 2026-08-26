using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class EnginePanel : MonoBehaviour
	{
		public Image EngineThrustFiller;

		public Text EngineThrustVal;

		public Text EngineAcceleration;

		public Image EngineStatus;

		public Sprite EngineOn;

		public Sprite EngineOff;

		public Image EngineReverse;

		public Color OrangeColor;

		public Color BlueColor;

		public Color RedColor;

		[HideInInspector] public Ship ParentShip;

		private void Start()
		{
			if (ParentShip == null)
			{
				ParentShip = GetComponentInParent<GeometryRoot>().MainObject as Ship;
			}
		}

		private void Update()
		{
			EngineThrustFiller.fillAmount = Mathf.Abs(ParentShip.EngineThrustPercentage);
			EngineThrustVal.text = (ParentShip.EngineThrustPercentage * 100f).ToString("f0");
			if (ParentShip.EngineThrustPercentage >= 0f)
			{
				if (EngineReverse.gameObject.activeInHierarchy)
				{
					EngineReverse.gameObject.SetActive(false);
				}
			}
			else if (!EngineReverse.gameObject.activeInHierarchy)
			{
				EngineReverse.gameObject.SetActive(true);
			}

			if (ParentShip.EngineOnLine)
			{
				if (EngineStatus.sprite != EngineOn)
				{
					EngineStatus.sprite = EngineOn;
				}

				if (ParentShip.EngineThrustPercentage > 0f)
				{
					if (EngineThrustFiller.color != BlueColor)
					{
						EngineThrustFiller.color = BlueColor;
					}

					EngineAcceleration.text = Mathf.Abs(ParentShip.EngineThrustPercentage * 10f).ToString("f1");
				}
				else if (ParentShip.EngineThrustPercentage == 0f)
				{
					if (EngineThrustFiller.color != OrangeColor)
					{
						EngineThrustFiller.color = OrangeColor;
					}

					EngineAcceleration.text = "0.0";
				}
				else
				{
					if (EngineThrustFiller.color != RedColor)
					{
						EngineThrustFiller.color = RedColor;
					}

					EngineAcceleration.text =
						Mathf.Abs(ParentShip.EngineThrustPercentage * 10f * ((!(ParentShip.Engine != null))
							? 0f
							: (ParentShip.Engine.ReverseAcceleration / ParentShip.Engine.Acceleration))).ToString("f1");
				}
			}
			else
			{
				if (EngineStatus.sprite != EngineOff)
				{
					EngineStatus.sprite = EngineOff;
				}

				if (EngineThrustFiller.color != OrangeColor)
				{
					EngineThrustFiller.color = OrangeColor;
				}

				EngineAcceleration.text = "0.0";
			}
		}
	}
}
