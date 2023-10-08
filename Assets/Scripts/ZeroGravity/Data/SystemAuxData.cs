using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ZeroGravity.Data
{
	public abstract class SystemAuxData : ISceneData
	{
		public abstract SystemAuxDataType AuxDataType { get; }

		public static object GetJsonData(JObject jo, JsonSerializer serializer)
		{
			SystemAuxDataType systemAuxDataType = (SystemAuxDataType)(int)jo["AuxDataType"];
			switch (systemAuxDataType)
			{
				case SystemAuxDataType.AirDevice:
					return jo.ToObject<SubSystemAirDevicesAuxData>(serializer);
				case SystemAuxDataType.ScrubberDevice:
					return jo.ToObject<SubSystemScrubberDeviceAuxData>(serializer);
				case SystemAuxDataType.ACDevice:
					return jo.ToObject<SubSystemACDeviceAuxData>(serializer);
				case SystemAuxDataType.RCS:
					return jo.ToObject<SubSystemRCSAuxData>(serializer);
				case SystemAuxDataType.Engine:
					return jo.ToObject<SubSystemEngineAuxData>(serializer);
				case SystemAuxDataType.FTL:
					return jo.ToObject<SubSystemFTLAuxData>(serializer);
				case SystemAuxDataType.Capacitor:
					return jo.ToObject<GeneratorCapacitorAuxData>(serializer);
				case SystemAuxDataType.PowerGenerator:
					return jo.ToObject<GeneratorPowerAuxData>(serializer);
				case SystemAuxDataType.Refinery:
					return jo.ToObject<SubSystemRefineryAuxData>(serializer);
				case SystemAuxDataType.Solar:
					return jo.ToObject<GeneratorSolarAuxData>(serializer);
				case SystemAuxDataType.ScrubbedAirGenerator:
					return jo.ToObject<GeneratorScrubbedAirAuxData>(serializer);
				case SystemAuxDataType.Fabricator:
					return jo.ToObject<SubSystemFabricatorAuxData>(serializer);
				case SystemAuxDataType.VesselBaseSystem:
					return jo.ToObject<VesselBaseSystemAuxData>(serializer);
				case SystemAuxDataType.AirTank:
					return jo.ToObject<AirTankAuxData>(serializer);
				case SystemAuxDataType.Radar:
					return jo.ToObject<RadarAuxData>(serializer);
				default:
					throw new Exception("Json deserializer was not implemented for item type " + systemAuxDataType);
			}
		}
	}
}
