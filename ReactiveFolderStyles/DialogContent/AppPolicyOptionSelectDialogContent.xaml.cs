using Prism.Mvvm;
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

namespace ReactiveFolderStyles.DialogContent
{
	/// <summary>
	/// AppPolicyOptionSelectDialogContent.xaml の相互作用ロジック
	/// </summary>
	public partial class AppPolicyOptionSelectDialogContent : UserControl
	{
		public AppPolicyOptionSelectDialogContent()
		{
			InitializeComponent();
		}
	}


	public class AppPolicyOptionSelectDialogContentViewModel : BindableBase
	{

		public List<AppPolicyOptionSelectItem> SelectItems { get; private set; }


		public IEnumerable<AppPolicyOptionSelectItem> GetSelectedItems()
		{
			return SelectItems.Where(x => x.IsSelected);
		}

		public AppPolicyOptionSelectDialogContentViewModel(IEnumerable<AppPolicyOptionSelectItem> items)
		{
			SelectItems = items.ToList();
		}
	}

	public class AppPolicyOptionSelectItem : BindableBase
	{
		public bool IsSelected { get; set; }
		public string OptionName { get; set; }
		public int OptionId { get; set; }
	}
}
