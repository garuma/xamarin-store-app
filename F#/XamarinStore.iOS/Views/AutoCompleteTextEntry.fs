namespace XamarinStore

open System
open MonoTouch.UIKit
open MonoTouch.Foundation
open System.Collections.Generic
open System.Linq
open System.Drawing

type AutoCompleteTextEntry() as this =
    inherit TextEntryView()

    let controller = new StringTableViewController ( ItemSelected = fun item -> this.Value <- item)
    do this.TextField.Started.Add(fun _ ->this.Search ())

    member val Items:IEnumerable<string> = new List<string>() :> IEnumerable<string> with get,set

    member this.Search () =
        if this.Items.Count() <> 0 then
            this.TextField.ResignFirstResponder() |> ignore
            controller.Title <- this.Title
            controller.Items <- this.Items.ToList()
            if this.PresenterView.NavigationController.TopViewController <> (controller :> UIViewController) then
                this.PresenterView.NavigationController.PushViewController(controller, true)

    member val PresenterView:UITableViewController = null with get,set
    member val Title:string = null with get,set
