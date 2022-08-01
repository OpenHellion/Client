using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Network;
using ZeroGravity.Objects;

public class GlowStick : Item
{
	public bool isOn;

	public Material Mat;

	private Color finalColor;

	public Light GlowLight;

	private new void Start()
	{
		base.Start();
		finalColor = Color.green * Mathf.LinearToGammaSpace(0.1f);
		GlowLight.enabled = false;
	}

	public override bool PrimaryFunction()
	{
		isOn = true;
		return false;
	}

	public override DynamicObjectAuxData GetAuxData()
	{
		GlowStickData baseAuxData = GetBaseAuxData<GlowStickData>();
		baseAuxData.IsOn = isOn;
		return baseAuxData;
	}

	public override void ProcesStatsData(DynamicObjectStats dos)
	{
		base.ProcesStatsData(dos);
		isOn = true;
		finalColor = Color.green * Mathf.LinearToGammaSpace(10f);
		GlowLight.enabled = true;
		Mat.SetColor("_EmissionColor", finalColor);
	}
}
