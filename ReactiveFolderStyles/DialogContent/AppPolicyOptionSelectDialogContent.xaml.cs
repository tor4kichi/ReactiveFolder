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

		public List<AppPolicyOptionSelectItem> Options { get; private set; }
		public List<AppPolicyOptionSelectItem> OutputOptions { get; private set; }


		public IEnumerable<AppPolicyOptionSelectItem> GetSelectedItems()
		{
			var outputSelected = OutputOptions.SingleOrDefault(x => x.IsSelected);
			if (outputSelected != null)
			{
				yield return outputSelected;
			}

			foreach (var opt in Options.Where(x => x.IsSelected))
			{
				yield return opt;
			}
		}

		public AppPolicyOptionSelectDialogContentViewModel(IEnumerable<AppPolicyOptionSelectItem> options, IEnumerable<AppPolicyOptionSelectItem> outputOptions)
		{
			Options = options.ToList();
			OutputOptions = outputOptions.ToList();
		}
	}

	public class AppPolicyOptionSelectItem : BindableBase
	{
		public bool IsSelected { get; set; }
		public string OptionName { get; set; }
		public int OptionId { get; set; }
	}
}
