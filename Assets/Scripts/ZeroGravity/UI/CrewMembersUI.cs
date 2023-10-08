using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;

namespace ZeroGravity.UI
{
	public class CrewMembersUI : MonoBehaviour
	{
		private AuthorizedPerson player;

		public Text PlayerNameText;

		public RawImage Avatar;

		public AuthorizedPerson Player
		{
			get { return player; }
			set
			{
				player = value;
				PlayerNameText.text = player.Name;
			}
		}
	}
}
