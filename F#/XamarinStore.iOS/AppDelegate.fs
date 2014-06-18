namespace XamarinStore

open System
open System.Linq
open System.Drawing
open System.Collections.Generic

open MonoTouch.UIKit
open MonoTouch.Foundation
open MonoTouch.CoreAnimation
open MonoTouch.CoreGraphics
open FileCache
open Product

[<Register ("AppDelegate")>]
type AppDelegate () as this =
    inherit UIApplicationDelegate ()

    let window = new UIWindow (UIScreen.MainScreen.Bounds)
    let mutable navigation:UINavigationController = null

    let button = new BasketButton(Frame = new RectangleF(0.0f, 0.0f, 44.0f, 44.0f))

    do button.TouchUpInside.Add(fun _ -> this.ShowBasket())

    member this.CreateBasketButton () =
        button.ItemsCount <- WebService.CurrentOrder.Products.Length
        new UIBarButtonItem(button)

    member this.ShowBasket () =
        let basketVc = new BasketViewController()
        basketVc.Checkout <- fun () -> this.ShowLogin ()
        navigation.PushViewController (basketVc, true)

    member this.ShowProductDetail (product: Product) =
        let productDetails = new ProductDetailViewController(product, fun ()->this.CreateBasketButton())
        productDetails.AddToBasket <- fun product->
                                        WebService.CurrentOrder <- { WebService.CurrentOrder with Products = product::WebService.CurrentOrder.Products }
                                        this.UpdateProductsCount()

        navigation.PushViewController (productDetails, true)

    // This method is invoked when the application is ready to run.
    override this.FinishedLaunching (app, options) =
        FileCache.saveLocation <- System.IO.Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.Personal)).ToString () + "/tmp"

        UIApplication.SharedApplication.SetStatusBarStyle (UIStatusBarStyle.LightContent, false)

        UINavigationBar.Appearance.SetTitleTextAttributes(new UITextAttributes (TextColor = UIColor.White))

        let productVc = new ProductListViewController (fun ()->this.CreateBasketButton())
        productVc.ProductTapped <-fun product-> this.ShowProductDetail product
        navigation <- new UINavigationController (productVc)

        navigation.NavigationBar.TintColor <- UIColor.White
        navigation.NavigationBar.BarTintColor <- Color.Blue.ToUIColor()

        window.RootViewController <- navigation
        
        window.MakeKeyAndVisible ()
        true

    member this.ShowLogin () =
        let loginVc = new LoginViewController ()
        loginVc.LoginSucceeded <- fun _ -> this.ShowAddress ()
        navigation.PushViewController (loginVc, true)

    member this.ShowAddress () =
        let addreesVc = new ShippingAddressViewController (WebService.Shared.CurrentUser)
        addreesVc.ShippingComplete <- fun _ -> this.ProccessOrder ()
        navigation.PushViewController (addreesVc, true)

    member this.ProccessOrder() =
        let processing = new ProcessingViewController (WebService.Shared.CurrentUser)
        processing.OrderPlaced <- fun _ -> this.OrderCompleted ()
        navigation.PresentViewController (new UINavigationController(processing), true, null)

    member this.OrderCompleted () =
        navigation.PopToRootViewController true |> ignore

    member this.UpdateProductsCount() =
        button.UpdateItemsCount WebService.CurrentOrder.Products.Length

module Main =
    [<EntryPoint>]
    let main args =
        UIApplication.Main (args, null, "AppDelegate")
        0

