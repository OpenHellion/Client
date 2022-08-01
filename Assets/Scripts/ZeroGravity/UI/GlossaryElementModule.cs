using UnityEngine;
using ZeroGravity.Data;

namespace ZeroGravity.UI
{
	[CreateAssetMenu(fileName = "GlossaryElementModule", menuName = "Glossary/Glossary Element Module")]
	public class GlossaryElementModule : AbstractGlossaryElement
	{
		public GameScenes.SceneID Module;

		public override GlossaryCategory Category
		{
			get
			{
				return GlossaryCategory.Modules;
			}
		}

		public override Sprite Icon
		{
			get
			{
				return Client.Instance.SpriteManager.GetSprite(Module);
			}
		}
	}
}
