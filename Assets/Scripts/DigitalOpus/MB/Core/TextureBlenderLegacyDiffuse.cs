using System;
using UnityEngine;

namespace DigitalOpus.MB.Core
{
	public class TextureBlenderLegacyDiffuse : TextureBlender
	{
		private bool doColor;

		private Color m_tintColor;

		private Color m_defaultTintColor = Color.white;

		public bool DoesShaderNameMatch(string shaderName)
		{
			if (shaderName.Equals("Legacy Shaders/Diffuse"))
			{
				return true;
			}

			if (shaderName.Equals("Diffuse"))
			{
				return true;
			}

			return false;
		}

		public void OnBeforeTintTexture(Material sourceMat, string shaderTexturePropertyName)
		{
			if (shaderTexturePropertyName.EndsWith("_MainTex"))
			{
				doColor = true;
				m_tintColor = sourceMat.GetColor("_Color");
			}
			else
			{
				doColor = false;
			}
		}

		public Color OnBlendTexturePixel(string propertyToDoshaderPropertyName, Color pixelColor)
		{
			if (doColor)
			{
				return new Color(pixelColor.r * m_tintColor.r, pixelColor.g * m_tintColor.g,
					pixelColor.b * m_tintColor.b, pixelColor.a * m_tintColor.a);
			}

			return pixelColor;
		}

		public bool NonTexturePropertiesAreEqual(Material a, Material b)
		{
			return TextureBlenderFallback._compareColor(a, b, m_defaultTintColor, "_Color");
		}

		public void SetNonTexturePropertyValuesOnResultMaterial(Material resultMaterial)
		{
			resultMaterial.SetColor("_Color", Color.white);
		}

		public Color GetColorIfNoTexture(Material m, ShaderTextureProperty texPropertyName)
		{
			if (texPropertyName.name.Equals("_MainTex") && m != null && m.HasProperty("_Color"))
			{
				try
				{
					return m.GetColor("_Color");
				}
				catch (Exception)
				{
				}
			}

			return new Color(1f, 1f, 1f, 0f);
		}
	}
}
