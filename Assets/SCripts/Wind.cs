using UnityEngine;

public class Wind : MonoBehaviour
{
	[MinValue(0f)]
	public float m_Speed = 1f;

	private void OnDrawGizmosSelected()
	{
		Vector3[] array = new Vector3[8]
		{
			new Vector3(0f, 0f, 1.5f),
			new Vector3(1f, 0f, 0.5f),
			new Vector3(0.5f, 0f, 0.5f),
			new Vector3(0.5f, 0f, -1f),
			new Vector3(-0.5f, 0f, -1f),
			new Vector3(-0.5f, 0f, 0.5f),
			new Vector3(-1f, 0f, 0.5f),
			new Vector3(0f, 0f, 1.5f)
		};
		Gizmos.matrix = base.transform.localToWorldMatrix;
		int num = array.Length;
		for (int i = 0; i < num; i++)
		{
			Gizmos.DrawLine(array[i], array[(i + 1) % num]);
		}
		Gizmos.matrix *= Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 90f), Vector3.one);
		for (int j = 0; j < num; j++)
		{
			Gizmos.DrawLine(array[j], array[(j + 1) % num]);
		}
	}
}
