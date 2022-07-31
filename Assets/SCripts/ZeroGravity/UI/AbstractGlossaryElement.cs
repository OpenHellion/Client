using UnityEngine;

namespace ZeroGravity.UI
{
	public abstract class AbstractGlossaryElement : ScriptableObject
	{
		public string Name;

		[TextArea(8, 8)]
		public string Description;

		public Sprite Image;

		public abstract GlossaryCategory Category { get; }

		public abstract Sprite Icon { get; }
	}
}
