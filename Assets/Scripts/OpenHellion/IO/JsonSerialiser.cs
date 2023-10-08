using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace OpenHellion.IO
{
	public static class JsonSerialiser
	{
		public enum Formatting
		{
			None = 0,
			Indented = 1
		}

		private static readonly AuxDataJsonConverter s_auxConverter = new AuxDataJsonConverter();

		private static readonly AttachPointDataJsonConverter
			s_attachPointConverter = new AttachPointDataJsonConverter();

		public static T LoadPersistent<T>(string filename)
		{
			return JsonConvert.DeserializeObject<T>(
				File.ReadAllText(Path.Combine(Application.persistentDataPath, filename)),
				new JsonConverter[2] { s_auxConverter, s_attachPointConverter });
		}

		public static T LoadResource<T>(string resourcePath)
		{
			return JsonConvert.DeserializeObject<T>(Resources.Load(resourcePath).ToString(),
				new JsonConverter[2] { s_auxConverter, s_attachPointConverter });
		}

		public static T LoadDataPath<T>(string filePath)
		{
			return JsonConvert.DeserializeObject<T>(File.ReadAllText(Path.Combine(Application.dataPath, filePath)),
				new JsonConverter[2] { s_auxConverter, s_attachPointConverter });
		}

		public static T Load<T>(string filePath)
		{
			return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath),
				new JsonConverter[2] { s_auxConverter, s_attachPointConverter });
		}

		public static string Serialize(object obj, Formatting format = Formatting.Indented)
		{
			return JsonConvert.SerializeObject(obj, (Newtonsoft.Json.Formatting)format);
		}

		public static T Deserialize<T>(string jsonString)
		{
			return JsonConvert.DeserializeObject<T>(jsonString,
				new JsonConverter[2] { s_auxConverter, s_attachPointConverter });
		}

		public static void SerializePersistent(object obj, string fileName, Formatting format = Formatting.Indented)
		{
			File.WriteAllText(Path.Combine(Application.persistentDataPath, fileName),
				JsonConvert.SerializeObject(obj, (Newtonsoft.Json.Formatting)format));
		}

		public static void SerializeDataPath(object obj, string dataPath, Formatting format = Formatting.Indented,
			bool useLFLineEnding = false)
		{
			if (useLFLineEnding)
			{
				File.WriteAllText(Path.Combine(Application.dataPath, dataPath),
					JsonConvert.SerializeObject(obj, (Newtonsoft.Json.Formatting)format).Replace("\r\n", "\n"));
			}
			else
			{
				File.WriteAllText(Path.Combine(Application.dataPath, dataPath),
					JsonConvert.SerializeObject(obj, (Newtonsoft.Json.Formatting)format));
			}
		}
	}
}
