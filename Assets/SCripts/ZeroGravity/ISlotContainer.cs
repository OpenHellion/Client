using System.Collections.Generic;
using ZeroGravity.Objects;

namespace ZeroGravity
{
	public interface ISlotContainer
	{
		InventorySlot GetSlotByID(short id);

		Dictionary<short, InventorySlot> GetAllSlots();

		Dictionary<short, InventorySlot> GetSlotsByGroup(InventorySlot.Group group);
	}
}
