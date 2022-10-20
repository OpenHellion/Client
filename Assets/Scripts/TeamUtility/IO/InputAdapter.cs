using System;
using System.Collections;
using UnityEngine;

namespace TeamUtility.IO
{
	public sealed class InputAdapter : MonoBehaviour
	{
		[SerializeField]
		private bool dontDestroyOnLoad;

		[SerializeField]
		private bool allowRealtimeInputDeviceSwitch = true;

		[SerializeField]
		[Range(0.1f, 1f)]
		private float updateInputDeviceInterval = 1f;

		[SerializeField]
		[Range(1f, 5f)]
		private float updateJoystickCountInterval = 1f;

		[SerializeField]
		private string keyboardConfiguration = "KeyboardAndMouse";

		[SerializeField]
		private string windowsJoystickConfiguration = "Windows_Gamepad";

		[SerializeField]
		private string linuxJoystickConfiguration = "Linux_Gamepad";

		[SerializeField]
		private string leftTriggerAxis = "LeftTrigger";

		[SerializeField]
		private string rightTriggerAxis = "RightTrigger";

		[SerializeField]
		private string dpadHorizontalAxis = "DPADHorizontal";

		[SerializeField]
		private string dpadVerticalAxis = "DPADVertical";

		private Vector2 _lastDpadValues = Vector2.zero;

		private Vector2 _currentDpadValues = Vector2.zero;

		private Vector2 _lastTriggerValues = Vector2.zero;

		private Vector2 _currentTriggerValues = Vector2.zero;

		private InputDevice _inputDevice;

		private int _joystickCount;

		private int _firstJoystickKey = 330;

		private bool _canUpdateInputDevice;

		private string _joystickConfiguration;

		private string _keyboardConfiguration;

		private static InputAdapter _instance;

		public static InputAdapter Instance
		{
			get
			{
				return _instance;
			}
		}

		public static InputDevice inputDevice
		{
			get
			{
				return _instance._inputDevice;
			}
			set
			{
				if (value != _instance._inputDevice)
				{
					if (value == InputDevice.Joystick && _instance._joystickCount > 0)
					{
						_instance.SetInputDevice(InputDevice.Joystick);
					}
					else
					{
						_instance.SetInputDevice(InputDevice.KeyboardAndMouse);
					}
				}
			}
		}

		public static string KeyboardConfiguration
		{
			get
			{
				return _instance._keyboardConfiguration;
			}
		}

		public static string JoystickConfiguration
		{
			get
			{
				return _instance._joystickConfiguration;
			}
		}

		public static Vector3 mousePosition
		{
			get
			{
				return InputManager.mousePosition;
			}
		}

		public event Action<InputDevice> InputDeviceChanged;

		public static float GetAxis(string axisName)
		{
			return InputManager.GetAxis(axisName);
		}

		public static float GetTriggerAxis(InputTriggerAxis axis)
		{
			if (_instance._inputDevice == InputDevice.KeyboardAndMouse)
			{
				return 0f;
			}
			if (axis == InputTriggerAxis.Left)
			{
				return InputManager.GetAxis(_instance.leftTriggerAxis);
			}
			return InputManager.GetAxis(_instance.rightTriggerAxis);
		}

		public static float GetDPADAxis(InputDPADAxis axis)
		{
			if (_instance._inputDevice == InputDevice.KeyboardAndMouse)
			{
				return 0f;
			}
			if (axis == InputDPADAxis.Horizontal)
			{
				return InputManager.GetAxis(_instance.dpadHorizontalAxis);
			}
			return InputManager.GetAxis(_instance.dpadVerticalAxis);
		}

		public static bool GetButton(string buttonName)
		{
			return InputManager.GetButton(buttonName);
		}

		public static bool GetButtonDown(string buttonName)
		{
			return InputManager.GetButtonDown(buttonName);
		}

		public static bool GetButtonUp(string buttonName)
		{
			return InputManager.GetButtonUp(buttonName);
		}

		public static bool GetMouseButton(int button)
		{
			return InputManager.GetMouseButton(button);
		}

		public static bool GetMouseButtonDown(int button)
		{
			return InputManager.GetMouseButtonDown(button);
		}

		public static bool GetMouseButtonUp(int button)
		{
			return InputManager.GetMouseButtonUp(button);
		}

