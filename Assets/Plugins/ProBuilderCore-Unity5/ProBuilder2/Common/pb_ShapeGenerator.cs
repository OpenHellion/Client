using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ProBuilder2.Common
{
	public class pb_ShapeGenerator
	{
		private static readonly Vector3[] ICOSAHEDRON_VERTICES = new Vector3[12]
		{
			new Vector3(-1f, 1.618034f, 0f),
			new Vector3(1f, 1.618034f, 0f),
			new Vector3(-1f, -1.618034f, 0f),
			new Vector3(1f, -1.618034f, 0f),
			new Vector3(0f, -1f, 1.618034f),
			new Vector3(0f, 1f, 1.618034f),
			new Vector3(0f, -1f, -1.618034f),
			new Vector3(0f, 1f, -1.618034f),
			new Vector3(1.618034f, 0f, -1f),
			new Vector3(1.618034f, 0f, 1f),
			new Vector3(-1.618034f, 0f, -1f),
			new Vector3(-1.618034f, 0f, 1f)
		};

		private static readonly int[] ICOSAHEDRON_TRIANGLES = new int[60]
		{
			0, 11, 5, 0, 5, 1, 0, 1, 7, 0,
			7, 10, 0, 10, 11, 1, 5, 9, 5, 11,
			4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
			3, 9, 4, 3, 4, 2, 3, 2, 6, 3,
			6, 8, 3, 8, 9, 4, 9, 5, 2, 4,
			11, 6, 2, 10, 8, 6, 7, 9, 8, 1
		};

		public static pb_Object StairGenerator(Vector3 size, int steps, bool buildSides)
		{
			Vector3[] array = new Vector3[4 * steps * 2];
			pb_Face[] array2 = new pb_Face[steps * 2];
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < steps; i++)
			{
				float num3 = (float)i / (float)steps;
				float num4 = (float)(i + 1) / (float)steps;
				float x = size.x;
				float x2 = 0f;
				float y = size.y * num3;
				float y2 = size.y * num4;
				float z = size.z * num3;
				float z2 = size.z * num4;
				ref Vector3 reference = ref array[num];
				reference = new Vector3(x, y, z);
				ref Vector3 reference2 = ref array[num + 1];
				reference2 = new Vector3(x2, y, z);
				ref Vector3 reference3 = ref array[num + 2];
				reference3 = new Vector3(x, y2, z);
				ref Vector3 reference4 = ref array[num + 3];
				reference4 = new Vector3(x2, y2, z);
				ref Vector3 reference5 = ref array[num + 4];
				reference5 = new Vector3(x, y2, z);
				ref Vector3 reference6 = ref array[num + 5];
				reference6 = new Vector3(x2, y2, z);
				ref Vector3 reference7 = ref array[num + 6];
				reference7 = new Vector3(x, y2, z2);
				ref Vector3 reference8 = ref array[num + 7];
				reference8 = new Vector3(x2, y2, z2);
				array2[num2] = new pb_Face(new int[6]
				{
					num,
					num + 1,
					num + 2,
					num + 1,
					num + 3,
					num + 2
				});
				array2[num2 + 1] = new pb_Face(new int[6]
				{
					num + 4,
					num + 5,
					num + 6,
					num + 5,
					num + 7,
					num + 6
				});
				num += 8;
				num2 += 2;
			}
			if (buildSides)
			{
				float num5 = 0f;
				for (int j = 0; j < 2; j++)
				{
					Vector3[] array3 = new Vector3[steps * 4 + (steps - 1) * 3];
					pb_Face[] array4 = new pb_Face[steps + steps - 1];
					int num6 = 0;
					int num7 = 0;
					for (int k = 0; k < steps; k++)
					{
						float y3 = (float)Mathf.Max(k, 1) / (float)steps * size.y;
						float y4 = (float)(k + 1) / (float)steps * size.y;
						float z3 = (float)k / (float)steps * size.z;
						float z4 = (float)(k + 1) / (float)steps * size.z;
						ref Vector3 reference9 = ref array3[num6];
						reference9 = new Vector3(num5, 0f, z3);
						ref Vector3 reference10 = ref array3[num6 + 1];
						reference10 = new Vector3(num5, 0f, z4);
						ref Vector3 reference11 = ref array3[num6 + 2];
						reference11 = new Vector3(num5, y3, z3);
						ref Vector3 reference12 = ref array3[num6 + 3];
						reference12 = new Vector3(num5, y4, z4);
						array4[num7++] = new pb_Face((j % 2 == 0) ? new int[6]
						{
							num,
							num + 1,
							num + 2,
							num + 1,
							num + 3,
							num + 2
						} : new int[6]
						{
							num + 2,
							num + 1,
							num,
							num + 2,
							num + 3,
							num + 1
						});
						array4[num7 - 1].textureGroup = j + 1;
						num += 4;
						num6 += 4;
						if (k > 0)
						{
							ref Vector3 reference13 = ref array3[num6];
							reference13 = new Vector3(num5, y3, z3);
							ref Vector3 reference14 = ref array3[num6 + 1];
							reference14 = new Vector3(num5, y4, z3);
							ref Vector3 reference15 = ref array3[num6 + 2];
							reference15 = new Vector3(num5, y4, z4);
							array4[num7++] = new pb_Face((j % 2 == 0) ? new int[3]
							{
								num + 2,
								num + 1,
								num
							} : new int[3]
							{
								num,
								num + 1,
								num + 2
							});
							array4[num7 - 1].textureGroup = j + 1;
							num += 3;
							num6 += 3;
						}
					}
					array = array.Concat(array3);
					array2 = array2.Concat(array4);
					num5 += size.x;
				}
				array = array.Concat(new Vector3[4]
				{
					new Vector3(0f, 0f, size.z),
					new Vector3(size.x, 0f, size.z),
					new Vector3(0f, size.y, size.z),
					new Vector3(size.x, size.y, size.z)
				});
				array2 = array2.Add(new pb_Face(new int[6]
				{
					num,
					num + 1,
					num + 2,
					num + 1,
					num + 3,
					num + 2
				}));
			}
			pb_Object pb_Object = pb_Object.CreateInstanceWithVerticesFaces(array, array2);
			pb_Object.gameObject.name = "Stairs";
			return pb_Object;
		}

		public static pb_Object CurvedStairGenerator(float stairWidth, float height, float innerRadius, float circumference, int steps, bool buildSides)
		{
			bool flag = innerRadius < Mathf.Epsilon;
			Vector3[] array = new Vector3[4 * steps + ((!flag) ? 4 : 3) * steps];
			pb_Face[] array2 = new pb_Face[steps * 2];
			int num = 0;
			int num2 = 0;
			float num3 = Mathf.Abs(circumference) * ((float)Math.PI / 180f);
			float num4 = innerRadius + stairWidth;
			for (int i = 0; i < steps; i++)
			{
				float num5 = (float)i / (float)steps * num3;
				float num6 = (float)(i + 1) / (float)steps * num3;
				float y = (float)i / (float)steps * height;
				float y2 = (float)(i + 1) / (float)steps * height;
				Vector3 vector = new Vector3(0f - Mathf.Cos(num5), 0f, Mathf.Sin(num5));
				Vector3 vector2 = new Vector3(0f - Mathf.Cos(num6), 0f, Mathf.Sin(num6));
				ref Vector3 reference = ref array[num];
				reference = vector * innerRadius;
				ref Vector3 reference2 = ref array[num + 1];
				reference2 = vector * num4;
				ref Vector3 reference3 = ref array[num + 2];
				reference3 = vector * innerRadius;
				ref Vector3 reference4 = ref array[num + 3];
				reference4 = vector * num4;
				array[num].y = y;
				array[num + 1].y = y;
				array[num + 2].y = y2;
				array[num + 3].y = y2;
				ref Vector3 reference5 = ref array[num + 4];
				reference5 = array[num + 2];
				ref Vector3 reference6 = ref array[num + 5];
				reference6 = array[num + 3];
				ref Vector3 reference7 = ref array[num + 6];
				reference7 = vector2 * num4;
				array[num + 6].y = y2;
				if (!flag)
				{
					ref Vector3 reference8 = ref array[num + 7];
					reference8 = vector2 * innerRadius;
					array[num + 7].y = y2;
				}
				array2[num2] = new pb_Face(new int[6]
				{
					num,
					num + 1,
					num + 2,
					num + 1,
					num + 3,
					num + 2
				});
				if (flag)
				{
					array2[num2 + 1] = new pb_Face(new int[3]
					{
						num + 4,
						num + 5,
						num + 6
					});
				}
				else
				{
					array2[num2 + 1] = new pb_Face(new int[6]
					{
						num + 4,
						num + 5,
						num + 6,
						num + 4,
						num + 6,
						num + 7
					});
				}
				float num7 = (num6 + num5) * -0.5f * 57.29578f;
				num7 %= 360f;
				if (num7 < 0f)
				{
					num7 = 360f + num7;
				}
				array2[num2 + 1].uv.rotation = num7;
				num += ((!flag) ? 8 : 7);
				num2 += 2;
			}
			if (buildSides)
			{
				float num8 = ((!flag) ? innerRadius : (innerRadius + stairWidth));
				for (int j = (flag ? 1 : 0); j < 2; j++)
				{
					Vector3[] array3 = new Vector3[steps * 4 + (steps - 1) * 3];
					pb_Face[] array4 = new pb_Face[steps + steps - 1];
					int num9 = 0;
					int num10 = 0;
					for (int k = 0; k < steps; k++)
					{
						float f = (float)k / (float)steps * num3;
						float f2 = (float)(k + 1) / (float)steps * num3;
						float y3 = (float)Mathf.Max(k, 1) / (float)steps * height;
						float y4 = (float)(k + 1) / (float)steps * height;
						Vector3 vector3 = new Vector3(0f - Mathf.Cos(f), 0f, Mathf.Sin(f)) * num8;
						Vector3 vector4 = new Vector3(0f - Mathf.Cos(f2), 0f, Mathf.Sin(f2)) * num8;
						array3[num9] = vector3;
						array3[num9 + 1] = vector4;
						array3[num9 + 2] = vector3;
						array3[num9 + 3] = vector4;
						array3[num9].y = 0f;
						array3[num9 + 1].y = 0f;
						array3[num9 + 2].y = y3;
						array3[num9 + 3].y = y4;
						array4[num10++] = new pb_Face((j % 2 == 0) ? new int[6]
						{
							num + 2,
							num + 1,
							num,
							num + 2,
							num + 3,
							num + 1
						} : new int[6]
						{
							num,
							num + 1,
							num + 2,
							num + 1,
							num + 3,
							num + 2
						});
						array4[num10 - 1].smoothingGroup = j + 1;
						num += 4;
						num9 += 4;
						if (k > 0)
						{
							array4[num10 - 1].textureGroup = j * steps + k;
							array3[num9] = vector3;
							array3[num9 + 1] = vector4;
							array3[num9 + 2] = vector3;
							array3[num9].y = y3;
							array3[num9 + 1].y = y4;
							array3[num9 + 2].y = y4;
							array4[num10++] = new pb_Face((j % 2 == 0) ? new int[3]
							{
								num + 2,
								num + 1,
								num
							} : new int[3]
							{
								num,
								num + 1,
								num + 2
							});
							array4[num10 - 1].textureGroup = j * steps + k;
							array4[num10 - 1].smoothingGroup = j + 1;
							num += 3;
							num9 += 3;
						}
					}
					array = array.Concat(array3);
					array2 = array2.Concat(array4);
					num8 += stairWidth;
				}
				float num11 = 0f - Mathf.Cos(num3);
				float num12 = Mathf.Sin(num3);
				array = array.Concat(new Vector3[4]
				{
					new Vector3(num11, 0f, num12) * innerRadius,
					new Vector3(num11, 0f, num12) * num4,
					new Vector3(num11 * innerRadius, height, num12 * innerRadius),
					new Vector3(num11 * num4, height, num12 * num4)
				});
				array2 = array2.Add(new pb_Face(new int[6]
				{
					num + 2,
					num + 1,
					num,
					num + 2,
					num + 3,
					num + 1
				}));
			}
			if (circumference < 0f)
			{
				Vector3 scale = new Vector3(-1f, 1f, 1f);
				for (int l = 0; l < array.Length; l++)
				{
					array[l].Scale(scale);
				}
				pb_Face[] array5 = array2;
				foreach (pb_Face pb_Face2 in array5)
				{
					pb_Face2.ReverseIndices();
				}
			}
			pb_Object pb_Object = pb_Object.CreateInstanceWithVerticesFaces(array, array2);
			pb_Object.gameObject.name = "Stairs";
			return pb_Object;
		}

		public static pb_Object StairGenerator(int steps, float width, float height, float depth, bool sidesGoToFloor, bool generateBack, bool platformsOnly)
		{
			int num = 0;
			List<Vector3> list = new List<Vector3>();
			Vector3[] array = ((!platformsOnly) ? new Vector3[16] : new Vector3[8]);
			float num2 = height / (float)steps;
			float num3 = depth / (float)steps;
			float num4 = num2;
			for (num = 0; num < steps; num++)
			{
				float num5 = width / 2f;
				float y = (float)num * num2;
				float num6 = (float)num * num3;
				if (sidesGoToFloor)
				{
					y = 0f;
				}
				num4 = (float)num * num2 + num2;
				ref Vector3 reference = ref array[0];
				reference = new Vector3(num5, (float)num * num2, num6);
				ref Vector3 reference2 = ref array[1];
				reference2 = new Vector3(0f - num5, (float)num * num2, num6);
				ref Vector3 reference3 = ref array[2];
				reference3 = new Vector3(num5, num4, num6);
				ref Vector3 reference4 = ref array[3];
				reference4 = new Vector3(0f - num5, num4, num6);
				ref Vector3 reference5 = ref array[4];
				reference5 = new Vector3(num5, num4, num6);
				ref Vector3 reference6 = ref array[5];
				reference6 = new Vector3(0f - num5, num4, num6);
				ref Vector3 reference7 = ref array[6];
				reference7 = new Vector3(num5, num4, num6 + num3);
				ref Vector3 reference8 = ref array[7];
				reference8 = new Vector3(0f - num5, num4, num6 + num3);
				if (!platformsOnly)
				{
					ref Vector3 reference9 = ref array[8];
					reference9 = new Vector3(num5, y, num6 + num3);
					ref Vector3 reference10 = ref array[9];
					reference10 = new Vector3(num5, y, num6);
					ref Vector3 reference11 = ref array[10];
					reference11 = new Vector3(num5, num4, num6 + num3);
					ref Vector3 reference12 = ref array[11];
					reference12 = new Vector3(num5, num4, num6);
					ref Vector3 reference13 = ref array[12];
					reference13 = new Vector3(0f - num5, y, num6);
					ref Vector3 reference14 = ref array[13];
					reference14 = new Vector3(0f - num5, y, num6 + num3);
					ref Vector3 reference15 = ref array[14];
					reference15 = new Vector3(0f - num5, num4, num6);
					ref Vector3 reference16 = ref array[15];
					reference16 = new Vector3(0f - num5, num4, num6 + num3);
				}
				list.AddRange(array);
			}
			if (generateBack)
			{
				list.Add(new Vector3((0f - width) / 2f, 0f, depth));
				list.Add(new Vector3(width / 2f, 0f, depth));
				list.Add(new Vector3((0f - width) / 2f, height, depth));
				list.Add(new Vector3(width / 2f, height, depth));
			}
			pb_Object pb_Object = pb_Object.CreateInstanceWithPoints(list.ToArray());
			pb_Object.gameObject.name = "Stairs";
			return pb_Object;
		}

		public static pb_Object CubeGenerator(Vector3 size)
		{
			Vector3[] array = new Vector3[pb_Constant.TRIANGLES_CUBE.Length];
			for (int i = 0; i < pb_Constant.TRIANGLES_CUBE.Length; i++)
			{
				ref Vector3 reference = ref array[i];
				reference = Vector3.Scale(pb_Constant.VERTICES_CUBE[pb_Constant.TRIANGLES_CUBE[i]], size);
			}
			pb_Object pb_Object = pb_Object.CreateInstanceWithPoints(array);
			pb_Object.gameObject.name = "Cube";
			return pb_Object;
		}

		public static pb_Object CylinderGenerator(int axisDivisions, float radius, float height, int heightCuts, int smoothing = -1)
		{
			if (axisDivisions % 2 != 0)
			{
				axisDivisions++;
			}
			if (axisDivisions > 64)
			{
				axisDivisions = 64;
			}
			float num = 360f / (float)axisDivisions;
			float num2 = height / (float)(heightCuts + 1);
			Vector3[] array = new Vector3[axisDivisions];
			for (int i = 0; i < axisDivisions; i++)
			{
				float f = num * (float)i * ((float)Math.PI / 180f);
				float x = Mathf.Cos(f) * radius;
				float z = Mathf.Sin(f) * radius;
				ref Vector3 reference = ref array[i];
				reference = new Vector3(x, 0f, z);
			}
			Vector3[] array2 = new Vector3[axisDivisions * (heightCuts + 1) * 4 + axisDivisions * 6];
			pb_Face[] array3 = new pb_Face[axisDivisions * (heightCuts + 1) + axisDivisions * 2];
			int num3 = 0;
			for (int j = 0; j < heightCuts + 1; j++)
			{
				float y = (float)j * num2;
				float y2 = (float)(j + 1) * num2;
				for (int k = 0; k < axisDivisions; k++)
				{
					ref Vector3 reference2 = ref array2[num3];
					reference2 = new Vector3(array[k].x, y, array[k].z);
					ref Vector3 reference3 = ref array2[num3 + 1];
					reference3 = new Vector3(array[k].x, y2, array[k].z);
					if (k != axisDivisions - 1)
					{
						ref Vector3 reference4 = ref array2[num3 + 2];
						reference4 = new Vector3(array[k + 1].x, y, array[k + 1].z);
						ref Vector3 reference5 = ref array2[num3 + 3];
						reference5 = new Vector3(array[k + 1].x, y2, array[k + 1].z);
					}
					else
					{
						ref Vector3 reference6 = ref array2[num3 + 2];
						reference6 = new Vector3(array[0].x, y, array[0].z);
						ref Vector3 reference7 = ref array2[num3 + 3];
						reference7 = new Vector3(array[0].x, y2, array[0].z);
					}
					num3 += 4;
				}
			}
			int num4 = 0;
			for (int l = 0; l < heightCuts + 1; l++)
			{
				for (int m = 0; m < axisDivisions * 4; m += 4)
				{
					int num5 = l * (axisDivisions * 4) + m;
					int num6 = num5;
					int num7 = num5 + 1;
					int num8 = num5 + 2;
					int num9 = num5 + 3;
					array3[num4++] = new pb_Face(new int[6] { num6, num7, num8, num7, num9, num8 }, pb_Constant.DefaultMaterial, new pb_UV(), smoothing, -1, -1, manualUV: false);
				}
			}
			int num10 = axisDivisions * (heightCuts + 1) * 4;
			int num11 = axisDivisions * (heightCuts + 1);
			for (int n = 0; n < axisDivisions; n++)
			{
				ref Vector3 reference8 = ref array2[num10];
				reference8 = new Vector3(array[n].x, 0f, array[n].z);
				ref Vector3 reference9 = ref array2[num10 + 1];
				reference9 = Vector3.zero;
				if (n != axisDivisions - 1)
				{
					ref Vector3 reference10 = ref array2[num10 + 2];
					reference10 = new Vector3(array[n + 1].x, 0f, array[n + 1].z);
				}
				else
				{
					ref Vector3 reference11 = ref array2[num10 + 2];
					reference11 = new Vector3(array[0].x, 0f, array[0].z);
				}
				array3[num11 + n] = new pb_Face(new int[3]
				{
					num10 + 2,
					num10 + 1,
					num10
				});
				num10 += 3;
				ref Vector3 reference12 = ref array2[num10];
				reference12 = new Vector3(array[n].x, height, array[n].z);
				ref Vector3 reference13 = ref array2[num10 + 1];
				reference13 = new Vector3(0f, height, 0f);
				if (n != axisDivisions - 1)
				{
					ref Vector3 reference14 = ref array2[num10 + 2];
					reference14 = new Vector3(array[n + 1].x, height, array[n + 1].z);
				}
				else
				{
					ref Vector3 reference15 = ref array2[num10 + 2];
					reference15 = new Vector3(array[0].x, height, array[0].z);
				}
				array3[num11 + (n + axisDivisions)] = new pb_Face(new int[3]
				{
					num10,
					num10 + 1,
					num10 + 2
				});
				num10 += 3;
			}
			pb_Object pb_Object = pb_Object.CreateInstanceWithVerticesFaces(array2, array3);
			pb_Object.gameObject.name = "Cylinder";
			return pb_Object;
		}

		public static pb_Object PrismGenerator(Vector3 size)
		{
			size.y *= 2f;
			Vector3[] array = new Vector3[6]
			{
				Vector3.Scale(new Vector3(-0.5f, 0f, -0.5f), size),
				Vector3.Scale(new Vector3(0.5f, 0f, -0.5f), size),
				Vector3.Scale(new Vector3(0f, 0.5f, -0.5f), size),
				Vector3.Scale(new Vector3(-0.5f, 0f, 0.5f), size),
				Vector3.Scale(new Vector3(0.5f, 0f, 0.5f), size),
				Vector3.Scale(new Vector3(0f, 0.5f, 0.5f), size)
			};
			Vector3[] v = new Vector3[18]
			{
				array[0],
				array[1],
				array[2],
				array[1],
				array[4],
				array[2],
				array[5],
				array[4],
				array[3],
				array[5],
				array[3],
				array[0],
				array[5],
				array[2],
				array[0],
				array[1],
				array[3],
				array[4]
			};
			pb_Face[] f = new pb_Face[5]
			{
				new pb_Face(new int[3] { 2, 1, 0 }),
				new pb_Face(new int[6] { 5, 4, 3, 5, 6, 4 }),
				new pb_Face(new int[3] { 9, 8, 7 }),
				new pb_Face(new int[6] { 12, 11, 10, 12, 13, 11 }),
				new pb_Face(new int[6] { 14, 15, 16, 15, 17, 16 })
			};
			pb_Object pb_Object = pb_Object.CreateInstanceWithVerticesFaces(v, f);
			pb_Object.gameObject.name = "Prism";
			return pb_Object;
		}

		public static pb_Object DoorGenerator(float totalWidth, float totalHeight, float ledgeHeight, float legWidth, float depth)
		{
			float num = totalWidth / 2f;
			legWidth = num - legWidth;
			ledgeHeight = totalHeight - ledgeHeight;
			Vector3[] array = new Vector3[12]
			{
				new Vector3(0f - num, 0f, depth),
				new Vector3(0f - legWidth, 0f, depth),
				new Vector3(legWidth, 0f, depth),
				new Vector3(num, 0f, depth),
				new Vector3(0f - num, ledgeHeight, depth),
				new Vector3(0f - legWidth, ledgeHeight, depth),
				new Vector3(legWidth, ledgeHeight, depth),
				new Vector3(num, ledgeHeight, depth),
				new Vector3(0f - num, totalHeight, depth),
				new Vector3(0f - legWidth, totalHeight, depth),
				new Vector3(legWidth, totalHeight, depth),
				new Vector3(num, totalHeight, depth)
			};
			List<Vector3> list = new List<Vector3>();
			list.Add(array[0]);
			list.Add(array[1]);
			list.Add(array[4]);
			list.Add(array[5]);
			list.Add(array[2]);
			list.Add(array[3]);
			list.Add(array[6]);
			list.Add(array[7]);
			list.Add(array[4]);
			list.Add(array[5]);
			list.Add(array[8]);
			list.Add(array[9]);
			list.Add(array[6]);
			list.Add(array[7]);
			list.Add(array[10]);
			list.Add(array[11]);
			list.Add(array[5]);
			list.Add(array[6]);
			list.Add(array[9]);
			list.Add(array[10]);
			List<Vector3> list2 = new List<Vector3>();
			for (int i = 0; i < list.Count; i += 4)
			{
				list2.Add(list[i + 1] - Vector3.forward * depth);
				list2.Add(list[i] - Vector3.forward * depth);
				list2.Add(list[i + 3] - Vector3.forward * depth);
				list2.Add(list[i + 2] - Vector3.forward * depth);
			}
			list.AddRange(list2);
			list.Add(array[6]);
			list.Add(array[5]);
			list.Add(array[6] - Vector3.forward * depth);
			list.Add(array[5] - Vector3.forward * depth);
			list.Add(array[2] - Vector3.forward * depth);
			list.Add(array[2]);
			list.Add(array[6] - Vector3.forward * depth);
			list.Add(array[6]);
			list.Add(array[1]);
			list.Add(array[1] - Vector3.forward * depth);
			list.Add(array[5]);
			list.Add(array[5] - Vector3.forward * depth);
			pb_Object pb_Object = pb_Object.CreateInstanceWithPoints(list.ToArray());
			pb_Object.gameObject.name = "Door";
			return pb_Object;
		}

		public static pb_Object PlaneGenerator(float _width, float _height, int widthCuts, int heightCuts, Axis axis, bool smooth)
		{
			int num = widthCuts + 1;
			int num2 = heightCuts + 1;
			Vector2[] array = ((!smooth) ? new Vector2[num * num2 * 4] : new Vector2[num * num2]);
			Vector3[] array2 = ((!smooth) ? new Vector3[num * num2 * 4] : new Vector3[num * num2]);
			pb_Face[] array3 = new pb_Face[num * num2];
			int num3 = 0;
			int num4 = 0;
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < num; j++)
				{
					float x = (float)j * (_width / (float)num) - _width / 2f - _width / (float)num / (float)num;
					float x2 = (float)(j + 1) * (_width / (float)num) - _width / 2f - _width / (float)num / (float)num;
					float y = (float)i * (_height / (float)num2) - _height / 2f - _height / (float)num2 / (float)num2;
					float y2 = (float)(i + 1) * (_height / (float)num2) - _height / 2f - _height / (float)num2 / (float)num2;
					ref Vector2 reference = ref array[num3];
					reference = new Vector2(x, y);
					ref Vector2 reference2 = ref array[num3 + 1];
					reference2 = new Vector2(x2, y);
					ref Vector2 reference3 = ref array[num3 + 2];
					reference3 = new Vector2(x, y2);
					ref Vector2 reference4 = ref array[num3 + 3];
					reference4 = new Vector2(x2, y2);
					array3[num4++] = new pb_Face(new int[6]
					{
						num3,
						num3 + 1,
						num3 + 2,
						num3 + 1,
						num3 + 3,
						num3 + 2
					});
					num3 += 4;
				}
			}
			switch (axis)
			{
			case Axis.Right:
				for (num3 = 0; num3 < array2.Length; num3++)
				{
					ref Vector3 reference8 = ref array2[num3];
					reference8 = new Vector3(0f, array[num3].x, array[num3].y);
				}
				break;
			case Axis.Left:
				for (num3 = 0; num3 < array2.Length; num3++)
				{
					ref Vector3 reference10 = ref array2[num3];
					reference10 = new Vector3(0f, array[num3].y, array[num3].x);
				}
				break;
			case Axis.Up:
				for (num3 = 0; num3 < array2.Length; num3++)
				{
					ref Vector3 reference6 = ref array2[num3];
					reference6 = new Vector3(array[num3].y, 0f, array[num3].x);
				}
				break;
			case Axis.Down:
				for (num3 = 0; num3 < array2.Length; num3++)
				{
					ref Vector3 reference9 = ref array2[num3];
					reference9 = new Vector3(array[num3].x, 0f, array[num3].y);
				}
				break;
			case Axis.Forward:
				for (num3 = 0; num3 < array2.Length; num3++)
				{
					ref Vector3 reference7 = ref array2[num3];
					reference7 = new Vector3(array[num3].x, array[num3].y, 0f);
				}
				break;
			case Axis.Backward:
				for (num3 = 0; num3 < array2.Length; num3++)
				{
					ref Vector3 reference5 = ref array2[num3];
					reference5 = new Vector3(array[num3].y, array[num3].x, 0f);
				}
				break;
			}
			pb_Object pb_Object = pb_Object.CreateInstanceWithVerticesFaces(array2, array3);
			pb_Object.gameObject.name = "Plane";
			Vector3 zero = Vector3.zero;
			Vector3[] array4 = pb_Object.VerticesInWorldSpace();
			Vector3[] array5 = array4;
			foreach (Vector3 vector in array5)
			{
				zero += vector;
			}
			zero /= (float)array4.Length;
			Vector3 offset = pb_Object.transform.position - zero;
			pb_Object.transform.position = zero;
			pb_Object.TranslateVertices_World(pb_Object.msh.triangles, offset);
			return pb_Object;
		}

		public static pb_Object PipeGenerator(float radius, float height, float thickness, int subdivAxis, int subdivHeight)
		{
			Vector2[] array = new Vector2[subdivAxis];
			Vector2[] array2 = new Vector2[subdivAxis];
			for (int i = 0; i < subdivAxis; i++)
			{
				ref Vector2 reference = ref array[i];
				reference = pb_Math.PointInCircumference(radius, (float)i * (360f / (float)subdivAxis), Vector2.zero);
				ref Vector2 reference2 = ref array2[i];
				reference2 = pb_Math.PointInCircumference(radius - thickness, (float)i * (360f / (float)subdivAxis), Vector2.zero);
			}
			List<Vector3> list = new List<Vector3>();
			subdivHeight++;
			for (int j = 0; j < subdivHeight; j++)
			{
				float y = (float)j * (height / (float)subdivHeight);
				float y2 = (float)(j + 1) * (height / (float)subdivHeight);
				for (int k = 0; k < subdivAxis; k++)
				{
					Vector2 vector = array[k];
					Vector2 vector2 = ((k >= subdivAxis - 1) ? array[0] : array[k + 1]);
					Vector3[] collection = new Vector3[4]
					{
						new Vector3(vector2.x, y, vector2.y),
						new Vector3(vector.x, y, vector.y),
						new Vector3(vector2.x, y2, vector2.y),
						new Vector3(vector.x, y2, vector.y)
					};
					vector = array2[k];
					vector2 = ((k >= subdivAxis - 1) ? array2[0] : array2[k + 1]);
					Vector3[] collection2 = new Vector3[4]
					{
						new Vector3(vector.x, y, vector.y),
						new Vector3(vector2.x, y, vector2.y),
						new Vector3(vector.x, y2, vector.y),
						new Vector3(vector2.x, y2, vector2.y)
					};
					list.AddRange(collection);
					list.AddRange(collection2);
				}
			}
			for (int l = 0; l < subdivAxis; l++)
			{
				Vector2 vector = array[l];
				Vector2 vector2 = ((l >= subdivAxis - 1) ? array[0] : array[l + 1]);
				Vector2 vector3 = array2[l];
				Vector2 vector4 = ((l >= subdivAxis - 1) ? array2[0] : array2[l + 1]);
				Vector3[] collection3 = new Vector3[4]
				{
					new Vector3(vector2.x, height, vector2.y),
					new Vector3(vector.x, height, vector.y),
					new Vector3(vector4.x, height, vector4.y),
					new Vector3(vector3.x, height, vector3.y)
				};
				Vector3[] collection4 = new Vector3[4]
				{
					new Vector3(vector.x, 0f, vector.y),
					new Vector3(vector2.x, 0f, vector2.y),
					new Vector3(vector3.x, 0f, vector3.y),
					new Vector3(vector4.x, 0f, vector4.y)
				};
				list.AddRange(collection4);
				list.AddRange(collection3);
			}
			pb_Object pb_Object = pb_Object.CreateInstanceWithPoints(list.ToArray());
			pb_Object.gameObject.name = "Pipe";
			return pb_Object;
		}

		public static pb_Object ConeGenerator(float radius, float height, int subdivAxis)
		{
			Vector3[] array = new Vector3[subdivAxis];
			for (int i = 0; i < subdivAxis; i++)
			{
				Vector2 vector = pb_Math.PointInCircumference(radius, (float)i * (360f / (float)subdivAxis), Vector2.zero);
				ref Vector3 reference = ref array[i];
				reference = new Vector3(vector.x, 0f, vector.y);
			}
			List<Vector3> list = new List<Vector3>();
			List<pb_Face> list2 = new List<pb_Face>();
			for (int j = 0; j < subdivAxis; j++)
			{
				list.Add(array[j]);
				list.Add((j >= subdivAxis - 1) ? array[0] : array[j + 1]);
				list.Add(Vector3.up * height);
				list.Add(array[j]);
				list.Add((j >= subdivAxis - 1) ? array[0] : array[j + 1]);
				list.Add(Vector3.zero);
			}
			for (int k = 0; k < subdivAxis * 6; k += 6)
			{
				list2.Add(new pb_Face(new int[3]
				{
					k + 2,
					k + 1,
					k
				}));
				list2.Add(new pb_Face(new int[3]
				{
					k + 3,
					k + 4,
					k + 5
				}));
			}
			pb_Object pb_Object = pb_Object.CreateInstanceWithVerticesFaces(list.ToArray(), list2.ToArray());
			pb_Object.gameObject.name = "Cone";
			return pb_Object;
		}

		public static pb_Object ArchGenerator(float angle, float radius, float width, float depth, int radialCuts, bool insideFaces, bool outsideFaces, bool frontFaces, bool backFaces, bool endCaps)
		{
			Vector2[] array = new Vector2[radialCuts];
			Vector2[] array2 = new Vector2[radialCuts];
			for (int i = 0; i < radialCuts; i++)
			{
				ref Vector2 reference = ref array[i];
				reference = pb_Math.PointInCircumference(radius, (float)i * (angle / (float)(radialCuts - 1)), Vector2.zero);
				ref Vector2 reference2 = ref array2[i];
				reference2 = pb_Math.PointInCircumference(radius - width, (float)i * (angle / (float)(radialCuts - 1)), Vector2.zero);
			}
			List<Vector3> list = new List<Vector3>();
			float z = 0f;
			for (int j = 0; j < radialCuts - 1; j++)
			{
				Vector2 vector = array[j];
				Vector2 vector2 = ((j >= radialCuts - 1) ? array[j] : array[j + 1]);
				Vector3[] collection = new Vector3[4]
				{
					new Vector3(vector.x, vector.y, z),
					new Vector3(vector2.x, vector2.y, z),
					new Vector3(vector.x, vector.y, depth),
					new Vector3(vector2.x, vector2.y, depth)
				};
				vector = array2[j];
				vector2 = ((j >= radialCuts - 1) ? array2[j] : array2[j + 1]);
				Vector3[] collection2 = new Vector3[4]
				{
					new Vector3(vector2.x, vector2.y, z),
					new Vector3(vector.x, vector.y, z),
					new Vector3(vector2.x, vector2.y, depth),
					new Vector3(vector.x, vector.y, depth)
				};
				if (outsideFaces)
				{
					list.AddRange(collection);
				}
				if (j != radialCuts - 1 && insideFaces)
				{
					list.AddRange(collection2);
				}
				if (angle < 360f && endCaps)
				{
					if (j == 0)
					{
						list.AddRange(new Vector3[4]
						{
							new Vector3(array[j].x, array[j].y, depth),
							new Vector3(array2[j].x, array2[j].y, depth),
							new Vector3(array[j].x, array[j].y, z),
							new Vector3(array2[j].x, array2[j].y, z)
						});
					}
					if (j == radialCuts - 2)
					{
						list.AddRange(new Vector3[4]
						{
							new Vector3(array2[j + 1].x, array2[j + 1].y, depth),
							new Vector3(array[j + 1].x, array[j + 1].y, depth),
							new Vector3(array2[j + 1].x, array2[j + 1].y, z),
							new Vector3(array[j + 1].x, array[j + 1].y, z)
						});
					}
				}
			}
			for (int k = 0; k < radialCuts - 1; k++)
			{
				Vector2 vector = array[k];
				Vector2 vector2 = ((k >= radialCuts - 1) ? array[k] : array[k + 1]);
				Vector2 vector3 = array2[k];
				Vector2 vector4 = ((k >= radialCuts - 1) ? array2[k] : array2[k + 1]);
				Vector3[] collection3 = new Vector3[4]
				{
					new Vector3(vector.x, vector.y, depth),
					new Vector3(vector2.x, vector2.y, depth),
					new Vector3(vector3.x, vector3.y, depth),
					new Vector3(vector4.x, vector4.y, depth)
				};
				Vector3[] collection4 = new Vector3[4]
				{
					new Vector3(vector2.x, vector2.y, 0f),
					new Vector3(vector.x, vector.y, 0f),
					new Vector3(vector4.x, vector4.y, 0f),
					new Vector3(vector3.x, vector3.y, 0f)
				};
				if (frontFaces)
				{
					list.AddRange(collection3);
				}
				if (backFaces)
				{
					list.AddRange(collection4);
				}
			}
			pb_Object pb_Object = pb_Object.CreateInstanceWithPoints(list.ToArray());
			pb_Object.gameObject.name = "Arch";
			return pb_Object;
		}

		public static pb_Object IcosahedronGenerator(float radius, int subdivisions)
		{
			Vector3[] array = new Vector3[ICOSAHEDRON_TRIANGLES.Length];
			for (int i = 0; i < ICOSAHEDRON_TRIANGLES.Length; i += 3)
			{
				ref Vector3 reference = ref array[i];
				reference = ICOSAHEDRON_VERTICES[ICOSAHEDRON_TRIANGLES[i]].normalized * radius;
				ref Vector3 reference2 = ref array[i + 1];
				reference2 = ICOSAHEDRON_VERTICES[ICOSAHEDRON_TRIANGLES[i + 1]].normalized * radius;
				ref Vector3 reference3 = ref array[i + 2];
				reference3 = ICOSAHEDRON_VERTICES[ICOSAHEDRON_TRIANGLES[i + 2]].normalized * radius;
			}
			for (int j = 0; j < subdivisions; j++)
			{
				array = SubdivideIcosahedron(array, radius);
			}
			pb_Face[] array2 = new pb_Face[array.Length / 3];
			for (int k = 0; k < array.Length; k += 3)
			{
				array2[k / 3] = new pb_Face(new int[3]
				{
					k,
					k + 1,
					k + 2
				});
				array2[k / 3].manualUV = true;
			}
			GameObject gameObject = new GameObject();
			pb_Object pb_Object = gameObject.AddComponent<pb_Object>();
			pb_Object.SetVertices(array);
			pb_Object.SetUV(new Vector2[array.Length]);
			pb_Object.SetFaces(array2);
			pb_IntArray[] array3 = new pb_IntArray[array.Length];
			for (int l = 0; l < array3.Length; l++)
			{
				array3[l] = new pb_IntArray(new int[1] { l });
			}
			pb_Object.SetSharedIndices(array3);
			pb_Object.ToMesh();
			pb_Object.Refresh();
			pb_Object.gameObject.name = "Icosphere";
			return pb_Object;
		}

		private static Vector3[] SubdivideIcosahedron(Vector3[] vertices, float radius)
		{
			Vector3[] array = new Vector3[vertices.Length * 4];
			int num = 0;
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			Vector3 zero3 = Vector3.zero;
			Vector3 zero4 = Vector3.zero;
			Vector3 zero5 = Vector3.zero;
			Vector3 zero6 = Vector3.zero;
			for (int i = 0; i < vertices.Length; i += 3)
			{
				zero = vertices[i];
				zero3 = vertices[i + 1];
				zero6 = vertices[i + 2];
				zero2 = ((zero + zero3) * 0.5f).normalized * radius;
				zero4 = ((zero + zero6) * 0.5f).normalized * radius;
				zero5 = ((zero3 + zero6) * 0.5f).normalized * radius;
				array[num++] = zero;
				array[num++] = zero2;
				array[num++] = zero4;
				array[num++] = zero2;
				array[num++] = zero3;
				array[num++] = zero5;
				array[num++] = zero2;
				array[num++] = zero5;
				array[num++] = zero4;
				array[num++] = zero4;
				array[num++] = zero5;
				array[num++] = zero6;
			}
			return array;
		}

		private static Vector3[] CircleVertices(int segments, float radius, float circumference, Quaternion rotation, float offset)
		{
			float num = (float)segments - 1f;
			Vector3[] array = new Vector3[(segments - 1) * 2];
			ref Vector3 reference = ref array[0];
			reference = new Vector3(Mathf.Cos(0f / num * circumference * ((float)Math.PI / 180f)) * radius, Mathf.Sin(0f / num * circumference * ((float)Math.PI / 180f)) * radius, 0f);
			ref Vector3 reference2 = ref array[1];
			reference2 = new Vector3(Mathf.Cos(1f / num * circumference * ((float)Math.PI / 180f)) * radius, Mathf.Sin(1f / num * circumference * ((float)Math.PI / 180f)) * radius, 0f);
			ref Vector3 reference3 = ref array[0];
			reference3 = rotation * (array[0] + Vector3.right * offset);
			ref Vector3 reference4 = ref array[1];
			reference4 = rotation * (array[1] + Vector3.right * offset);
			int num2 = 2;
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 2; i < segments; i++)
			{
				float f = (float)i / num * circumference * ((float)Math.PI / 180f);
				stringBuilder.AppendLine(f.ToString());
				ref Vector3 reference5 = ref array[num2];
				reference5 = array[num2 - 1];
				ref Vector3 reference6 = ref array[num2 + 1];
				reference6 = rotation * (new Vector3(Mathf.Cos(f) * radius, Mathf.Sin(f) * radius, 0f) + Vector3.right * offset);
				num2 += 2;
			}
			return array;
		}

		public static pb_Object TorusGenerator(int InRows, int InColumns, float InRadius, float InTubeRadius, bool InSmooth, float InHorizontalCircumference, float InVerticalCircumference)
		{
			int num = Mathf.Clamp(InRows + 1, 4, 128);
			int num2 = Mathf.Clamp(InColumns + 1, 4, 128);
			float num3 = Mathf.Clamp(InRadius, 0.01f, 2048f);
			float num4 = Mathf.Clamp(InTubeRadius, 0.01f, num3);
			num3 -= num4;
			float num5 = Mathf.Clamp(InHorizontalCircumference, 0.01f, 360f);
			float circumference = Mathf.Clamp(InVerticalCircumference, 0.01f, 360f);
			List<Vector3> list = new List<Vector3>();
			int num6 = num2 - 1;
			Vector3[] collection = CircleVertices(num, num4, circumference, Quaternion.Euler(Vector3.up * 0f * num5), num3);
			for (int i = 1; i < num2; i++)
			{
				list.AddRange(collection);
				Quaternion rotation = Quaternion.Euler(Vector3.up * ((float)i / (float)num6 * num5));
				collection = CircleVertices(num, num4, circumference, rotation, num3);
				list.AddRange(collection);
			}
			List<pb_Face> list2 = new List<pb_Face>();
			int num7 = 0;
			for (int j = 0; j < (num2 - 1) * 2; j += 2)
			{
				for (int k = 0; k < num - 1; k++)
				{
					int num8 = j * ((num - 1) * 2) + k * 2;
					int num9 = (j + 1) * ((num - 1) * 2) + k * 2;
					int num10 = j * ((num - 1) * 2) + k * 2 + 1;
					int num11 = (j + 1) * ((num - 1) * 2) + k * 2 + 1;
					list2.Add(new pb_Face(new int[6] { num8, num9, num10, num9, num11, num10 }));
					list2[num7].smoothingGroup = (InSmooth ? 1 : (-1));
					list2[num7].manualUV = true;
					num7++;
				}
			}
			pb_Object pb_Object = pb_Object.CreateInstanceWithVerticesFaces(list.ToArray(), list2.ToArray());
			pb_Object.gameObject.name = "Torus";
			return pb_Object;
		}
	}
}
