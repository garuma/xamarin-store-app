namespace XamarinStore

open System
open System.Linq

open Android.App
open Android.OS
open Android.Views
open Android.Widget
open Android.Animation
open Android.Graphics
open Product
open Helpers
open Order
open System.Threading.Tasks

type ProductDetailsFragment(product:Product, slidingDelta) as this =
    inherit Fragment()

    let productImage : ImageView = null
    let mutable currentProduct = ref product
    let images = product.GetImageUrls().ToArray().RandomArrayPermutation()

    let productDrawable = new KenBurnsDrawable(Color.DarkBlue.ToAndroidColor())
    let mutable kenBurnsMovement: ValueAnimator = null
    let mutable kenBurnsAlpha:ValueAnimator = null

    let mutable currentIndex = 0
    let mutable productImage:ImageView = null
    let mutable shouldAnimatePop = false
    let mutable basketBadge: BadgeDrawable option = None
    let mutable sizeSpinner:Spinner = null
    let mutable colorSpinner:Spinner = null

    let mutable cached = false

    let PrecacheNextImage () =
        if (currentIndex + 1 >= images.Length) then
            cached <- true
        else if not cached then
            let next = currentIndex + 1
            let image = images.[next]
            //No need to await the precache to finish
            FileCache.Download (Product.ImageForSize (image, Images.ScreenWidth)) |> Async.Ignore |> Async.StartImmediate

    let NextImage () = async {
        currentIndex <- (currentIndex + 1) % images.Length
        let image = images.[currentIndex]
        do! Images.SetBitmapImageFromUrlAsync productDrawable (Product.ImageForSize(image, Images.ScreenWidth))
        PrecacheNextImage () } |> Async.StartImmediate

    let AnimateImages () =
        if images.Length = 1 then
            Images.SetImageViewFromUrlAsync productImage (Product.ImageForSize(images.[0], Images.ScreenWidth))
            |> Async.StartImmediate
        if images.Length > 1 then
            productImage.ViewTreeObserver.AddOnGlobalLayoutListener (this)

    interface ViewTreeObserver.IOnGlobalLayoutListener with
        member this.OnGlobalLayout() = 
            async{ productImage.ViewTreeObserver.RemoveGlobalOnLayoutListener this
                   let! img1 = Images.FromUrl(Product.ImageForSize (images.[0], Images.ScreenWidth))

                   let! img2 = Images.FromUrl (Product.ImageForSize (images.[1], Images.ScreenWidth))

                   productDrawable.FirstBitmap <- img1
                   productDrawable.SecondBitmap <- img2

                   productImage.SetImageDrawable productDrawable

                   currentIndex <- currentIndex + 1
                      
                   let evaluator = new MatrixEvaluator ()
                   let finalMatrix = new Matrix ()

                   finalMatrix.SetTranslate (-100.0f, -(float32 productDrawable.FirstBitmap.Height) / 1.3f + (float32 productImage.Height))

                   finalMatrix.PostScale (1.27f, 1.27f) |> ignore
                   kenBurnsMovement <- ValueAnimator.OfObject (evaluator, new Matrix (), finalMatrix)
                   kenBurnsMovement.Update.Add(fun x-> productDrawable.SetMatrix (x.Animation.AnimatedValue :?> Matrix))
                   kenBurnsMovement.SetDuration 14000L |> ignore
                   kenBurnsMovement.RepeatMode <- ValueAnimatorRepeatMode.Reverse
                   kenBurnsMovement.RepeatCount <- ValueAnimator.Infinite
                   kenBurnsMovement.Start()
                     
                   kenBurnsAlpha <- ObjectAnimator.OfInt (productDrawable, "alpha", 0, 0, 0, 255, 255, 255)
                   kenBurnsAlpha.SetDuration kenBurnsMovement.Duration |> ignore
                   kenBurnsAlpha.RepeatMode <- ValueAnimatorRepeatMode.Reverse
                   kenBurnsAlpha.RepeatCount <- ValueAnimator.Infinite
                   kenBurnsAlpha.AnimationRepeat.Add(fun x-> NextImage())
                   kenBurnsAlpha.Start()
                   }

            |> Async.StartImmediate
            |> ignore

    member val AddToBasket = ignore with get,set

    override this.OnCreate savedInstanceState =
        base.OnCreate savedInstanceState
        this.RetainInstance <- true
        this.SetHasOptionsMenu true

    override this.OnCreateView(inflater, container, savedInstanceState) =
        inflater.Inflate(Resource_Layout.ProductDetail, null, true)


    member this.LoadOptions() =
        let sizeAdapter = new ArrayAdapter<ProductSize>(this.Activity, Android.Resource.Layout.SimpleSpinnerDropDownItem, (!currentProduct).Sizes.ToArray())
        sizeAdapter.SetDropDownViewResource (Android.Resource.Layout.SimpleSpinnerDropDownItem)

        sizeSpinner.Adapter <- sizeAdapter
        sizeSpinner.SetSelection ((!currentProduct).Sizes |> Array.findIndex(fun x->x = ((!currentProduct).Size)))

        let colorAdapter = new ArrayAdapter<ProductColor>(this.Activity, Android.Resource.Layout.SimpleSpinnerDropDownItem, (!currentProduct).Colors.ToArray())
        colorAdapter.SetDropDownViewResource (Android.Resource.Layout.SimpleSpinnerDropDownItem)

        colorSpinner.Adapter <- colorAdapter

    override this.OnViewCreated(view, savedInstanceState) =
        base.OnViewCreated(view, savedInstanceState)

        productImage <- this.View.FindViewById<ImageView> Resource_Id.productImage
        sizeSpinner <- this.View.FindViewById<Spinner> Resource_Id.productSize
        colorSpinner <- this.View.FindViewById<Spinner> Resource_Id.productColor

        let addToBasket = this.View.FindViewById<Button> Resource_Id.addToBasket
        addToBasket.Click.Add(fun x->
            currentProduct := { (!currentProduct) with 
                                    Size = (!currentProduct).Sizes.[sizeSpinner.SelectedItemPosition]
                                    Color = (!currentProduct).Colors.[colorSpinner.SelectedItemPosition] }
            shouldAnimatePop <- true
            this.Activity.FragmentManager.PopBackStack()
            this.AddToBasket !currentProduct)

        this.View.FindViewById<TextView>(Resource_Id.productTitle).Text <- (!currentProduct).Name
        this.View.FindViewById<TextView>(Resource_Id.productPrice).Text <- (!currentProduct).GetPriceDescription()
        this.View.FindViewById<TextView>(Resource_Id.productDescription).Text <- (!currentProduct).Description

        (this.View :?> SlidingLayout).InitialMainViewDelta <- slidingDelta

        this.LoadOptions ()

    override this.OnStart () =
        base.OnStart ()
        AnimateImages ()

    override this.OnStop () =
        base.OnStop ()
        if not (kenBurnsAlpha = null) then kenBurnsAlpha.Cancel ()
        if not (kenBurnsMovement = null) then kenBurnsMovement.Cancel ()

    override this.OnCreateOptionsMenu (menu, inflater) =
        inflater.Inflate (Resource_Menu.menu, menu)
        let cartItem = menu.FindItem (Resource_Id.cart_menu_item)
        let tempBasketBadge = new BadgeDrawable (cartItem.Icon)
        basketBadge <- Some tempBasketBadge
        cartItem.SetIcon tempBasketBadge |> ignore

        let order = WebService.CurrentOrder
        tempBasketBadge.Count <- order.Products.Length
        WebService.CurrentOrder <- { order with Notification = Some (fun (x:Order) -> tempBasketBadge.SetCountAnimated (x.Products.Length)) }
        base.OnCreateOptionsMenu (menu, inflater)

    override this.OnCreateAnimator(transit, enter, nextAnim) =
        if not enter && shouldAnimatePop then
            AnimatorInflater.LoadAnimator (this.View.Context, Resource_Animation.add_to_basket_in) |> ignore
        base.OnCreateAnimator (transit, enter, nextAnim)
