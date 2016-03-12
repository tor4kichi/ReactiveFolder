using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ReactiveFolderStyles.Behaviors
{
	// this code not lisenced on ReactiveFolder.
	// copy from http://www.wpfsharp.com/2012/03/22/mvvm-and-drag-and-drop-command-binding-with-an-attached-behavior/
	// (クラス名を変更して利用しています)


	/// <summary>
	/// This is an Attached Behavior and is intended for use with
	/// XAML objects to enable binding a drag and drop event to
	/// an ICommand.
	/// </summary>
	public static class AcceptFileDrop
	{
		#region The dependecy Property
		/// <summary>
		/// The Dependency property. To allow for Binding, a dependency
		/// property must be used.
		/// </summary>
		private static readonly DependencyProperty PreviewDropCommandProperty =
					DependencyProperty.RegisterAttached
					(
						"PreviewDropCommand",
						typeof(ICommand),
						typeof(AcceptFileDrop),
						new PropertyMetadata(PreviewDropCommandPropertyChangedCallBack)
					);
		#endregion

		#region The getter and setter
		/// <summary>
		/// The setter. This sets the value of the PreviewDropCommandProperty
		/// Dependency Property. It is expected that you use this only in XAML
		///
		/// This appears in XAML with the "Set" stripped off.
		/// XAML usage:
		///
		/// <Grid mvvm:DropBehavior.PreviewDropCommand="{Binding DropCommand}" />
		///
		/// </summary>
		/// <param name="inUIElement">A UIElement object. In XAML this is automatically passed
		/// in, so you don't have to enter anything in XAML.</param>
		/// <param name="inCommand">An object that implements ICommand.</param>
		public static void SetPreviewDropCommand(this UIElement inUIElement, ICommand inCommand)
		{
			inUIElement.SetValue(PreviewDropCommandProperty, inCommand);
		}

		/// <summary>
		/// Gets the PreviewDropCommand assigned to the PreviewDropCommandProperty
		/// DependencyProperty. As this is only needed by this class, it is private.
		/// </summary>
		/// <param name="inUIElement">A UIElement object.</param>
		/// <returns>An object that implements ICommand.</returns>
		private static ICommand GetPreviewDropCommand(UIElement inUIElement)
		{
			return (ICommand)inUIElement.GetValue(PreviewDropCommandProperty);
		}
		#endregion

		#region The PropertyChangedCallBack method
		/// <summary>
		/// The OnCommandChanged method. This event handles the initial binding and future
		/// binding changes to the bound ICommand
		/// </summary>
		/// <param name="inDependencyObject">A DependencyObject</param>
		/// <param name="inEventArgs">A DependencyPropertyChangedEventArgs object.</param>
		private static void PreviewDropCommandPropertyChangedCallBack(
			DependencyObject inDependencyObject, DependencyPropertyChangedEventArgs inEventArgs)
		{
			UIElement uiElement = inDependencyObject as UIElement;
			if (null == uiElement) return;

			uiElement.PreviewDragOver += (sender, args) =>
			{
				if (args.Data.IsFileDropAction())
				{
					args.Effects = DragDropEffects.Copy;
					args.Handled = true;
				}
			};

			uiElement.Drop += (sender, args) =>
			{
				if (args.Data.IsFileDropAction())
				{
					var files = args.Data.GetFiles();
					var command = GetPreviewDropCommand(uiElement);
					if (command.CanExecute(files))
					{
						command.Execute(files);
					}
					args.Handled = true;
				}
			};
		}

		#endregion


		private static bool IsFileDropAction(this IDataObject obj)
		{
			return obj.GetDataPresent(DataFormats.FileDrop);
		}

		private static string[] GetFiles(this IDataObject obj)
		{
			if (obj.IsFileDropAction())
			{
				return (string[])obj.GetData(DataFormats.FileDrop);
			}
			else
			{
				return null;
			}
		}
	}
}
