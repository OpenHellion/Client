using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace TeamUtility.IO
{
	public class InputManager : MonoBehaviour
	{
		[CompilerGenerated]
		private sealed class _003CStartKeyScan_003Ec__AnonStorey0
		{
			internal KeyScanHandler scanHandler;

			internal bool _003C_003Em__0(ScanResult result)
			{
				return scanHandler(result.key, (object[])result.userData);
			}
		}

		[CompilerGenerated]
		private sealed class _003CStartMouseAxisScan_003Ec__AnonStorey1
		{
			internal AxisScanHandler scanHandler;

			internal bool _003C_003Em__0(ScanResult result)
			{
				return scanHandler(result.mouseAxis, (object[])result.userData);
			}
		}

		[CompilerGenerated]
		private sealed class _003CStartJoystickAxisScan_003Ec__AnonStorey2
		{
			internal AxisScanHandler scanHandler;

			internal bool _003C_003Em__0(ScanResult result)
			{
				return scanHandler(result.joystickAxis, (object[])result.userData);
			}
		}

		public List<InputConfiguration> inputConfigurations = new List<InputConfiguration>();

		public string playerOneDefault;

		public string playerTwoDefault;

		public string playerThreeDefault;

		public string playerFourDefault;

		public bool dontDestroyOnLoad;

		public bool ignoreTimescale;

		private static InputManager _instance;

		private InputConfiguration _playerOneConfig;

		private InputConfiguration _playerTwoConfig;

		private InputConfiguration _playerThreeConfig;

		private InputConfiguration _playerFourConfig;

		private ScanHandler _scanHandler;

		private ScanResult _scanResult;

		private ScanFlags _scanFlags;

		private string _cancelScanButton;

		private float _scanStartTime;

		private float _scanTimeout;

		private int _scanJoystick;

		private object _scanUserData;

		private string[] _rawMouseAxes;

		private string[] _rawJoystickAxes;

		private KeyCode[] _keys;

		private Dictionary<string, InputConfiguration> _configurationTable;

		private Dictionary<string, Dictionary<string, AxisConfiguration>> _axesTable;

		public static InputManager Instance
		{
			get
			{
				return _instance;
			}
		}

		[Obsolete("Use InputManager.PlayerOneConfiguration instead", true)]
		public static InputConfiguration CurrentConfiguration
		{
			get
			{
				return _instance._playerOneConfig;
			}
		}

		public static InputConfiguration PlayerOneConfiguration
		{
			get
			{
				return _instance._playerOneConfig;
			}
		}

		public static InputConfiguration PlayerTwoConfiguration
		{
			get
			{
				return _instance._playerTwoConfig;
			}
		}

		public static InputConfiguration PlayerThreeConfiguration
		{
			get
			{
				return _instance._playerThreeConfig;
			}
		}

		public static InputConfiguration PlayerFourConfiguration
		{
			get
			{
				return _instance._playerFourConfig;
			}
		}

		public static bool IsScanning
		{
			get
			{
				return _instance._scanFlags != ScanFlags.None;
			}
		}

		public static bool IgnoreTimescale
		{
			get
			{
				return _instance.ignoreTimescale;
			}
		}

		public static Vector3 acceleration
		{
			get
			{
				return Input.acceleration;
			}
		}

		public static int accelerationEventCount
		{
			get
			{
				return Input.accelerationEventCount;
			}
		}

		public static AccelerationEvent[] accelerationEvents
		{
			get
			{
				return Input.accelerationEvents;
			}
		}

		public static bool anyKey
		{
			get
			{
				return Input.anyKey;
			}
		}

		public static bool anyKeyDown
		{
			get
			{
				return Input.anyKeyDown;
			}
		}

		public static Compass compass
		{
			get
			{
				return Input.compass;
			}
		}

		public static string compositionString
		{
			get
			{
				return Input.compositionString;
			}
		}

		public static DeviceOrientation deviceOrientation
		{
			get
			{
				return Input.deviceOrientation;
			}
		}

		public static Gyroscope gyro
		{
			get
			{
				return Input.gyro;
			}
		}

		public static bool imeIsSelected
		{
			get
			{
				return Input.imeIsSelected;
			}
		}

		public static string inputString
		{
			get
			{
				return Input.inputString;
			}
		}

		public static LocationService location
		{
			get
			{
				return Input.location;
			}
		}

		public static Vector2 mousePosition
		{
			get
			{
				return Input.mousePosition;
			}
		}

		public static bool mousePresent
		{
			get
			{
				return Input.mousePresent;
			}
		}

		public static bool touchSupported
		{
			get
			{
				return Input.touchSupported;
			}
		}

		public static int touchCount
		{
			get
			{
				return Input.touchCount;
			}
		}

		public static Touch[] touches
		{
			get
			{
				return Input.touches;
			}
		}

		public static bool compensateSensors
		{
			get
			{
				return Input.compensateSensors;
			}
			set
			{
				Input.compensateSensors = value;
			}
		}

		public static Vector2 compositionCursorPos
		{
			get
			{
				return Input.compositionCursorPos;
			}
			set
			{
				Input.compositionCursorPos = value;
			}
		}

		public static IMECompositionMode imeCompositionMode
		{
			get
			{
				return Input.imeCompositionMode;
			}
			set
			{
				Input.imeCompositionMode = value;
			}
		}

		public static bool multiTouchEnabled
		{
			get
			{
				return Input.multiTouchEnabled;
			}
			set
			{
				Input.multiTouchEnabled = value;
			}
		}

		public event Action<PlayerID> ConfigurationChanged;

		public event Action<string> ConfigurationDirty;

		public event Action Loaded;

		public event Action Saved;

		public event RemoteUpdateDelegate RemoteUpdate;

		private void Awake()
		{
			if (_instance != null)
			{
				UnityEngine.Object.Destroy(this);
				return;
			}
			if (dontDestroyOnLoad)
			{
				UnityEngine.Object.DontDestroyOnLoad(this);
			}
			_instance = this;
			_keys = (KeyCode[])Enum.GetValues(typeof(KeyCode));
			_configurationTable = new Dictionary<string, InputConfiguration>();
			_axesTable = new Dictionary<string, Dictionary<string, AxisConfiguration>>();
			SetRawAxisNames();
			Initialize();
		}

		private void SetRawAxisNames()
		{
			_rawMouseAxes = new string[3];
			for (int i = 0; i < _rawMouseAxes.Length; i++)
			{
				_rawMouseAxes[i] = "mouse_axis_" + i;
			}
			_rawJoystickAxes = new string[40];
			for (int j = 0; j < 4; j++)
			{
				for (int k = 0; k < 10; k++)
				{
					_rawJoystickAxes[j * 10 + k] = "joy_" + j + "_axis_" + k;
				}
			}
		}

		private void Initialize()
		{
			_playerOneConfig = null;
			_playerTwoConfig = null;
			_playerThreeConfig = null;
			_playerFourConfig = null;
			if (inputConfigurations.Count == 0)
			{
				return;
			}
			PopulateLookupTables();
			if (!string.IsNullOrEmpty(playerOneDefault) && _configurationTable.ContainsKey(playerOneDefault))
			{
				_playerOneConfig = _configurationTable[playerOneDefault];
			}
			else if (inputConfigurations.Count > 0)
			{
				_playerOneConfig = inputConfigurations[0];
			}
			if (!string.IsNullOrEmpty(playerTwoDefault) && _configurationTable.ContainsKey(playerTwoDefault))
			{
				_playerTwoConfig = _configurationTable[playerTwoDefault];
			}
			if (!string.IsNullOrEmpty(playerThreeDefault) && _configurationTable.ContainsKey(playerThreeDefault))
			{
				_playerThreeConfig = _configurationTable[playerThreeDefault];
			}
			if (!string.IsNullOrEmpty(playerFourDefault) && _configurationTable.ContainsKey(playerFourDefault))
			{
				_playerFourConfig = _configurationTable[playerFourDefault];
			}
			foreach (InputConfiguration inputConfiguration in inputConfigurations)
			{
				foreach (AxisConfiguration axis in inputConfiguration.axes)
				{
					axis.Initialize();
				}
			}
			Input.ResetInputAxes();
		}

		private void PopulateLookupTables()
		{
			_configurationTable.Clear();
			foreach (InputConfiguration inputConfiguration in inputConfigurations)
			{
				if (!_configurationTable.ContainsKey(inputConfiguration.name))
				{
					_configurationTable.Add(inputConfiguration.name, inputConfiguration);
				}
				else
				{
					Debug.LogWarning(string.Format("An input configuration named '{0}' already exists in the lookup table", inputConfiguration.name));
				}
			}
			_axesTable.Clear();
			foreach (InputConfiguration inputConfiguration2 in inputConfigurations)
			{
				Dictionary<string, AxisConfiguration> dictionary = new Dictionary<string, AxisConfiguration>();
				foreach (AxisConfiguration axis in inputConfiguration2.axes)
				{
					if (!dictionary.ContainsKey(axis.name))
					{
						dictionary.Add(axis.name, axis);
					}
					else
					{
						Debug.LogWarning(string.Format("Input configuration '{0}' already contains an axis named '{1}'", inputConfiguration2.name, axis.name));
					}
				}
				_axesTable.Add(inputConfiguration2.name, dictionary);
			}
		}

		private void Update()
		{
			UpdateInputConfiguration(_playerOneConfig, PlayerID.One);
			UpdateInputConfiguration(_playerTwoConfig, PlayerID.Two);
			UpdateInputConfiguration(_playerThreeConfig, PlayerID.Three);
			UpdateInputConfiguration(_playerFourConfig, PlayerID.Four);
			if (_playerOneConfig != null)
			{
				if (_scanFlags != 0)
				{
					ScanInput();
				}
			}
			else if (_scanFlags != 0)
			{
				StopInputScan();
			}
		}

		private void UpdateInputConfiguration(InputConfiguration inputConfig, PlayerID playerID)
		{
			if (inputConfig != null)
			{
				for (int i = 0; i < inputConfig.axes.Count; i++)
				{
					inputConfig.axes[i].Update();
				}
				if (this.RemoteUpdate != null)
				{
					this.RemoteUpdate(playerID);
				}
			}
		}

		private void ScanInput()
		{
			float num = ((!ignoreTimescale) ? (Time.time - _scanStartTime) : (Time.realtimeSinceStartup - _scanStartTime));
			if ((!string.IsNullOrEmpty(_cancelScanButton) && GetButtonDown(_cancelScanButton)) || num >= _scanTimeout)
			{
				StopInputScan();
				return;
			}
			bool flag = false;
			if ((_scanFlags & ScanFlags.Key) == ScanFlags.Key)
			{
				flag = ScanKey();
			}
			if (!flag && (_scanFlags & ScanFlags.JoystickButton) == ScanFlags.JoystickButton)
			{
				flag = ScanJoystickButton();
			}
			if (!flag && (_scanFlags & ScanFlags.JoystickAxis) == ScanFlags.JoystickAxis)
			{
				flag = ScanJoystickAxis();
			}
			if (!flag && (_scanFlags & ScanFlags.MouseAxis) == ScanFlags.MouseAxis)
			{
				ScanMouseAxis();
			}
		}

		private bool ScanKey()
		{
			int num = _keys.Length;
			for (int i = 0; i < num && _keys[i] < KeyCode.JoystickButton0; i++)
			{
				if (Input.GetKeyDown(_keys[i]))
				{
					_scanResult.scanFlags = ScanFlags.Key;
					_scanResult.key = _keys[i];
					_scanResult.joystick = -1;
					_scanResult.joystickAxis = -1;
					_scanResult.joystickAxisValue = 0f;
					_scanResult.mouseAxis = -1;
					_scanResult.userData = _scanUserData;
					if (_scanHandler(_scanResult))
					{
						_scanHandler = null;
						_scanResult.userData = null;
						_scanFlags = ScanFlags.None;
						return true;
					}
				}
			}
			return false;
		}

		private bool ScanJoystickButton()
		{
			for (int i = 330; i < 429; i++)
			{
				if (Input.GetKeyDown((KeyCode)i))
				{
					_scanResult.scanFlags = ScanFlags.JoystickButton;
					_scanResult.key = (KeyCode)i;
					_scanResult.joystick = -1;
					_scanResult.joystickAxis = -1;
					_scanResult.joystickAxisValue = 0f;
					_scanResult.mouseAxis = -1;
					_scanResult.userData = _scanUserData;
					if (_scanHandler(_scanResult))
					{
						_scanHandler = null;
						_scanResult.userData = null;
						_scanFlags = ScanFlags.None;
						return true;
					}
				}
			}
			return false;
		}

		private bool ScanJoystickAxis()
		{
			int num = _scanJoystick * 10;
			float num2 = 0f;
			for (int i = 0; i < 10; i++)
			{
				num2 = Input.GetAxisRaw(_rawJoystickAxes[num + i]);
				if (Mathf.Abs(num2) >= 1f)
				{
					_scanResult.scanFlags = ScanFlags.JoystickAxis;
					_scanResult.key = KeyCode.None;
					_scanResult.joystick = _scanJoystick;
					_scanResult.joystickAxis = i;
					_scanResult.joystickAxisValue = num2;
					_scanResult.mouseAxis = -1;
					_scanResult.userData = _scanUserData;
					if (_scanHandler(_scanResult))
					{
						_scanHandler = null;
						_scanResult.userData = null;
						_scanFlags = ScanFlags.None;
						return true;
					}
				}
			}
			return false;
		}

		private bool ScanMouseAxis()
		{
			for (int i = 0; i < _rawMouseAxes.Length; i++)
			{
				if (Mathf.Abs(Input.GetAxis(_rawMouseAxes[i])) > 0f)
				{
					_scanResult.scanFlags = ScanFlags.MouseAxis;
					_scanResult.key = KeyCode.None;
					_scanResult.joystick = -1;
					_scanResult.joystickAxis = -1;
					_scanResult.joystickAxisValue = 0f;
					_scanResult.mouseAxis = i;
					_scanResult.userData = _scanUserData;
					if (_scanHandler(_scanResult))
					{
						_scanHandler = null;
						_scanResult.userData = null;
						_scanFlags = ScanFlags.None;
						return true;
					}
				}
			}
			return false;
		}

		private void StopInputScan()
		{
			_scanResult.scanFlags = ScanFlags.None;
			_scanResult.key = KeyCode.None;
			_scanResult.joystick = -1;
			_scanResult.joystickAxis = -1;
			_scanResult.joystickAxisValue = 0f;
			_scanResult.mouseAxis = -1;
			_scanResult.userData = _scanUserData;
			_scanHandler(_scanResult);
			_scanHandler = null;
			_scanResult.userData = null;
			_scanFlags = ScanFlags.None;
		}

		private void SetInputConfigurationByPlayerID(PlayerID playerID, InputConfiguration inputConfig)
		{
			switch (playerID)
			{
			case PlayerID.One:
				_playerOneConfig = inputConfig;
				break;
			case PlayerID.Two:
				_playerTwoConfig = inputConfig;
				break;
			case PlayerID.Three:
				_playerThreeConfig = inputConfig;
				break;
			case PlayerID.Four:
				_playerFourConfig = inputConfig;
				break;
			}
		}

		private InputConfiguration GetInputConfigurationByPlayerID(PlayerID playerID)
		{
			switch (playerID)
			{
			case PlayerID.One:
				return _playerOneConfig;
			case PlayerID.Two:
				return _playerTwoConfig;
			case PlayerID.Three:
				return _playerThreeConfig;
			case PlayerID.Four:
				return _playerFourConfig;
			default:
				return null;
			}
		}

		private PlayerID? IsInputConfigurationInUse(string name)
		{
			if (_playerOneConfig != null && _playerOneConfig.name == name)
			{
				return PlayerID.One;
			}
			if (_playerTwoConfig != null && _playerTwoConfig.name == name)
			{
				return PlayerID.Two;
			}
			if (_playerThreeConfig != null && _playerThreeConfig.name == name)
			{
				return PlayerID.Three;
			}
			if (_playerFourConfig != null && _playerFourConfig.name == name)
			{
				return PlayerID.Four;
			}
			return null;
		}

		public void Load(SaveLoadParameters parameters)
		{
			if (parameters != null)
			{
				inputConfigurations = parameters.inputConfigurations;
				playerOneDefault = parameters.playerOneDefault;
				playerTwoDefault = parameters.playerTwoDefault;
				playerThreeDefault = parameters.playerThreeDefault;
				playerFourDefault = parameters.playerFourDefault;
			}
		}

		public SaveLoadParameters GetSaveParameters()
		{
			SaveLoadParameters saveLoadParameters = new SaveLoadParameters();
			saveLoadParameters.inputConfigurations = inputConfigurations;
			saveLoadParameters.playerOneDefault = playerOneDefault;
			saveLoadParameters.playerTwoDefault = playerTwoDefault;
			saveLoadParameters.playerThreeDefault = playerThreeDefault;
			saveLoadParameters.playerFourDefault = playerFourDefault;
			return saveLoadParameters;
		}

		private void RaiseInputConfigurationChangedEvent(PlayerID playerID)
		{
			if (this.ConfigurationChanged != null)
			{
				this.ConfigurationChanged(playerID);
			}
		}

		private void RaiseConfigurationDirtyEvent(string configName)
		{
			if (this.ConfigurationDirty != null)
			{
				this.ConfigurationDirty(configName);
			}
		}

		private void RaiseLoadedEvent()
		{
			if (this.Loaded != null)
			{
				this.Loaded();
			}
		}

		private void RaiseSavedEvent()
		{
			if (this.Saved != null)
			{
				this.Saved();
			}
		}

		public static bool AnyInput()
		{
			return AnyInput(_instance._playerOneConfig) || AnyInput(_instance._playerTwoConfig) || AnyInput(_instance._playerThreeConfig) || AnyInput(_instance._playerFourConfig);
		}

		public static bool AnyInput(PlayerID playerID)
		{
			return AnyInput(_instance.GetInputConfigurationByPlayerID(playerID));
		}

		public static bool AnyInput(string inputConfigName)
		{
			InputConfiguration value;
			if (_instance._configurationTable.TryGetValue(inputConfigName, out value))
			{
				int count = value.axes.Count;
				for (int i = 0; i < count; i++)
				{
					if (value.axes[i].AnyInput)
					{
						return true;
					}
				}
			}
			return false;
		}

		private static bool AnyInput(InputConfiguration inputConfig)
		{
			if (inputConfig != null)
			{
				int count = inputConfig.axes.Count;
				for (int i = 0; i < count; i++)
				{
					if (inputConfig.axes[i].AnyInput)
					{
						return true;
					}
				}
			}
			return false;
		}

		[Obsolete("Use the method overload that takes in the input configuration name", true)]
		public static void SetRemoteAxisValue(string axisName, float value)
		{
			SetRemoteAxisValue(_instance._playerOneConfig.name, axisName, value);
		}

		public static void SetRemoteAxisValue(string inputConfigName, string axisName, float value)
		{
			AxisConfiguration axisConfiguration = GetAxisConfiguration(inputConfigName, axisName);
			if (axisConfiguration != null)
			{
				axisConfiguration.SetRemoteAxisValue(value);
			}
			else
			{
				Debug.LogError(string.Format("An axis named '{0}' does not exist in the input configuration named '{1}'", axisName, inputConfigName));
			}
		}

		[Obsolete("Use the method overload that takes in the input configuration name", true)]
		public static void SetRemoteButtonValue(string buttonName, bool down, bool justChanged)
		{
			SetRemoteButtonValue(_instance._playerOneConfig.name, buttonName, down, justChanged);
		}

		public static void SetRemoteButtonValue(string inputConfigName, string buttonName, bool down, bool justChanged)
		{
			AxisConfiguration axisConfiguration = GetAxisConfiguration(inputConfigName, buttonName);
			if (axisConfiguration != null)
			{
				axisConfiguration.SetRemoteButtonValue(down, justChanged);
			}
			else
			{
				Debug.LogError(string.Format("A remote button named '{0}' does not exist in the input configuration named '{1}'", buttonName, inputConfigName));
			}
		}

		public static void Reinitialize()
		{
			_instance.Initialize();
		}

		public static void ResetInputConfiguration(PlayerID playerID)
		{
			InputConfiguration inputConfigurationByPlayerID = _instance.GetInputConfigurationByPlayerID(playerID);
			if (inputConfigurationByPlayerID != null)
			{
				int count = inputConfigurationByPlayerID.axes.Count;
				for (int i = 0; i < count; i++)
				{
					inputConfigurationByPlayerID.axes[i].Reset();
				}
			}
		}

		[Obsolete("Use the method overload that takes in the player ID", true)]
		public static void SetInputConfiguration(string name)
		{
			SetInputConfiguration(name, PlayerID.One);
		}

		public static void SetInputConfiguration(string name, PlayerID playerID)
		{
			PlayerID? playerID2 = _instance.IsInputConfigurationInUse(name);
			if (playerID2.HasValue && playerID2.Value != playerID)
			{
				Debug.LogErrorFormat("The input configuration named '{0}' is already being used by player {1}", name, playerID2.Value.ToString());
			}
			else if (!playerID2.HasValue || playerID2.Value != playerID)
			{
				InputConfiguration value = null;
				if (_instance._configurationTable.TryGetValue(name, out value))
				{
					_instance.SetInputConfigurationByPlayerID(playerID, value);
					ResetInputConfiguration(playerID);
					_instance.RaiseInputConfigurationChangedEvent(playerID);
				}
				else
				{
					Debug.LogError(string.Format("An input configuration named '{0}' does not exist", name));
				}
			}
		}

		public static InputConfiguration GetInputConfiguration(string name)
		{
			InputConfiguration value = null;
			if (_instance._configurationTable.TryGetValue(name, out value))
			{
				return value;
			}
			return null;
		}

		public static InputConfiguration GetInputConfiguration(PlayerID playerID)
		{
			return _instance.GetInputConfigurationByPlayerID(playerID);
		}

		public static AxisConfiguration GetAxisConfiguration(string inputConfigName, string axisName)
		{
			Dictionary<string, AxisConfiguration> value;
			AxisConfiguration value2;
			if (_instance._axesTable.TryGetValue(inputConfigName, out value) && value.TryGetValue(axisName, out value2))
			{
				return value2;
			}
			return null;
		}

		public static AxisConfiguration GetAxisConfiguration(PlayerID playerID, string axisName)
		{
			InputConfiguration inputConfigurationByPlayerID = _instance.GetInputConfigurationByPlayerID(playerID);
			if (inputConfigurationByPlayerID == null)
			{
				return null;
			}
			Dictionary<string, AxisConfiguration> value;
			AxisConfiguration value2;
			if (_instance._axesTable.TryGetValue(inputConfigurationByPlayerID.name, out value) && value.TryGetValue(axisName, out value2))
			{
				return value2;
			}
			return null;
		}

		public static InputConfiguration CreateInputConfiguration(string name)
		{
			if (_instance._configurationTable.ContainsKey(name))
			{
				Debug.LogError(string.Format("An input configuration named '{0}' already exists", name));
				return null;
			}
			InputConfiguration inputConfiguration = new InputConfiguration(name);
			_instance.inputConfigurations.Add(inputConfiguration);
			_instance._configurationTable.Add(name, inputConfiguration);
			_instance._axesTable.Add(name, new Dictionary<string, AxisConfiguration>());
			return inputConfiguration;
		}

		public static bool DeleteInputConfiguration(string name)
		{
			InputConfiguration inputConfiguration = GetInputConfiguration(name);
			if (inputConfiguration == null)
			{
				return false;
			}
			_instance._axesTable.Remove(name);
			_instance._configurationTable.Remove(name);
			_instance.inputConfigurations.Remove(inputConfiguration);
			if (_instance._playerOneConfig.name == inputConfiguration.name)
			{
				_instance._playerOneConfig = null;
			}
			if (_instance._playerTwoConfig.name == inputConfiguration.name)
			{
				_instance._playerTwoConfig = null;
			}
			if (_instance._playerThreeConfig.name == inputConfiguration.name)
			{
				_instance._playerThreeConfig = null;
			}
			if (_instance._playerFourConfig.name == inputConfiguration.name)
			{
				_instance._playerFourConfig = null;
			}
			return true;
		}

		public static AxisConfiguration CreateButton(string inputConfigName, string buttonName, KeyCode primaryKey)
		{
			return CreateButton(inputConfigName, buttonName, primaryKey, KeyCode.None);
		}

		public static AxisConfiguration CreateButton(string inputConfigName, string buttonName, KeyCode primaryKey, KeyCode secondaryKey)
		{
			InputConfiguration inputConfiguration = GetInputConfiguration(inputConfigName);
			if (inputConfiguration == null)
			{
				Debug.LogError(string.Format("An input configuration named '{0}' does not exist", inputConfigName));
				return null;
			}
			if (_instance._axesTable[inputConfigName].ContainsKey(buttonName))
			{
				Debug.LogError(string.Format("The input configuration named {0} already contains an axis configuration named {1}", inputConfigName, buttonName));
				return null;
			}
			AxisConfiguration axisConfiguration = new AxisConfiguration(buttonName);
			axisConfiguration.type = InputType.Button;
			axisConfiguration.positive = primaryKey;
			axisConfiguration.altPositive = secondaryKey;
			axisConfiguration.Initialize();
			inputConfiguration.axes.Add(axisConfiguration);
			Dictionary<string, AxisConfiguration> dictionary = _instance._axesTable[inputConfigName];
			dictionary.Add(buttonName, axisConfiguration);
			return axisConfiguration;
		}

		public static AxisConfiguration CreateDigitalAxis(string inputConfigName, string axisName, KeyCode positive, KeyCode negative, float gravity, float sensitivity)
		{
			return CreateDigitalAxis(inputConfigName, axisName, positive, negative, KeyCode.None, KeyCode.None, gravity, sensitivity);
		}

		public static AxisConfiguration CreateDigitalAxis(string inputConfigName, string axisName, KeyCode positive, KeyCode negative, KeyCode altPositive, KeyCode altNegative, float gravity, float sensitivity)
		{
			InputConfiguration inputConfiguration = GetInputConfiguration(inputConfigName);
			if (inputConfiguration == null)
			{
				Debug.LogError(string.Format("An input configuration named '{0}' does not exist", inputConfigName));
				return null;
			}
			if (_instance._axesTable[inputConfigName].ContainsKey(axisName))
			{
				Debug.LogError(string.Format("The input configuration named {0} already contains an axis configuration named {1}", inputConfigName, axisName));
				return null;
			}
			AxisConfiguration axisConfiguration = new AxisConfiguration(axisName);
			axisConfiguration.type = InputType.DigitalAxis;
			axisConfiguration.positive = positive;
			axisConfiguration.negative = negative;
			axisConfiguration.altPositive = altPositive;
			axisConfiguration.altNegative = altNegative;
			axisConfiguration.gravity = gravity;
			axisConfiguration.sensitivity = sensitivity;
			axisConfiguration.Initialize();
			inputConfiguration.axes.Add(axisConfiguration);
			Dictionary<string, AxisConfiguration> dictionary = _instance._axesTable[inputConfigName];
			dictionary.Add(axisName, axisConfiguration);
			return axisConfiguration;
		}

		public static AxisConfiguration CreateMouseAxis(string inputConfigName, string axisName, int axis, float sensitivity)
		{
			InputConfiguration inputConfiguration = GetInputConfiguration(inputConfigName);
			if (inputConfiguration == null)
			{
				Debug.LogError(string.Format("An input configuration named '{0}' does not exist", inputConfigName));
				return null;
			}
			if (_instance._axesTable[inputConfigName].ContainsKey(axisName))
			{
				Debug.LogError(string.Format("The input configuration named {0} already contains an axis configuration named {1}", inputConfigName, axisName));
				return null;
			}
			if (axis < 0 || axis > 2)
			{
				Debug.LogError("Mouse axis is out of range. Cannot create new mouse axis.");
				return null;
			}
			AxisConfiguration axisConfiguration = new AxisConfiguration(axisName);
			axisConfiguration.type = InputType.MouseAxis;
			axisConfiguration.axis = axis;
			axisConfiguration.sensitivity = sensitivity;
			axisConfiguration.Initialize();
			inputConfiguration.axes.Add(axisConfiguration);
			Dictionary<string, AxisConfiguration> dictionary = _instance._axesTable[inputConfigName];
			dictionary.Add(axisName, axisConfiguration);
			return axisConfiguration;
		}

		public static AxisConfiguration CreateAnalogAxis(string inputConfigName, string axisName, int joystick, int axis, float sensitivity, float deadZone)
		{
			InputConfiguration inputConfiguration = GetInputConfiguration(inputConfigName);
			if (inputConfiguration == null)
			{
				Debug.LogError(string.Format("An input configuration named '{0}' does not exist", inputConfigName));
				return null;
			}
			if (_instance._axesTable[inputConfigName].ContainsKey(axisName))
			{
				Debug.LogError(string.Format("The input configuration named {0} already contains an axis configuration named {1}", inputConfigName, axisName));
				return null;
			}
			if (axis < 0 || axis >= 10)
			{
				Debug.LogError("Joystick axis is out of range. Cannot create new analog axis.");
				return null;
			}
			if (joystick < 0 || joystick >= 4)
			{
				Debug.LogError("Joystick is out of range. Cannot create new analog axis.");
				return null;
			}
			AxisConfiguration axisConfiguration = new AxisConfiguration(axisName);
			axisConfiguration.type = InputType.AnalogAxis;
			axisConfiguration.axis = axis;
			axisConfiguration.joystick = joystick;
			axisConfiguration.deadZone = deadZone;
			axisConfiguration.sensitivity = sensitivity;
			axisConfiguration.Initialize();
			inputConfiguration.axes.Add(axisConfiguration);
			Dictionary<string, AxisConfiguration> dictionary = _instance._axesTable[inputConfigName];
			dictionary.Add(axisName, axisConfiguration);
			return axisConfiguration;
		}

		public static AxisConfiguration CreateRemoteAxis(string inputConfigName, string axisName)
		{
			InputConfiguration inputConfiguration = GetInputConfiguration(inputConfigName);
			if (inputConfiguration == null)
			{
				Debug.LogError(string.Format("An input configuration named '{0}' does not exist", inputConfigName));
				return null;
			}
			if (_instance._axesTable[inputConfigName].ContainsKey(axisName))
			{
				Debug.LogError(string.Format("The input configuration named {0} already contains an axis configuration named {1}", inputConfigName, axisName));
				return null;
			}
			AxisConfiguration axisConfiguration = new AxisConfiguration(axisName);
			axisConfiguration.type = InputType.RemoteAxis;
			axisConfiguration.positive = KeyCode.None;
			axisConfiguration.negative = KeyCode.None;
			axisConfiguration.altPositive = KeyCode.None;
			axisConfiguration.altNegative = KeyCode.None;
			axisConfiguration.Initialize();
			inputConfiguration.axes.Add(axisConfiguration);
			Dictionary<string, AxisConfiguration> dictionary = _instance._axesTable[inputConfigName];
			dictionary.Add(axisName, axisConfiguration);
			return axisConfiguration;
		}

		public static AxisConfiguration CreateRemoteButton(string inputConfigName, string buttonName)
		{
			InputConfiguration inputConfiguration = GetInputConfiguration(inputConfigName);
			if (inputConfiguration == null)
			{
				Debug.LogError(string.Format("An input configuration named '{0}' does not exist", inputConfigName));
				return null;
			}
			if (_instance._axesTable[inputConfigName].ContainsKey(buttonName))
			{
				Debug.LogError(string.Format("The input configuration named {0} already contains an axis configuration named {1}", inputConfigName, buttonName));
				return null;
			}
			AxisConfiguration axisConfiguration = new AxisConfiguration(buttonName);
			axisConfiguration.type = InputType.RemoteButton;
			axisConfiguration.positive = KeyCode.None;
			axisConfiguration.negative = KeyCode.None;
			axisConfiguration.altPositive = KeyCode.None;
			axisConfiguration.altNegative = KeyCode.None;
			axisConfiguration.Initialize();
			inputConfiguration.axes.Add(axisConfiguration);
			Dictionary<string, AxisConfiguration> dictionary = _instance._axesTable[inputConfigName];
			dictionary.Add(buttonName, axisConfiguration);
			return axisConfiguration;
		}

		public static AxisConfiguration CreateAnalogButton(string inputConfigName, string buttonName, int joystick, int axis)
		{
			InputConfiguration inputConfiguration = GetInputConfiguration(inputConfigName);
			if (inputConfiguration == null)
			{
				Debug.LogError(string.Format("An input configuration named '{0}' does not exist", inputConfigName));
				return null;
			}
			if (_instance._axesTable[inputConfigName].ContainsKey(buttonName))
			{
				Debug.LogError(string.Format("The input configuration named {0} already contains an axis configuration named {1}", inputConfigName, buttonName));
				return null;
			}
			if (axis < 0 || axis >= 10)
			{
				Debug.LogError("Joystick axis is out of range. Cannot create new analog button.");
				return null;
			}
			if (joystick < 0 || joystick >= 4)
			{
				Debug.LogError("Joystick is out of range. Cannot create new analog button.");
				return null;
			}
			AxisConfiguration axisConfiguration = new AxisConfiguration(buttonName);
			axisConfiguration.type = InputType.AnalogButton;
			axisConfiguration.joystick = joystick;
			axisConfiguration.axis = axis;
			axisConfiguration.positive = KeyCode.None;
			axisConfiguration.negative = KeyCode.None;
			axisConfiguration.altPositive = KeyCode.None;
			axisConfiguration.altNegative = KeyCode.None;
			axisConfiguration.Initialize();
			inputConfiguration.axes.Add(axisConfiguration);
			Dictionary<string, AxisConfiguration> dictionary = _instance._axesTable[inputConfigName];
			dictionary.Add(buttonName, axisConfiguration);
			return axisConfiguration;
		}

		public static AxisConfiguration CreateEmptyAxis(string inputConfigName, string axisName)
		{
			InputConfiguration inputConfiguration = GetInputConfiguration(inputConfigName);
			if (inputConfiguration == null)
			{
				Debug.LogError(string.Format("An input configuration named '{0}' does not exist", inputConfigName));
				return null;
			}
			if (_instance._axesTable[inputConfigName].ContainsKey(axisName))
			{
				Debug.LogError(string.Format("The input configuration named {0} already contains an axis configuration named {1}", inputConfigName, axisName));
				return null;
			}
			AxisConfiguration axisConfiguration = new AxisConfiguration(axisName);
			axisConfiguration.Initialize();
			inputConfiguration.axes.Add(axisConfiguration);
			Dictionary<string, AxisConfiguration> dictionary = _instance._axesTable[inputConfigName];
			dictionary.Add(axisName, axisConfiguration);
			return axisConfiguration;
		}

		public static bool DeleteAxisConfiguration(string inputConfigName, string axisName)
		{
			InputConfiguration inputConfiguration = GetInputConfiguration(inputConfigName);
			AxisConfiguration axisConfiguration = GetAxisConfiguration(inputConfigName, axisName);
			if (inputConfiguration != null && axisConfiguration != null)
			{
				_instance._axesTable[inputConfiguration.name].Remove(axisConfiguration.name);
				inputConfiguration.axes.Remove(axisConfiguration);
				return true;
			}
			return false;
		}

		public static void StartKeyScan(KeyScanHandler scanHandler, float timeout, string cancelScanButton, params object[] userData)
		{
			_003CStartKeyScan_003Ec__AnonStorey0 _003CStartKeyScan_003Ec__AnonStorey = new _003CStartKeyScan_003Ec__AnonStorey0();
			_003CStartKeyScan_003Ec__AnonStorey.scanHandler = scanHandler;
			if (_instance._scanFlags != 0)
			{
				_instance.StopInputScan();
			}
			_instance._scanTimeout = timeout;
			_instance._scanFlags = (ScanFlags)6;
			_instance._scanStartTime = ((!_instance.ignoreTimescale) ? Time.time : Time.realtimeSinceStartup);
			_instance._cancelScanButton = cancelScanButton;
			_instance._scanUserData = userData;
			_instance._scanHandler = _003CStartKeyScan_003Ec__AnonStorey._003C_003Em__0;
		}

		public static void StartMouseAxisScan(AxisScanHandler scanHandler, float timeout, string cancelScanButton, params object[] userData)
		{
			_003CStartMouseAxisScan_003Ec__AnonStorey1 _003CStartMouseAxisScan_003Ec__AnonStorey = new _003CStartMouseAxisScan_003Ec__AnonStorey1();
			_003CStartMouseAxisScan_003Ec__AnonStorey.scanHandler = scanHandler;
			if (_instance._scanFlags != 0)
			{
				_instance.StopInputScan();
			}
			_instance._scanTimeout = timeout;
			_instance._scanFlags = ScanFlags.MouseAxis;
			_instance._scanStartTime = ((!_instance.ignoreTimescale) ? Time.time : Time.realtimeSinceStartup);
			_instance._cancelScanButton = cancelScanButton;
			_instance._scanUserData = userData;
			_instance._scanHandler = _003CStartMouseAxisScan_003Ec__AnonStorey._003C_003Em__0;
		}

		public static void StartJoystickAxisScan(AxisScanHandler scanHandler, int joystick, float timeout, string cancelScanButton, params object[] userData)
		{
			_003CStartJoystickAxisScan_003Ec__AnonStorey2 _003CStartJoystickAxisScan_003Ec__AnonStorey = new _003CStartJoystickAxisScan_003Ec__AnonStorey2();
			_003CStartJoystickAxisScan_003Ec__AnonStorey.scanHandler = scanHandler;
			if (joystick < 0 || joystick >= 10)
			{
				Debug.LogError("Joystick is out of range. Cannot start joystick axis scan.");
				return;
			}
			if (_instance._scanFlags != 0)
			{
				_instance.StopInputScan();
			}
			_instance._scanTimeout = timeout;
			_instance._scanFlags = ScanFlags.JoystickAxis;
			_instance._scanStartTime = ((!_instance.ignoreTimescale) ? Time.time : Time.realtimeSinceStartup);
			_instance._cancelScanButton = cancelScanButton;
			_instance._scanJoystick = joystick;
			_instance._scanUserData = userData;
			_instance._scanHandler = _003CStartJoystickAxisScan_003Ec__AnonStorey._003C_003Em__0;
		}

		public static void StartScan(ScanSettings settings, ScanHandler scanHandler)
		{
			if (settings.joystick < 0 || settings.joystick >= 10)
			{
				Debug.LogError("Joystick is out of range. Cannot start scan.");
				return;
			}
			if (_instance._scanFlags != 0)
			{
				_instance.StopInputScan();
			}
			_instance._scanTimeout = settings.timeout;
			_instance._scanFlags = settings.scanFlags;
			_instance._scanStartTime = ((!_instance.ignoreTimescale) ? Time.time : Time.realtimeSinceStartup);
			_instance._cancelScanButton = settings.cancelScanButton;
			_instance._scanJoystick = settings.joystick;
			_instance._scanUserData = settings.userData;
			_instance._scanHandler = scanHandler;
		}

		public static void CancelScan()
		{
			if (_instance._scanFlags != 0)
			{
				_instance.StopInputScan();
			}
		}

		public static void SetConfigurationDirty(string inputConfigName)
		{
			_instance.RaiseConfigurationDirtyEvent(inputConfigName);
		}

		public static void Save()
		{
			string filename = Path.Combine(Application.persistentDataPath, "input_config.xml");
			Save(new InputSaverXML(filename));
		}

		public static void Save(string filename)
		{
			Save(new InputSaverXML(filename));
		}

		public static void Save(IInputSaver inputSaver)
		{
			if (inputSaver != null)
			{
				inputSaver.Save(_instance.GetSaveParameters());
				_instance.RaiseSavedEvent();
			}
			else
			{
				Debug.LogError("InputSaver is null. Cannot save input configurations.");
			}
		}

		public static void Load()
		{
			string text = Path.Combine(Application.persistentDataPath, "input_config.xml");
			if (File.Exists(text))
			{
				Load(new InputLoaderXML(text));
			}
		}

		public static void Load(string filename)
		{
			Load(new InputLoaderXML(filename));
		}

		public static void Load(IInputLoader inputLoader)
		{
			if (inputLoader != null)
			{
				_instance.Load(inputLoader.Load());
				_instance.Initialize();
				_instance.RaiseLoadedEvent();
			}
			else
			{
				Debug.LogError("InputLoader is null. Cannot load input configurations.");
			}
		}

		public static AccelerationEvent GetAccelerationEvent(int index)
		{
			return Input.GetAccelerationEvent(index);
		}

		public static float GetAxis(string name, PlayerID playerID = PlayerID.One)
		{
			AxisConfiguration axisConfiguration = GetAxisConfiguration(playerID, name);
			if (axisConfiguration != null)
			{
				return axisConfiguration.GetAxis();
			}
			Debug.LogError(string.Format("An axis named '{0}' does not exist in the active input configuration for player {1}", name, playerID));
			return 0f;
		}

		public static float GetAxisRaw(string name, PlayerID playerID = PlayerID.One)
		{
			AxisConfiguration axisConfiguration = GetAxisConfiguration(playerID, name);
			if (axisConfiguration != null)
			{
				return axisConfiguration.GetAxisRaw();
			}
			Debug.LogError(string.Format("An axis named '{0}' does not exist in the active input configuration for player {1}", name, playerID));
			return 0f;
		}

		public static bool GetButton(string name, PlayerID playerID = PlayerID.One)
		{
			AxisConfiguration axisConfiguration = GetAxisConfiguration(playerID, name);
			if (axisConfiguration != null)
			{
				return axisConfiguration.GetButton();
			}
			Debug.LogError(string.Format("An button named '{0}' does not exist in the active input configuration for player {1}", name, playerID));
			return false;
		}

		public static bool GetButtonDown(string name, PlayerID playerID = PlayerID.One)
		{
			AxisConfiguration axisConfiguration = GetAxisConfiguration(playerID, name);
			if (axisConfiguration != null)
			{
				return axisConfiguration.GetButtonDown();
			}
			Debug.LogError(string.Format("An button named '{0}' does not exist in the active input configuration for player {1}", name, playerID));
			return false;
		}

		public static bool GetButtonUp(string name, PlayerID playerID = PlayerID.One)
		{
			AxisConfiguration axisConfiguration = GetAxisConfiguration(playerID, name);
			if (axisConfiguration != null)
			{
				return axisConfiguration.GetButtonUp();
			}
			Debug.LogError(string.Format("An button named '{0}' does not exist in the active input configuration for player {1}", name, playerID));
			return false;
		}

		public static bool GetKey(KeyCode key)
		{
			return Input.GetKey(key);
		}

		public static bool GetKeyDown(KeyCode key)
		{
			return Input.GetKeyDown(key);
		}

		public static bool GetKeyUp(KeyCode key)
		{
			return Input.GetKeyUp(key);
		}

		public static bool GetMouseButton(int index)
		{
			return Input.GetMouseButton(index);
		}

		public static bool GetMouseButtonDown(int index)
		{
			return Input.GetMouseButtonDown(index);
		}

		public static bool GetMouseButtonUp(int index)
		{
			return Input.GetMouseButtonUp(index);
		}

		public static Touch GetTouch(int index)
		{
			return Input.GetTouch(index);
		}

		public static string[] GetJoystickNames()
		{
			return Input.GetJoystickNames();
		}

		public static void ResetInputAxes()
		{
			Input.ResetInputAxes();
		}
	}
}
