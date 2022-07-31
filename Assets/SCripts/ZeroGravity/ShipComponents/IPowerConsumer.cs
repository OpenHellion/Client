namespace ZeroGravity.ShipComponents
{
	public interface IPowerConsumer
	{
		float GetPowerConsumption(bool? working = null);

		void SwitchOn();

		void SwitchOff();

		void Toggle();
	}
}
