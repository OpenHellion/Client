using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TextTypeEffect : MonoBehaviour
{
	[Multiline]
	public string Text = string.Empty;

	[Range(0f, 1f)]
	public float Substring;

	public float WriteSpeed = 1f;

	public float Buffer = 1f;

	[Tooltip("How many seconds to wait after type is finished before calling OnTypeFineshed")]
	private float buffer = 1f;

	private bool finished;

	public UnityEvent OnTypeFinished;

	public Text TextField;

	public int LastPause;

	public int TextMarkerReached;

	public int VoiceMarkerReached;

	public bool Pause;

	public void SetText()
	{
		string text = Text.Substring(0, Mathf.FloorToInt(Substring));
		if (text.LastIndexOf('|') != LastPause && text.LastIndexOf('|') != -1)
		{
			LastPause = text.LastIndexOf('|');
			Substring = LastPause + 1;
			text = Text.Substring(0, Mathf.FloorToInt(Substring));
			TextMarkerReached++;
			CheckForPause();
		}
		TextField.text = text.Replace('|', ' ');
		Substring += Time.deltaTime * WriteSpeed;
	}

	public void CheckForPause()
	{
		if (TextMarkerReached > VoiceMarkerReached)
		{
			Pause = true;
		}
		else
		{
			Pause = false;
		}
	}

	public void OnVoiceMarkerReached()
	{
		VoiceMarkerReached++;
		CheckForPause();
	}

	private void Awake()
	{
		TextField.text = string.Empty;
		LastPause = 0;
		Pause = false;
		base.enabled = false;
		TextMarkerReached = 0;
		VoiceMarkerReached = 0;
	}

	public void RestartAttributes()
	{
		TextField.text = string.Empty;
		LastPause = 0;
		Pause = false;
		base.enabled = false;
		TextMarkerReached = 0;
		VoiceMarkerReached = 0;
	}

	private void OnEnable()
	{
		TextField.text = string.Empty;
		LastPause = 0;
		Substring = 0f;
		buffer = Buffer;
		finished = false;
		Pause = false;
		TextMarkerReached = 0;
		VoiceMarkerReached = 0;
	}

	private void OnDisable()
	{
		TextField.text = string.Empty;
		LastPause = 0;
		Pause = false;
	}

	public void ResetWithNewText(string text)
	{
		TextField.text = string.Empty;
		Substring = 0f;
		buffer = Buffer;
		finished = false;
		Text = text;
		LastPause = 0;
		Pause = false;
	}

	private void Update()
	{
		if (Pause)
		{
			return;
		}
		if (Substring >= (float)Text.Length)
		{
			Substring = Text.Length;
			finished = true;
		}
		SetText();
		if (!finished || Pause)
		{
			return;
		}
		if (LastPause != 0)
		{
			OnTypeFinished.Invoke();
			finished = false;
			buffer = Buffer;
			LastPause = 0;
			return;
		}
		buffer -= Time.deltaTime;
		if (buffer < 0f)
		{
			OnTypeFinished.Invoke();
			finished = false;
			buffer = Buffer;
			LastPause = 0;
		}
	}
}
