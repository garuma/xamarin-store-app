namespace XamarinStore

open System
open MonoTouch.UIKit

type UrlUIImage() as this =
    inherit UIImageView()

    let mutable (url:string) = null
    let progress = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.WhiteLarge)

    do this.AddSubview progress

    member this.Url
        with get () = url
        and set value = url <- value
                        this.DownloadImage () |> ignore

    member this.DownloadImage () = async {
        if not (String.IsNullOrEmpty(url)) then
            let! t = FileCache.Download (url)
            progress.StartAnimating ()
            let image = UIImage.FromFile(t)

            UIView.Animate (0.3, 
                (fun () -> this.Image <- image),
                fun () -> progress.StopAnimating ()) }

    override this.LayoutSubviews () =
        base.LayoutSubviews ()
        progress.Center <- this.Center