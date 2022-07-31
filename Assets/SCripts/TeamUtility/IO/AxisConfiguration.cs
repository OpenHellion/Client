using System;
using Newtonsoft.Json;
using UnityEngine;

namespace TeamUtility.IO
{
	[Serializable]
	public sealed class AxisConfiguration
	{
		private enum ButtonState
		{
			Pressed = 0,
			JustPressed = 1,
			Released = 2,
			JustReleased = 3
		}

		public const float Neutral = 0f;

		public const float Positive = 1f;

		public const float Negative = -1f;

		public const int MaxMouseAxes = 3;

		public const int MaxJoystickAxes = 10;

		public const int MaxJoysticks = 4;

		public string name;

		public string description;

		public KeyCode positive;

		public KeyCode negative;

		public KeyCode altPositive;

		public KeyCode altNegative;

		public float deadZone;

		public bool ModPositive;

		public bool ModAltPositive;

		public bool ModNegative;

		public bool ModAltNegative;

		public KeyCode ModifierKey = KeyCode.X;

		public float gravity = 1f;

		public float sensitivity = 1f;

		public bool snap;

		public bool invert;

		public InputType type = InputType.DigitalAxis;

		public int axis;

		public int joystick;

		private string _rawAxisName;

		private float _value;

		private int _lastAxis;

		private int _lastJoystick;

		private InputType _lastType;

		private float _lastUpdateTime;

		private float _deltaTime;

		private ButtonState _remoteButtonState;

		private ButtonState _analogButtonState;

		[JsonIgnore]
		public bool AnyInput
		{
			get
			{
				if (type == InputType.Button)
				{
					return Input.GetKey(positive) || Input.GetKey(altPositive);
				}
				if (type == InputType.RemoteButton)
				{
					return _remoteButtonState == ButtonState.Pressed || _remoteButtonState == ButtonState.JustPressed;
				}
				if (type == InputType.AnalogButton)
				{
					return _analogButtonState == ButtonState.Pressed || _analogButtonState == ButtonState.JustPressed;
				}
				if (type == InputType.DigitalAxis || type == InputType.RemoteAxis)
				{
					return Mathf.Abs(_value) >= 1f;
				}
				return Mathf.Abs(Input.GetAxisRaw(_rawAxisName)) >= 1f;
			}
		}

		[JsonIgnore]
		public bool AnyKey
		{
			get
			{
				return Input.GetKey(positive) || Input.GetKey(altPositive) || Input.GetKey(negative) || Input.GetKey(altNegative);
			}
		}

		[JsonIgnore]
		public bool AnyKeyDown
		{
			get
			{
				return Input.GetKeyDown(positive) || Input.GetKeyDown(altPositive) || Input.GetKeyDown(negative) || Input.GetKeyDown(altNegative);
			}
		}

		[JsonIgnore]
		public bool AnyKeyUp
		{
			get
			{
				return Input.GetKeyUp(positive) || Input.GetKeyUp(altPositive) || Input.GetKeyUp(negative) || Input.GetKeyUp(altNegative);
			}
		}

		public AxisConfiguration()
			: this("New Axis")
		{
		}

		public AxisConfiguration(string name)
		{
			this.name = name;
			description = string.Empty;
			positive = KeyCode.None;
			altPositive = KeyCode.None;
			negative = KeyCode.None;
			altNegative = KeyCode.None;
			type = InputType.Button;
			gravity = 1f;
			sensitivity = 1f;
		}

		public void Initialize()
		{
			UpdateRawAxisName();
			_value = 0f;
			_lastUpdateTime = Time.realtimeSinceStartup;
			_remoteButtonState = ButtonState.Released;
			_analogButtonState = ButtonState.Released;
		}

		public void Update()
		{
			_deltaTime = ((!InputManager.IgnoreTimescale) ? Time.deltaTime : (Time.realtimeSinceStartup - _lastUpdateTime));
			_lastUpdateTime = Time.realtimeSinceStartup;
			if (_lastType != type || _lastAxis != axis || _lastJoystick != joystick)
			{
				if (_lastType != type && (type == InputType.DigitalAxis || type == InputType.RemoteAxis))
				{
					_value = 0f;
				}
				UpdateRawAxisName();
				_lastType = type;
				_lastAxis = axis;
				_lastJoystick = joystick;
			}
			bool flag = (Input.GetKey(positive) || Input.GetKey(altPositive)) && (Input.GetKey(negative) || Input.GetKey(altNegative));
			if (type == InputType.DigitalAxis && !flag)
			{
				UpdateDigitalAxisValue();
			}
			if (type == InputType.AnalogButton)
			{
				UpdateAnalogButtonValue();
			}
		}

