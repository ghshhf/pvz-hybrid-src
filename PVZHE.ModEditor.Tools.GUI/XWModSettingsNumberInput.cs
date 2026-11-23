using System;
using System.Globalization;
using Godot;
using PVZHE.ModEditor.ModSystem;

namespace PVZHE.ModEditor.Tools.GUI;

internal sealed class XWModSettingsNumberInput
{
	private readonly XWModSettingDefinition _definition;

	private readonly double _step;

	private double _lastValid;

	internal VBoxContainer View { get; } = new VBoxContainer
	{
		SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
	};

	internal LineEdit TextInput { get; } = new LineEdit
	{
		SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
	};

	internal Button Decrease { get; } = new Button
	{
		Text = "−",
		TooltipText = "减小",
		CustomMinimumSize = new Vector2(36f, 36f)
	};

	internal Button Increase { get; } = new Button
	{
		Text = "+",
		TooltipText = "增大",
		CustomMinimumSize = new Vector2(36f, 36f)
	};

	internal HSlider Slider { get; }

	internal object DraftValue { get; private set; }

	internal event Action Changed;

	internal XWModSettingsNumberInput(XWModSettingDefinition definition, object initial)
	{
		_definition = definition;
		_step = definition.Step ?? ((definition.Type == "int") ? 1.0 : 0.01);
		HBoxContainer hBoxContainer = new HBoxContainer
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
		};
		hBoxContainer.AddChild(TextInput, forceReadableName: false, Node.InternalMode.Disabled);
		hBoxContainer.AddChild(Decrease, forceReadableName: false, Node.InternalMode.Disabled);
		hBoxContainer.AddChild(Increase, forceReadableName: false, Node.InternalMode.Disabled);
		View.AddChild(hBoxContainer, forceReadableName: false, Node.InternalMode.Disabled);
		Decrease.Pressed += delegate
		{
			if (!Decrease.Disabled)
			{
				Increment(-1);
			}
		};
		Increase.Pressed += delegate
		{
			if (!Increase.Disabled)
			{
				Increment(1);
			}
		};
		double? min = definition.Min;
		if (min.HasValue)
		{
			double min2 = min.GetValueOrDefault();
			min = definition.Max;
			if (min.HasValue)
			{
				double max = min.GetValueOrDefault();
				if (min2 < max && double.IsFinite(max - min2))
				{
					Slider = new HSlider
					{
						MinValue = min2,
						MaxValue = max,
						Step = 0.0,
						Scrollable = false,
						SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
						CustomMinimumSize = new Vector2(0f, 36f)
					};
					View.AddChild(Slider, forceReadableName: false, Node.InternalMode.Disabled);
					Slider.ValueChanged += delegate(double value)
					{
						if (Slider.Editable)
						{
							AcceptValue(Snap(value));
						}
					};
					Slider.GuiInput += delegate(InputEvent input)
					{
						if (Slider.Editable && input is InputEventKey { Pressed: not false } inputEventKey)
						{
							if (inputEventKey.IsActionPressed("ui_left") || inputEventKey.IsActionPressed("ui_down"))
							{
								Increment(-1);
							}
							else if (inputEventKey.IsActionPressed("ui_right") || inputEventKey.IsActionPressed("ui_up"))
							{
								Increment(1);
							}
							else if (inputEventKey.IsActionPressed("ui_home"))
							{
								AcceptValue(min2);
							}
							else
							{
								if (!inputEventKey.IsActionPressed("ui_end"))
								{
									return;
								}
								AcceptValue(max);
							}
							Slider.AcceptEvent();
						}
					};
				}
			}
		}
		TextInput.TextChanged += delegate(string text)
		{
			if (TextInput.Editable)
			{
				double result;
				object obj = (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out result) ? ((object)result) : text);
				if (XWModSettingsService.TryValue(_definition, obj, out var normalized))
				{
					DraftValue = normalized;
					_lastValid = Convert.ToDouble(normalized, CultureInfo.InvariantCulture);
					Slider?.SetValueNoSignal(_lastValid);
				}
				else
				{
					DraftValue = obj;
				}
				this.Changed?.Invoke();
			}
		};
		SetDraft(initial);
	}

	internal void SetDraft(object value)
	{
		if (!XWModSettingsService.TryValue(_definition, value, out var normalized))
		{
			throw new ArgumentException("无效的数值配置。", "value");
		}
		DraftValue = normalized;
		_lastValid = Convert.ToDouble(normalized, CultureInfo.InvariantCulture);
		Slider?.SetValueNoSignal(_lastValid);
		TextInput.Text = _lastValid.ToString("R", CultureInfo.CurrentCulture);
	}

	private void Increment(int direction)
	{
		bool flag = _definition.Type == "int";
		double min = _definition.Min ?? (flag ? (-2147483648.0) : (-1.7976931348623157E+308));
		double max = _definition.Max ?? (flag ? 2147483647.0 : 1.7976931348623157E+308);
		double value = _lastValid + (double)direction * _step;
		if (TryDecimal(_lastValid, out var result) && TryDecimal(_step, out var result2))
		{
			try
			{
				value = (double)(result + (decimal)direction * result2);
			}
			catch (OverflowException)
			{
			}
		}
		AcceptValue(Math.Clamp(value, min, max));
	}

	private void AcceptValue(double value)
	{
		SetDraft(value);
		this.Changed?.Invoke();
	}

	private double Snap(double value)
	{
		double minValue = Slider.MinValue;
		double maxValue = Slider.MaxValue;
		if (value <= minValue)
		{
			return minValue;
		}
		if (value >= maxValue)
		{
			return maxValue;
		}
		if (TryDecimal(minValue, out var result) && TryDecimal(maxValue, out var result2) && TryDecimal(value, out var result3) && TryDecimal(_step, out var result4))
		{
			try
			{
				decimal num = result + decimal.Floor((result3 - result) / result4) * result4;
				decimal num2 = Math.Min(num + result4, result2);
				return Math.Clamp((double)((result3 - num < num2 - result3) ? num : num2), minValue, maxValue);
			}
			catch (OverflowException)
			{
			}
		}
		double num3 = (value - minValue) / _step;
		double value2 = (double.IsFinite(num3) ? (minValue + Math.Round(num3, MidpointRounding.AwayFromZero) * _step) : value);
		value2 = Math.Clamp(value2, minValue, maxValue);
		if (!(value >= value2 + (maxValue - value2) / 2.0))
		{
			return value2;
		}
		return maxValue;
	}

	private static bool TryDecimal(double value, out decimal result)
	{
		if (decimal.TryParse(value.ToString("R", CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture, out result))
		{
			return (double)result == value;
		}
		return false;
	}
}
