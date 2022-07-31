using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public class GlossaryGroupUI : MonoBehaviour
	{
		public GlosserySubGroup GlossarySubGrp;

		public List<GlossaryItemUI> AllItems;

		public Text Name;

		public Transform ItemsHolder;
	}
}
