using UnityEngine;

public abstract class LightOverride : MonoBehaviour
{
	public enum Type
	{
		None = 0,
		Point = 1,
		Tube = 2,
		Area = 3,
		Directional = 4
	}

	[Header("Overrides")]
	public float m_IntensityMult = 1f;

	[MinValue(0f)]
	public float m_RangeMult = 1f;

	private Type m_Type;

	private bool m_Initialized;

	private Light m_Light;

	private TubeLight m_TubeLight;

	private AreaLight m_AreaLight;

	public bool isOn
	{
		get
		{
			if (!base.isActiveAndEnabled)
			{
				return false;
			}
			Init();
			switch (m_Type)
			{
			case Type.Point:
				return m_Light.enabled || GetForceOn();
			case Type.Tube:
				return m_TubeLight.enabled || GetForceOn();
			case Type.Area:
				return m_AreaLight.enabled || GetForceOn();
			case Type.Directional:
				return m_Light.enabled || GetForceOn();
			default:
				return false;
			}
		}
		private set
		{
		}
	}

	public new Light light
	{
		get
		{
			Init();
			return m_Light;
		}
		private set
		{
		}
	}

	public TubeLight tubeLight
	{
		get
		{
			Init();
			return m_TubeLight;
		}
		private set
		{
		}
	}

	public AreaLight areaLight
	{
		get
		{
			Init();
			return m_AreaLight;
		}
		private set
		{
		}
	}

	public Type type
	{
		get
		{
			Init();
			return m_Type;
		}
		private set
		{
		}
	}

	private void Update()
	{
	}

	public abstract bool GetForceOn();

	private void Init()
	{
		if (m_Initialized)
		{
			return;
		}
		if ((m_Light = GetComponent<Light>()) != null)
		{
			switch (m_Light.type)
			{
			case LightType.Point:
				m_Type = Type.Point;
				break;
			case LightType.Directional:
				m_Type = Type.Directional;
				break;
			default:
				m_Type = Type.None;
				break;
			}
		}
		else if ((m_TubeLight = GetComponent<TubeLight>()) != null)
		{
			m_Type = Type.Tube;
		}
		else if ((m_AreaLight = GetComponent<AreaLight>()) != null)
		{
			m_Type = Type.Area;
		}
		m_Initialized = true;
	}
}
