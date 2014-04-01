using System;
using MonoTouch.UIKit;

namespace XamarinStore
{

	public class CustomViewCell : UITableViewCell
	{
		UIView child;
		public UIEdgeInsets Padding { get; set; }
		public bool ResizeChild { get; set;}
		public CustomViewCell (UIView child)
			: base (UITableViewCellStyle.Default, "CustomViewCell")
		{
			SelectionStyle = UITableViewCellSelectionStyle.None;
			ResizeChild = true;
			Padding = new UIEdgeInsets ();
			this.child = child;
			var frame = child.Frame;
			frame.Height = frame.Bottom;
			frame.Y = 0;
			Frame = frame;
			this.ContentView.AddSubview (child);
		}

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			if (!ResizeChild) {
				child.Center = ContentView.Center;
				return;
			}
			var bounds = ContentView.Bounds;
			bounds.X += Padding.Left;
			bounds.Y += Padding.Top;
			bounds.Height -= (Padding.Bottom + Padding.Top);
			bounds.Width -= (Padding.Left + Padding.Right);
			child.Frame = bounds;
		}
	}
}

