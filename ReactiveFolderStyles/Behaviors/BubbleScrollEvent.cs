using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

// This code is not Lisenced in ReactiveFolder.

// copy from http://stackoverflow.com/questions/14348517/child-elements-of-scrollviewer-preventing-scrolling-with-mouse-wheel#16110178

namespace ReactiveFolderStyles.Behaviors
{
	public sealed class BubbleScrollEvent : Behavior<UIElement>
	{
		protected override void OnAttached()
		{
			base.OnAttached();
			AssociatedObject.PreviewMouseWheel += AssociatedObject_PreviewMouseWheel;
		}

		protected override void OnDetaching()
		{
			AssociatedObject.PreviewMouseWheel -= AssociatedObject_PreviewMouseWheel;
			base.OnDetaching();
		}

		void AssociatedObject_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			e.Handled = true;
			var e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
			e2.RoutedEvent = UIElement.MouseWheelEvent;
			AssociatedObject.RaiseEvent(e2);
		}
	}
}
