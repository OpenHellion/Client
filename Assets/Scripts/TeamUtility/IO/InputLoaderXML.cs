using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace TeamUtility.IO
{
	public sealed class InputLoaderXML : IInputLoader
	{
		private string _filename;

		private Stream _inputStream;

		private TextReader _textReader;

		public InputLoaderXML(string filename)
		{
			if (filename == null)
			{
				throw new ArgumentNullException("filename");
			}
			_filename = filename;
			_inputStream = null;
			_textReader = null;
		}

		public InputLoaderXML(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			_filename = null;
			_textReader = null;
			_inputStream = stream;
		}

		public InputLoaderXML(TextReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			_filename = null;
			_inputStream = null;
			_textReader = reader;
		}

		public SaveLoadParameters Load()
		{
			SaveLoadParameters saveLoadParameters = new SaveLoadParameters();
			using (XmlReader xmlReader = CreateXmlReader())
			{
				saveLoadParameters.inputConfigurations = new List<InputConfiguration>();
				saveLoadParameters.playerOneDefault = string.Empty;
				saveLoadParameters.playerTwoDefault = string.Empty;
				saveLoadParameters.playerThreeDefault = string.Empty;
				saveLoadParameters.playerFourDefault = string.Empty;
				while (xmlReader.Read())
				{
					if (xmlReader.IsStartElement("Input"))
					{
						saveLoadParameters.playerOneDefault = xmlReader["playerOneDefault"];
						saveLoadParameters.playerTwoDefault = xmlReader["playerTwoDefault"];
						saveLoadParameters.playerThreeDefault = xmlReader["playerThreeDefault"];
						saveLoadParameters.playerFourDefault = xmlReader["playerFourDefault"];
					}
					else if (xmlReader.IsStartElement("InputConfiguration"))
					{
						saveLoadParameters.inputConfigurations.Add(ReadInputConfiguration(xmlReader));
					}
				}
				return saveLoadParameters;
			}
		}

		public InputConfiguration LoadSelective(string inputConfigName)
		{
			InputConfiguration result = null;
			using (XmlReader xmlReader = CreateXmlReader())
			{
				while (xmlReader.Read())
				{
					if (xmlReader.IsStartElement("InputConfiguration") && xmlReader["name"] == inputConfigName)
					{
						return ReadInputConfiguration(xmlReader);
					}
				}
				return result;
			}
		}

		private XmlReader CreateXmlReader()
		{
			if (_filename != null)
			{
				return XmlReader.Create(_filename);
			}
			if (_inputStream != null)
			{
				return XmlReader.Create(_inputStream);
			}
			if (_textReader != null)
			{
				return XmlReader.Create(_textReader);
			}
			return null;
		}

		private InputConfiguration ReadInputConfiguration(XmlReader reader)
		{
			InputConfiguration inputConfiguration = new InputConfiguration();
			inputConfiguration.name = reader["name"];
			while (reader.Read() && reader.IsStartElement("AxisConfiguration"))
			{
				inputConfiguration.axes.Add(ReadAxisConfiguration(reader));
			}
			return inputConfiguration;
		}

		private AxisConfiguration ReadAxisConfiguration(XmlReader reader)
		{
			AxisConfiguration axisConfiguration = new AxisConfiguration();
			axisConfiguration.name = reader["name"];
			bool flag = false;
			while (reader.Read() && reader.IsStartElement() && !flag)
			{
				switch (reader.LocalName)
				{
				case "description":
					axisConfiguration.description = ((!reader.IsEmptyElement) ? reader.ReadElementContentAsString() : string.Empty);
					break;
				case "positive":
					axisConfiguration.positive = AxisConfiguration.StringToKey(reader.ReadElementContentAsString());
					break;
				case "altPositive":
					axisConfiguration.altPositive = AxisConfiguration.StringToKey(reader.ReadElementContentAsString());
					break;
				case "negative":
					axisConfiguration.negative = AxisConfiguration.StringToKey(reader.ReadElementContentAsString());
					break;
				case "altNegative":
					axisConfiguration.altNegative = AxisConfiguration.StringToKey(reader.ReadElementContentAsString());
					break;
				case "deadZone":
					axisConfiguration.deadZone = reader.ReadElementContentAsFloat();
					break;
				case "gravity":
					axisConfiguration.gravity = reader.ReadElementContentAsFloat();
					break;
				case "sensitivity":
					axisConfiguration.sensitivity = reader.ReadElementContentAsFloat();
					break;
				case "snap":
					axisConfiguration.snap = reader.ReadElementContentAsBoolean();
					break;
				case "invert":
					axisConfiguration.invert = reader.ReadElementContentAsBoolean();
					break;
				case "type":
					axisConfiguration.type = AxisConfiguration.StringToInputType(reader.ReadElementContentAsString());
					break;
				case "axis":
					axisConfiguration.axis = reader.ReadElementContentAsInt();
					break;
				case "joystick":
					axisConfiguration.joystick = reader.ReadElementContentAsInt();
					break;
				default:
					flag = true;
					break;
				}
			}
			return axisConfiguration;
		}
	}
}
