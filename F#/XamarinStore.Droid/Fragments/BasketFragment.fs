namespace XamarinStore

open System
open System.Collections.Generic
open System.Globalization
open System.Linq
open System.Text
open Android.App
open Android.Content
open Android.OS
open Android.Runtime
open Android.Util
open Android.Views
open Android.Widget
open Order
open Product
open Images
open Helpers

type GroceryListAdapter(context:Context) =
    inherit Android.Widget.BaseAdapter()

    override this.GetItemId position =
        int64 position

    override this.GetItem position =
        new Java.Lang.String (WebService.CurrentOrder.Products.[position].ToString()) :> Java.Lang.Object

    override this.Count with get () = WebService.CurrentOrder.Products.Length

    override this.GetView (position, convertView, parent) =
        let product = WebService.CurrentOrder.Products.[position]

        let view = if not (convertView = null)
                   then convertView
                   else let tempView = LayoutInflater.From(context).Inflate (Resource_Layout.BasketItem, parent, false)
                        let swipper = (tempView :?> SwipableListItem).SwipeListener
                        swipper.SwipeGestureBegin <- fun () -> (parent :?> ListView).RequestDisallowInterceptTouchEvent true
                        swipper.SwipeGestureEnd <- fun () -> (parent :?> ListView).RequestDisallowInterceptTouchEvent false
                        swipper.ItemSwipped <- fun () -> if tempView.Parent <> null then
                                                             let p = (parent :?> ListView).GetPositionForView tempView
                                                             let productToRemove = WebService.CurrentOrder.Products.[p]
                                                             WebService.CurrentOrder <- WebService.CurrentOrder.WithoutProduct productToRemove
                                                             this.NotifyDataSetChanged ()
                        tempView

        view.FindViewById<TextView>(Resource_Id.productTitle).Text <- product.Name
        view.FindViewById<TextView>(Resource_Id.productPrice).Text <- product.GetPriceDescription()
        view.FindViewById<TextView>(Resource_Id.productColor).Text <- product.Color.ToString()
        view.FindViewById<TextView>(Resource_Id.productSize).Text <- product.Size.Description

        let orderImage = view.FindViewById<ImageView> (Resource_Id.productImage)
        orderImage.SetImageResource (Resource_Drawable.blue_shirt)
        //No need to wait for the async download to return the view
        Images.SetImageViewFromUrlAsync orderImage (product.ImageForSize (Images.ScreenWidth)) |> Async.StartImmediate
        view

type BasketFragment() =
    inherit ListFragment()

    let mutable checkoutButton = null

    override this.OnCreate savedInstanceState =
        base.OnCreate (savedInstanceState)
        this.RetainInstance <- true

    member val CheckoutClicked = fun ()->() with get, set

    override this.OnCreateView(inflater, container, savedInstanceState) =
        let shoppingCartView = inflater.Inflate (Resource_Layout.Basket, container, false)

        checkoutButton <- shoppingCartView.FindViewById<Button> (Resource_Id.checkoutBtn)
        checkoutButton.Click.Add(fun x-> this.CheckoutClicked ())

        shoppingCartView.PivotY <- 0.0f
        shoppingCartView.PivotX <- float32 container.Width

        shoppingCartView
    
    override this.OnViewCreated (view, savedInstanceState) =
        base.OnViewCreated (view, savedInstanceState)
        this.ListView.DividerHeight <- 0
        this.ListView.Divider <- null
        this.ListAdapter <- (new GroceryListAdapter (view.Context) :> IListAdapter)
        if this.ListAdapter.Count = 0 then
            checkoutButton.Visibility <- ViewStates.Invisible
        WebService.CurrentOrder <- { WebService.CurrentOrder 
                                        with Notification = Some (fun order -> checkoutButton.Visibility <- if order.Products.Any () 
                                                                                                                then ViewStates.Visible 
                                                                                                                else ViewStates.Invisible) }