using UnityEngine;
using UnityEngine.Video;

public class testvideotexture : MonoBehaviour
{
	private void Start()
	{
		VideoPlayer player = gameObject.AddComponent<VideoPlayer>();
		player.isLooping = true;
		player.Play();

		//MovieTexture movieTexture = (MovieTexture)GetComponent<MeshRenderer>().material.mainTexture;
		//movieTexture.loop = true;
		//movieTexture.Play();
	}

	private void Update()
	{
	}
}
