namespace XamarinStore

open System
open System.Collections.Generic
open System.Drawing
open System.Linq
open System.Text
open MonoTouch.UIKit
open MonoTouch.Foundation

open Helpers

type TopAlignedImageView() as this =
    inherit UIView()

    let mutable originalSize = SizeF.Empty
    let mutable image: UIImage = null
    let imageView = new UIImageView()
    let progress = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge)

    do this.ClipsToBounds <- true
       this.AddSubview imageView
       this.AddSubview progress
       this.TranslatesAutoresizingMaskIntoConstraints <- false

    member this.Image 
        with get () = image
        and set value =
            image <- value
            imageView.Image <- value
            originalSize <- if value = null then SizeF.Empty else value.Size
            this.LayoutSubviews ()

    override this.LayoutSubviews() =
        base.LayoutSubviews()
        progress.Center <- this.Center
        if not (originalSize = SizeF.Empty) then
            let mutable frame = this.Bounds
            let scale = frame.Width/originalSize.Width
            frame.Height <- originalSize.Height * scale
            imageView.Frame <- frame

    member this.LoadUrl(url : string) = async {
        if not (String.IsNullOrEmpty (url)) then
            progress.StartAnimating ()

            let! image = FileCache.Download (url)
            this.Image <- UIImage.FromFile image
            progress.StopAnimating () }

