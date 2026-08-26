using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public class HandGunUI : MonoBehaviour
	{
		private int _BulletCount;

		private bool _IsSingleFire;

		[SerializeField] private Text AmmoCoaunt;

		[SerializeField] private Text AutomaticModText;

		[SerializeField] private Image AutomaticModSquare;

		[SerializeField] private Text SingleModText;

		[SerializeField] private Image SingleModSquare;

		private Color NotSelectedSquare = new Color(1f, 0f, 0f, 1f / 15f);

		private Color NotSelectedChar = new Color(1f, 0f, 0f, 0.47058824f);

		private Color SelectedSquare = new Color(1f, 0f, 0f, 14f / 85f);

		private Color SelectedChar = new Color(1f, 0f, 0f, 1f);

		public int BulletCount
		{
			get { return _BulletCount; }
			set
			{
				_BulletCount = value;
				AmmoCoaunt.text = _BulletCount.ToString();
			}
		}

		public bool IsSingleFire
		{
			get { return _IsSingleFire; }
			set
			{
				_IsSingleFire = value;
				if (value)
				{
					AutomaticModText.color = NotSelectedChar;
					AutomaticModSquare.color = NotSelectedSquare;
					SingleModText.color = SelectedChar;
					SingleModSquare.color = SelectedSquare;
				}
				else
				{
					AutomaticModText.color = SelectedChar;
					AutomaticModSquare.color = SelectedSquare;
					SingleModText.color = NotSelectedChar;
					SingleModSquare.color = NotSelectedSquare;
				}
			}
		}
	}
}
