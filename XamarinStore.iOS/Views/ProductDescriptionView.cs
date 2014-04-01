using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoTouch.UIKit;

namespace XamarinStore
{
	public class ProductDescriptionView : UIView
	{
		public UILabel Name { get; set; }
		public UILabel DescriptionLabel { get; set; }
		public UILabel Price { get; set; }
		const float padding = 20f;
		const float priceWidth = 50f;
		public ProductDescriptionView(Product product) : this()
		{
			this.Update(product);
		}
		public ProductDescriptionView ()
		{
			//this.TranslatesAutoresizingMaskIntoConstraints = false;
			Name = new UILabel
				{
					Text = "Name",
					BackgroundColor = UIColor.Clear,
					TextColor = UIColor.DarkGray,
					Font = UIFont.SystemFontOfSize(25f),
					TranslatesAutoresizingMaskIntoConstraints = false,
				};
			Name.SizeToFit();
			this.AddSubview(Name);
			

			DescriptionLabel = new UILabel
				{
					BackgroundColor = UIColor.Clear,
					TextColor = UIColor.DarkGray,
					TranslatesAutoresizingMaskIntoConstraints = false,
					Font = UIFont.SystemFontOfSize(12),
					LineBreakMode = UILineBreakMode.WordWrap,
					Lines = 0
				};
			this.AddSubview(DescriptionLabel);

			Price = new UILabel
				{
					BackgroundColor = UIColor.Clear,
					Text = "Price",
					TextColor = Color.Blue,
					TranslatesAutoresizingMaskIntoConstraints = false,
				};
			Price.SizeToFit();
			this.AddSubview(Price);
		}
		public void Update(Product product)
		{
			Name.Text = product.Name;
			DescriptionLabel.Text = product.Description;
			Price.Text = product.PriceDescription;
		}
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			var bounds = Bounds;
			var frame = Name.Frame;

			frame.Width = bounds.Width - (priceWidth + padding *2);
			frame.Y = frame.X = padding;
			Name.Frame = frame;

			frame = Price.Frame;
			frame.Y = padding + (Name.Frame.Height - frame.Height)/2;
			frame.X = Name.Frame.Right + padding;
			frame.Width = priceWidth;
			Price.Frame = frame;

			frame = bounds;
			frame.Y = Name.Frame.Bottom;
			frame.X = padding;
			frame.Width -= padding*2;
			frame.Height -= frame.Y ;
			DescriptionLabel.Frame = frame;
		}
	}
}
