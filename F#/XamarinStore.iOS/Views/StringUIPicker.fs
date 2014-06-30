namespace XamarinStore

open System
open System.Linq
open System.Collections.Generic
open MonoTouch.UIKit
open System.Drawing

type PickerModel() =
    inherit UIPickerViewModel()

    member val Parent: StringUIPicker = null with get, set
    member val Items: string array = [||] with get, set

    override this.GetComponentCount picker =
        1

    override this.GetRowsInComponent (picker, ``component``) =
        this.Items.Length

    override this.GetTitle (picker, row, ``component``) =
        this.Items.[row]

    override this.Selected (picker, row, ``component``) =
        if this.Parent <> null then
            this.Parent.SelectedIndex <- row

and [<AllowNullLiteralAttribute>] StringUIPicker() =
    inherit UIPickerView()

    let mutable currentIndex = 0
    let mutable items :string array = [||]
    let mutable sheet : UIActionSheet = null

    member this.Items
        with get () = items :> string seq
        and set (value:string seq) = items <- value.ToArray()
                                     this.Model <- new PickerModel(Items = items, Parent = this)

    member val SelectedItemChanged = fun x->() with get,set

    member this.SelectedIndex
        with get () = currentIndex
        and set value =
            if currentIndex <> value then
                currentIndex <- value
                this.Select (currentIndex, 0, true)
                this.SelectedItemChanged this

    member this.SelectedItem
        with get () = if items.Length <= currentIndex then "" else items.[currentIndex]
        and set value = if items.Contains(value) then
                            currentIndex <- Array.FindIndex(items, fun x-> x = value)

    member this.ShowPicker(viewForPicker : UIView) =
        sheet <- new UIActionSheet()

        sheet.AddSubview this

        let doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done)
        doneButton.Clicked.Add(fun _ -> sheet.DismissWithClickedButtonIndex (0, true))
        let toolbarPicker = new UIToolbar (new RectangleF (0.0f, 0.0f, viewForPicker.Frame.Width, 44.0f))
        toolbarPicker.Items <- [|new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace); doneButton|]
        toolbarPicker.BarTintColor <- this.BackgroundColor

        sheet.AddSubviews toolbarPicker

        sheet.BackgroundColor <- UIColor.Clear
        sheet.ShowInView viewForPicker
        UIView.Animate(0.25, fun () -> sheet.Bounds <- new RectangleF (0.0f, 0.0f, viewForPicker.Frame.Width, 485.0f))