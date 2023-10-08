using System;
using UnityEngine;

namespace DigitalOpus.MB.Core
{
	public class TextureBlenderFallback : TextureBlender
	{
		private bool m_doTintColor;

		private Color m_tintColor;

		private Color m_defaultColor = Color.white;

		public bool DoesShaderNameMatch(string shaderName)
		{
			return true;
		}

		public void OnBeforeTintTexture(Material sourceMat, string shaderTexturePropertyName)
		{
			if (shaderTexturePropertyName.Equals("_MainTex"))
			{
				m_doTintColor = true;
				m_tintColor = Color.white;
				if (sourceMat.HasProperty("_Color"))
				{
					m_tintColor = sourceMat.GetColor("_Color");
				}
			}
			else
			{
				m_doTintColor = false;
			}
		}

		public Color OnBlendTexturePixel(string shaderPropertyName, Color pixelColor)
		{
			if (m_doTintColor)
			{
				return new Color(pixelColor.r * m_tintColor.r, pixelColor.g * m_tintColor.g,
					pixelColor.b * m_tintColor.b, pixelColor.a * m_tintColor.a);
			}

			return pixelColor;
		}

		public bool NonTexturePropertiesAreEqual(Material a, Material b)
		{
			if (_compareColor(a, b, m_defaultColor, "_Color"))
			{
				return true;
			}

			return false;
		}

		public void SetNonTexturePropertyValuesOnResultMaterial(Material resultMaterial)
		{
			if (resultMaterial.HasProperty("_Color"))
			{
				resultMaterial.SetColor("_Color", m_defaultColor);
			}
		}

		public Color GetColorIfNoTexture(Material mat, ShaderTextureProperty texProperty)
		{
			if (texProperty.isNormalMap)
			{
				return new Color(0.5f, 0.5f, 1f);
			}

			if (texProperty.name.Equals("_MainTex"))
			{
				if (mat != null && mat.HasProperty("_Color"))
				{
					try
					{
						return mat.GetColor("_Color");
					}
					catch (Exception)
					{
					}
				}
			}
			else if (texProperty.name.Equals("_SpecGlossMap"))
			{
				if (mat != null && mat.HasProperty("_SpecColor"))
				{
					try
					{
						Color color = mat.GetColor("_SpecColor");
						if (mat.HasProperty("_Glossiness"))
						{
							try
							{
								color.a = mat.GetFloat("_Glossiness");
							}
							catch (Exception)
							{
							}
						}

						Debug.LogWarning(color);
						return color;
					}
					catch (Exception)
					{
					}
				}
			}
			else if (texProperty.name.Equals("_MetallicGlossMap"))
			{
				if (mat != null && mat.HasProperty("_Metallic"))
				{
					try
					{
						float @float = mat.GetFloat("_Metallic");
						Color result = new Color(@float, @float, @float);
						if (mat.HasProperty("_Glossiness"))
						{
							try
							{
								result.a = mat.GetFloat("_Glossiness");
							}
							catch (Exception)
							{
							}
						}

						return result;
					}
					catch (Exception)
					{
					}
				}
			}
			else
			{
				if (texProperty.name.Equals("_ParallaxMap"))
				{
					return new Color(0f, 0f, 0f, 0f);
				}

				if (texProperty.name.Equals("_OcclusionMap"))
				{
					return new Color(1f, 1f, 1f, 1f);
				}

				if (texProperty.name.Equals("_EmissionMap"))
				{
					if (mat != null && mat.HasProperty("_EmissionScaleUI"))
					{
						if (mat.HasProperty("_EmissionColor") && mat.HasProperty("_EmissionColorUI"))
						{
							try
							{
								Color color2 = mat.GetColor("_EmissionColor");
								Color color3 = mat.GetColor("_EmissionColorUI");
								float float2 = mat.GetFloat("_EmissionScaleUI");
								if (color2 == new Color(0f, 0f, 0f, 0f) && color3 == new Color(1f, 1f, 1f, 1f))
								{
									return new Color(float2, float2, float2, float2);
								}

								return color3;
							}
							catch (Exception)
							{
							}
						}
						else
						{
							try
							{
								float float3 = mat.GetFloat("_EmissionScaleUI");
								return new Color(float3, float3, float3, float3);
							}
							catch (Exception)
							{
							}
						}
					}
				}
				else if (texProperty.name.Equals("_DetailMask"))
				{
					return new Color(0f, 0f, 0f, 0f);
				}
			}

			return new Color(1f, 1f, 1f, 0f);
		}

		public static bool _compareColor(Material a, Material b, Color defaultVal, string propertyName)
		{
			Color color = defaultVal;
			Color color2 = defaultVal;
			if (a.HasProperty(propertyName))
			{
				color = a.GetColor(propertyName);
			}

			if (b.HasProperty(propertyName))
			{
				color2 = b.GetColor(propertyName);
			}

			if (color != color2)
			{
				return false;
			}

			return true;
		}

		public static bool _compareFloat(Material a, Material b, float defaultVal, string propertyName)
		{
			float num = defaultVal;
			float num2 = defaultVal;
			if (a.HasProperty(propertyName))
			{
				num = a.GetFloat(propertyName);
			}

			if (b.HasProperty(propertyName))
			{
				num2 = b.GetFloat(propertyName);
			}

			if (num != num2)
			{
				return false;
			}

			return true;
		}
	}
}
