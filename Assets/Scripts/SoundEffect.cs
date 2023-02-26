using System.Collections.Generic;
using UnityEngine;
using ZeroGravity;
using ZeroGravity.Objects;

public class SoundEffect : AkGameObj
{
	public enum Listener
	{
		MyPlayer = 0,
		UiCamera = 1,
		Default = 2,
		Custom = 3
	}

	public string DefaultEnvironment;

	public uint DefaultEnvironmentID;

	public List<SoundEvent> SoundEvents = new List<SoundEvent>();

	public List<SoundEvent> FpsSoundEvents = new List<SoundEvent>();

	public List<AkAudioListener> Listeners = new List<AkAudioListener>();

	public bool UseFPSSounds;

	public bool UseParticleEvents;

	public ParticleSystem ParticleSystem;

	public bool CanPlay = true;

	public bool MustPlay;

	public Listener ListenerType;

	public bool IsPlaying
	{
		get
		{
			bool result = false;
			foreach (SoundEvent soundEvent in SoundEvents)
			{
				if (soundEvent.IsPlaying)
				{
					return true;
				}
			}
			return result;
		}
	}

	private void OnEnable()
	{
		if (IsPlaying)
		{
			return;
		}
		if (UseParticleEvents && ParticleSystem == null)
		{
			ParticleSystem = GetComponent<ParticleSystem>();
			if (ParticleSystem == null)
			{
				ParticleSystem = GetComponentInParent<ParticleSystem>();
				if (ParticleSystem == null)
				{
					UseParticleEvents = false;
				}
			}
		}
		SetEnvironment(DefaultEnvironment);
		if (ListenerType != Listener.Default)
		{
			SetListeners();
		}
		PlayOnAwake();
	}

	private void Start()
	{
		if (UseParticleEvents && ParticleSystem == null)
		{
			ParticleSystem = GetComponent<ParticleSystem>();
			if (ParticleSystem == null)
			{
				ParticleSystem = GetComponentInParent<ParticleSystem>();
				if (ParticleSystem == null)
				{
					UseParticleEvents = false;
				}
			}
		}
		SetEnvironment(DefaultEnvironment);
		if (ListenerType != Listener.Default)
		{
			SetListeners();
		}
		PlayOnAwake();
	}

	private void Update()
	{
		if (UseParticleEvents)
		{
			if (ParticleSystem.isPlaying && !SoundEvents[0].IsPlaying)
			{
				Play(0);
			}
			if (!ParticleSystem.isPlaying)
			{
				AkSoundEngine.StopAll(base.gameObject);
				SoundEvents[0].IsPlaying = false;
			}
		}
	}

	public void PlayOnAwake()
	{
		foreach (SoundEvent soundEvent in SoundEvents)
		{
			if (soundEvent.PlayOnAwake)
			{
				Play(soundEvent);
			}
		}
	}

	public void SetListeners()
	{
		if (ListenerType == Listener.Custom)
		{
			if (Listeners.Count <= 0)
			{
				return;
			}
			ulong[] array = new ulong[Listeners.Count];
			for (int i = 0; i < Listeners.Count; i++)
			{
				if (Listeners[i] != null)
				{
					array[i] = AkSoundEngine.GetAkGameObjectID(Listeners[i].gameObject);
				}
				else
				{
					array[i] = 0uL;
				}
			}
			foreach (SoundEvent soundEvent in SoundEvents)
			{
				if (soundEvent.Source != null)
				{
					AkSoundEngine.SetListeners(soundEvent.Source.gameObject, array, (uint)array.Length);
				}
			}
			AkSoundEngine.SetListeners(base.gameObject, array, (uint)array.Length);
		}
		else if (ListenerType == Listener.MyPlayer)
		{
			if (!(MyPlayer.Instance != null))
			{
				return;
			}
			ulong[] array2 = new ulong[1] { AkSoundEngine.GetAkGameObjectID(MyPlayer.Instance.FpsController.MainCamera.gameObject) };
			foreach (SoundEvent soundEvent2 in SoundEvents)
			{
				if (soundEvent2.Source != null)
				{
					AkSoundEngine.SetListeners(soundEvent2.Source.gameObject, array2, (uint)array2.Length);
				}
			}
			AkSoundEngine.SetListeners(base.gameObject, array2, (uint)array2.Length);
		}
		else
		{
			if (ListenerType != Listener.UiCamera)
			{
				return;
			}
			Client instance = Client.Instance;
			object obj;
			if ((object)instance == null)
			{
				obj = null;
			}
			else
			{
				CanvasManager canvasManager = instance.CanvasManager;
				obj = (((object)canvasManager != null) ? canvasManager.Canvas : null);
			}
			if (!((Object)obj != null))
			{
				return;
			}
			ulong[] array3 = new ulong[1] { AkSoundEngine.GetAkGameObjectID(Client.Instance.CanvasManager.Canvas.worldCamera.gameObject) };
			foreach (SoundEvent soundEvent3 in SoundEvents)
			{
				if (soundEvent3.Source != null)
				{
					AkSoundEngine.SetListeners(soundEvent3.Source.gameObject, array3, (uint)array3.Length);
				}
			}
			AkSoundEngine.SetListeners(base.gameObject, array3, (uint)array3.Length);
		}
	}

