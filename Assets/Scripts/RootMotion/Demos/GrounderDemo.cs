using UnityEngine;

namespace RootMotion.Demos
{
	public class GrounderDemo : MonoBehaviour
	{
		public GameObject[] characters;

		private void OnGUI()
		{
			if (GUILayout.Button("Biped"))
			{
				Activate(0);
			}

			if (GUILayout.Button("Quadruped"))
			{
				Activate(1);
			}

			if (GUILayout.Button("Mech"))
			{
				Activate(2);
			}

			if (GUILayout.Button("Bot"))
			{
				Activate(3);
			}
		}

		public void Activate(int index)
		{
			for (int i = 0; i < characters.Length; i++)
			{
				characters[i].SetActive(i == index);
			}
		}
	}
}