		public static bool GetDPADButton(InputDPADButton button)
		{
			if (_instance._inputDevice == InputDevice.KeyboardAndMouse)
			{
				return false;
			}
			if (button == InputDPADButton.Any)
			{
				return !Mathf.Approximately(_instance._currentDpadValues.x, 0f) || !Mathf.Approximately(_instance._currentDpadValues.y, 0f);
			}
			bool result = false;
			switch (button)
			{
			case InputDPADButton.Left_Up:
				result = _instance._currentDpadValues.x <= -1f && _instance._currentDpadValues.y >= 1f;
				break;
			case InputDPADButton.Right_Up:
				result = _instance._currentDpadValues.x >= 1f && _instance._currentDpadValues.y >= 1f;
				break;
			case InputDPADButton.Left_Down:
				result = _instance._currentDpadValues.x <= -1f && _instance._currentDpadValues.y <= -1f;
				break;
			case InputDPADButton.Right_Down:
				result = _instance._currentDpadValues.x >= 1f && _instance._currentDpadValues.y <= -1f;
				break;
			case InputDPADButton.Left:
				result = _instance._currentDpadValues.x <= -1f;
				break;
			case InputDPADButton.Right:
				result = _instance._currentDpadValues.x >= 1f;
				break;
			case InputDPADButton.Up:
				result = _instance._currentDpadValues.y >= 1f;
				break;
			case InputDPADButton.Down:
				result = _instance._currentDpadValues.y <= -1f;
				break;
			}
			return result;
		}

		public static bool GetDPADButtonDown(InputDPADButton button)
		{
			if (_instance._inputDevice == InputDevice.KeyboardAndMouse)
			{
				return false;
			}
			if (button == InputDPADButton.Any)
			{
				return (!Mathf.Approximately(_instance._currentDpadValues.x, 0f) && Mathf.Approximately(_instance._lastDpadValues.x, 0f)) || (!Mathf.Approximately(_instance._currentDpadValues.y, 0f) && Mathf.Approximately(_instance._lastDpadValues.y, 0f));
			}
			bool result = false;
			switch (button)
			{
			case InputDPADButton.Left_Up:
				result = _instance._currentDpadValues.x <= -1f && _instance._lastDpadValues.x > -1f && _instance._currentDpadValues.y >= 1f && _instance._lastDpadValues.y < 1f;
				break;
			case InputDPADButton.Right_Up:
				result = _instance._currentDpadValues.x >= 1f && _instance._lastDpadValues.x < 1f && _instance._currentDpadValues.y >= 1f && _instance._lastDpadValues.y < 1f;
				break;
			case InputDPADButton.Left_Down:
				result = _instance._currentDpadValues.x <= -1f && _instance._lastDpadValues.x > -1f && _instance._currentDpadValues.y <= -1f && _instance._lastDpadValues.y > -1f;
				break;
			case InputDPADButton.Right_Down:
				result = _instance._currentDpadValues.x >= 1f && _instance._lastDpadValues.x < 1f && _instance._currentDpadValues.y <= -1f && _instance._lastDpadValues.y > -1f;
				break;
			case InputDPADButton.Left:
				result = _instance._currentDpadValues.x <= -1f && _instance._lastDpadValues.x > -1f;
				break;
			case InputDPADButton.Right:
				result = _instance._currentDpadValues.x >= 1f && _instance._lastDpadValues.x < 1f;
				break;
			case InputDPADButton.Up:
				result = _instance._currentDpadValues.y >= 1f && _instance._lastDpadValues.y < 1f;
				break;
			case InputDPADButton.Down:
				result = _instance._currentDpadValues.y <= -1f && _instance._lastDpadValues.y > -1f;
				break;
			}
			return result;
		}

