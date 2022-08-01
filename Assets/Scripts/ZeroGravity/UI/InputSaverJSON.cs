using TeamUtility.IO;

namespace ZeroGravity.UI
{
	public class InputSaverJSON : IInputSaver
	{
		public void Save(SaveLoadParameters parameters)
		{
			Json.SerializePersistent(parameters.inputConfigurations, "Controls.json");
		}
	}
}
