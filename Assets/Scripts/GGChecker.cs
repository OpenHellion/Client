using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GGChecker : MonoBehaviour
{
	private float ggniz = 4.3f;

	private void Start()
	{
		byte[] arrBytes = TestFja(ggniz);
		float num = ByteArrayToType<float>(arrBytes);
	}

	public static T ByteArrayToType<T>(byte[] arrBytes)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			memoryStream.Write(arrBytes, 0, arrBytes.Length);
			memoryStream.Seek(0L, SeekOrigin.Begin);
			object obj = binaryFormatter.Deserialize(memoryStream);
			return (T)obj;
		}
	}

	public byte[] TestFja(object o)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using (MemoryStream memoryStream = new MemoryStream())
		{
			binaryFormatter.Serialize(memoryStream, o);
			return memoryStream.ToArray();
		}
	}

	private void Update()
	{
	}
}
