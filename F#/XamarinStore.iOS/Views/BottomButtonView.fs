namespace XamarinStore

open System
open MonoTouch.UIKit
open MonoTouch.CoreGraphics
open System.Drawing

[<AllowNullLiteralAttribute>]
type BottomButtonView() as this =
    inherit BrightlyBlurredUIView()

    let padding = 15.0f
    let button = new ImageButton()

    do this.AddSubview button
       button.Layer.BackgroundColor <- Color.Green.ToUIColor().CGColor
       button.Layer.CornerRadius <- 5.0f
       button.Font <- UIFont.BoldSystemFontOfSize (UIFont.ButtonFontSize)
       button.SizeToFit()
       button.TouchUpInside.Add( fun _ -> this.ButtonTapped() )
       this.TintColor <- UIColor.White
       this.AccentColorIntensity <- 0.0f
    
    member val Button = button with get
    member val ButtonTapped = fun ()->() with get,set
    member this.ButtonText
        with get () = button.Text
        and set value = button.Text <- value

    static member val Height = 75.0f with get

    override this.LayoutSubviews () =
        base.LayoutSubviews ()
        let mutable bound = this.Bounds
        bound.X <- padding
        bound.Y <- padding
        bound.Width <- bound.Width - padding * 2.0f
        bound.Height <- bound.Height - padding * 2.0f
        button.Frame <- bound
