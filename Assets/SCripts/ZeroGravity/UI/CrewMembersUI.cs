using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.UI
{
	public class CrewMembersUI : MonoBehaviour
	{
		private SecuritySystem.PlayerSecurityData player;

		public Text PlayerNameText;

		public RawImage Avatar;

		public SecuritySystem.PlayerSecurityData Player
		{
			get
			{
				return player;
			}
			set
			{
				player = value;
				PlayerNameText.text = player.Name;
			}
		}
	}
}
