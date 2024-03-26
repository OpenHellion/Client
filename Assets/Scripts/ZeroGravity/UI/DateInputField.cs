using System;
using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public class DateInputField : MonoBehaviour
	{
		public delegate void OnChangeDelegate(DateTime date);

		private InputField yearField;

		private InputField monthField;

		private InputField dayField;

		private InputField hourField;

		private InputField minField;

		private InputField secField;

		public DateTime CurrentDate = DateTime.Now;

		public OnChangeDelegate OnChangeDate;

		private bool yearApplyChange = true;

		private bool monthApplyChange = true;

		private bool dayApplyChange = true;

		private bool hourApplyChange = true;

		private bool minApplyChange = true;

		private bool secApplyChange = true;

		private void Awake()
		{
			yearField = transform.Find("NumericInputField_Year").GetComponent<InputField>();
			monthField = transform.Find("NumericInputField_Month").GetComponent<InputField>();
			dayField = transform.Find("NumericInputField_Day").GetComponent<InputField>();
			hourField = transform.Find("NumericInputField_Hour").GetComponent<InputField>();
			minField = transform.Find("NumericInputField_Min").GetComponent<InputField>();
			secField = transform.Find("NumericInputField_Sec").GetComponent<InputField>();
			yearField.text = CurrentDate.Year.ToString();
			monthField.text = CurrentDate.Month.ToString();
			dayField.text = CurrentDate.Day.ToString();
			hourField.text = CurrentDate.Hour.ToString();
			minField.text = CurrentDate.Minute.ToString();
			secField.text = CurrentDate.Second.ToString();
			yearField.onValueChanged.AddListener(OnValueChangedYear);
			monthField.onValueChanged.AddListener(OnValueChangedMonth);
			dayField.onValueChanged.AddListener(OnValueChangedDay);
			hourField.onValueChanged.AddListener(OnValueChangedHour);
			minField.onValueChanged.AddListener(OnValueChangedMin);
			secField.onValueChanged.AddListener(OnValueChangedSec);
		}

		public void SetDateTime(DateTime dateTime)
		{
			yearField.text = dateTime.Year.ToString();
			monthField.text = dateTime.Month.ToString();
			dayField.text = dateTime.Day.ToString();
			hourField.text = dateTime.Hour.ToString();
			minField.text = dateTime.Minute.ToString();
			secField.text = dateTime.Second.ToString();
		}

		public void SetDateTimeTest(DateTime dateTime)
		{
			yearField.text = dateTime.Year.ToString();
			monthField.text = dateTime.Month.ToString();
			dayField.text = dateTime.Day.ToString();
			hourField.text = dateTime.Hour.ToString();
			minField.text = dateTime.Minute.ToString();
			secField.text = dateTime.Second.ToString();
			CurrentDate = dateTime;
		}

		private void UpdateFields()
		{
			if (yearField.text != CurrentDate.Year.ToString())
			{
				yearApplyChange = false;
				yearField.text = CurrentDate.Year.ToString();
			}

			if (monthField.text != CurrentDate.Month.ToString())
			{
				monthApplyChange = false;
				monthField.text = CurrentDate.Month.ToString();
			}

			if (dayField.text != CurrentDate.Day.ToString())
			{
				dayApplyChange = false;
				dayField.text = CurrentDate.Day.ToString();
			}

			if (hourField.text != CurrentDate.Hour.ToString())
			{
				hourApplyChange = false;
				hourField.text = CurrentDate.Hour.ToString();
			}

			if (minField.text != CurrentDate.Minute.ToString())
			{
				minApplyChange = false;
				minField.text = CurrentDate.Minute.ToString();
			}

			if (secField.text != CurrentDate.Second.ToString())
			{
				secApplyChange = false;
				secField.text = CurrentDate.Second.ToString();
			}

			if (OnChangeDate != null)
			{
				OnChangeDate(CurrentDate);
			}
		}

		private void OnValueChangedYear(string tmpVal)
		{
			try
			{
				if (!yearApplyChange)
				{
					yearApplyChange = true;
					return;
				}

				int result = CurrentDate.Year;
				if (int.TryParse(yearField.text, out result))
				{
					CurrentDate = CurrentDate.AddYears(result - CurrentDate.Year);
				}
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			UpdateFields();
		}

		private void OnValueChangedMonth(string tmpVal)
		{
			try
			{
				if (!monthApplyChange)
				{
					monthApplyChange = true;
					return;
				}

				int result = CurrentDate.Month;
				if (int.TryParse(monthField.text, out result))
				{
					CurrentDate = CurrentDate.AddMonths(result - CurrentDate.Month);
				}
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			UpdateFields();
		}

		private void OnValueChangedDay(string tmpVal)
		{
			try
			{
				if (!dayApplyChange)
				{
					dayApplyChange = true;
					return;
				}

				int result = CurrentDate.Day;
				if (int.TryParse(dayField.text, out result))
				{
					CurrentDate = CurrentDate.AddDays(result - CurrentDate.Day);
				}
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			UpdateFields();
		}

		private void OnValueChangedHour(string tmpVal)
		{
			try
			{
				if (!hourApplyChange)
				{
					hourApplyChange = true;
					return;
				}

				int result = CurrentDate.Hour;
				if (int.TryParse(hourField.text, out result))
				{
					CurrentDate = CurrentDate.AddHours(result - CurrentDate.Hour);
				}
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			UpdateFields();
		}

		private void OnValueChangedMin(string tmpVal)
		{
			try
			{
				if (!minApplyChange)
				{
					minApplyChange = true;
					return;
				}

				int result = CurrentDate.Minute;
				if (int.TryParse(minField.text, out result))
				{
					CurrentDate = CurrentDate.AddMinutes(result - CurrentDate.Minute);
				}
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			UpdateFields();
		}

		private void OnValueChangedSec(string tmpVal)
		{
			try
			{
				if (!secApplyChange)
				{
					secApplyChange = true;
					return;
				}

				int result = CurrentDate.Second;
				if (int.TryParse(secField.text, out result))
				{
					CurrentDate = CurrentDate.AddSeconds(result - CurrentDate.Second);
				}
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			UpdateFields();
		}
	}
}
