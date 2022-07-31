using System;
using System.IO;
using System.Text;
using System.Xml;

namespace TeamUtility.IO
{
	public sealed class InputSaverXML : IInputSaver
	{
		private string _filename;

		private Stream _outputStream;

		private StringBuilder _output;

		public InputSaverXML(string filename)
		{
			if (filename == null)
			{
				throw new ArgumentNullException("filename");
			}
			_filename = filename;
			_outputStream = null;
			_output = null;
		}

		public InputSaverXML(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			_filename = null;
			_output = null;
			_outputStream = stream;
		}

		public InputSaverXML(StringBuilder output)
		{
			if (output == null)
			{
				throw new ArgumentNullException("output");
			}
			_filename = null;
			_outputStream = null;
			_output = output;
		}

		public void Save(SaveLoadParameters parameters)
		{
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.Encoding = Encoding.UTF8;
			xmlWriterSettings.Indent = true;
			using (XmlWriter xmlWriter = CreateXmlWriter(xmlWriterSettings))
			{
				xmlWriter.WriteStartDocument(true);
				xmlWriter.WriteStartElement("Input");
				xmlWriter.WriteAttributeString("playerOneDefault", parameters.playerOneDefault);
				xmlWriter.WriteAttributeString("playerTwoDefault", parameters.playerTwoDefault);
				xmlWriter.WriteAttributeString("playerThreeDefault", parameters.playerThreeDefault);
				xmlWriter.WriteAttributeString("playerFourDefault", parameters.playerFourDefault);
				foreach (InputConfiguration inputConfiguration in parameters.inputConfigurations)
				{
					WriteInputConfiguration(inputConfiguration, xmlWriter);
				}
				xmlWriter.WriteEndElement();
				xmlWriter.WriteEndDocument();
			}
		}

		private XmlWriter CreateXmlWriter(XmlWriterSettings settings)
		{
			if (_filename != null)
			{
				return XmlWriter.Create(_filename, settings);
			}
			if (_outputStream != null)
			{
				return XmlWriter.Create(_outputStream, settings);
			}
			if (_output != null)
			{
				return XmlWriter.Create(_output, settings);
			}
			return null;
		}

		private void WriteInputConfiguration(InputConfiguration inputConfig, XmlWriter writer)
		{
			writer.WriteStartElement("InputConfiguration");
			writer.WriteAttributeString("name", inputConfig.name);
			foreach (AxisConfiguration axis in inputConfig.axes)
			{
				WriteAxisConfiguration(axis, writer);
			}
			writer.WriteEndElement();
		}

		private void WriteAxisConfiguration(AxisConfiguration axisConfig, XmlWriter writer)
		{
			writer.WriteStartElement("AxisConfiguration");
			writer.WriteAttributeString("name", axisConfig.name);
			writer.WriteElementString("description", axisConfig.description);
			writer.WriteElementString("positive", axisConfig.positive.ToString());
			writer.WriteElementString("altPositive", axisConfig.altPositive.ToString());
			writer.WriteElementString("negative", axisConfig.negative.ToString());
			writer.WriteElementString("altNegative", axisConfig.altNegative.ToString());
			writer.WriteElementString("deadZone", axisConfig.deadZone.ToString());
			writer.WriteElementString("gravity", axisConfig.gravity.ToString());
			writer.WriteElementString("sensitivity", axisConfig.sensitivity.ToString());
			writer.WriteElementString("snap", axisConfig.snap.ToString().ToLower());
			writer.WriteElementString("invert", axisConfig.invert.ToString().ToLower());
			writer.WriteElementString("type", axisConfig.type.ToString());
			writer.WriteElementString("axis", axisConfig.axis.ToString());
			writer.WriteElementString("joystick", axisConfig.joystick.ToString());
			writer.WriteEndElement();
		}
	}
}
