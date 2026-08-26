using System;
using UnityEngine;
using UnityEngine.EventSystems;
using OpenHellion.UI;
using OpenHellion;

namespace ZeroGravity.UI
{
	public class SfxUI : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
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
    			Globals.SoundEffect.Play(1);
    		}
    		else if (Type == ButtonType.SpawnPoint)
    		{
    			Globals.SoundEffect.Play(1);
    		}
    		else
    		{
    			Globals.SoundEffect.Play(1);
    		}
    	}

    	public void OnPointerEnter(PointerEventData eventData)
    	{
    		if (Type == ButtonType.Cancel)
    		{
    			Globals.SoundEffect.Play(1);
    		}
    		else if (Type == ButtonType.SpawnPoint)
    		{
    			Globals.SoundEffect.Play(0);
    		}
    		else
    		{
    			Globals.SoundEffect.Play(0);
    		}
    	}
    }
}
