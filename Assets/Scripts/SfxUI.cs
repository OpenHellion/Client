using System;
using UnityEngine;
using UnityEngine.EventSystems;
using OpenHellion;

public class SfxUI : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
	private static World _world;

	private void Awake()
	{
		_world ??= GameObject.Find("/World").GetComponent<World>();
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
			_world.InGameGUI.SoundEffect.Play(1);
		}
		else if (Type == ButtonType.SpawnPoint)
		{
			_world.InGameGUI.SoundEffect.Play(1);
		}
		else
		{
			_world.InGameGUI.SoundEffect.Play(1);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (Type == ButtonType.Cancel)
		{
			_world.InGameGUI.SoundEffect.Play(1);
		}
		else if (Type == ButtonType.SpawnPoint)
		{
			_world.InGameGUI.SoundEffect.Play(0);
		}
		else
		{
			_world.InGameGUI.SoundEffect.Play(0);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Cursor.SetCursor(_world.DefaultCursor, Vector2.zero, CursorMode.Auto);
	}
}
