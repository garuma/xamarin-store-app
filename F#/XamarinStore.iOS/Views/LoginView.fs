namespace XamarinStore

open System
open System.Drawing

open MonoTouch.Foundation
open MonoTouch.UIKit
open Gravatar

type EmailFieldDelegate() =
    inherit UITextFieldDelegate()

    override this.ShouldBeginEditing textField =
        false

[<AllowNullLiteralAttribute>]
type LoginView(xamarinAccountEmail:string) as this =
    inherit UIView()
    let gravatarSize = new SizeF (85.0f, 85.0f)
    let gravatarView = new UIImageView (new RectangleF (PointF.Empty, gravatarSize), 
                                        TranslatesAutoresizingMaskIntoConstraints = false,
                                        Image = UIImage.FromBundle ("user-default-avatar"))

    let emailField = new UITextField (new RectangleF (10.0f, 10.0f, 300.0f, 30.0f), 
                                      BorderStyle = UITextBorderStyle.RoundedRect,
                                      Text = xamarinAccountEmail,
                                      TranslatesAutoresizingMaskIntoConstraints = false,
                                      Delegate = new EmailFieldDelegate ())

    let passwordField = new UITextField (new RectangleF (10.0f, 10.0f, 300.0f, 30.0f),
                                         BorderStyle = UITextBorderStyle.RoundedRect,
                                         SecureTextEntry = true,
                                         TranslatesAutoresizingMaskIntoConstraints = false,
                                         ReturnKeyType = UIReturnKeyType.Go,
                                         Placeholder = "Password" )
                                                
    let displayGravatar email = async {
        try
            let! data = Gravatar.GetImageData email ((int gravatarSize.Width) * 2) Rating.G
            gravatarView.Image <- UIImage.LoadFromData (data)
        with e->() }

    let AddConstantSizeConstraints (view:UIView, size:SizeF) =
        this.AddConstraint (NSLayoutConstraint.Create ( view,
                                                        NSLayoutAttribute.Width,
                                                        NSLayoutRelation.Equal,
                                                        null,
                                                        NSLayoutAttribute.NoAttribute,
                                                        1.0f, size.Width))

        this.AddConstraint (NSLayoutConstraint.Create ( view,
                                                        NSLayoutAttribute.Height,
                                                        NSLayoutRelation.Equal,
                                                        null,
                                                        NSLayoutAttribute.NoAttribute,
                                                        1.0f, size.Height))

    do this.BackgroundColor <- UIColor.White
       this.Add gravatarView
       gravatarView.Layer.CornerRadius <- gravatarSize.Width / 2.0f
       gravatarView.Layer.MasksToBounds <- true

       displayGravatar xamarinAccountEmail |> Async.StartImmediate

       this.AddConstraint (NSLayoutConstraint.Create ( gravatarView,
                                                       NSLayoutAttribute.Top,
                                                       NSLayoutRelation.Equal,
                                                       this,
                                                       NSLayoutAttribute.Top,
                                                       1.0f, 90.0f ) )

       this.AddConstraint (NSLayoutConstraint.Create ( gravatarView,
                                                       NSLayoutAttribute.CenterX,
                                                       NSLayoutRelation.Equal,
                                                       this,
                                                       NSLayoutAttribute.CenterX,
                                                       1.0f, 0.0f ))

       AddConstantSizeConstraints (gravatarView, gravatarSize)

       this.Add emailField

       this.AddConstraint (NSLayoutConstraint.Create ( emailField,
                                                       NSLayoutAttribute.Top,
                                                       NSLayoutRelation.Equal,
                                                       gravatarView,
                                                       NSLayoutAttribute.Bottom,
                                                       1.0f, 30.0f ))

       this.AddConstraint (NSLayoutConstraint.Create ( emailField,
                                                       NSLayoutAttribute.CenterX,
                                                       NSLayoutRelation.Equal,
                                                       gravatarView,
                                                       NSLayoutAttribute.CenterX,
                                                       1.0f, 0.0f ))

       let textSize = (new NSString("hello")).StringSize (UIFont.SystemFontOfSize (12.0f))
       AddConstantSizeConstraints (emailField, new SizeF (260.0f, textSize.Height + 16.0f))
       this.Add passwordField

       this.AddConstraint (NSLayoutConstraint.Create ( passwordField,
                                                       NSLayoutAttribute.Top,
                                                       NSLayoutRelation.Equal,
                                                       emailField,
                                                       NSLayoutAttribute.Bottom,
                                                       1.0f, 10.0f))

       this.AddConstraint (NSLayoutConstraint.Create ( passwordField,
                                                       NSLayoutAttribute.CenterX,
                                                       NSLayoutRelation.Equal,
                                                       emailField,
                                                       NSLayoutAttribute.CenterX,
                                                       1.0f, 0.0f))

       AddConstantSizeConstraints (passwordField, new SizeF (260.0f, textSize.Height + 16.0f))

       passwordField.ShouldReturn <- (fun field -> field.ResignFirstResponder() |> ignore
                                                   this.UserDidLogin this
                                                   true)

       passwordField.BecomeFirstResponder () |> ignore

    member val UserDidLogin = fun (x:LoginView) -> () with get,set
    member val PasswordField = passwordField with get
