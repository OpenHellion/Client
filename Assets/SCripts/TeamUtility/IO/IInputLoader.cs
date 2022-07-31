namespace TeamUtility.IO
{
	public interface IInputLoader
	{
		SaveLoadParameters Load();

		InputConfiguration LoadSelective(string inputConfigName);
	}
}
