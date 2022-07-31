using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using ZeroGravity.Data;

namespace ZeroGravity.ShipComponents
{
	public class GeneratorAir : Generator, IPowerConsumer, ILifeProvider
	{
		[SerializeField]
		private ResourceRequirement[] _ResourceRequirements = new ResourceRequirement[3]
		{
			new ResourceRequirement
			{
				ResourceType = DistributionSystemType.Power
			},
			new ResourceRequirement
			{
				ResourceType = DistributionSystemType.Oxygen
			},
			new ResourceRequirement
			{
				ResourceType = DistributionSystemType.Nitrogen
			}
		};

		[CompilerGenerated]
		private static Func<ResourceRequirement, bool> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Func<ResourceRequirement, bool> _003C_003Ef__am_0024cache1;

		public override GeneratorType Type
		{
			get
			{
				return GeneratorType.Air;
			}
		}

		public override ResourceRequirement[] ResourceRequirements
		{
			get
			{
				return _ResourceRequirements;
			}
		}

		public override GeneratorData GetData()
		{
			try
			{
				ResourceRequirement[] resourceRequirements = ResourceRequirements;
				if (_003C_003Ef__am_0024cache0 == null)
				{
					_003C_003Ef__am_0024cache0 = _003CGetData_003Em__0;
				}
				ResourceRequirement resourceRequirement = resourceRequirements.FirstOrDefault(_003C_003Ef__am_0024cache0);
				ResourceRequirement[] resourceRequirements2 = ResourceRequirements;
				if (_003C_003Ef__am_0024cache1 == null)
				{
					_003C_003Ef__am_0024cache1 = _003CGetData_003Em__1;
				}
				ResourceRequirement resourceRequirement2 = resourceRequirements2.FirstOrDefault(_003C_003Ef__am_0024cache1);
				if (resourceRequirement != null && resourceRequirement2 != null)
				{
					float num = resourceRequirement.Nominal + resourceRequirement2.Nominal;
					float num2 = NominalOutput / num;
					resourceRequirement.Nominal *= num2;
					resourceRequirement2.Nominal *= num2;
				}
			}
			catch
			{
			}
			return base.GetData();
		}

		[CompilerGenerated]
		private static bool _003CGetData_003Em__0(ResourceRequirement m)
		{
			return m.ResourceType == DistributionSystemType.Oxygen;
		}

		[CompilerGenerated]
		private static bool _003CGetData_003Em__1(ResourceRequirement m)
		{
			return m.ResourceType == DistributionSystemType.Nitrogen;
		}
	}
}
