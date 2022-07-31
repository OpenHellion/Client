using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "TutorialVideo", menuName = "Tutorial/Video")]
public class TutorialVideo : ScriptableObject
{
	public VideoClip Video;

	public string Name;
}
