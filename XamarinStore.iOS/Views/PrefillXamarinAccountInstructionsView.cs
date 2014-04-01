using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;

namespace XamarinStore
{

	class PrefillXamarinAccountInstructionsView : UIView
	{
		public PrefillXamarinAccountInstructionsView ()
		{
			BackgroundColor = UIColor.White;

			var mockup = new UIImageView (UIImage.FromBundle ("fill-details-instructions-mockup")) {
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			AddSubview (mockup);

			AddConstraint (NSLayoutConstraint.Create (
				mockup,
				NSLayoutAttribute.CenterY,
				NSLayoutRelation.Equal,
				this,
				NSLayoutAttribute.CenterY,
				1f, -40f
			));
		}
	}
	
}
