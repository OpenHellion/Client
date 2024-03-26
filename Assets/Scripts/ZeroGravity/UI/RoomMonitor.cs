using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.LevelDesign;

namespace ZeroGravity.UI
{
	public class RoomMonitor : MonoBehaviour
	{
		public SceneTriggerRoom roomTrigger;

		public Image GravityImage;

		public Image GravityBorder;

		public Sprite Gravity0sprite;

		public Sprite Gravity0borderSprite;

		public Sprite Gravity1borderSprite;

		public Sprite Gravity1sprite1;

		public Sprite Gravity1sprite2;

		public Sprite Gravity1sprite3;

		public Text TemperatureText;

		public Text PressureText;

		public Image TemperatureAlarm;

		public Image PreassureAlarm;

		public Image PreassureFiller;

		public Image AirQualityFiller;

		public Text AirQualityText;

		public Image WarrningImage;

		private int gravityLerp = -1;

		private float korak = 0.02f;

		private Color redColor = new Color(0.545f, 0.003f, 0.047f);

		private Color orangeColor = new Color(0.8588f, 0.4274f, 0f);

		private float gravitySpriteCouter;

		private float gravitySpriteThreshold = 1f;

		private void Update()
		{
			if (roomTrigger.GravityForce.y.IsNotEpsilonZero())
			{
				if (GravityBorder.sprite != Gravity1borderSprite)
				{
					GravityBorder.sprite = Gravity1borderSprite;
					GravityImage.sprite = Gravity1sprite1;
					gravitySpriteCouter = Time.time;
				}

				if (GravityImage.color.a != 1f)
				{
					Color color = GravityImage.color;
					color.a = 1f;
					GravityImage.color = color;
				}

				if (Time.time - gravitySpriteCouter > gravitySpriteThreshold)
				{
					if (GravityImage.sprite == Gravity1sprite1)
					{
						GravityImage.sprite = Gravity1sprite2;
						gravitySpriteCouter = Time.time;
					}
					else if (GravityImage.sprite == Gravity1sprite2)
					{
						GravityImage.sprite = Gravity1sprite3;
						gravitySpriteCouter = Time.time;
					}
					else
					{
						GravityImage.sprite = Gravity1sprite1;
						gravitySpriteCouter = Time.time;
					}
				}
			}
			else
			{
				if (GravityImage.sprite != Gravity0sprite)
				{
					GravityImage.sprite = Gravity0sprite;
					GravityBorder.sprite = Gravity0borderSprite;
				}

				Color color2 = GravityImage.color;
				if (gravityLerp > 0)
				{
					color2.a = GravityImage.color.a + gravityLerp * korak;
					if (color2.a > 1f)
					{
						gravityLerp = -1;
					}
				}
				else
				{
					color2.a = GravityImage.color.a + gravityLerp * korak;
					if (color2.a < 0f)
					{
						gravityLerp = 1;
					}
				}

				GravityImage.color = color2;
			}

			PreassureFiller.fillAmount = roomTrigger.AirPressure;
			PressureText.text = roomTrigger.AirPressure.ToString("F1");
			if (roomTrigger.AirPressure > 0.3f && roomTrigger.AirPressure < 1f)
			{
				if (PreassureFiller.color != orangeColor)
				{
					PreassureFiller.color = orangeColor;
				}
			}
			else if (roomTrigger.AirPressure < 0.3f && PreassureFiller.color != redColor)
			{
				PreassureFiller.color = redColor;
			}

			AirQualityFiller.fillAmount = roomTrigger.AirQuality;
			AirQualityText.text = (roomTrigger.AirQuality * 100f).ToString("f0");
			if (roomTrigger.AirPressure < -0.67 * roomTrigger.AirQuality + 1.0)
			{
				if (!WarrningImage.gameObject.activeInHierarchy)
				{
					WarrningImage.gameObject.SetActive(true);
				}
			}
			else if (WarrningImage.gameObject.activeInHierarchy)
			{
				WarrningImage.gameObject.SetActive(false);
			}
		}
	}
}
