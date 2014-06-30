namespace XamarinStore

open System
open System.Linq
open System.Collections.Generic
open Android.App
open Android.OS
open Android.Views
open Android.Widget
open Android.Content
open Android.Graphics
open Android.Graphics.Drawables
open Android.Views.Animations
open Images
open Product
open Order

type ProductListViewAdapter (context:Context) =
    inherit BaseAdapter()

    let appearInterpolator = new DecelerateInterpolator ()
    let mutable newItems = 0L

    let LoadProductImage (mainView:View) (progressView:ProgressBar) (imageView:ImageView) (product:Product) = async {
        try
            let currentId = mainView.Id
            progressView.Visibility <- ViewStates.Visible
            imageView.SetImageResource (Android.Resource.Color.Transparent)
            do! imageView.SetImageFromUrlAsync (product.ImageForSize (Images.ScreenWidth))
            progressView.Visibility <- ViewStates.Invisible 
        with e->()}

    member val Products = ([||]: Product.Product array) with get,set
    override this.Count = this.Products.Length

    override this.GetItem position = new Java.Lang.String(this.Products.[position].ToString()) :> Java.Lang.Object

    override this.GetItemId position = int64 (this.Products.[position].GetHashCode())

    override this.GetView(position, convertView, parent) =
        let mutable convertView = convertView
        if convertView = null then
            let inflator = LayoutInflater.FromContext context
            convertView <- inflator.Inflate(Resource_Layout.ProductListItem, parent, false)
            convertView.Id <- 0x60000000
        convertView.Id <- convertView.Id + 1

        let imageView = convertView.FindViewById<ImageView> (Resource_Id.productImage)
        let nameLabel = convertView.FindViewById<TextView> (Resource_Id.productTitle)
        let priceLabel = convertView.FindViewById<TextView> (Resource_Id.productPrice)
        let progressView = convertView.FindViewById<ProgressBar> (Resource_Id.productImageSpinner)

        let product = this.Products.[position]
        nameLabel.Text <- product.Name
        priceLabel.Text <- product.GetPriceDescription()

        LoadProductImage convertView progressView imageView product |> Async.StartImmediate

        if ((newItems >>> position) &&& 1L) = 0L then
            newItems <- newItems ||| (1L <<< position)
            let density = context.Resources.DisplayMetrics.Density
            convertView.TranslationY <- 60.0f * density
            convertView.RotationX <- 12.0f
            convertView.ScaleX <- 1.1f
            convertView.PivotY <- 180.0f * density
            convertView.PivotX <- (float32 parent.Width) / 2.0f
            convertView.Animate()
                       .TranslationY(0.0f)
                       .RotationX(0.0f)
                       .ScaleX(1.0f)
                       .SetDuration(450L)
                       .SetInterpolator(appearInterpolator)
                       .Start()
        convertView

type ProductListFragment () =
    inherit ListFragment()

    let mutable badgeCount = 0
    let mutable basketBadge = None

    let setBasketCount (basket:BadgeDrawable) (order:Order) = 
        basket.SetCountAnimated (order.Products.Count())

    member this.GetData () = async {
        let adapter = this.ListAdapter :?> ProductListViewAdapter
        let! products = WebService.Shared.GetProducts()
        adapter.Products <- products
        WebService.Shared.PreloadImages (Images.ScreenWidth) |> Async.StartImmediate
        adapter.NotifyDataSetChanged () }

    member val ProductSelected = (fun x i->()) with get,set

    override this.OnCreate savedInstanceState =
        base.OnCreate savedInstanceState
        this.RetainInstance <- true
        this.SetHasOptionsMenu true
    
    override this.OnCreateView(inflater, container, savedInstanceState) =
        let view = inflater.Inflate (Resource_Layout.XamarinListLayout, container, false)
        (view.FindViewById(Android.Resource.Id.Empty):?> XamarinSpinnerView).StartAnimation() |> ignore
        view
    
    override this.OnViewCreated(view, savedInstanceState) =
        base.OnViewCreated(view, savedInstanceState)
        this.ListView.SetDrawSelectorOnTop true
        this.ListView.Selector <- new ColorDrawable (new Android.Graphics.Color 0x30ffffff)
        if this.ListAdapter = null then
            this.ListAdapter <- new ProductListViewAdapter (view.Context)
            this.GetData() |> Async.StartImmediate

    override this.OnListItemClick(l, v, position, id) =
        base.OnListItemClick(l, v, position, id)
        let adapter = this.ListView.Adapter :?> ProductListViewAdapter
        this.ProductSelected adapter.Products.[position] (float32 v.Top)

    override this.OnCreateOptionsMenu(menu, inflater) =
        inflater.Inflate (Resource_Menu.menu, menu)
        let cartItem = menu.FindItem (Resource_Id.cart_menu_item)
        let tempBasketBadge = new BadgeDrawable(cartItem.Icon)
        basketBadge <- Some(tempBasketBadge)
        cartItem.SetIcon tempBasketBadge |> ignore

        let order = WebService.CurrentOrder

        if not (badgeCount = order.Products.Length) 
        then tempBasketBadge.SetCountAnimated (order.Products.Count())
        else tempBasketBadge.Count <- order.Products.Count()

        badgeCount <- order.Products.Count()
        WebService.CurrentOrder <- { order with Notification = Some (setBasketCount tempBasketBadge) }
        base.OnCreateOptionsMenu (menu, inflater)
