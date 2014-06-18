namespace XamarinStore

open System
open MonoTouch.UIKit
open System.Threading.Tasks
open MonoTouch.CoreGraphics
open System.Drawing

module UIImageHelpers =
    type UIImageView with
        member this.LoadUrl url = async {
            if not (String.IsNullOrEmpty(url)) then
                let progress = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.WhiteLarge, 
                                                            Center = new PointF(this.Bounds.GetMidX(), this.Bounds.GetMidY()))
                this.AddSubview (progress)

                progress.StartAnimating ()
                                                            
                let! t = FileCache.Download (url)
                let image = UIImage.FromFile(t)

                UIView.Animate (0.3, 
                    (fun () -> this.Image <- image),
                    fun () -> progress.StopAnimating ()
                              progress.RemoveFromSuperview () ) }