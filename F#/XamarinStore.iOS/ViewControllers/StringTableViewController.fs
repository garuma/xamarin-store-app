namespace XamarinStore

open System
open MonoTouch.UIKit
open System.Collections.Generic
open System.Linq

type TableViewSource(parent:StringTableViewController, filteredItems) =
    inherit UITableViewSource()

    //region implemented abstract members of UITableViewSource
    member val FilteredItems = filteredItems with get,set
    member val Parent:StringTableViewController = parent with get,set

    override this.RowsInSection (tableview, section) =
        if this.FilteredItems = null then 
            0
        else 
            let items:List<string> = this.FilteredItems
            items.Count

    override this.GetCell (tableView, indexPath) =
        let cell = tableView.DequeueReusableCell ("stringCell") 
        let cell = if cell <> null then cell else new UITableViewCell(UITableViewCellStyle.Default, "stringCell")
        if this.FilteredItems <> null then
            let items:List<string> = this.FilteredItems
            cell.TextLabel.Text <- items.ElementAt indexPath.Row
        cell

    //endregion

    override this.RowSelected (tableView, indexPath) =
        if this.Parent <> null then
            let items:List<string> = this.FilteredItems
            let item = items.ElementAt indexPath.Row
            this.Parent.ItemSelected item
            this.Parent.NavigationController.PopViewControllerAnimated true |> ignore

and [<AllowNullLiteralAttribute>]StringTableViewController() as this =
    inherit UITableViewController()

    let searchBar = new UISearchBar ()
    let mutable items = new List<string>()
    let mutable filteredItems = new List<string>()
    let source = new TableViewSource (this, filteredItems)

    do this.TableView.Source <- source
       searchBar.TextChanged.Add(fun _ -> this.FilteredItems <- items.Where(fun (x:string) -> x.IndexOf(searchBar.Text, StringComparison.CurrentCultureIgnoreCase) >= 0).ToList()
                                          this.TableView.ReloadData() )
       searchBar.SizeToFit ()
       this.TableView.TableHeaderView <- searchBar

    member this.FilteredItems
        with get () = filteredItems
        and set value = source.FilteredItems <- value
                        filteredItems <- value

    member val ItemSelected = fun (x:string) -> () with get,set

    member this.Items
        with get () = items
        and set value = items <- value
                        this.FilteredItems <- items
                        this.TableView.ReloadData ()

    override this.ViewWillAppear animated =
        base.ViewWillAppear animated
        source.Parent <- this

    override this.ViewDidAppear animated =
        base.ViewDidAppear animated
        searchBar.BecomeFirstResponder () |> ignore 

    override this.ViewDidDisappear animated =
        base.ViewDidDisappear animated
        source.Parent <- null

