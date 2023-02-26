using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity
{
	public static class Colors
	{
		public static Color PowerRed = hexToColor("#EA4141");

		public static Color Green = hexToColor("#2DAE15");

		public static Color Red = hexToColor("#AE1515");

		public static Color Standby = hexToColor("#FFF94E");

		public static Color Defective = hexToColor("#E41414");

		public static Color GrayDefault = hexToColor("#777777");

		public static Dictionary<SystemStatus, Color> Status = new Dictionary<SystemStatus, Color>
		{
			{
				SystemStatus.Offline,
				hexToColor("#AE1515")
			},
			{
				SystemStatus.Online,
				hexToColor("#2DAE15")
			},
			{
				SystemStatus.Cooldown,
				hexToColor("#146EE2")
			},
			{
				SystemStatus.Powerup,
				hexToColor("#00F0EA")
			},
			{
				SystemStatus.None,
				new Color(0f, 0f, 0f)
			}
		};

		public static Dictionary<SystemStatus, Color> ReversedStatus = new Dictionary<SystemStatus, Color>
		{
			{
				SystemStatus.Offline,
				hexToColor("#2DAE15")
			},
			{
				SystemStatus.Online,
				hexToColor("#AE1515")
			},
			{
				SystemStatus.Cooldown,
				hexToColor("#146EE2")
			},
			{
				SystemStatus.Powerup,
				hexToColor("#00F0EA")
			},
			{
				SystemStatus.None,
				new Color(0f, 0f, 0f)
			}
		};

		public static Dictionary<RadarVisibilityType, Color> RadarVisibility = new Dictionary<RadarVisibilityType, Color>
		{
			{
				RadarVisibilityType.Visible,
				hexToColor("#1d8e4c")
			},
			{
				RadarVisibilityType.Unknown,
				hexToColor("#c06228")
			},
			{
				RadarVisibilityType.AlwaysVisible,
				hexToColor("#288fc0")
			},
			{
				RadarVisibilityType.Doomed,
				hexToColor("#EA4141")
			},
			{
				RadarVisibilityType.Warp,
				hexToColor("#49F2F0")
			},
			{
				RadarVisibilityType.Distress,
				hexToColor("#aa2020")
			},
			{
				RadarVisibilityType.Invisible,
				hexToColor("#ffffff")
			}
		};

		public static Dictionary<int, Color> Tier = new Dictionary<int, Color>
		{
			{
				1,
				new Color(1f, 1f, 1f, 1f)
			},
			{
				2,
				new Color(10f / 51f, 2f / 3f, 46f / 51f, 1f)
			},
			{
				3,
				new Color(14f / 51f, 1f, 0.47058824f, 1f)
			},
			{
				4,
				new Color(1f, 50f / 51f, 16f / 51f, 1f)
			}
		};

		public static Dictionary<InventorySlot.Group, Color> ItemSlot = new Dictionary<InventorySlot.Group, Color>
		{
			{
				InventorySlot.Group.Ammo,
				hexToColor("#702C2C")
			},
			{
				InventorySlot.Group.Consumable,
				hexToColor("#31623E")
			},
			{
				InventorySlot.Group.Tool,
				hexToColor("#875C35")
			},
			{
				InventorySlot.Group.Utility,
				hexToColor("#643587")
			}
		};

		public static Dictionary<int, Color> TypeOfResource = new Dictionary<int, Color>
		{
			{
				1,
				hexToColor("#804935")
			},
			{
				2,
				hexToColor("#3FDEE8")
			},
			{
				3,
				hexToColor("#2DAE15")
			}
		};

		public static Dictionary<Type, Color> AttachPointSlotColor = new Dictionary<Type, Color>
		{
			{
				typeof(ActiveSceneAttachPoint),
				hexToColor("#383838")
			},
			{
				typeof(SceneAttachPoint),
				hexToColor("#383838")
			},
			{
				typeof(SceneMachineryPartSlot),
				hexToColor("#643587")
			},
			{
				typeof(SceneItemRecycler),
				hexToColor("#E41414")
			},
			{
				typeof(SceneResourcesAutoTransferPoint),
				hexToColor("#875C35")
			},
			{
				typeof(SceneResourcesTransferPoint),
				hexToColor("#875C35")
			},
			{
				typeof(SceneTriggerBatteryRecharge),
				hexToColor("#878E43")
			}
		};

		public static Color GrayText = new Color(69f / 85f, 69f / 85f, 69f / 85f, 1f);

		public static Color RedText = new Color(0.8156863f, 0.11372549f, 0.11372549f, 1f);

		public static Color GreenText = new Color(0.14901961f, 40f / 51f, 0.14901961f, 1f);

		public static Color Transparent = new Color(1f, 1f, 1f, 0f);

		public static Color White = new Color(1f, 1f, 1f, 1f);

		public static Color WhiteHalfTransparent = new Color(1f, 1f, 1f, 0.5f);

		public static Color Black = new Color(0f, 0f, 0f, 1f);

		public static Color Blue = new Color(0f, 0f, 1f, 1f);

		public static Color BlueDark = new Color(0f, 0f, 0.8f, 1f);

		public static Color BlueLight = new Color(0.14901961f, 36f / 85f, 0.56078434f, 1f);

		public static Color BlueScan = new Color(1f / 17f, 0.30980393f, 37f / 85f, 1f);

		public static Color Yellow = new Color(1f, 1f, 0f, 1f);

		public static Color YellowDark = new Color(0.8f, 0.8f, 0f, 1f);

		public static Color Gray = new Color(0.21960784f, 0.21960784f, 0.21960784f, 1f);

		public static Color Orange = new Color(0.9254902f, 1f / 3f, 0f, 1f);

		public static Color BlueHighlight = new Color(23f / 51f, 0.5137255f, 0.6431373f, 1f);

		public static Color BlueHighlightTransparent = new Color(23f / 51f, 0.5137255f, 0.6431373f, 0.11764706f);

		public static Color OxyJetpackFillers = new Color(0.78039217f, 0.85490197f, 0.56078434f, 1f);

		public static Color FuelJetpackFillers = new Color(0.08627451f, 32f / 51f, 0.69803923f, 1f);

		public static Color AltcorpJetpackDanger = new Color(47f / 85f, 0.1764706f, 1f / 15f, 1f);

		public static Color DistressRed = new Color(0.45490196f, 0.050980393f, 0.050980393f, 1f);

		public static Color AlwaysVisibleBlue = new Color(2f / 85f, 0.8745098f, 0.8745098f, 1f);

		public static Color BlueNavColor = new Color(1f / 17f, 0.30980393f, 37f / 85f, 1f);

		public static Color GreenNavColor = new Color(0f, 0.3372549f, 0.101960786f, 1f);

		public static Color Cyan = hexToColor("#00FFFF");

		public static Color Tier01 = new Color(1f, 1f, 1f, 1f);

		public static Color Tier02 = new Color(10f / 51f, 2f / 3f, 46f / 51f, 1f);

		public static Color Tier03 = new Color(14f / 51f, 1f, 0.47058824f, 1f);

		public static Color Tier04 = new Color(1f, 50f / 51f, 16f / 51f, 1f);

		public static Color Red50 = new Color(58f / 85f, 7f / 85f, 7f / 85f, 0.5f);

		public static Color Blue50 = new Color(0.14901961f, 36f / 85f, 0.56078434f, 0.5f);

		public static Color SlotGray = hexToColor("#BDBDBD");

		public static Color FormatedRed = hexToColor("#EA4141");

		public static Color RadarTarget = hexToColor("#22D79F");

		public static Color GreenMenu = hexToColor("#B6FDEE");

		public static Color DisabledSlot = hexToColor("#E01C1C");

		public static Color ChildSlotAvailable = hexToColor("#E0E01C");

		public static Color HealthHigh = hexToColor("#98c65c");

		public static Color HealthMid = hexToColor("#d28c25");

		public static Color HealthLow = hexToColor("#d22525");

		public static Color ArmorActive = hexToColor("#22D79F");

		public static Color NoArmor = hexToColor("#FF9C5C");

		public static Color BlueprintIcon = hexToColor("#8C8C8C");

		public static Dictionary<CanvasUI.NotificationType, Color> Notification = new Dictionary<CanvasUI.NotificationType, Color>
		{
			{
				CanvasUI.NotificationType.General,
				new Color(0.7647059f, 0.7647059f, 0.7647059f, 10f / 51f)
			},
			{
				CanvasUI.NotificationType.Alert,
				new Color(0.7058824f, 10f / 51f, 10f / 51f, 10f / 51f)
			},
			{
				CanvasUI.NotificationType.Quest,
				new Color(0.28627452f, 0.9490196f, 0.9411765f, 10f / 51f)
			}
		};

		public static Color hexToColor(string hex)
		{
			hex = hex.Replace("#", string.Empty);
			byte a = byte.MaxValue;
			byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
			byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
			byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
			if (hex.Length == 8)
			{
				a = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
			}
			return new Color32(r, g, b, a);
		}
	}
}
