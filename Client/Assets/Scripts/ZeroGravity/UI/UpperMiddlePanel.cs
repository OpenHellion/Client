using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.LevelDesign;

namespace ZeroGravity.UI
{
	public class UpperMiddlePanel : MonoBehaviour
	{
		public SceneDoor InnerDoor;

		public SceneDoor OutterDoor;

		public Image InnerDoorImage;

		public Image OuterDoorImage;

		public Color RedColor;

		public Color OrangeColor;

		private void Start()
		{
		}

		private void Update()
		{
			if (InnerDoor != null)
			{
				if (InnerDoor.IsOpen)
				{
					if (InnerDoorImage.color != RedColor)
					{
						InnerDoorImage.color = RedColor;
					}
				}
				else if (InnerDoorImage.color != OrangeColor)
				{
					InnerDoorImage.color = OrangeColor;
				}
			}

			if (!(OutterDoor != null))
			{
				return;
			}

			if (OutterDoor.IsOpen)
			{
				if (OuterDoorImage.color != RedColor)
				{
					OuterDoorImage.color = RedColor;
				}
			}
			else if (OuterDoorImage.color != OrangeColor)
			{
				OuterDoorImage.color = OrangeColor;
			}
		}
	}
}
