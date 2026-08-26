using System.Collections.Generic;
using UnityEngine;

namespace ProBuilder2.Common
{
	[AddComponentMenu("")]
	[DisallowMultipleComponent]
	public class pb_BezierShape : MonoBehaviour
	{
		public List<pb_BezierPoint> m_Points = new List<pb_BezierPoint>();

		public bool m_CloseLoop;

		public float m_Radius = 0.5f;

		public int m_Rows = 8;

		public int m_Columns = 16;

		public bool m_Smooth = true;

		public bool m_IsEditing;

		private pb_Object m_Mesh;

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

		public void Init()
		{
			Vector3 vector = new Vector3(0f, 0f, 2f);
			Vector3 vector2 = new Vector3(3f, 0f, 0f);
			m_Points.Add(new pb_BezierPoint(Vector3.zero, -vector, vector, Quaternion.identity));
			m_Points.Add(new pb_BezierPoint(vector2, vector2 + vector, vector2 + -vector, Quaternion.identity));
		}

		public void Refresh()
		{
			pb_Object target = mesh;
			pb_Spline.Extrude(m_Points, m_Radius, m_Columns, m_Rows, m_CloseLoop, m_Smooth, ref target);
		}
	}
}
