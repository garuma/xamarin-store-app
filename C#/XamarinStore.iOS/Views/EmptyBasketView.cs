using System;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;

namespace XamarinStore
{
	public class EmptyBasketView : UIView
	{
		UIImageView image;
		UILabel text;
		public EmptyBasketView ()
		{
			BackgroundColor = UIColor.White;
			this.AddSubview(image = new UIImageView (UIImage.FromBundle ("empty-basket")));
			this.Add (text = new UILabel {
				Text = "Your basket is empty",
				TextColor = UIColor.LightGray,
				Font = UIFont.BoldSystemFontOfSize(20f),
				TextAlignment = UITextAlignment.Center,
				BackgroundColor = UIColor.Clear,
			});
			text.SizeToFit ();
		}

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();

			var center = image.Center = new System.Drawing.PointF(Bounds.GetMidX(),Bounds.GetMidY());

			center.Y +=  (image.Frame.Height + text.Frame.Height) / 2;
			text.Center = center;


		}
	}
}