		public static bool GetDPADButtonUp(InputDPADButton button)
		{
			if (_instance._inputDevice == InputDevice.KeyboardAndMouse)
			{
				return false;
			}
			if (button == InputDPADButton.Any)
			{
				return (Mathf.Approximately(_instance._currentDpadValues.x, 0f) && !Mathf.Approximately(_instance._lastDpadValues.x, 0f)) || (Mathf.Approximately(_instance._currentDpadValues.y, 0f) && !Mathf.Approximately(_instance._lastDpadValues.y, 0f));
			}
			bool result = false;
			switch (button)
			{
			case InputDPADButton.Left_Up:
				result = _instance._currentDpadValues.x > -1f && _instance._lastDpadValues.x <= -1f && _instance._currentDpadValues.y < 1f && _instance._lastDpadValues.y >= 1f;
				break;
			case InputDPADButton.Right_Up:
				result = _instance._currentDpadValues.x < 1f && _instance._lastDpadValues.x >= 1f && _instance._currentDpadValues.y < 1f && _instance._lastDpadValues.y >= 1f;
				break;
			case InputDPADButton.Left_Down:
				result = _instance._currentDpadValues.x > -1f && _instance._lastDpadValues.x <= -1f && _instance._currentDpadValues.y > -1f && _instance._lastDpadValues.y <= -1f;
				break;
			case InputDPADButton.Right_Down:
				result = _instance._currentDpadValues.x < 1f && _instance._lastDpadValues.x >= 1f && _instance._currentDpadValues.y > -1f && _instance._lastDpadValues.y <= -1f;
				break;
			case InputDPADButton.Left:
				result = _instance._currentDpadValues.x > -1f && _instance._lastDpadValues.x <= -1f;
				break;
			case InputDPADButton.Right:
				result = _instance._currentDpadValues.x < 1f && _instance._lastDpadValues.x >= 1f;
				break;
			case InputDPADButton.Up:
				result = _instance._currentDpadValues.y < 1f && _instance._lastDpadValues.y >= 1f;
				break;
			case InputDPADButton.Down:
				result = _instance._currentDpadValues.y > -1f && _instance._lastDpadValues.y <= -1f;
				break;
			}
			return result;
		}

		public static bool GetTriggerButton(InputTriggerButton button)
		{
			if (_instance._inputDevice == InputDevice.KeyboardAndMouse)
			{
				return false;
			}
			switch (button)
			{
			case InputTriggerButton.Left:
				return _instance._currentTriggerValues.x >= 1f;
			case InputTriggerButton.Right:
				return _instance._currentTriggerValues.y >= 1f;
			default:
				return _instance._currentTriggerValues.x >= 1f || _instance._currentTriggerValues.y >= 1f;
			}
		}

		public static bool GetTriggerButtonDown(InputTriggerButton button)
		{
			if (_instance._inputDevice == InputDevice.KeyboardAndMouse)
			{
				return false;
			}
			switch (button)
			{
			case InputTriggerButton.Left:
				return _instance._currentTriggerValues.x >= 1f && _instance._lastTriggerValues.x < 1f;
			case InputTriggerButton.Right:
				return _instance._currentTriggerValues.y >= 1f && _instance._lastTriggerValues.y < 1f;
			default:
				return (_instance._currentTriggerValues.x >= 1f && _instance._lastTriggerValues.x < 1f) || (_instance._currentTriggerValues.y >= 1f && _instance._lastTriggerValues.y < 1f);
			}
		}

		public static bool GetTriggerButtonUp(InputTriggerButton button)
		{
			if (_instance._inputDevice == InputDevice.KeyboardAndMouse)
			{
				return false;
			}
			switch (button)
			{
			case InputTriggerButton.Left:
				return _instance._currentTriggerValues.x < 1f && _instance._lastTriggerValues.x >= 1f;
			case InputTriggerButton.Right:
				return _instance._currentTriggerValues.y < 1f && _instance._lastTriggerValues.y >= 1f;
			default:
				return (_instance._currentTriggerValues.x < 1f && _instance._lastTriggerValues.x >= 1f) || (_instance._currentTriggerValues.y < 1f && _instance._lastTriggerValues.y >= 1f);
			}
		}

		public static void ResetInputAxes()
		{
			InputManager.ResetInputAxes();
		}

		public static string[] GetJoystickNames()
		{
			return InputManager.GetJoystickNames();
		}

		public static bool IsUsingJoystick()
		{
			return inputDevice == InputDevice.Joystick;
		}

		public static bool IsUsingKeyboardAndMouse()
		{
			return inputDevice == InputDevice.KeyboardAndMouse;
		}

		private void Awake()
		{
			if (_instance != null)
			{
				UnityEngine.Object.Destroy(this);
				return;
			}
			SetInputManagerConfigurations();
			SetInputDevice(InputDevice.KeyboardAndMouse);
			_instance = this;
			_joystickCount = InputManager.GetJoystickNames().Length;
			if (dontDestroyOnLoad)
			{
				UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			}
		}

