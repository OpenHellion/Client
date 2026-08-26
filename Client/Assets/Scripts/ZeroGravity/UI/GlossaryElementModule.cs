using UnityEngine;
using ZeroGravity.Data;

namespace ZeroGravity.UI
{
	[CreateAssetMenu(fileName = "GlossaryElementModule", menuName = "Glossary/Glossary Element Module")]
	public class GlossaryElementModule : AbstractGlossaryElement
	{
		public GameScenes.SceneId Module;

		public override GlossaryCategory Category
		{
			get { return GlossaryCategory.Modules; }
		}

		public override Sprite Icon
		{
			get { return SpriteManager.Instance.GetSprite(Module); }
		}
	}
}
