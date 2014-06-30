namespace XamarinStore

open System
open System.Collections.Generic
open System.Linq
open System.Text
open MonoTouch.UIKit
open System.Threading.Tasks
open MonoTouch.CoreGraphics
open MonoTouch.CoreAnimation
open System.Drawing
open MonoTouch.Foundation

type BasketButton() as this =
    inherit UIControl()

    let padding = 10.0f
    let BasketImage = lazy ( UIImage.FromBundle("cart"))
    let badge = new BadgeView(Frame=new System.Drawing.RectangleF(20.0f, 5.0f, 0.0f, 0.0f))
    let imageView = new UIImageView(BasketImage.Value.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), TintColor = UIColor.White)
    do this.AddSubview imageView
       this.AddSubview badge

    member this.ItemsCount
        with get () = badge.BadgeNumber
        and set value = badge.BadgeNumber <- value

    member this.UpdateItemsCount count =
        this.ItemsCount <- count
        let pathAnimation = CAKeyFrameAnimation.GetFromKeyPath("transform")
        pathAnimation.CalculationMode <- CAAnimation.AnimationPaced.ToString()
        pathAnimation.FillMode <- CAFillMode.Forwards.ToString()
        pathAnimation.TimingFunction <- CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseOut)
        pathAnimation.Duration <- 0.2

        let transform = CATransform3D.MakeScale (2.0f, 2.0f, 1.0f)
        pathAnimation.Values <- [| NSValue.FromCATransform3D(CATransform3D.Identity)
                                   NSValue.FromCATransform3D(transform)
                                   NSValue.FromCATransform3D(CATransform3D.Identity)|]
        badge.Layer.AddAnimation (pathAnimation, "pulse")

    override this.LayoutSubviews() =
        base.LayoutSubviews()
        let mutable bounds = this.Bounds
        bounds.X <- bounds.X + padding + 15.0f
        bounds.Y <- bounds.Y + padding
        bounds.Width <- bounds.Width - padding * 2.0f
        bounds.Height <- bounds.Height - padding * 2.0f
        imageView.Frame <- bounds

