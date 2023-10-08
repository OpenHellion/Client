using UnityEngine;

namespace RootMotion.Demos
{
	[RequireComponent(typeof(FPSAiming))]
	[RequireComponent(typeof(Animator))]
	public class FPSCharacter : MonoBehaviour
	{
		[Range(0f, 1f)] public float walkSpeed = 0.5f;

		private float sVel;

		private Animator animator;

		private FPSAiming FPSAiming;

		private void Start()
		{
			animator = GetComponent<Animator>();
			FPSAiming = GetComponent<FPSAiming>();
		}

		private void Update()
		{
			FPSAiming.sightWeight =
				Mathf.SmoothDamp(FPSAiming.sightWeight, (!Input.GetMouseButton(1)) ? 0f : 1f, ref sVel, 0.1f);
			if (FPSAiming.sightWeight < 0.001f)
			{
				FPSAiming.sightWeight = 0f;
			}

			if (FPSAiming.sightWeight > 0.999f)
			{
				FPSAiming.sightWeight = 1f;
			}

			animator.SetFloat("Speed", walkSpeed);
		}

		private void OnGUI()
		{
			GUI.Label(new Rect(Screen.width - 210, 10f, 200f, 25f), "Hold RMB to aim down the sight");
		}
	}
}
