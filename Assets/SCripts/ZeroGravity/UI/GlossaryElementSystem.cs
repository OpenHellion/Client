using UnityEngine;

namespace ZeroGravity.UI
{
	[CreateAssetMenu(fileName = "GlossaryElementSystem", menuName = "Glossary/Glossary Element System")]
	public class GlossaryElementSystem : AbstractGlossaryElement
	{
		[SerializeField]
		private Sprite _Icon;

		public override GlossaryCategory Category
		{
			get
			{
				return GlossaryCategory.Systems;
			}
		}

		public override Sprite Icon
		{
			get
			{
				return _Icon;
			}
		}
	}
}
