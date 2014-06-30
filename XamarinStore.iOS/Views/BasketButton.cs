using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoTouch.UIKit;
using System.Threading.Tasks;
using MonoTouch.CoreGraphics;
using MonoTouch.CoreAnimation;
using System.Drawing;
using MonoTouch.Foundation;

namespace XamarinStore
{
	class BasketButton : UIControl
	{
		static Lazy<UIImage> BasketImage = new Lazy<UIImage>(() => UIImage.FromBundle("cart"));
		BadgeView badge;
		UIImageView imageView;
		public BasketButton()
		{

			imageView = new UIImageView(BasketImage.Value.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate)){
				TintColor = UIColor.White,
			};
			this.AddSubview(imageView);

			badge = new BadgeView(){

				Frame =  new System.Drawing.RectangleF (20,5,0,0)
			};
			this.AddSubview(badge);
		}
		public int ItemsCount
		{
			get { return badge.BadgeNumber; }
			set { badge.BadgeNumber = value; }
		}

		public void UpdateItemsCount(int count)
		{
			ItemsCount = count;
			var pathAnimation = CAKeyFrameAnimation.GetFromKeyPath("transform");	
			pathAnimation.CalculationMode = CAAnimation.AnimationPaced;
			pathAnimation.FillMode = CAFillMode.Forwards;
			pathAnimation.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseOut);
//			pathAnimation.RemovedOnCompletion = false;
			pathAnimation.Duration = .2;

			var transform = CATransform3D.MakeScale (2f, 2f, 1);
			pathAnimation.Values = new [] {
				NSValue.FromCATransform3D(CATransform3D.Identity),
				NSValue.FromCATransform3D(transform),
				NSValue.FromCATransform3D(CATransform3D.Identity),
			};
			badge.Layer.AddAnimation (pathAnimation, "pulse");
		}

		const float padding = 10;
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			var bounds = this.Bounds;
			bounds.X += padding + 15;
			bounds.Y += padding;
			bounds.Width -= padding*2;
			bounds.Height -= padding*2;
			imageView.Frame = bounds;
		}
	}
}
