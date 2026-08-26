using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;

namespace ZeroGravity.UI
{
	public class RecycleResultUI : MonoBehaviour
	{
		[HideInInspector] public ResourceType Resource;

		public Text Name;

		public Text Value;

		public Image Icon;

		private void Start()
		{
			Name.text = Resource.ToLocalizedString();
			Icon.sprite = SpriteManager.Instance.GetSprite(Resource);
		}
	}
}
