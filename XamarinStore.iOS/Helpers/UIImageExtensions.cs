using System;
using MonoTouch.UIKit;
using System.Threading.Tasks;
using MonoTouch.CoreGraphics;
using System.Drawing;

namespace XamarinStore
{
	public static class UIImageExtensions
	{
		public static async Task LoadUrl(this UIImageView imageView, string url)
		{	
			if (string.IsNullOrEmpty (url))
				return;
			var progress = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.WhiteLarge)
			{
				Center = new PointF(imageView.Bounds.GetMidX(), imageView.Bounds.GetMidY()),
			};
			imageView.AddSubview (progress);

		
			var t = FileCache.Download (url);
			if (t.IsCompleted) {
				imageView.Image = UIImage.FromFile(t.Result);
				progress.RemoveFromSuperview ();
				return;
			}
			progress.StartAnimating ();
			var image = UIImage.FromFile(await t);

			UIView.Animate (.3, 
				() => imageView.Image = image,
				() => {
					progress.StopAnimating ();
					progress.RemoveFromSuperview ();
				});
		}
	}
}

