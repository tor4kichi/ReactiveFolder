using System.Windows;
using System.Windows.Controls;

namespace Modules.Main.Views
{
	/// <summary>
	/// FolderListPage.xaml の相互作用ロジック
	/// </summary>
	public partial class FolderReactionManagePage : UserControl
	{
		public FolderReactionManagePage()
		{
			InitializeComponent();
		}
	}



	public class ReactionEditControlTemplateSelecter : DataTemplateSelector
	{

		public DataTemplate EmptyTemplate { get; set; }
		public DataTemplate ReactionEditControlTemplate { get; set; }

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item  == null)
			{
				return EmptyTemplate;
			}
			else if (item is ViewModels.ReactionEditPageViewModel)
			{
				return ReactionEditControlTemplate;
			}

			return base.SelectTemplate(item, container);
		}
	}
}