		private void UpdateDigitalAxisValue()
		{
			if (Input.GetKey(positive) || Input.GetKey(altPositive))
			{
				if (_value < 0f && snap)
				{
					_value = 0f;
				}
				_value += sensitivity * _deltaTime;
				if (_value > 1f)
				{
					_value = 1f;
				}
			}
			else if (Input.GetKey(negative) || Input.GetKey(altNegative))
			{
				if (_value > 0f && snap)
				{
					_value = 0f;
				}
				_value -= sensitivity * _deltaTime;
				if (_value < -1f)
				{
					_value = -1f;
				}
			}
			else if (_value < 0f)
			{
				_value += gravity * _deltaTime;
				if (_value > 0f)
				{
					_value = 0f;
				}
			}
			else if (_value > 0f)
			{
				_value -= gravity * _deltaTime;
				if (_value < 0f)
				{
					_value = 0f;
				}
			}
		}

		private void UpdateAnalogButtonValue()
		{
			float num = Input.GetAxisRaw(_rawAxisName) * (float)((!invert) ? 1 : (-1));
			if (num >= 1f)
			{
				if (_analogButtonState == ButtonState.Released || _analogButtonState == ButtonState.JustReleased)
				{
					_analogButtonState = ButtonState.JustPressed;
				}
				else if (_analogButtonState == ButtonState.JustPressed)
				{
					_analogButtonState = ButtonState.Pressed;
				}
			}
			else if (_analogButtonState == ButtonState.Pressed || _analogButtonState == ButtonState.JustPressed)
			{
				_analogButtonState = ButtonState.JustReleased;
			}
			else if (_analogButtonState == ButtonState.JustReleased)
			{
				_analogButtonState = ButtonState.Released;
			}
		}

		public float GetAxis()
		{
			float num = 0f;
			if (type == InputType.DigitalAxis || type == InputType.RemoteAxis)
			{
				num = _value;
			}
			else if (type == InputType.MouseAxis)
			{
				if (_rawAxisName != null)
				{
					num = Input.GetAxis(_rawAxisName) * sensitivity;
				}
			}
			else if (type == InputType.AnalogAxis && _rawAxisName != null)
			{
				num = Mathf.Clamp(Input.GetAxis(_rawAxisName) * sensitivity, -1f, 1f);
				if (num > 0f - deadZone && num < deadZone)
				{
					num = 0f;
				}
			}
			return (!invert) ? num : (0f - num);
		}

		public float GetAxisRaw()
		{
			float num = 0f;
			if (type == InputType.DigitalAxis)
			{
				if (Input.GetKey(positive) || Input.GetKey(altPositive))
				{
					num = 1f;
				}
				else if (Input.GetKey(negative) || Input.GetKey(altNegative))
				{
					num = -1f;
				}
			}
			else if ((type == InputType.MouseAxis || type == InputType.AnalogAxis) && _rawAxisName != null)
			{
				num = Input.GetAxisRaw(_rawAxisName);
			}
			return (!invert) ? num : (0f - num);
		}

		public bool GetButton()
		{
			if (type == InputType.Button)
			{
				return Input.GetKey(positive) || Input.GetKey(altPositive);
			}
			if (type == InputType.RemoteButton)
			{
				return _remoteButtonState == ButtonState.Pressed || _remoteButtonState == ButtonState.JustPressed;
			}
			if (type == InputType.AnalogButton)
			{
				return _analogButtonState == ButtonState.Pressed || _analogButtonState == ButtonState.JustPressed;
			}
			return false;
		}

		public bool GetButtonDown()
		{
			if (type == InputType.Button)
			{
				return Input.GetKeyDown(positive) || Input.GetKeyDown(altPositive);
			}
			if (type == InputType.RemoteButton)
			{
				return _remoteButtonState == ButtonState.JustPressed;
			}
			if (type == InputType.AnalogButton)
			{
				return _analogButtonState == ButtonState.JustPressed;
			}
			return false;
		}

		public bool GetButtonUp()
		{
			if (type == InputType.Button)
			{
				return Input.GetKeyUp(positive) || Input.GetKeyUp(altPositive);
			}
			if (type == InputType.RemoteButton)
			{
				return _remoteButtonState == ButtonState.JustReleased;
			}
			if (type == InputType.AnalogButton)
			{
				return _analogButtonState == ButtonState.JustReleased;
			}
			return false;
		}

		public void SetMouseAxis(int axis)
		{
			if (type == InputType.MouseAxis)
			{
				this.axis = axis;
				_lastAxis = axis;
				UpdateRawAxisName();
			}
		}

