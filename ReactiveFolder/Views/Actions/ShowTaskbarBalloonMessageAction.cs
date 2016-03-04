using Hardcodet.Wpf.TaskbarNotification;
using Prism.Interactivity.InteractionRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;

namespace ReactiveFolder.Views.Actions
{
	public class ShowTaskbarBalloonMessageAction : TriggerAction<DependencyObject>
	{
		protected override void Invoke(object parameter)
		{
			var args = parameter as InteractionRequestedEventArgs;
			if (args == null)
			{
				return;
			}

			var taskbarIcon = this.AssociatedObject as TaskbarIcon;
			if (taskbarIcon == null)
			{
				return;
			}

			taskbarIcon.ShowBalloonTip(args.Context.Title, (string) args.Context.Content, BalloonIcon.None);

			args.Callback();
		}
	}
}
