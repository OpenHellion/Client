using System;
using UnityEngine;

[Serializable]
public class LightEmission
{
	public MeshRenderer renderer;

	public Color emissionColor;

	public void GetDefaultColor()
	{
		if (renderer != null)
		{
			if (renderer.material.shader.name == "Standard")
			{
				emissionColor = renderer.material.GetColor("_EmissionColor");
			}
			if (renderer.material.shader.name == "ZeroGravity/Surface/MultiMaterial")
			{
				emissionColor = renderer.material.GetColor("_EmColor");
			}
		}
	}

	public void SetColor(Color color)
	{
		if (renderer != null)
		{
			if (renderer.material.shader.name == "Standard")
			{
				renderer.material.SetColor("_EmissionColor", color);
			}
			if (renderer.material.shader.name == "ZeroGravity/Surface/MultiMaterial")
			{
				renderer.material.SetColor("_EmColor", color);
			}
		}
	}

	public void SetDefaultColor()
	{
		if (renderer != null)
		{
			if (renderer.material.shader.name == "Standard")
			{
				renderer.material.SetColor("_EmissionColor", emissionColor);
			}
			if (renderer.material.shader.name == "ZeroGravity/Surface/MultiMaterial")
			{
				renderer.material.SetColor("_EmColor", emissionColor);
			}
		}
	}
}
