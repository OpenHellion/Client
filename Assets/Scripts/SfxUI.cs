using UnityEngine;
using UnityEngine.EventSystems;
using ZeroGravity;

public class SfxUI : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler, IEventSystemHandler
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
			Client.Instance.CanvasManager.SoundEffect.Play(1);
		}
		else if (Type == ButtonType.SpawnPoint)
		{
			Client.Instance.CanvasManager.SoundEffect.Play(1);
		}
		else
		{
			Client.Instance.CanvasManager.SoundEffect.Play(1);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (Type == ButtonType.Cancel)
		{
			Client.Instance.CanvasManager.SoundEffect.Play(1);
		}
		else if (Type == ButtonType.SpawnPoint)
		{
			Client.Instance.CanvasManager.SoundEffect.Play(0);
		}
		else
		{
			Client.Instance.CanvasManager.SoundEffect.Play(0);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Cursor.SetCursor(Client.Instance.DefaultCursor, Vector2.zero, CursorMode.Auto);
	}
}
