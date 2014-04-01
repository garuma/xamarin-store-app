using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace XamarinStore
{
	public class BottomButtonView : BrightlyBlurredUIView
	{
		public readonly ImageButton Button;
		public Action ButtonTapped { get; set; }
		public string ButtonText
		{
			get{ return Button.Text; }
			set{ Button.Text = value; }
		}
		public const float Height = 75;

		public BottomButtonView()
		{
			this.AddSubview(Button = new  ImageButton());
			Button.Layer.BackgroundColor = Color.Green;
			Button.Layer.CornerRadius = 5f;
			Button.Font = UIFont.BoldSystemFontOfSize (UIFont.ButtonFontSize);
			Button.SizeToFit();
			Button.TouchUpInside += (object sender, EventArgs e) => ButtonTapped();
			this.TintColor = UIColor.White;
			this.AccentColorIntensity = 0f;
		}
		const float padding = 15f;
		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			var bound = Bounds;
			bound.X = padding;
			bound.Y = padding;
			bound.Width -= padding * 2;
			bound.Height -= padding * 2;
			Button.Frame = bound;
		}
	}
}

