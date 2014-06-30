namespace XamarinStore

open System
open MonoTouch.UIKit

type SpinnerCell() =
    inherit CustomViewCell(SpinnerCell.CreateView())

    static member CreateView() =
        let indicator = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.Gray)
        indicator.StartAnimating ()
        indicator
