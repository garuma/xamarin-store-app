using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace XamarinStore
{
	public class ImageScroller : UIScrollView
	{
		public event Action<int> ImageChanged;
		bool isAnimating;
		public ImageScroller ()
		{
			PagingEnabled = true;
			ShowsHorizontalScrollIndicator = false;
			this.Scrolled += (object sender, EventArgs e) => {
				Scrolling ();
			};
			this.ScrollAnimationEnded += (object sender, EventArgs e) => {
				isAnimating = false;
				Scrolling ();
			};
		}

		List<UIImageView> imageViews = new List<UIImageView> ();
		UIImage[] images = new UIImage[0];
		public IEnumerable<UIImage> Images
		{
			get{ return images; }
			set{ 
				images = value.ToArray();
				updateImages ();
			 }
		}

		void updateImages()
		{
			foreach (var view in imageViews)
				view.RemoveFromSuperview ();
			imageViews.Clear ();

			var frame = this.Bounds;
			foreach (var image in images) {
				var imageView = new UIImageView (image) {
					ContentMode = UIViewContentMode.ScaleAspectFit,
					Frame = frame,
				};
				this.AddSubview (imageView);
				imageViews.Add (imageView);
				frame.X += frame.Width;
			}
			ScrollToImage (CurrentIndex);
		}

		public override RectangleF Frame {
			get {
				return base.Frame;
			}
			set {
				base.Frame = value;
				var frame = value;
				foreach (var view in Subviews) {
					view.Frame = frame;
					frame.X += frame.Width;
				}
				frame.Width = frame.X;
				this.ContentSize = frame.Size;
				ScrollToImage (CurrentIndex);
			}
		}
		public void ScrollToImage (int index)
		{
			if (index >= imageViews.Count || index == -1)
				return;
			isAnimating = true;
			var imageView = imageViews [index];
			this.ScrollRectToVisible (imageView.Frame, true);

		}

		public void ScrollToImage (UIImage image)
		{
			if (!images.Contains (image))
				return;
			var index = Array.IndexOf (images, image);
			ScrollToImage (index);
			isAnimating = false;
		
		}
		int currentIndex;
		public int CurrentIndex
		{
			get{ return currentIndex; }
			set{
				if (currentIndex == value)
					return;
				currentIndex = value;
				ScrollToImage (currentIndex);
				if (ImageChanged != null)
					ImageChanged (currentIndex);
			}
		}
		void Scrolling ()
		{
			if (isAnimating)
				return;
			var page = imageViews.Where (x=> x.Frame.Contains(this.ContentOffset)).FirstOrDefault ();
			var pageIndex = Math.Max(imageViews.IndexOf (page),0);
			if (ImageChanged != null && pageIndex != CurrentIndex)
				ImageChanged (pageIndex);
			currentIndex = pageIndex;
		}
	}
}

