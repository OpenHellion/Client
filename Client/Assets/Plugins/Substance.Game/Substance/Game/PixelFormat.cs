using System;
using UnityEngine;

namespace Substance.Game
{
	public static class PixelFormat
	{
		public const int Substance_PF_BC = 1;

		public const int Substance_PF_PVRTC = 3;

		public const int Substance_PF_ETC = 3;

		public const int Substance_PF_RGBA = 0;

		public const int Substance_PF_RGB = 8;

		public const int Substance_PF_L = 12;

		public const int Substance_PF_BC1 = 1;

		public const int Substance_PF_BC2 = 9;

		public const int Substance_PF_BC3 = 17;

		public const int Substance_PF_BC4 = 25;

		public const int Substance_PF_BC5 = 21;

		public const int Substance_PF_DXT4 = 13;

		public const int Substance_PF_PVRTC2 = 3;

		public const int Substance_PF_PVRTC4 = 7;

		public const int Substance_PF_ETC1 = 11;

		public static TextureFormat GetUnityTextureFormat(int pSubstanceFormat)
		{
			switch (pSubstanceFormat)
			{
			case 0:
				return TextureFormat.RGBA32;
			case 8:
				return TextureFormat.RGB24;
			case 12:
				return TextureFormat.Alpha8;
			case 1:
				return TextureFormat.DXT1;
			case 17:
				return TextureFormat.DXT5;
			case 25:
				return TextureFormat.BC4;
			case 21:
				return TextureFormat.BC5;
			case 13:
				return TextureFormat.DXT5;
			case 11:
				return TextureFormat.ETC_RGB4;
			case 3:
				return TextureFormat.PVRTC_RGBA2;
			case 7:
				return TextureFormat.PVRTC_RGBA4;
			default:
				throw new ArgumentException();
			}
		}

		public static bool IsUnityTextureFormatCompressed(TextureFormat tf)
		{
			switch (tf)
			{
			case TextureFormat.Alpha8:
			case TextureFormat.ARGB4444:
			case TextureFormat.RGB24:
			case TextureFormat.RGBA32:
			case TextureFormat.ARGB32:
			case TextureFormat.RGB565:
			case TextureFormat.R16:
			case TextureFormat.RGBA4444:
			case TextureFormat.BGRA32:
			case TextureFormat.RHalf:
			case TextureFormat.RGHalf:
			case TextureFormat.RGBAHalf:
			case TextureFormat.RFloat:
			case TextureFormat.RGFloat:
			case TextureFormat.RGBAFloat:
			case TextureFormat.YUY2:
			case TextureFormat.RGB9e5Float:
			case TextureFormat.RG16:
			case TextureFormat.R8:
				return false;
			default:
				return true;
			}
		}

		public static int GetSubstanceTextureFormat(TextureFormat unityFormat)
		{
			switch (unityFormat)
			{
			case TextureFormat.RGBA32:
				return 0;
			case TextureFormat.RGB24:
				return 8;
			case TextureFormat.Alpha8:
				return 12;
			case TextureFormat.DXT1:
				return 1;
			case TextureFormat.DXT5:
				return 17;
			case TextureFormat.BC4:
				return 25;
			case TextureFormat.BC5:
				return 21;
			case TextureFormat.ETC_RGB4:
				return 11;
			case TextureFormat.PVRTC_RGBA2:
				return 3;
			case TextureFormat.PVRTC_RGBA4:
				return 7;
			default:
				throw new ArgumentException();
			}
		}

		public static int GenerateImageSizeWithMipMaps(int width, int height, int substanceFormat, int mipCount)
		{
			if (width == 0 || height == 0)
			{
				return 0;
			}
			int num = 0;
			for (int i = 0; i < mipCount; i++)
			{
				num += GenerateImageSize(width, height, substanceFormat);
				width >>= 1;
				height >>= 1;
				if (width < 1)
				{
					width = 1;
				}
				if (height < 1)
				{
					height = 1;
				}
			}
			return num;
		}

		public static int GenerateImageSize(int width, int height, int substanceFormat)
		{
			if (width <= 0 || height <= 0)
			{
				return 0;
			}
			switch (substanceFormat)
			{
			case 1:
			case 25:
				return (width + 3) / 4 * ((height + 3) / 4) * 8;
			case 9:
			case 13:
			case 17:
			case 21:
				return (width + 3) / 4 * ((height + 3) / 4) * 16;
			case 7:
				return (Math.Max(width, 8) * Math.Max(height, 8) * 4 + 7) / 8;
			case 3:
				return (Math.Max(width, 16) * Math.Max(height, 8) * 2 + 7) / 8;
			case 0:
				return width * height * 4;
			case 8:
				return width * height * 3;
			case 12:
				return width * height;
			default:
				throw new Exception();
			}
		}
	}
}
