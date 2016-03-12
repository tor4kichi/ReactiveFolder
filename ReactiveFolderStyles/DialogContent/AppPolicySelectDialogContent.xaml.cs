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
	/// AppPolicySelectDialogContent.xaml の相互作用ロジック
	/// </summary>
	public partial class AppPolicySelectDialogContent : UserControl
	{
		public AppPolicySelectDialogContent()
		{
			InitializeComponent();
		}
	}


	public class AppPolicySelectDialogContentViewModel : BindableBase
	{
		public List<AppPolicySelectItem> SelectItems { get; private set; }

		private AppPolicySelectItem _SelectedItem;
		public AppPolicySelectItem SelectedItem
		{
			get
			{
				return _SelectedItem;
			}
			set
			{
				if (SetProperty(ref _SelectedItem, value))
				{
					IsItemSelected = _SelectedItem != null;
				}
			}
		}




		private bool _IsItemSelected;
		public bool IsItemSelected
		{
			get
			{
				return _IsItemSelected;
			}
			set
			{
				SetProperty(ref _IsItemSelected, value);
			}
		}
		

		public AppPolicySelectDialogContentViewModel(IEnumerable<AppPolicySelectItem> items)
		{
			SelectItems = items.ToList();
			IsItemSelected = false;
		}

	}

	public class AppPolicySelectItem
	{
		public string AppName { get; set; }
		public Guid AppGuid { get; set; }
	}
}
