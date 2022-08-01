using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Data;

namespace ZeroGravity.LevelDesign
{
	public class SceneObjectTag : MonoBehaviour
	{
		public TagAction TagAction;

		[ContextMenuItem("Test", "Test")]
		public List<SceneTagObject> Tags;

		[Tooltip("If true same tag will be used for child executers")]
		public bool OverrideChildTags = true;

		public void Test()
		{
		}
	}
}
