using Luminosity.IO;
namespace ZeroGravity.UI
{
	public class InputSaverJSON : IInputSaver
	{
		void IInputSaver.Save(SaveData saveData)
		{
			Json.SerializePersistent(saveData.ControlSchemes, "Controls.json");
		}
	}
}
