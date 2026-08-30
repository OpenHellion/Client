using UnityEngine;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class ArtificialBodyDistress : MonoBehaviour
	{
		private ArtificialBody _parentBody;

		private void Start()
		{
			_parentBody = GetComponentInParent<GeometryRoot>().MainObject as ArtificialBody;
		}

		public void ToggleDistressCall(bool isActive)
		{
			if (_parentBody != null)
			{
				_parentBody.SendDistressCall(isActive);
			}
		}
	}
}
