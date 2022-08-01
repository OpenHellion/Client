using UnityEngine;

[ExecuteInEditMode]
public class lineEdit : MonoBehaviour
{
	public float Speed = 1f;

	private void Start()
	{
	}

	private void Update()
	{
		base.gameObject.GetComponent<LineRenderer>().material.SetFloat("_Speed", Speed);
	}
}
