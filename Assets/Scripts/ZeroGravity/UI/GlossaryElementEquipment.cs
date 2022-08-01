using UnityEngine;
using UnityEngine.Serialization;
using ZeroGravity.Data;

namespace ZeroGravity.UI
{
	[CreateAssetMenu(fileName = "GlossaryElementEquipment", menuName = "Glossary/Glossary Element Equipment")]
	public class GlossaryElementEquipment : AbstractGlossaryElement
	{
		[FormerlySerializedAs("Item")]
		public ItemCompoundType CompoundType;

		public bool Tiers;

		[TextArea(4, 4)]
		public string Tier1;

		[TextArea(4, 4)]
		public string Tier2;

		[TextArea(4, 4)]
		public string Tier3;

		[TextArea(4, 4)]
		public string Tier4;

		public override GlossaryCategory Category
		{
			get
			{
				return GlossaryCategory.Items;
			}
		}

		public override Sprite Icon
		{
			get
			{
				return Client.Instance.SpriteManager.GetSprite(CompoundType);
			}
		}
	}
}
