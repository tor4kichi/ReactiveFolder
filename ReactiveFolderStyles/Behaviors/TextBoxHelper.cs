using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReactiveFolderStyles.Behaviors
{
	// code from : http://d.hatena.ne.jp/Yoichiro/20101107/1289074958


	/// <summary>
	/// TextBox 添付ビヘイビア
	/// </summary>
	public class TextBoxHelper
	{

		/// <summary>
		/// True なら入力を数字のみに制限します。
		/// </summary>
		public static readonly DependencyProperty IsNumericProperty =
					DependencyProperty.RegisterAttached(
						"IsNumeric", typeof(bool),
						typeof(TextBoxHelper),
						new UIPropertyMetadata(false, IsNumericChanged)
					);

		[AttachedPropertyBrowsableForType(typeof(TextBox))]
		public static bool GetIsNumeric(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsNumericProperty);
		}

		[AttachedPropertyBrowsableForType(typeof(TextBox))]
		public static void SetIsNumeric(DependencyObject obj, bool value)
		{
			obj.SetValue(IsNumericProperty, value);
		}

		private static void IsNumericChanged
			(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{

			var textBox = sender as TextBox;
			if (textBox == null) return;

			// イベントを登録・削除 
			textBox.KeyDown -= OnKeyDown;
			textBox.TextChanged -= OnTextChanged;
			var newValue = (bool)e.NewValue;
			if (newValue)
			{
				textBox.KeyDown += OnKeyDown;
				textBox.TextChanged += OnTextChanged;
			}
		}

		static void OnKeyDown(object sender, KeyEventArgs e)
		{
			var textBox = sender as TextBox;
			if (textBox == null) return;

			if ((Key.D0 <= e.Key && e.Key <= Key.D9) ||
				(Key.NumPad0 <= e.Key && e.Key <= Key.NumPad9) ||
				(Key.Delete == e.Key) || (Key.Back == e.Key) || (Key.Tab == e.Key))
			{
				e.Handled = false;
			}
			else {
				e.Handled = true;
			}
		}

		private static void OnTextChanged(object sender, TextChangedEventArgs e)
		{
			var textBox = sender as TextBox;
			if (textBox == null) return;

			if (string.IsNullOrEmpty(textBox.Text))
			{
				textBox.Text = "0";
			}
		}
	}
}
