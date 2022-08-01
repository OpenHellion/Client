using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZeroGravity.Data;

namespace ZeroGravity
{
	public class AttachPointDataJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			if (objectType == typeof(BaseAttachPointData))
			{
				return true;
			}
			return false;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			try
			{
				JObject jObject = JObject.Load(reader);
				switch ((int)jObject["AttachPointType"])
				{
				case 1:
					return jObject.ToObject<AttachPointData>(serializer);
				case 8:
					return jObject.ToObject<ActiveAttachPointData>(serializer);
				case 7:
					return jObject.ToObject<ScrapAttachPointData>(serializer);
				case 2:
					return jObject.ToObject<MachineryPartSlotData>(serializer);
				case 3:
					return jObject.ToObject<ResourcesTransferPointData>(serializer);
				case 5:
					return jObject.ToObject<ResourcesAutoTransferPointData>(serializer);
				case 4:
					return jObject.ToObject<BatteryRechargePointData>(serializer);
				case 6:
					return jObject.ToObject<ItemRecyclerAtachPointData>(serializer);
				}
			}
			catch
			{
			}
			return null;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value);
		}
	}
}
