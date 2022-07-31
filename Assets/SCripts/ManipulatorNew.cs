using UnityEngine;
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
		RaycastHit hitInfo = default(RaycastHit);
		if (Physics.Raycast(MainCamera.ScreenPointToRay(Input.mousePosition), out hitInfo, float.PositiveInfinity) && InputManager.GetButtonDown(InputManager.AxisNames.Mouse1))
		{
			float z = hitInfo.distance + MainCamera.nearClipPlane;
			Vector3 position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, z);
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
			if (InputManager.GetButtonDown(InputManager.AxisNames.Mouse1))
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
				startMove = Input.mousePosition;
				endMove = Input.mousePosition;
				startRot = base.transform.localRotation;
			}
			if (InputManager.GetButton(InputManager.AxisNames.Mouse1))
			{
			}
			angleOld = num;
		}
		if (InputManager.GetButtonUp(InputManager.AxisNames.Mouse1))
		{
			X.SetActive(true);
			Y.SetActive(true);
			Z.SetActive(true);
			isMoving = false;
		}
		if (isMoving && InputManager.GetButton(InputManager.AxisNames.Mouse1))
		{
			Vector3 vector2 = MainCamera.WorldToScreenPoint(TheOne.transform.position) - MainCamera.WorldToScreenPoint(base.transform.position);
			Vector3 vector3 = Input.mousePosition - MainCamera.WorldToScreenPoint(base.transform.position);
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
			float axis = InputManager.GetAxis(InputManager.AxisNames.LookHorizontal);
			float axis2 = InputManager.GetAxis(InputManager.AxisNames.LookVertical);
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
