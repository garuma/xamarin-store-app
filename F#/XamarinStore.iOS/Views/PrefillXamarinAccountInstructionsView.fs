namespace XamarinStore

open System
open System.Drawing
open MonoTouch.Foundation
open MonoTouch.UIKit
open MonoTouch.CoreGraphics

[<AllowNullLiteralAttribute>]
type PrefillXamarinAccountInstructionsView() as this =
    inherit UIView()

    do this.BackgroundColor <- UIColor.White;

       let mockup = new UIImageView (UIImage.FromBundle ("fill-details-instructions-mockup"),
                                     TranslatesAutoresizingMaskIntoConstraints = false)
       
       this.AddSubview (mockup)
       
       this.AddConstraint( NSLayoutConstraint.Create ( mockup,
                                                       NSLayoutAttribute.CenterY,
                                                       NSLayoutRelation.Equal,
                                                       this,
                                                       NSLayoutAttribute.CenterY,
                                                       1.0f, -40.0f ))