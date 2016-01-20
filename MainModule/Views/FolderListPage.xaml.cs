﻿using Microsoft.Practices.Prism.Mvvm;
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

namespace Main.Views
{
	/// <summary>
	/// FolderListPage.xaml の相互作用ロジック
	/// </summary>
	public partial class FolderListPage : Page, IView
	{
		public FolderListPage()
		{
			InitializeComponent();

			ViewModelLocator.SetAutoWireViewModel(this, true);
		}
	}
}
