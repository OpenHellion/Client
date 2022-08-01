using UnityEngine;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class ArtificialBodyDistress : MonoBehaviour
	{
		private ArtificialBody parentBody;

		private void Start()
		{
			if (Client.IsGameBuild)
			{
				parentBody = GetComponentInParent<GeometryRoot>().MainObject as ArtificialBody;
			}
		}

		public void ToggleDistressCall(bool isActive)
		{
			if (parentBody != null)
			{
				Client.Instance.SendDistressCall(parentBody, isActive);
			}
		}
	}
}
