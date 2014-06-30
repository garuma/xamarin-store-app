namespace XamarinStore

open System
open System.Collections.Generic
open System.Drawing
open System.Linq
open System.Text
open MonoTouch.CoreGraphics
open MonoTouch.UIKit
open MonoTouch.Foundation

type BadgeView() as this =
    inherit UILabel()

    let height = 14.0f
    let mutable badgeNumber = 0
    let mutable numberSize = new SizeF()
    do this.BackgroundColor <- UIColor.Clear
       this.TextColor <- Color.Blue.ToUIColor()
       this.Font <- UIFont.BoldSystemFontOfSize 10.0f
       this.UserInteractionEnabled <- false
       this.Layer.CornerRadius <- height / 2.0f
       this.BackgroundColor <- UIColor.White
       this.TextAlignment <- UITextAlignment.Center

    let calculateSize () =
        numberSize <- this.StringSize(badgeNumber.ToString(), this.Font)
        this.Frame <- new RectangleF (this.Frame.Location, new SizeF (Math.Max (numberSize.Width, height), height))

    member this.BadgeNumber
        with get () = badgeNumber
        and set value = badgeNumber <- value
                        this.Text <- value.ToString ()
                        calculateSize ()
                        this.Alpha <- if badgeNumber > 0 then 1.0f else 0.0f
                        this.SetNeedsDisplay ()
