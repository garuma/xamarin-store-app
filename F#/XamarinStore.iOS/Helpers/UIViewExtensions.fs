namespace XamarinStore

open System
open MonoTouch.UIKit

module UIViewHelpers =

    let rec private GetParentViewController (view:UIView) =
            match view.NextResponder with
            | :? UIViewController as nextResponder -> nextResponder
            | :? UIView as view ->GetParentViewController view
            | _ -> null

    type UIView with
        member this.GetParentViewController() =
            GetParentViewController this