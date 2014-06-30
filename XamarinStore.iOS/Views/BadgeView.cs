using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;

namespace XamarinStore
{
	class BadgeView : UILabel
	{
		const float height = 14;

		public int BadgeNumber {
			get { return badgeNumber; }
			set {
				Text = (badgeNumber = value).ToString ();
				CalculateSize ();
				Alpha = badgeNumber > 0 ? 1 : 0;
				SetNeedsDisplay ();
			}
		}

		SizeF numberSize;
		int badgeNumber;

		public BadgeView ()
		{
			BackgroundColor = UIColor.Clear;
			TextColor = Color.Blue;
			Font = UIFont.BoldSystemFontOfSize (10f);;
			UserInteractionEnabled = false;
			Layer.CornerRadius = height / 2;
			Layer.BackgroundColor = UIColor.White.CGColor;
			TextAlignment = UITextAlignment.Center;
		}

		void CalculateSize ()
		{
			numberSize = StringSize (badgeNumber.ToString (), Font);
			Frame = new RectangleF (Frame.Location, new SizeF (Math.Max (numberSize.Width, height), height));
		}
	}
}
