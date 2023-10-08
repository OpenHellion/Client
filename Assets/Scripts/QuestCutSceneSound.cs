using UnityEngine;
using UnityEngine.Events;
using ZeroGravity.Objects;

public class QuestCutSceneSound : MonoBehaviour
{
	public bool IsPlaying;

	public UnityEvent OnFinishedEvent;

	public UnityEvent OnMarkerEvent;

	public AkAudioListener Listener;

	public void Play(string eventString)
	{
		if (!IsPlaying)
		{
			if (MyPlayer.Instance != null)
			{
				ulong[] array = new ulong[1]
					{ AkSoundEngine.GetAkGameObjectID(MyPlayer.Instance.FpsController.MainCamera.gameObject) };
				AkSoundEngine.SetListeners(base.gameObject, array, (uint)array.Length);
			}
			else
			{
				ulong[] array2 = new ulong[1] { AkSoundEngine.GetAkGameObjectID(Listener.gameObject) };
				AkSoundEngine.SetListeners(base.gameObject, array2, (uint)array2.Length);
			}

			AkSoundEngine.PostEvent(eventString, base.gameObject, 5u, OnCutSceneSoundEventEnd, eventString);
			IsPlaying = true;
		}
	}

	public void Stop()
	{
		if (IsPlaying)
		{
			AkSoundEngine.PostEvent("StopQuestAudio", base.gameObject);
		}
	}

	private void OnCutSceneSoundEventEnd(object in_cookie, AkCallbackType in_type, object in_info)
	{
		if (in_type == AkCallbackType.AK_Marker)
		{
			OnMarkerEvent.Invoke();
		}

		if (in_type == AkCallbackType.AK_EndOfEvent)
		{
			IsPlaying = false;
			OnMarkerEvent.Invoke();
			OnFinishedEvent.Invoke();
		}
	}
}
