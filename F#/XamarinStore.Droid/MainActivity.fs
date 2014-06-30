namespace XamarinStore

open Android.App
open Android.Content.PM
open Android.OS
open Android.Views
open Android.Util
open FileCache
open Order

[<Activity (Label = "Xamarin Store", MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)>]
type MainActivity () =
    inherit Activity ()

    let mutable baseFragment:int = 1

    let GetAnimationsForFragment (fragment:Fragment) =
        match fragment.GetType().Name with
        | "ProductDetailsFragment"-> Resource_Animation.product_detail_in, Resource_Animation.product_detail_out
        | "BasketFragment"        -> Resource_Animation.basket_in,         Resource_Animation.exit
        | _                       -> Resource_Animation.enter,             Resource_Animation.exit

    member private this.SetupActionBar (?showUp) =
        let showUp = defaultArg showUp false
        this.ActionBar.SetDisplayHomeAsUpEnabled showUp

    member private this.SwitchScreens(fragment:Fragment, ?animated, ?isRoot) =
        let animated = defaultArg animated true
        let isRoot = defaultArg isRoot false
        let transaction = this.FragmentManager.BeginTransaction ()

        if animated then
            GetAnimationsForFragment fragment 
            |> transaction.SetCustomAnimations |> ignore

        transaction.Replace (Resource_Id.contentArea, fragment) |> ignore
        if not isRoot then
            transaction.AddToBackStack (null) |> ignore

        this.SetupActionBar (not isRoot)

        transaction.Commit () |> ignore

    member this.ShowProductDetail product itemVerticalOffset =
        let productDetails = new ProductDetailsFragment (product, itemVerticalOffset)

        productDetails.AddToBasket <- (fun p-> 
            WebService.CurrentOrder <- { WebService.CurrentOrder with Products = p::WebService.CurrentOrder.Products }
            this.SetupActionBar ())

        this.SwitchScreens (productDetails)

    override this.OnCreate (bundle) =

        let metrics = new DisplayMetrics()
        this.WindowManager.DefaultDisplay.GetMetrics (metrics)
        Images.ScreenWidth <- float32 metrics.WidthPixels
        FileCache.saveLocation <- this.CacheDir.AbsolutePath
        base.OnCreate (bundle)
        this.SetContentView (Resource_Layout.Main)

        if this.FragmentManager.BackStackEntryCount = 0 then
            let productFragment = new ProductListFragment (ProductSelected = this.ShowProductDetail)
            baseFragment <- productFragment.Id
            this.SwitchScreens (productFragment, false, true)

    override this.OnSaveInstanceState outState =
        base.OnSaveInstanceState outState
        outState.PutInt ("baseFragment", baseFragment)

    override this.OnRestoreInstanceState savedInstanceState =
        base.OnRestoreInstanceState savedInstanceState
        baseFragment <- savedInstanceState.GetInt "baseFragment"

    member private this.OrderCompleted () =
        this.FragmentManager.PopBackStack (baseFragment, PopBackStackFlags.Inclusive)
        this.SetupActionBar ()

        this.SwitchScreens (new BragFragment (), true, true)

    member private this.ShowAddress () =
        let addressFragment = new ShippingDetailsFragment (WebService.Shared.CurrentUser, OrderPlaced = this.OrderCompleted)
        this.SwitchScreens (addressFragment)

    member this.ShowLogin () =
        let loginVc = new LoginFragment ()
        loginVc.LoginSucceeded <- fun () -> this.ShowAddress()
        this.SwitchScreens loginVc
      
    member this.ShowBasket () =
        this.SwitchScreens (new BasketFragment (CheckoutClicked = this.ShowLogin))

    override this.OnMenuItemSelected ( featureId, item) =
        let itemId = item.ItemId
        let CartMenuItem = Resource_Id.cart_menu_item
        if item.ItemId = CartMenuItem 
        then this.ShowBasket ()
             true
        else if item.ItemId = Android.Resource.Id.Home then
            //pop full backstack when going home.   
            this.FragmentManager.PopBackStack (baseFragment, PopBackStackFlags.Inclusive)
            this.SetupActionBar ()
            true
        else base.OnMenuItemSelected (featureId, item)

    override this.OnBackPressed () =
        base.OnBackPressed ()
        this.SetupActionBar (this.FragmentManager.BackStackEntryCount <> 0)
