using System;
using UnityEngine;
using UnityEngine.EventSystems;
using OpenHellion.UI;

namespace ZeroGravity.UI
{
	public class SfxUI : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
    	private static InGameGUI _inGameGUI;

    	private void Awake()
	    {
		    GameObject inGameGui = GameObject.Find("/InGameGUI");
		    if (inGameGui is null)
		    {
			    Debug.LogError("SFX UI could not find InGameGUI GameObject in scene.");
			    Destroy(this);
			    return;
		    }

		    _inGameGUI ??= inGameGui.GetComponent<InGameGUI>();
    	}

    	public enum ButtonType
    	{
    		Default = 0,
    		Cancel = 1,
    		SpawnPoint = 2
    	}

    	public ButtonType Type;

    	public void OnPointerClick(PointerEventData eventData)
    	{
    		if (Type == ButtonType.Cancel)
    		{
    			_inGameGUI.SoundEffect.Play(1);
    		}
    		else if (Type == ButtonType.SpawnPoint)
    		{
    			_inGameGUI.SoundEffect.Play(1);
    		}
    		else
    		{
    			_inGameGUI.SoundEffect.Play(1);
    		}
    	}

    	public void OnPointerEnter(PointerEventData eventData)
    	{
    		if (Type == ButtonType.Cancel)
    		{
    			_inGameGUI.SoundEffect.Play(1);
    		}
    		else if (Type == ButtonType.SpawnPoint)
    		{
    			_inGameGUI.SoundEffect.Play(0);
    		}
    		else
    		{
    			_inGameGUI.SoundEffect.Play(0);
    		}
    	}
    }
}
