using UnityEngine;

namespace UnityStandardAssets.CinematicEffects
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Image Effects/Anti-aliasing")]
	[ImageEffectAllowedInSceneView]
	public class AntiAliasing : MonoBehaviour
	{
		public enum Method
		{
			Smaa = 0,
			Fxaa = 1
		}

		[SerializeField]
		private SMAA m_SMAA = new SMAA();

		[SerializeField]
		private FXAA m_FXAA = new FXAA();

		[SerializeField]
		[HideInInspector]
		private int m_Method;

		private Camera m_Camera;

		public int method
		{
			get
			{
				return m_Method;
			}
			set
			{
				if (m_Method != value)
				{
					m_Method = value;
				}
			}
		}

		public IAntiAliasing current
		{
			get
			{
				if (method == 0)
				{
					return m_SMAA;
				}
				return m_FXAA;
			}
		}

		public Camera cameraComponent
		{
			get
			{
				if (m_Camera == null)
				{
					m_Camera = GetComponent<Camera>();
				}
				return m_Camera;
			}
		}

		private void OnEnable()
		{
			m_SMAA.OnEnable(this);
			m_FXAA.OnEnable(this);
		}

		private void OnDisable()
		{
			m_SMAA.OnDisable();
			m_FXAA.OnDisable();
		}

		private void OnPreCull()
		{
			current.OnPreCull(cameraComponent);
		}

		private void OnPostRender()
		{
			current.OnPostRender(cameraComponent);
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			current.OnRenderImage(cameraComponent, source, destination);
		}
	}
}