	public void SetEnvironment(string env)
	{
		AkAuxSendArray akAuxSendArray = new AkAuxSendArray();
		if (env != "None")
		{
			foreach (SoundEvent soundEvent in SoundEvents)
			{
				if (soundEvent.Source != null)
				{
					akAuxSendArray.Add(soundEvent.Source.gameObject, AkSoundEngine.GetIDFromString(env), 1f);
					AkSoundEngine.SetGameObjectAuxSendValues(soundEvent.Source.gameObject, akAuxSendArray, (uint)akAuxSendArray.Count());
				}
				else
				{
					akAuxSendArray.Add(base.gameObject, AkSoundEngine.GetIDFromString(env), 1f);
					AkSoundEngine.SetGameObjectAuxSendValues(base.gameObject, akAuxSendArray, (uint)akAuxSendArray.Count());
				}
			}
			{
				foreach (SoundEvent fpsSoundEvent in FpsSoundEvents)
				{
					if (fpsSoundEvent.Source != null)
					{
						akAuxSendArray.Add(fpsSoundEvent.Source.gameObject, AkSoundEngine.GetIDFromString(env), 1f);
						AkSoundEngine.SetGameObjectAuxSendValues(fpsSoundEvent.Source.gameObject, akAuxSendArray, (uint)akAuxSendArray.Count());
					}
					else
					{
						akAuxSendArray.Add(base.gameObject, AkSoundEngine.GetIDFromString(env), 1f);
						AkSoundEngine.SetGameObjectAuxSendValues(base.gameObject, akAuxSendArray, (uint)akAuxSendArray.Count());
					}
				}
				return;
			}
		}
		foreach (SoundEvent soundEvent2 in SoundEvents)
		{
			if (soundEvent2.Source != null)
			{
				AkSoundEngine.SetGameObjectAuxSendValues(soundEvent2.Source.gameObject, akAuxSendArray, 0u);
			}
			else
			{
				AkSoundEngine.SetGameObjectAuxSendValues(base.gameObject, akAuxSendArray, 0u);
			}
		}
		foreach (SoundEvent fpsSoundEvent2 in FpsSoundEvents)
		{
			if (fpsSoundEvent2.Source != null)
			{
				AkSoundEngine.SetGameObjectAuxSendValues(fpsSoundEvent2.Source.gameObject, akAuxSendArray, 0u);
			}
			else
			{
				AkSoundEngine.SetGameObjectAuxSendValues(base.gameObject, akAuxSendArray, 0u);
			}
		}
	}

	private void OnDestroy()
	{
		AkSoundEngine.StopAll(base.gameObject);
		foreach (SoundEvent soundEvent in SoundEvents)
		{
			if (soundEvent.Source != null)
			{
				AkSoundEngine.StopAll(soundEvent.Source.gameObject);
			}
		}
	}

	private void OnDisable()
	{
		AkSoundEngine.StopAll(base.gameObject);
		foreach (SoundEvent soundEvent in SoundEvents)
		{
			if (soundEvent.Source != null)
			{
				AkSoundEngine.StopAll(soundEvent.Source.gameObject);
			}
		}
	}

	public void Play()
	{
		Play(0);
	}

	public void InvokePlay(float time)
	{
		Invoke("Play", time);
	}

	public void Play(int index, bool dontPlayIfPlaying = false)
	{
		if (!UseFPSSounds)
		{
			if (SoundEvents.Count > 0 && index < SoundEvents.Count && index >= 0 && (!SoundEvents[index].IsPlaying || !dontPlayIfPlaying))
			{
				Play(SoundEvents[index]);
			}
		}
		else if (FpsSoundEvents.Count > 0 && index < FpsSoundEvents.Count && index >= 0 && (!FpsSoundEvents[index].IsPlaying || !dontPlayIfPlaying))
		{
			Play(FpsSoundEvents[index]);
		}
	}

	public void PlayIndex(int index)
	{
		Play(index);
	}

