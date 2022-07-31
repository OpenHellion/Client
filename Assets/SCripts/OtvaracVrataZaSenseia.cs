using UnityEngine;
using ZeroGravity;
using ZeroGravity.Objects;

public class OtvaracVrataZaSenseia : MonoBehaviour
{
	private Animator anim;

	private AudioSource audioSrc;

	public AudioClip open;

	public AudioClip close;

	private void Awake()
	{
		anim = GetComponent<Animator>();
		audioSrc = GetComponent<AudioSource>();
	}

	public void ToggleDoor(bool isOpen)
	{
		if (isOpen)
		{
			anim.Play("Open");
			audioSrc.PlayOneShot(open, 1f);
		}
		else
		{
			anim.Play("Close");
			audioSrc.PlayOneShot(close, 1f);
		}
	}

	private void Update()
	{
		if (!Client.IsGameBuild)
		{
			if (Input.GetKeyDown(KeyCode.LeftBracket))
			{
				ToggleDoor(false);
			}
			if (Input.GetKeyDown(KeyCode.RightBracket))
			{
				ToggleDoor(true);
			}
		}
	}

	private void OnTriggerEnter(Collider coli)
	{
		if (coli.GetComponent<MyPlayer>() != null)
		{
			ToggleDoor(true);
		}
	}

	private void OnTriggerExit(Collider coli)
	{
		if (coli.GetComponent<MyPlayer>() != null)
		{
			ToggleDoor(false);
		}
	}
}
