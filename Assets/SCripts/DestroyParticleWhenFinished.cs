using UnityEngine;

public class DestroyParticleWhenFinished : MonoBehaviour
{
	private ParticleSystem ps;

	private bool canDestroy;

	private void OnTransformParentChanged()
	{
		if (base.transform.parent == null)
		{
			canDestroy = true;
		}
	}

	private void Start()
	{
		ps = GetComponent<ParticleSystem>();
	}

	private void Update()
	{
		if (canDestroy && ps.isStopped)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
