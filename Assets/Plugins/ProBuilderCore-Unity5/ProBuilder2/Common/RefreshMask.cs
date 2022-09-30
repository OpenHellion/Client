using System;

namespace ProBuilder2.Common
{
	[Flags]
	public enum RefreshMask : ushort
	{
		All = 0xFF,
		UV = 1,
		Colors = 2,
		Normals = 4,
		Tangents = 8,
		Collisions = 0x10
	}
}
