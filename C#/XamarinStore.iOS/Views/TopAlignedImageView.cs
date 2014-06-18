using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace XamarinStore
{
	public class TopAlignedImageView : UIView
	{
		SizeF origionalSize;
		public UIImage Image
		{
			get { return image; }
			set
			{
				origionalSize = value == null ? SizeF.Empty : value.Size;
				ImageView.Image = image = value;
				LayoutSubviews ();
			}
		}

		UIImageView ImageView;
		UIImage image;
		UIActivityIndicatorView progress;

		public TopAlignedImageView()
		{
			this.ClipsToBounds = true;
			ImageView = new UIImageView();
			this.AddSubview(ImageView);

			AddSubview (progress = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.WhiteLarge));
			this.TranslatesAutoresizingMaskIntoConstraints = false;
		}
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			progress.Center = Center;
			if (origionalSize == SizeF.Empty) {
				return;
			}
			var frame = Bounds;
			var scale = frame.Width/origionalSize.Width ;
			frame.Height = origionalSize.Height * scale;
			ImageView.Frame = frame;
		}
		public async void LoadUrl(string url)
		{
			if (string.IsNullOrEmpty (url))
				return;
			var t = FileCache.Download (url);
			if (t.IsCompleted) {
				Image = UIImage.FromFile(t.Result);
				return;
			}
			progress.StartAnimating ();
			var image = UIImage.FromFile(await t);

			UIView.Animate (.3, 
				() => Image = image,
				() => progress.StopAnimating ());
		}

	
	}
}
