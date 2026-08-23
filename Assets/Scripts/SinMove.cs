using UnityEngine;

public class SinMove : MonoBehaviour
{
	public float Offset;

	public float Speed = 1f;

	public float Distance = 0.05f;

	private Vector3 _basePosition;

	private float _totalTime;

	private void Start()
	{
		_basePosition = transform.position;
		_totalTime = Offset;
	}

	private void Update()
	{
		_totalTime += Time.deltaTime * Speed;
		transform.position = _basePosition + Distance * Vector3.up * Mathf.Sin(_totalTime);
	}
}
