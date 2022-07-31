using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.UI
{
	public class LSResourceTank : MonoBehaviour
	{
		[HideInInspector]
		public ResourceContainer Container;

		public Text Name;

		public Text Value;

		public Image Filler;
	}
}
