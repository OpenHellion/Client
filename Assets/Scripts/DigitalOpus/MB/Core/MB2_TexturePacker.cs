using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DigitalOpus.MB.Core
{
	public class MB2_TexturePacker
	{
		private class PixRect
		{
			public int x;

			public int y;

			public int w;

			public int h;

			public PixRect()
			{
			}

			public PixRect(int xx, int yy, int ww, int hh)
			{
				x = xx;
				y = yy;
				w = ww;
				h = hh;
			}
		}

		private class Image
		{
			public int imgId;

			public int w;

			public int h;

			public int x;

			public int y;

			public Image(int id, int tw, int th, int padding, int minImageSizeX, int minImageSizeY)
			{
				imgId = id;
				w = Mathf.Max(tw + padding * 2, minImageSizeX);
				h = Mathf.Max(th + padding * 2, minImageSizeY);
			}

			public Image(Image im)
			{
				imgId = im.imgId;
				w = im.w;
				h = im.h;
				x = im.x;
				y = im.y;
			}
		}

		private class ImgIDComparer : IComparer<Image>
		{
			public int Compare(Image x, Image y)
			{
				if (x.imgId > y.imgId)
				{
					return 1;
				}

				if (x.imgId == y.imgId)
				{
					return 0;
				}

				return -1;
			}
		}

		private class ImageHeightComparer : IComparer<Image>
		{
			public int Compare(Image x, Image y)
			{
				if (x.h > y.h)
				{
					return -1;
				}

				if (x.h == y.h)
				{
					return 0;
				}

				return 1;
			}
		}

		private class ImageWidthComparer : IComparer<Image>
		{
			public int Compare(Image x, Image y)
			{
				if (x.w > y.w)
				{
					return -1;
				}

				if (x.w == y.w)
				{
					return 0;
				}

				return 1;
			}
		}

		private class ImageAreaComparer : IComparer<Image>
		{
			public int Compare(Image x, Image y)
			{
				int num = x.w * x.h;
				int num2 = y.w * y.h;
				if (num > num2)
				{
					return -1;
				}

				if (num == num2)
				{
					return 0;
				}

				return 1;
			}
		}

		private class ProbeResult
		{
			public int w;

			public int h;

			public Node root;

			public bool fitsInMaxSize;

			public float efficiency;

			public float squareness;

			public void Set(int ww, int hh, Node r, bool fits, float e, float sq)
			{
				w = ww;
				h = hh;
				root = r;
				fitsInMaxSize = fits;
				efficiency = e;
				squareness = sq;
			}

			public float GetScore(bool doPowerOfTwoScore)
			{
				float num = ((!fitsInMaxSize) ? 0f : 1f);
				if (doPowerOfTwoScore)
				{
					return num * 2f + efficiency;
				}

				return squareness + 2f * efficiency + num;
			}
		}

		private class Node
		{
			public Node[] child = new Node[2];

			public PixRect r;

			public Image img;

			private bool isLeaf()
			{
				if (child[0] == null || child[1] == null)
				{
					return true;
				}

				return false;
			}

			public Node Insert(Image im, bool handed)
			{
				int num;
				int num2;
				if (handed)
				{
					num = 0;
					num2 = 1;
				}
				else
				{
					num = 1;
					num2 = 0;
				}

				if (!isLeaf())
				{
					Node node = child[num].Insert(im, handed);
					if (node != null)
					{
						return node;
					}

					return child[num2].Insert(im, handed);
				}

				if (img != null)
				{
					return null;
				}

				if (r.w < im.w || r.h < im.h)
				{
					return null;
				}

				if (r.w == im.w && r.h == im.h)
				{
					img = im;
					return this;
				}

				child[num] = new Node();
				child[num2] = new Node();
				int num3 = r.w - im.w;
				int num4 = r.h - im.h;
				if (num3 > num4)
				{
					child[num].r = new PixRect(r.x, r.y, im.w, r.h);
					child[num2].r = new PixRect(r.x + im.w, r.y, r.w - im.w, r.h);
				}
				else
				{
					child[num].r = new PixRect(r.x, r.y, r.w, im.h);
					child[num2].r = new PixRect(r.x, r.y + im.h, r.w, r.h - im.h);
				}

				return child[num].Insert(im, handed);
			}
		}

		public MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;

		private ProbeResult bestRoot;

		public bool doPowerOfTwoTextures = true;

		private static void printTree(Node r, string spc)
		{
			if (r.child[0] != null)
			{
				printTree(r.child[0], spc + "  ");
			}

			if (r.child[1] != null)
			{
				printTree(r.child[1], spc + "  ");
			}
		}

		private static void flattenTree(Node r, List<Image> putHere)
		{
			if (r.img != null)
			{
				r.img.x = r.r.x;
				r.img.y = r.r.y;
				putHere.Add(r.img);
			}

			if (r.child[0] != null)
			{
				flattenTree(r.child[0], putHere);
			}

			if (r.child[1] != null)
			{
				flattenTree(r.child[1], putHere);
			}
		}

		private static void drawGizmosNode(Node r)
		{
			Vector3 size = new Vector3(r.r.w, r.r.h, 0f);
			Vector3 center = new Vector3((float)r.r.x + size.x / 2f, (float)(-r.r.y) - size.y / 2f, 0f);
			Gizmos.DrawWireCube(center, size);
			if (r.img != null)
			{
				Gizmos.color = Color.blue;
				size = new Vector3(r.img.w, r.img.h, 0f);
				center = new Vector3((float)r.img.x + size.x / 2f, (float)(-r.img.y) - size.y / 2f, 0f);
				Gizmos.DrawCube(center, size);
			}

			if (r.child[0] != null)
			{
				Gizmos.color = Color.red;
				drawGizmosNode(r.child[0]);
			}

			if (r.child[1] != null)
			{
				Gizmos.color = Color.green;
				drawGizmosNode(r.child[1]);
			}
		}

		private static Texture2D createFilledTex(Color c, int w, int h)
		{
			Texture2D texture2D = new Texture2D(w, h);
			for (int i = 0; i < w; i++)
			{
				for (int j = 0; j < h; j++)
				{
					texture2D.SetPixel(i, j, c);
				}
			}

			texture2D.Apply();
			return texture2D;
		}

		public void DrawGizmos()
		{
			if (bestRoot != null)
			{
				drawGizmosNode(bestRoot.root);
			}
		}

		private bool Probe(Image[] imgsToAdd, int idealAtlasW, int idealAtlasH, float imgArea, int maxAtlasDim,
			ProbeResult pr)
		{
			Node node = new Node();
			node.r = new PixRect(0, 0, idealAtlasW, idealAtlasH);
			for (int i = 0; i < imgsToAdd.Length; i++)
			{
				Node node2 = node.Insert(imgsToAdd[i], false);
				if (node2 == null)
				{
					return false;
				}

				if (i != imgsToAdd.Length - 1)
				{
					continue;
				}

				int x = 0;
				int y = 0;
				GetExtent(node, ref x, ref y);
				bool flag;
				float num6;
				float num7;
				if (doPowerOfTwoTextures)
				{
					int num = Mathf.Min(CeilToNearestPowerOfTwo(x), maxAtlasDim);
					int num2 = Mathf.Min(CeilToNearestPowerOfTwo(y), maxAtlasDim);
					if (num2 < num / 2)
					{
						num2 = num / 2;
					}

					if (num < num2 / 2)
					{
						num = num2 / 2;
					}

					flag = x <= maxAtlasDim && y <= maxAtlasDim;
					float num3 = Mathf.Max(1f, (float)x / (float)maxAtlasDim);
					float num4 = Mathf.Max(1f, (float)y / (float)maxAtlasDim);
					float num5 = (float)num * num3 * (float)num2 * num4;
					num6 = 1f - (num5 - imgArea) / num5;
					num7 = 1f;
				}
				else
				{
					num6 = 1f - ((float)(x * y) - imgArea) / (float)(x * y);
					num7 = ((x >= y) ? ((float)y / (float)x) : ((float)x / (float)y));
					flag = x <= maxAtlasDim && y <= maxAtlasDim;
				}

				pr.Set(x, y, node, flag, num6, num7);
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("Probe success efficiency w=" + x + " h=" + y + " e=" + num6 + " sq=" + num7 +
					                 " fits=" + flag);
				}

				return true;
			}

			Debug.LogError("Should never get here.");
			return false;
		}

		private void GetExtent(Node r, ref int x, ref int y)
		{
			if (r.img != null)
			{
				if (r.r.x + r.img.w > x)
				{
					x = r.r.x + r.img.w;
				}

				if (r.r.y + r.img.h > y)
				{
					y = r.r.y + r.img.h;
				}
			}

			if (r.child[0] != null)
			{
				GetExtent(r.child[0], ref x, ref y);
			}

			if (r.child[1] != null)
			{
				GetExtent(r.child[1], ref x, ref y);
			}
		}

		private int StepWidthHeight(int oldVal, int step, int maxDim)
		{
			if (doPowerOfTwoTextures && oldVal < maxDim)
			{
				return oldVal * 2;
			}

			int num = oldVal + step;
			if (num > maxDim && oldVal < maxDim)
			{
				num = maxDim;
			}

			return num;
		}

		public int RoundToNearestPositivePowerOfTwo(int x)
		{
			int num = (int)Mathf.Pow(2f, Mathf.RoundToInt(Mathf.Log(x) / Mathf.Log(2f)));
			if (num == 0 || num == 1)
			{
				num = 2;
			}

			return num;
		}

		public int CeilToNearestPowerOfTwo(int x)
		{
			int num = (int)Mathf.Pow(2f, Mathf.Ceil(Mathf.Log(x) / Mathf.Log(2f)));
			if (num == 0 || num == 1)
			{
				num = 2;
			}

			return num;
		}

		public Rect[] GetRects(List<Vector2> imgWidthHeights, int maxDimension, int padding, out int outW, out int outH)
		{
			return _GetRects(imgWidthHeights, maxDimension, padding, 2 + padding * 2, 2 + padding * 2, 2 + padding * 2,
				2 + padding * 2, out outW, out outH, 0);
		}

		private Rect[] _GetRects(List<Vector2> imgWidthHeights, int maxDimension, int padding, int minImageSizeX,
			int minImageSizeY, int masterImageSizeX, int masterImageSizeY, out int outW, out int outH,
			int recursionDepth)
		{
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				Debug.Log(string.Format(
					"_GetRects numImages={0}, maxDimension={1}, padding={2}, minImageSizeX={3}, minImageSizeY={4}, masterImageSizeX={5}, masterImageSizeY={6}, recursionDepth={7}",
					imgWidthHeights.Count, maxDimension, padding, minImageSizeX, minImageSizeY, masterImageSizeX,
					masterImageSizeY, recursionDepth));
			}

			if (recursionDepth > 10)
			{
				Debug.LogError("Maximum recursion depth reached. Couldn't find packing for these textures.");
				outW = 0;
				outH = 0;
				return new Rect[0];
			}

			float num = 0f;
			int num2 = 0;
			int num3 = 0;
			Image[] array = new Image[imgWidthHeights.Count];
			for (int i = 0; i < array.Length; i++)
			{
				Image image = (array[i] = new Image(i, (int)imgWidthHeights[i].x, (int)imgWidthHeights[i].y, padding,
					minImageSizeX, minImageSizeY));
				num += (float)(image.w * image.h);
				num2 = Mathf.Max(num2, image.w);
				num3 = Mathf.Max(num3, image.h);
			}

			if ((float)num3 / (float)num2 > 2f)
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("Using height Comparer");
				}

				Array.Sort(array, new ImageHeightComparer());
			}
			else if ((double)((float)num3 / (float)num2) < 0.5)
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("Using width Comparer");
				}

				Array.Sort(array, new ImageWidthComparer());
			}
			else
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("Using area Comparer");
				}

				Array.Sort(array, new ImageAreaComparer());
			}

			int num4 = (int)Mathf.Sqrt(num);
			int num6;
			int num5;
			if (doPowerOfTwoTextures)
			{
				num6 = (num5 = RoundToNearestPositivePowerOfTwo(num4));
				if (num2 > num6)
				{
					num6 = CeilToNearestPowerOfTwo(num6);
				}

				if (num3 > num5)
				{
					num5 = CeilToNearestPowerOfTwo(num5);
				}
			}
			else
			{
				num6 = num4;
				num5 = num4;
				if (num2 > num4)
				{
					num6 = num2;
					num5 = Mathf.Max(Mathf.CeilToInt(num / (float)num2), num3);
				}

				if (num3 > num4)
				{
					num6 = Mathf.Max(Mathf.CeilToInt(num / (float)num3), num2);
					num5 = num3;
				}
			}

			if (num6 == 0)
			{
				num6 = 1;
			}

			if (num5 == 0)
			{
				num5 = 1;
			}

			int num7 = (int)((float)num6 * 0.15f);
			int num8 = (int)((float)num5 * 0.15f);
			if (num7 == 0)
			{
				num7 = 1;
			}

			if (num8 == 0)
			{
				num8 = 1;
			}

			int num9 = 2;
			int num10 = num6;
			int num11 = num5;
			while (num9 >= 1 && num11 < num4 * 1000)
			{
				bool flag = false;
				num9 = 0;
				num10 = num6;
				while (!flag && num10 < num4 * 1000)
				{
					ProbeResult probeResult = new ProbeResult();
					if (LOG_LEVEL >= MB2_LogLevel.trace)
					{
						Debug.Log("Probing h=" + num11 + " w=" + num10);
					}

					if (Probe(array, num10, num11, num, maxDimension, probeResult))
					{
						flag = true;
						if (bestRoot == null)
						{
							bestRoot = probeResult;
						}
						else if (probeResult.GetScore(doPowerOfTwoTextures) > bestRoot.GetScore(doPowerOfTwoTextures))
						{
							bestRoot = probeResult;
						}

						continue;
					}

					num9++;
					num10 = StepWidthHeight(num10, num7, maxDimension);
					if (LOG_LEVEL >= MB2_LogLevel.debug)
					{
						MB2_Log.LogDebug("increasing Width h=" + num11 + " w=" + num10);
					}
				}

				num11 = StepWidthHeight(num11, num8, maxDimension);
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("increasing Height h=" + num11 + " w=" + num10);
				}
			}

			outW = 0;
			outH = 0;
			if (doPowerOfTwoTextures)
			{
				outW = Mathf.Min(CeilToNearestPowerOfTwo(bestRoot.w), maxDimension);
				outH = Mathf.Min(CeilToNearestPowerOfTwo(bestRoot.h), maxDimension);
				if (outH < outW / 2)
				{
					outH = outW / 2;
				}

				if (outW < outH / 2)
				{
					outW = outH / 2;
				}
			}
			else
			{
				outW = bestRoot.w;
				outH = bestRoot.h;
			}

			if (bestRoot == null)
			{
				return null;
			}

			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug("Best fit found: atlasW=" + outW + " atlasH" + outH + " w=" + bestRoot.w + " h=" +
				                 bestRoot.h + " efficiency=" + bestRoot.efficiency + " squareness=" +
				                 bestRoot.squareness + " fits in max dimension=" + bestRoot.fitsInMaxSize);
			}

			List<Image> list = new List<Image>();
			flattenTree(bestRoot.root, list);
			list.Sort(new ImgIDComparer());
			if (list.Count != array.Length)
			{
				Debug.LogError("Result images not the same lentgh as source");
			}

			int minImageSizeX2 = minImageSizeX;
			int minImageSizeY2 = minImageSizeY;
			bool flag2 = false;
			float num12 = (float)padding / (float)outW;
			if (bestRoot.w > maxDimension)
			{
				num12 = (float)padding / (float)maxDimension;
				float num13 = (float)maxDimension / (float)bestRoot.w;
				if (LOG_LEVEL >= MB2_LogLevel.warn)
				{
					Debug.LogWarning("Packing exceeded atlas width shrinking to " + num13);
				}

				for (int j = 0; j < list.Count; j++)
				{
					Image image2 = list[j];
					if ((float)image2.w * num13 < (float)masterImageSizeX)
					{
						if (LOG_LEVEL >= MB2_LogLevel.debug)
						{
							Debug.Log(
								"Small images are being scaled to zero. Will need to redo packing with larger minTexSizeX.");
						}

						flag2 = true;
						minImageSizeX2 = Mathf.CeilToInt((float)minImageSizeX / num13);
					}

					int num14 = (int)((float)(image2.x + image2.w) * num13);
					image2.x = (int)(num13 * (float)image2.x);
					image2.w = num14 - image2.x;
				}

				outW = maxDimension;
			}

			float num15 = (float)padding / (float)outH;
			if (bestRoot.h > maxDimension)
			{
				num15 = (float)padding / (float)maxDimension;
				float num16 = (float)maxDimension / (float)bestRoot.h;
				if (LOG_LEVEL >= MB2_LogLevel.warn)
				{
					Debug.LogWarning("Packing exceeded atlas height shrinking to " + num16);
				}

				for (int k = 0; k < list.Count; k++)
				{
					Image image3 = list[k];
					if ((float)image3.h * num16 < (float)masterImageSizeY)
					{
						if (LOG_LEVEL >= MB2_LogLevel.debug)
						{
							Debug.Log(
								"Small images are being scaled to zero. Will need to redo packing with larger minTexSizeY.");
						}

						flag2 = true;
						minImageSizeY2 = Mathf.CeilToInt((float)minImageSizeY / num16);
					}

					int num17 = (int)((float)(image3.y + image3.h) * num16);
					image3.y = (int)(num16 * (float)image3.y);
					image3.h = num17 - image3.y;
				}

				outH = maxDimension;
			}

			Rect[] array2;
			if (!flag2)
			{
				array2 = new Rect[list.Count];
				for (int l = 0; l < list.Count; l++)
				{
					Image image4 = list[l];
					Rect rect = (array2[l] = new Rect((float)image4.x / (float)outW + num12,
						(float)image4.y / (float)outH + num15, (float)image4.w / (float)outW - num12 * 2f,
						(float)image4.h / (float)outH - num15 * 2f));
					if (LOG_LEVEL >= MB2_LogLevel.debug)
					{
						MB2_Log.LogDebug("Image: " + l + " imgID=" + image4.imgId + " x=" + rect.x * (float)outW +
						                 " y=" + rect.y * (float)outH + " w=" + rect.width * (float)outW + " h=" +
						                 rect.height * (float)outH + " padding=" + padding);
					}
				}
			}
			else
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					Debug.Log("==================== REDOING PACKING ================");
				}

				bestRoot = null;
				array2 = _GetRects(imgWidthHeights, maxDimension, padding, minImageSizeX2, minImageSizeY2,
					masterImageSizeX, masterImageSizeY, out outW, out outH, recursionDepth + 1);
			}

			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug("Done GetRects");
			}

			return array2;
		}

		public void RunTestHarness()
		{
			int num = 32;
			int min = 126;
			int max = 2046;
			List<Vector2> list = new List<Vector2>();
			for (int i = 0; i < num; i++)
			{
				list.Add(new Vector2(Random.Range(min, max), Random.Range(min, max) * 5));
			}

			doPowerOfTwoTextures = true;
			LOG_LEVEL = MB2_LogLevel.trace;
			int padding = 1;
			int outW;
			int outH;
			GetRects(list, 4096, padding, out outW, out outH);
		}
	}
}
