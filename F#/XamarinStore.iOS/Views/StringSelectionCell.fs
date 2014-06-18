namespace XamarinStore

open System
open MonoTouch.UIKit
open System.Collections.Generic
open System.Linq

[<AllowNullLiteralAttribute>]
type StringSelectionCell(viewForPicker:UIView) as this =
    inherit UITableViewCell(UITableViewCellStyle.Value1, "SelectionCell")

    let Key = "SelectionCell"

    let picker = new StringUIPicker ()

    do picker.SelectedItemChanged <- fun _ -> this.DetailTextLabel.Text <- picker.SelectedItem
                                              this.SelectionChanged()
       picker.Items <- [||]
       this.SelectionStyle <- UITableViewCellSelectionStyle.None
       this.TextLabel.TextColor <- Color.Purple.ToUIColor()
       this.Accessory <- UITableViewCellAccessory.DisclosureIndicator

    member val SelectionChanged = fun ()->() with get,set

    member this.Items
        with get () = picker.Items
        and set value =
            picker.Items <- value

            match value.Count() with
            | 1 -> this.DetailTextLabel.TextColor <- UIColor.Gray
                   this.Accessory <- UITableViewCellAccessory.None
            | _ -> this.DetailTextLabel.TextColor <- UIColor.Black
                   this.Accessory <- UITableViewCellAccessory.DisclosureIndicator

    member this.SelectedItem
        with get () = picker.SelectedItem
        and set value = this.DetailText <- value
                        picker.SelectedItem <- value

    member this.SelectedIndex
        with get () = picker.SelectedIndex
        and set value = picker.SelectedIndex <- value

    member this.Text 
        with get () = this.TextLabel.Text
        and set value = this.TextLabel.Text <- value

    member this.DetailText
        with get () = this.DetailTextLabel.Text
        and set value = this.DetailTextLabel.Text <- value



    member this.Tap () =
        // Don't show the picker when we don't have options.
        if this.Items.Count() <> 1 then
            picker.SelectedIndex <- this.Items.ToList().IndexOf this.DetailText
            picker.ShowPicker viewForPicker
