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

namespace Modules.AppPolicy.Views
{
	/// <summary>
	/// AppPolicyListPage.xaml の相互作用ロジック
	/// </summary>
	public partial class AppPolicyManagePage : UserControl
	{
		public AppPolicyManagePage()
		{
			InitializeComponent();
		}
	}


	public class AppPolicyEditTemplateSelecter : DataTemplateSelector
	{
		public DataTemplate AppPolicyEditTemplate { get; set; }
		public DataTemplate EmptyTemplate { get; set; }

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item == null)
			{
				return EmptyTemplate;
			}
			else if (item is ViewModels.AppPolicyEditControlViewModel)
			{
				return AppPolicyEditTemplate;
			}
			else
			{
				return base.SelectTemplate(item, container);
			}
		}

	}
}
