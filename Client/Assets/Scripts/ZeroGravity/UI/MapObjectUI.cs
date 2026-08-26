using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.UI
{
	public class MapObjectUI : MonoBehaviour
	{
		public MapObject MapObj;

		public Text Name;

		public Image Icon;

		private void Start()
		{
			UpdateUI();
		}

		public void UpdateUI()
		{
			if (MapObj != null)
			{
				Name.text = MapObj.Name;
				Icon.sprite = MapObj.Icon;
			}
		}

		public void SelectObject()
		{
			MapObj.Map.SelectMapObject(MapObj);
		}

		public void FocusObject()
		{
			MapObj.Map.FocusToObject(MapObj);
		}
	}
}
