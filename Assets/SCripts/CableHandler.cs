using UnityEngine;

public class CableHandler : MonoBehaviour
{
	public GameObject ChainStart;

	public GameObject ChainEnd;

	public GameObject FirstJoint;

	public GameObject LastJoint;

	private bool PhysicsEnabled;

	private bool CanUpdate;

	private void EnablePhysics()
	{
		Rigidbody[] componentsInChildren = GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody rigidbody in componentsInChildren)
		{
			if (rigidbody.gameObject == FirstJoint || rigidbody.gameObject == LastJoint)
			{
				rigidbody.isKinematic = true;
			}
			else
			{
				rigidbody.isKinematic = false;
			}
		}
	}

	private void DisablePhysics()
	{
		Rigidbody[] componentsInChildren = GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody rigidbody in componentsInChildren)
		{
			rigidbody.isKinematic = true;
		}
	}

	private void Awake()
	{
		if (ChainStart != null && ChainEnd != null && FirstJoint != null && LastJoint != null)
		{
			PhysicsEnabled = true;
		}
		else
		{
			DisablePhysics();
		}
	}

	private void LateUpdate()
	{
		if (PhysicsEnabled && !CanUpdate)
		{
			base.transform.position = ChainStart.transform.position;
			base.transform.rotation = ChainStart.transform.rotation;
			EnablePhysics();
			CanUpdate = true;
		}
	}

	private void Update()
	{
		if (CanUpdate)
		{
			FirstJoint.transform.position = ChainStart.transform.position;
			FirstJoint.transform.rotation = ChainStart.transform.rotation;
			LastJoint.transform.position = ChainEnd.transform.position;
			LastJoint.transform.rotation = ChainEnd.transform.rotation;
		}
	}
}
