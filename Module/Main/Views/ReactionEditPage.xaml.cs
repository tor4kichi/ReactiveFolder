using Modules.Main.ViewModels.ReactionEditer;
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

namespace Modules.Main.Views
{
	/// <summary>
	/// ReactionEditerPage.xaml の相互作用ロジック
	/// </summary>
	public partial class ReactionEditPage : UserControl
	{
		public ReactionEditPage()
		{
			InitializeComponent();
		}
	}


	public class FilterContentDataTemplateSelector : DataTemplateSelector
	{
		public DataTemplate FileTemplate { get; set; }
		public DataTemplate FolderTemplate { get; set; }
		public DataTemplate EmptyTemplate { get; set; }


		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is FileFilterViewModel)
			{
				return FileTemplate;
			}
			else if (item is FolderFilterViewModel)
			{
				return FolderTemplate;
			}
			else
			{
				return EmptyTemplate;
			}
		}
	}
}
