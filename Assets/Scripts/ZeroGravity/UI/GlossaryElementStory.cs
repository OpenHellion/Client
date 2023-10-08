using UnityEngine;

namespace ZeroGravity.UI
{
	[CreateAssetMenu(fileName = "GlossaryElementStory", menuName = "Glossary/Glossary Element Story")]
	public class GlossaryElementStory : AbstractGlossaryElement
	{
		[SerializeField] private Sprite _Icon;

		public override GlossaryCategory Category
		{
			get { return GlossaryCategory.Story; }
		}

		public override Sprite Icon
		{
			get { return _Icon; }
		}
	}
}
