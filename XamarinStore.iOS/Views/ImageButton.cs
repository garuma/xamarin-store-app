using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace XamarinStore
{
	public class ImageButton : UIControl
	{
		public readonly UIImageView ImageView;
		UILabel label;
		public ImageButton () : base(new RectangleF(0,0,250,50))
		{
			ImageView = new UIImageView ();
			AddSubview (ImageView);

			label = new UILabel {
				TextColor = TintColor,
			};
			AddSubview (label);

			Layer.BorderColor = TintColor.CGColor;
			Layer.BorderWidth = 1f;
			Layer.CornerRadius = 5f;
		}
		public override void TintColorDidChange ()
		{
			base.TintColorDidChange ();
			Layer.BorderColor = TintColor.CGColor;
			label.TextColor = TintColor;
		}
		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();

			const float padding = 10f;
			var imageSize = ImageView.SizeThatFits (Bounds.Size);

			var availableWidth = Bounds.Width - padding * 3 - imageSize.Width;
			var stringSize = label.SizeThatFits (new System.Drawing.SizeF (availableWidth, Bounds.Height - padding * 2));

			availableWidth = Bounds.Width ;
			availableWidth -= stringSize.Width;
			availableWidth -= imageSize.Width;

			var x = availableWidth / 2;

			var frame = new RectangleF (new PointF (x, Bounds.GetMidY () - imageSize.Height / 2), imageSize);
			ImageView.Frame = frame;

			frame.X = frame.Right + (imageSize.Width > 0 ? padding : 0);
			frame.Size = stringSize;
			frame.Height = Bounds.Height;
			frame.Y = 0;
			label.Frame = frame;
		}

		public string Text {
			get{ return label.Text; }
			set{ label.Text = value; }
		}

		public UIImage Image {
			get{ return ImageView.Image; }
			set{ 
				ImageView.Image = value;
			 }
		}

		public UIFont Font {
			get{ return label.Font; }
			set{ label.Font = value; }
		}

		public override void TouchesBegan (MonoTouch.Foundation.NSSet touches, UIEvent evt)
		{
			base.TouchesBegan (touches, evt);
			TintAdjustmentMode = UIViewTintAdjustmentMode.Dimmed;
		}

		public override void TouchesEnded (MonoTouch.Foundation.NSSet touches, UIEvent evt)
		{
			base.TouchesEnded (touches, evt);
			TintAdjustmentMode = UIViewTintAdjustmentMode.Automatic;
		}
		public override void TouchesCancelled (MonoTouch.Foundation.NSSet touches, UIEvent evt)
		{
			base.TouchesCancelled (touches, evt);
			TintAdjustmentMode = UIViewTintAdjustmentMode.Automatic;
		}

	}
}