	public void PlayIndexIfNotPlaying(int index)
	{
		Play(index, true);
	}

	public void Play(SoundEvent soundEvent)
	{
		if (!MustPlay && !CanPlay)
		{
			return;
		}
		ArtificialBody componentInParent = GetComponentInParent<ArtificialBody>();
		if (componentInParent != null)
		{
			if (componentInParent is Pivot)
			{
				if (MyPlayer.Instance.Parent != componentInParent)
				{
					return;
				}
			}
			else if (componentInParent is SpaceObjectVessel)
			{
				SpaceObjectVessel mainVessel = (componentInParent as SpaceObjectVessel).MainVessel;
				if ((MyPlayer.Instance.Parent is SpaceObjectVessel && (MyPlayer.Instance.Parent as SpaceObjectVessel).MainVessel != mainVessel) || (MyPlayer.Instance.Parent is Pivot && MyPlayer.Instance.FpsController.StickToVessel != null && MyPlayer.Instance.FpsController.StickToVessel.MainVessel != mainVessel))
				{
					return;
				}
			}
		}
		if (soundEvent.Source != null)
		{
			AkSoundEngine.PostEvent(AkSoundEngine.GetIDFromString(soundEvent.EventString), soundEvent.Source.gameObject);
			return;
		}
		soundEvent.IsPlaying = true;
		AkSoundEngine.PostEvent(AkSoundEngine.GetIDFromString(soundEvent.EventString), base.gameObject, 1u, OnSoundEventEnd, soundEvent);
	}

	private void OnSoundEventEnd(object in_cookie, AkCallbackType in_type, object in_info)
	{
		(in_cookie as SoundEvent).IsPlaying = false;
	}

	public void SetRTPCValue(string parameter, float value)
	{
		foreach (SoundEvent soundEvent in SoundEvents)
		{
			if (soundEvent.Source != null)
			{
				AkSoundEngine.SetRTPCValue(AkSoundEngine.GetIDFromString(parameter), value, soundEvent.Source.gameObject);
			}
			else
			{
				AkSoundEngine.SetRTPCValue(AkSoundEngine.GetIDFromString(parameter), value, base.gameObject);
			}
		}
		foreach (SoundEvent fpsSoundEvent in FpsSoundEvents)
		{
			if (fpsSoundEvent.Source != null)
			{
				AkSoundEngine.SetRTPCValue(AkSoundEngine.GetIDFromString(parameter), value, fpsSoundEvent.Source.gameObject);
			}
			else
			{
				AkSoundEngine.SetRTPCValue(AkSoundEngine.GetIDFromString(parameter), value, base.gameObject);
			}
		}
	}

	public void SwitchAmbience(string ambience)
	{
		foreach (SoundEvent soundEvent in SoundEvents)
		{
			if (soundEvent.Source != null)
			{
				AkSoundEngine.SetSwitch("Ambience", ambience, soundEvent.Source.gameObject);
			}
			else
			{
				AkSoundEngine.SetSwitch("Ambience", ambience, base.gameObject);
			}
		}
		foreach (SoundEvent fpsSoundEvent in FpsSoundEvents)
		{
			if (fpsSoundEvent.Source != null)
			{
				AkSoundEngine.SetSwitch("Ambience", ambience, fpsSoundEvent.Source.gameObject);
			}
			else
			{
				AkSoundEngine.SetSwitch("Ambience", ambience, base.gameObject);
			}
		}
	}

	public void Switch(string group, string swtch)
	{
		foreach (SoundEvent soundEvent in SoundEvents)
		{
			if (soundEvent.Source != null)
			{
				AkSoundEngine.SetSwitch(AkSoundEngine.GetIDFromString(group), AkSoundEngine.GetIDFromString(swtch), soundEvent.Source.gameObject);
			}
			else
			{
				AkSoundEngine.SetSwitch(AkSoundEngine.GetIDFromString(group), AkSoundEngine.GetIDFromString(swtch), base.gameObject);
			}
		}
		foreach (SoundEvent fpsSoundEvent in FpsSoundEvents)
		{
			if (fpsSoundEvent.Source != null)
			{
				AkSoundEngine.SetSwitch(AkSoundEngine.GetIDFromString(group), AkSoundEngine.GetIDFromString(swtch), fpsSoundEvent.Source.gameObject);
			}
			else
			{
				AkSoundEngine.SetSwitch(AkSoundEngine.GetIDFromString(group), AkSoundEngine.GetIDFromString(swtch), base.gameObject);
			}
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawIcon(base.transform.position, "SoundEffect", false);
	}
}
