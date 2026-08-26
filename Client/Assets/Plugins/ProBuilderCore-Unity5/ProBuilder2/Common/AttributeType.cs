using System;

namespace ProBuilder2.Common
{
	[Flags]
	public enum AttributeType : ushort
	{
		Position = 1,
		UV0 = 2,
		UV1 = 4,
		UV2 = 8,
		UV3 = 0x10,
		Color = 0x20,
		Normal = 0x40,
		Tangent = 0x80,
		All = 0xFF
	}
}
