using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using ZeroGravity;
using ZeroGravity.Math;
using ZeroGravity.UI;

public class ManipulatorNew : MonoBehaviour
{
	public Camera MainCamera;

	public Transform Root;

	public GameObject Topkata;

	public GameObject Pokazivac;

	private float angleOld;

	private GameObject SelectedGO;

	public GameObject X;

	public GameObject Y;

	public GameObject Z;

	public Text DbgTxt0;

	public Text DbgTxt1;

	public GameObject TheOne;

	private Vector3 pomeraj;

	public Image dbgimg0;

	public Image dbgimg1;

	private Vector2 startMove;

	private Vector2 endMove;

	private bool isMoving;

	private Quaternion startRot;

	private void Update()
	{
		RaycastHit hitInfo = default;
		if (Physics.Raycast(MainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()), out hitInfo, float.PositiveInfinity) && Mouse.current.leftButton.wasPressedThisFrame)
		{
			float z = hitInfo.distance + MainCamera.nearClipPlane;
			Vector3 position = new Vector3(Mouse.current.position.x.ReadValue(), Mouse.current.position.y.ReadValue(), z);
			Vector3 position2 = MainCamera.ScreenToWorldPoint(position);
			Topkata.transform.position = position2;
			Vector3 vec = Topkata.transform.localPosition - Root.localPosition;
			float num = 0f;
			pomeraj = new Vector3(0f, 0f, 0f);
			if (hitInfo.collider.gameObject == X)
			{
				num = MathHelper.AngleSigned(Vector3.forward, vec, Vector3.right);
				SelectedGO = X;
			}
			else if (hitInfo.collider.gameObject == Y)
			{
				num = MathHelper.AngleSigned(Vector3.up, vec, Vector3.forward);
				SelectedGO = Y;
			}
			else if (hitInfo.collider.gameObject == Z)
			{
				num = MathHelper.AngleSigned(Vector3.forward, vec, Vector3.up);
				SelectedGO = Z;
			}
			if (Mouse.current.leftButton.isPressed)
			{
				TheOne.transform.position = position2;
				if (SelectedGO == X)
				{
					Y.SetActive(false);
					Z.SetActive(false);
					pomeraj = Vector3.right;
				}
				else if (SelectedGO == Y)
				{
					X.SetActive(false);
					Z.SetActive(false);
					pomeraj = Vector3.up;
				}
				else if (SelectedGO == Z)
				{
					X.SetActive(false);
					Y.SetActive(false);
					pomeraj = Vector3.forward;
				}
				isMoving = true;
				startMove = Mouse.current.position.ReadValue();
				endMove = Mouse.current.position.ReadValue();
				startRot = base.transform.localRotation;
			}
			if (Mouse.current.leftButton.isPressed)
			{
			}
			angleOld = num;
		}
		if (Mouse.current.leftButton.wasReleasedThisFrame)
		{
			X.SetActive(true);
			Y.SetActive(true);
			Z.SetActive(true);
			isMoving = false;
		}
		if (isMoving && Mouse.current.leftButton.isPressed)
		{
			Vector3 vector2 = MainCamera.WorldToScreenPoint(TheOne.transform.position) - MainCamera.WorldToScreenPoint(base.transform.position);
			Vector3 vector3 = (Vector3)Mouse.current.position.ReadValue() - MainCamera.WorldToScreenPoint(base.transform.position);
			dbgimg0.transform.position = vector2;
			dbgimg1.transform.position = vector3;
			Debug.DrawLine(vector2, vector3);
			float num2 = Vector2.Angle(vector3, vector2);
			float num3 = MathHelper.AngleSigned(vector3, vector2, Vector3.forward);
			if (SelectedGO == X)
			{
				pomeraj = Vector3.right;
			}
			else if (SelectedGO == Y)
			{
				pomeraj = Vector3.up;
			}
			else if (SelectedGO == Z)
			{
				pomeraj = Vector3.forward;
			}
			float axis = Mouse.current.delta.x.ReadValue();
			float axis2 = Mouse.current.delta.y.ReadValue();
			if (axis.IsNotEpsilonZero() || axis2.IsNotEpsilonZero())
			{
				Vector3 vector4 = MainCamera.WorldToScreenPoint(TheOne.transform.position);
				vector4.z = 0f;
				Vector3 vector5 = new Vector3(axis, axis2, 0f);
				num3 = MathHelper.AngleSigned(MainCamera.WorldToScreenPoint(TheOne.transform.position), vector5, Vector3.forward);
				DbgTxt1.text = num3.ToString("f5");
				DbgTxt0.text = string.Concat(vector4, "   ", vector5);
				base.transform.localRotation *= Quaternion.Euler(pomeraj * num3 * 0.07f);
			}
		}
	}
}
