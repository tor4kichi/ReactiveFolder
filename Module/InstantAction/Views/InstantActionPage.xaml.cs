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

namespace Modules.InstantAction.Views
{
	/// <summary>
	/// InstantActionPage.xaml の相互作用ロジック
	/// </summary>
	public partial class InstantActionPage : UserControl
	{
		public InstantActionPage()
		{
			InitializeComponent();
		}
	}


	public class InstantActionStepDataTemplateSelecter : DataTemplateSelector
	{
		public DataTemplate EmptyTemplate { get; set; }
		public DataTemplate FileSelectTemplate { get; set; }
		public DataTemplate ActionSelectTemplate { get; set; }
		public DataTemplate FinishingTemplate { get; set; }


		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item == null)
			{
				return EmptyTemplate;
			}
			else if (item is ViewModels.FileSelectInstantActionStepViewModel)
			{
				return FileSelectTemplate;
			}
			else if (item is ViewModels.ActionsSelectInstantActionStepViewModel)
			{
				return ActionSelectTemplate;
			}
			else if (item is ViewModels.FinishingInstantActionStepViewModel)
			{
				return FinishingTemplate;
			}


			return base.SelectTemplate(item, container);
		}
	}

}
