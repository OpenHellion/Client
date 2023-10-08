using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity
{
	public class ConsoleSpawnOptionUI : MonoBehaviour
	{
		[CompilerGenerated]
		private sealed class _003CSetSpawnOption_003Ec__AnonStorey0
		{
			internal string command;

			internal ConsoleSpawnOptionUI _0024this;

			internal void _003C_003Em__0()
			{
				_0024this.Console.Spawn(command);
			}
		}

		public InGameConsole Console;

		public Image Icon;

		public Text Name;

		public void SetSpawnOption(string command)
		{
			_003CSetSpawnOption_003Ec__AnonStorey0 _003CSetSpawnOption_003Ec__AnonStorey =
				new _003CSetSpawnOption_003Ec__AnonStorey0();
			_003CSetSpawnOption_003Ec__AnonStorey.command = command;
			_003CSetSpawnOption_003Ec__AnonStorey._0024this = this;
			GetComponent<Button>().onClick.AddListener(_003CSetSpawnOption_003Ec__AnonStorey._003C_003Em__0);
		}
	}
}
