namespace XamarinStore

open System
open MonoTouch.UIKit
open System.Collections.Generic
open System.Linq
open System.Drawing

type ImageScroller() as this =
    inherit UIScrollView()

    let mutable isAnimating = false
    let imageViews = new List<UIImageView> ()
    let mutable images: UIImage array = [||]
    let mutable currentIndex = 0

    do this.PagingEnabled <- true
       this.ShowsHorizontalScrollIndicator <- false
       this.Scrolled.Add(fun _ -> this.Scrolling ())
       this.ScrollAnimationEnded.Add(fun _ -> isAnimating <- false
                                              this.Scrolling ())

    let updateImages() =
        for view in imageViews do
            view.RemoveFromSuperview ()
        imageViews.Clear()

        let mutable frame = this.Bounds
        for image in images do
            let imageView = new UIImageView (image, 
                                             ContentMode = UIViewContentMode.ScaleAspectFit,
                                             Frame = frame )
            this.AddSubview (imageView)
            imageViews.Add (imageView)
            frame.X <- frame.X + frame.Width
        this.ScrollToImage currentIndex

    member val ImageChanged = fun (x:int) ->()

    member this.Images
        with get () = images :> IEnumerable<UIImage>
        and set (value:IEnumerable<UIImage>) = images <- value.ToArray()
                                               updateImages ()

    override this.Frame
        with get () = base.Frame
        and set value = base.Frame <- value
                        let mutable frame = value
                        for view in this.Subviews do
                            view.Frame <- frame
                            frame.X <- frame.X + frame.Width

                        frame.Width <- frame.X
                        this.ContentSize <- frame.Size
                        this.ScrollToImage currentIndex

    member this.ScrollToImage index =
        if not (index >= imageViews.Count || index = -1) then
            isAnimating <- true
            let imageView = imageViews.[index]
            this.ScrollRectToVisible (imageView.Frame, true)

    member this.ScrollToImage (image:UIImage) =
        if images.Contains (image) then
            let index = Array.IndexOf(images, image)
            this.ScrollToImage index
            isAnimating <- false

    member this.CurrentIndex
        with get () = currentIndex
        and set value = if currentIndex <> value then
                            currentIndex <- value
                            this.ScrollToImage currentIndex
                            this.ImageChanged currentIndex

    member this.Scrolling () =
        if not isAnimating then
            let page = imageViews.Where(fun (x:UIImageView) -> x.Frame.Contains(this.ContentOffset)).FirstOrDefault ()
            let pageIndex = Math.Max(imageViews.IndexOf(page), 0)
            if pageIndex <> currentIndex then
                this.ImageChanged pageIndex
            currentIndex <- pageIndex
