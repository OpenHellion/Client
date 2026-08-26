using System.Collections.Generic;
using UnityEngine;

namespace ProBuilder2.Common
{
	[AddComponentMenu("")]
	[DisallowMultipleComponent]
	[ProGridsConditionalSnap]
	public class pb_PolyShape : MonoBehaviour
	{
		public enum PolyEditMode
		{
			None = 0,
			Path = 1,
			Height = 2,
			Edit = 3
		}

		public List<Vector3> points = new List<Vector3>();

		public float extrude;

		public PolyEditMode polyEditMode;

		public bool flipNormals;

		private pb_Object m_Mesh;

		public bool isOnGrid = true;

		public pb_Object mesh
		{
			get
			{
				if (m_Mesh == null)
				{
					m_Mesh = GetComponent<pb_Object>();
				}
				return m_Mesh;
			}
			set
			{
				m_Mesh = value;
			}
		}

		private bool IsSnapEnabled()
		{
			return isOnGrid;
		}
	}
}
