namespace XamarinStore

open System
open MonoTouch.UIKit
open MonoTouch.CoreAnimation

[<AllowNullLiteralAttribute>]
type BrightlyBlurredUIView() as this=
    inherit UIView()

    let toolbar = new UIToolbar(Opaque = true)
    let accentView = new UIView(BackgroundColor = UIColor.White, Alpha = 0.7f, Opaque = false)
    let blurLayer = toolbar.Layer
    let accentLayer = accentView.Layer

    do this.Layer.AddSublayer(blurLayer)
       blurLayer.InsertSublayer(accentLayer, 1)
       this.Layer.MasksToBounds <- true
       this.BackgroundColor <- UIColor.Clear


    override this.LayoutSubviews () =
        base.LayoutSubviews ()
        blurLayer.Frame <- this.Bounds
        accentLayer.Frame <- this.Bounds

    member this.AccentColorIntensity
        with get () = accentView.Alpha
        and set value = accentView.Alpha <- value

    override this.TintColor
        with get () = base.TintColor
        and set value =
            toolbar.BarTintColor <- value
            accentView.BackgroundColor <- value
            base.TintColor <- value
