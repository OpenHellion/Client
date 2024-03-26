using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ScrollList : MonoBehaviour
{
	public delegate void ScrollListMoveDelegate();

	public ScrollListMoveDelegate ScrollListMove;

	public AnimationCurve Curve;

	public ScrollRect ScrollView;

	public int VisibleMembers = 5;

	public float MaxHeight = 50f;

	public float MinHeight = 20f;

	public Transform ListContent;

	public GameObject MemberPref;

	public List<GameObject> listGO = new List<GameObject>();

	public float ChangeLength = 0.7f;

	public bool CoorutineActive;

	public int AddToTop;

	public int AddToBot;

	public int TrenutniIndex;

	public int MemberToGoTo;

	private void Start()
	{
	}

	private void Update()
	{
		int num = 0;
		foreach (GameObject item in listGO)
		{
			Color color = item.GetComponent<Image>().color;
			float a = (color.a =
				Curve.Evaluate(
					(0f - (ListContent.localPosition.y + item.transform.localPosition.y) +
					 item.GetComponent<RectTransform>().rect.height / 2f) /
					ScrollView.GetComponent<RectTransform>().rect.height));
			item.GetComponent<Image>().color = color;
			Text component = item.transform.Find("Text").GetComponent<Text>();
			Color color2 = component.color;
			color2.a = a;
			component.color = color2;
			float num2 = Mathf.Lerp(MinHeight, MaxHeight,
				Curve.Evaluate(
					(0f - (ListContent.localPosition.y + item.transform.localPosition.y) +
					 item.GetComponent<RectTransform>().rect.height / 2f) /
					ScrollView.GetComponent<RectTransform>().rect.height));
			item.GetComponent<LayoutElement>().minHeight = num2;
			if (num2 >= MaxHeight * 0.99)
			{
				TrenutniIndex = num;
			}

			num++;
		}

		if (Keyboard.current.spaceKey.isPressed)
		{
			GoToMember(MemberToGoTo);
		}
	}

	public void MakeTopBuffers()
	{
		for (int i = 1; i <= AddToTop; i++)
		{
			MakeMember(string.Empty);
		}
	}

	public void MakeBotBuffers()
	{
		for (int i = 1; i <= AddToBot; i++)
		{
			MakeMember(string.Empty);
		}
	}

	public void GoToMember(int index)
	{
		Vector3 localPosition = ListContent.localPosition;
		float num = 0f;
		num = localPosition.y + (index - (TrenutniIndex - AddToTop)) * MinHeight;
		ListContent.localPosition = new Vector3(localPosition.x, num, localPosition.z);
	}

	public GameObject MakeMember(string text)
	{
		GameObject gameObject = Object.Instantiate(MemberPref, base.transform.position, base.transform.rotation);
		gameObject.transform.SetParent(ListContent);
		gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
		gameObject.SetActive(true);
		gameObject.transform.name = text;
		gameObject.transform.Find("Text").GetComponent<Text>().text = text;
		listGO.Add(gameObject);
		return gameObject;
	}

	public void MoveList(bool goUp, int listCount)
	{
		if (!CoorutineActive)
		{
			StartCoroutine(MoveListSlowly(goUp, listCount));
		}
	}

	private IEnumerator MoveListSlowly(bool goUp, int listCount)
	{
		bool didMove = false;
		CoorutineActive = true;
		float start = ListContent.localPosition.y;
		float target2 = 0f;
		if (listCount % 2 == 0)
		{
			float test2 = listCount / 2f;
		}
		else
		{
			float test2 = listCount / 2f - 1f;
		}

		if (goUp)
		{
			if (TrenutniIndex <= AddToTop)
			{
				target2 = ListContent.localPosition.y;
			}
			else
			{
				didMove = true;
				target2 = ListContent.localPosition.y - MinHeight;
			}
		}
		else if (TrenutniIndex <= listCount + AddToTop - 2)
		{
			didMove = true;
			target2 = ListContent.localPosition.y + MinHeight;
		}
		else
		{
			target2 = ListContent.localPosition.y;
		}

		float elapsedTime = 0f;
		Vector3 oldVal = ListContent.localPosition;
		while (ListContent.localPosition.y != target2)
		{
			elapsedTime += Time.deltaTime;
			float tmp = Mathf.SmoothStep(start, target2, elapsedTime / ChangeLength);
			ListContent.localPosition = new Vector3(oldVal.x, tmp, oldVal.z);
			yield return new WaitForEndOfFrame();
		}

		CoorutineActive = false;
		if (didMove)
		{
			ScrollListMove();
		}

		yield return null;
	}

	public void PurgeAll()
	{
		foreach (GameObject item in listGO)
		{
			Object.Destroy(item);
		}

		listGO.Clear();
	}
}
