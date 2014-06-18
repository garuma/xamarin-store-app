namespace XamarinStore

open System
open MonoTouch.UIKit
open System.Drawing
open MonoTouch.CoreGraphics

type ImageButton() as this =
    inherit UIControl(new RectangleF(0.0f,0.0f,250.0f, 50.0f))

    let label = new UILabel(TextColor = this.TintColor)
    let imageView = new UIImageView()

    do this.AddSubview imageView
       this.AddSubview label

       this.Layer.BorderColor <- this.TintColor.CGColor
       this.Layer.BorderWidth <- 1.0f
       this.Layer.CornerRadius <- 5.0f

    member val ImageView = imageView with get

    override this.TintColorDidChange () =
        base.TintColorDidChange ()
        this.Layer.BorderColor <- this.TintColor.CGColor
        label.TextColor <- this.TintColor

    override this.LayoutSubviews () =
        base.LayoutSubviews ()

        let padding = 10.0f
        let imageSize = this.ImageView.SizeThatFits this.Bounds.Size

        let mutable availableWidth = this.Bounds.Width - padding * 3.0f - imageSize.Width
        let stringSize = label.SizeThatFits (new System.Drawing.SizeF (availableWidth, this.Bounds.Height - padding * 2.0f))

        let availableWidth = this.Bounds.Width - stringSize.Width - imageSize.Width
       
        let x = availableWidth / 2.0f

        let mutable frame = new RectangleF (new PointF (x, this.Bounds.GetMidY () - imageSize.Height / 2.0f), imageSize)
        imageView.Frame <- frame

        frame.X <- frame.Right + if imageSize.Width > 0.0f then padding else 0.0f
        frame.Size <- stringSize
        frame.Height <- this.Bounds.Height
        frame.Y <- 0.0f
        label.Frame <- frame

    member this.Text 
        with get () = label.Text
        and set value = label.Text <- value

    member this.Image 
        with get () = imageView.Image
        and set value = imageView.Image <- value

    member this.Font 
        with get () = label.Font
        and set value = label.Font <- value

    override this.TouchesBegan (touches, evt) =
        base.TouchesBegan (touches, evt)
        this.TintAdjustmentMode <- UIViewTintAdjustmentMode.Dimmed

    override this.TouchesEnded (touches, evt) =
        base.TouchesEnded (touches, evt)
        this.TintAdjustmentMode <- UIViewTintAdjustmentMode.Automatic

    override this.TouchesCancelled (touches, evt) =
        base.TouchesCancelled (touches, evt)
        this.TintAdjustmentMode <- UIViewTintAdjustmentMode.Automatic
