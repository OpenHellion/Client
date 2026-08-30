using System.Collections.Generic;
using ZeroGravity.Data;

namespace ZeroGravity.Objects
{
	public interface IDamageable
	{
		float MaxHealth { get; set; }

		float Health { get; set; }

		bool Damageable { get; }

		bool Repairable { get; }

		void TakeDamage(Dictionary<TypeOfDamage, float> damages);

		bool Explode();
	}
}
