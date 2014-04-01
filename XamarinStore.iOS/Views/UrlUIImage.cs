using System;
using MonoTouch.UIKit;

namespace XamarinStore
{
	public class UrlUIImage : UIImageView
	{
		UIActivityIndicatorView progress;

		public UrlUIImage ()
		{
			AddSubview (progress = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.WhiteLarge));
		}

		string url;

		public string Url {
			get {
				return url;
			}
			set {
				url = value;
				DownloadImage ();
			}
		}

		async void DownloadImage ()
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
		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			progress.Center = Center;
		}
	}
}
