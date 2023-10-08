using UnityEngine;
using ZeroGravity.Data;

namespace ZeroGravity.UI
{
	[CreateAssetMenu(fileName = "GlossaryElementResource", menuName = "Glossary/Glossary Element Resource")]
	public class GlossaryElementResource : AbstractGlossaryElement
	{
		public ResourceType ResourceType;

		public override GlossaryCategory Category
		{
			get { return GlossaryCategory.Resources; }
		}

		public override Sprite Icon
		{
			get { return SpriteManager.Instance.GetSprite(ResourceType); }
		}
	}
}
