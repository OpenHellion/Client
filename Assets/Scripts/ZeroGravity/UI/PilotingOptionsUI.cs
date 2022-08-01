using UnityEngine;
using ZeroGravity.LevelDesign;

namespace ZeroGravity.UI
{
	public class PilotingOptionsUI : MonoBehaviour
	{
		public GameObject Piloting;

		public GameObject Navigation;

		public GameObject NavigationDisabled;

		public GameObject Docking;

		public GameObject Lights;

		public GameObject LightsMalfunction;

		public void SetPilotingMode(ShipControlMode mode)
		{
			Piloting.SetActive(mode == ShipControlMode.Piloting);
			Navigation.SetActive(mode == ShipControlMode.Navigation);
			Docking.SetActive(mode == ShipControlMode.Docking);
		}
	}
}