		public void SetAnalogAxis(int joystick, int axis)
		{
			if (type == InputType.AnalogAxis)
			{
				this.joystick = joystick;
				this.axis = axis;
				_lastAxis = axis;
				_lastJoystick = joystick;
				UpdateRawAxisName();
			}
		}

		public void SetAnalogButton(int joystick, int axis)
		{
			if (type == InputType.AnalogButton)
			{
				this.joystick = joystick;
				this.axis = axis;
				_lastAxis = axis;
				_lastJoystick = joystick;
				UpdateRawAxisName();
			}
		}

		public void SetRemoteAxisValue(float value)
		{
			if (type == InputType.RemoteAxis)
			{
				_value = value;
			}
			else
			{
				Debug.LogWarning(string.Format("You are trying to manually change the value of axis '{0}' which is not of type 'RemoteAxis'", name));
			}
		}

		public void SetRemoteButtonValue(bool down, bool justChanged)
		{
			if (type == InputType.RemoteButton)
			{
				if (down)
				{
					if (justChanged)
					{
						_remoteButtonState = ButtonState.JustPressed;
					}
					else
					{
						_remoteButtonState = ButtonState.Pressed;
					}
				}
				else if (justChanged)
				{
					_remoteButtonState = ButtonState.JustReleased;
				}
				else
				{
					_remoteButtonState = ButtonState.Released;
				}
			}
			else
			{
				Debug.LogWarning(string.Format("You are trying to manually change the value of button '{0}' which is not of type 'RemoteButton'", name));
			}
		}

		public void Copy(AxisConfiguration source)
		{
			name = source.name;
			description = source.description;
			positive = source.positive;
			altPositive = source.altPositive;
			negative = source.negative;
			altNegative = source.altNegative;
			deadZone = source.deadZone;
			gravity = source.gravity;
			sensitivity = source.sensitivity;
			snap = source.snap;
			invert = source.invert;
			type = source.type;
			axis = source.axis;
			joystick = source.joystick;
		}

		public void Reset()
		{
			_value = 0f;
			_remoteButtonState = ButtonState.Released;
			_analogButtonState = ButtonState.Released;
		}

		private void UpdateRawAxisName()
		{
			if (type == InputType.MouseAxis)
			{
				if (axis < 0 || axis >= 3)
				{
					string message = string.Format("Desired mouse axis is out of range. Mouse axis will be clamped to {0}.", Mathf.Clamp(axis, 0, 2));
					Debug.LogWarning(message);
				}
				_rawAxisName = "mouse_axis_" + Mathf.Clamp(axis, 0, 2);
			}
			else if (type == InputType.AnalogAxis || type == InputType.AnalogButton)
			{
				if (joystick < 0 || joystick >= 4)
				{
					string message2 = string.Format("Desired joystick is out of range. Joystick has been clamped to {0}.", Mathf.Clamp(joystick, 0, 3));
					Debug.LogWarning(message2);
				}
				if (axis >= 10)
				{
					string message3 = string.Format("Desired joystick axis is out of range. Joystick axis will be clamped to {0}.", Mathf.Clamp(axis, 0, 9));
					Debug.LogWarning(message3);
				}
				_rawAxisName = "joy_" + Mathf.Clamp(joystick, 0, 3) + "_axis_" + Mathf.Clamp(axis, 0, 9);
			}
			else
			{
				_rawAxisName = string.Empty;
			}
		}

		public static KeyCode StringToKey(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return KeyCode.None;
			}
			try
			{
				return (KeyCode)Enum.Parse(typeof(KeyCode), value, true);
			}
			catch
			{
				return KeyCode.None;
			}
		}

		public static InputType StringToInputType(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return InputType.Button;
			}
			try
			{
				return (InputType)Enum.Parse(typeof(InputType), value, true);
			}
			catch
			{
				return InputType.Button;
			}
		}

		public static AxisConfiguration Duplicate(AxisConfiguration source)
		{
			AxisConfiguration axisConfiguration = new AxisConfiguration();
			axisConfiguration.name = source.name;
			axisConfiguration.description = source.description;
			axisConfiguration.positive = source.positive;
			axisConfiguration.altPositive = source.altPositive;
			axisConfiguration.negative = source.negative;
			axisConfiguration.altNegative = source.altNegative;
			axisConfiguration.deadZone = source.deadZone;
			axisConfiguration.gravity = source.gravity;
			axisConfiguration.sensitivity = source.sensitivity;
			axisConfiguration.snap = source.snap;
			axisConfiguration.invert = source.invert;
			axisConfiguration.type = source.type;
			axisConfiguration.axis = source.axis;
			axisConfiguration.joystick = source.joystick;
			return axisConfiguration;
		}
	}
}
