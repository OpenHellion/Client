using System.Collections.Generic;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualCursor : MonoBehaviour
{
	public GameObject virtualCursor;

	public float Sensitivity = 2f;

	private PointerEventData pointerEventData;

	private GraphicRaycaster graphicRaycaster;

	private void Start()
	{
		pointerEventData = new PointerEventData(EventSystem.current);
	}

	private void Update()
	{
		Vector3 localPosition = virtualCursor.transform.localPosition;
		Vector3 localPosition2 = new Vector3(localPosition.x + InputManager.GetAxis("LookHorizontal") * Sensitivity, localPosition.y + InputManager.GetAxis("LookVertical") * Sensitivity, localPosition.z);
		virtualCursor.transform.localPosition = localPosition2;
		List<RaycastResult> raycastResults = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointerEventData, raycastResults);
	}
}