		private void Start()
		{
			StartCoroutine(UpdateJoystickCount());
			if (allowRealtimeInputDeviceSwitch)
			{
				StartCoroutine(SetCanUpdateInputDevice());
			}
		}

		private void Update()
		{
			if (_canUpdateInputDevice)
			{
				UpdateInputDevice();
				_canUpdateInputDevice = false;
			}
			UpdateTriggerAndDPAD();
		}

		private void UpdateTriggerAndDPAD()
		{
			if (_inputDevice == InputDevice.Joystick)
			{
				_lastDpadValues = _currentDpadValues;
				_currentDpadValues.x = InputManager.GetAxis(_instance.dpadHorizontalAxis);
				_currentDpadValues.y = InputManager.GetAxis(_instance.dpadVerticalAxis);
				_lastTriggerValues = _currentTriggerValues;
				_currentTriggerValues.x = InputManager.GetAxis(_instance.leftTriggerAxis);
				_currentTriggerValues.y = InputManager.GetAxis(_instance.rightTriggerAxis);
			}
			else
			{
				_lastDpadValues = Vector2.zero;
				_currentDpadValues = Vector2.zero;
				_lastTriggerValues = Vector2.zero;
				_lastTriggerValues = Vector2.zero;
			}
		}

		private IEnumerator UpdateJoystickCount()
		{
			while (true)
			{
				_joystickCount = InputManager.GetJoystickNames().Length;
				if (_inputDevice == InputDevice.Joystick && _joystickCount == 0)
				{
					Debug.LogWarning("Lost connection with joystick. Switching to keyboard and mouse input.");
					SetInputDevice(InputDevice.KeyboardAndMouse);
				}
				yield return new WaitForSeconds(updateJoystickCountInterval);
			}
		}

		private IEnumerator SetCanUpdateInputDevice()
		{
			while (true)
			{
				_canUpdateInputDevice = true;
				yield return new WaitForSeconds(updateInputDeviceInterval);
			}
		}

		private void UpdateInputDevice()
		{
			bool flag = false;
			if (_inputDevice == InputDevice.Joystick)
			{
				if (InputManager.anyKey)
				{
					for (int i = 0; i <= 19; i++)
					{
						if (InputManager.GetKey((KeyCode)(_firstJoystickKey + i)))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						SetInputDevice(InputDevice.KeyboardAndMouse);
					}
				}
				else if (InputManager.AnyInput(_keyboardConfiguration))
				{
					SetInputDevice(InputDevice.KeyboardAndMouse);
				}
			}
			else if (InputManager.anyKey)
			{
				for (int j = 0; j <= 19; j++)
				{
					if (InputManager.GetKey((KeyCode)(_firstJoystickKey + j)))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					SetInputDevice(InputDevice.Joystick);
				}
			}
			else if (InputManager.AnyInput(_joystickConfiguration))
			{
				SetInputDevice(InputDevice.Joystick);
			}
		}

		private void SetInputDevice(InputDevice inpuDevice)
		{
			_inputDevice = inpuDevice;
			if (inpuDevice == InputDevice.Joystick)
			{
				Cursor.visible = false;
				Debug.Log("Current Input Device: Joystick");
			}
			else
			{
				Cursor.visible = true;
				Debug.Log("Current Input Device: KeyboardAndMouse");
			}
			InputManager.ResetInputAxes();
			RaiseInputDeviceChangedEvent();
		}

		private void SetInputManagerConfigurations()
		{
			_keyboardConfiguration = keyboardConfiguration;
			switch (Application.platform)
			{
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.WindowsPlayer:
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.LinuxPlayer:
				_joystickConfiguration = linuxJoystickConfiguration;
				break;
			default:
				Debug.LogWarning("Unsupported XBOX 360 Controller driver. The default Windows driver configuration has been chosen.");
				_joystickConfiguration = windowsJoystickConfiguration;
				break;
			}
		}

		private void RaiseInputDeviceChangedEvent()
		{
			if (this.InputDeviceChanged != null)
			{
				this.InputDeviceChanged(_inputDevice);
			}
		}

		private void OnDestroy()
		{
			Cursor.visible = true;
			StopAllCoroutines();
		}
	}
}
