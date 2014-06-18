namespace XamarinStore

open System
open MonoTouch.UIKit
open System.Drawing

type TextEntryView() as this =
    inherit UIView(new RectangleF(0.0f, 0.0f, 320.0f, 44.0f))

    let textField = new UITextField ( BorderStyle = UITextBorderStyle.RoundedRect,
                                      ShouldReturn = fun tf -> tf.ResignFirstResponder() |> ignore
                                                               true)
    do this.AddSubview textField

    member val TextField = textField with get,set

    member val ValueChanged = fun (x:string) -> () with get,set

    member this.KeyboardType
        with get () = textField.KeyboardType
        and set value = textField.KeyboardType <- value

    member this.AutocapitalizationType
        with get () = textField.AutocapitalizationType
        and set value = textField.AutocapitalizationType <- value

    member this.PlaceHolder
        with get () = textField.Placeholder
        and set value = textField.Placeholder <- value

    member this.Value
        with get () = textField.Text
        and set value = textField.Text <- value
                        this.ValueChanged (textField.Text)

    override this.LayoutSubviews () =
        let sidePadding = 10.0f
        let topPadding = 5.0f
        base.LayoutSubviews ()

        let width = this.Bounds.Width - sidePadding * 2.0f
        let height = this.Bounds.Height - topPadding * 2.0f

        textField.Frame <- new RectangleF (sidePadding, topPadding, width, height)
