using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Modules.AppPolicy.Views.DialogContent
{
	/// <summary>
	/// OptionDeclarationEditDialogCcontent.xaml の相互作用ロジック
	/// </summary>
	public partial class OptionDeclarationEditDialogCcontent : UserControl
	{
		public OptionDeclarationEditDialogCcontent()
		{
			InitializeComponent();
		}
	}


	public class AppOptionPropertyTemplateSelecter : DataTemplateSelector
	{
		public DataTemplate IOPath { get; set; }
		public DataTemplate StringList { get; set; }
		public DataTemplate Number { get; set; }
		public DataTemplate LimitedNumber { get; set; }
		public DataTemplate RangeNumber { get; set; }

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is ViewModels.IOPathAppOptionPropertyViewModel)
			{
				return IOPath;
			}
			else if (item is ViewModels.StringListAppOptionPropertyViewModel)
			{
				return StringList;
			}

			// Note: 派生元のクラスが先に選択されないように派生関係が深いクラスを先に評価する
			else if (item is ViewModels.RangeNumberAppOptionPropertyViewModel)
			{
				return RangeNumber;
			}
			else if (item is ViewModels.LimitedNumberAppOptionPropertyViewModel)
			{
				return LimitedNumber;
			}
			else if (item is ViewModels.NumberAppOptionPropertyViewModel)
			{
				return Number;
			}


			return base.SelectTemplate(item, container);
		}
	}
}
