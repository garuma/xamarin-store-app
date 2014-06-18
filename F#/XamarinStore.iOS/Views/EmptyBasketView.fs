namespace XamarinStore

open System
open MonoTouch.UIKit
open MonoTouch.CoreGraphics

[<AllowNullLiteralAttribute>]
type EmptyBasketView() as this =
    inherit UIView()

    let image = new UIImageView (UIImage.FromBundle ("empty-basket"))
    let text = new UILabel ( Text = "Your basket is empty",
                             TextColor = UIColor.LightGray,
                             Font = UIFont.BoldSystemFontOfSize(20.0f),
                             TextAlignment = UITextAlignment.Center,
                             BackgroundColor = UIColor.Clear )

    do this.BackgroundColor <- UIColor.White
       this.AddSubview image
       this.Add text
       text.SizeToFit()

    override this.LayoutSubviews () =
        base.LayoutSubviews ()

        let mutable center = new System.Drawing.PointF(this.Bounds.GetMidX(), this.Bounds.GetMidY())
        image.Center <- center
        center.Y <- center.Y + (image.Frame.Height + text.Frame.Height) / 2.0f
        text.Center <- center
