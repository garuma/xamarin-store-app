namespace XamarinStore

open System
open System.Collections.Generic
open System.Linq
open MonoTouch.Foundation
open MonoTouch.UIKit
open System.Drawing

open System.Threading.Tasks
open MonoTouch.CoreGraphics
open MonoTouch.CoreAnimation
open Product
open Helpers

type ProductDetailPageSource(items: UITableViewCell array) =
    inherit UITableViewSource()

    override this.RowsInSection (tableview, section) =
        items.Length

    override this.GetCell(tableView, indexPath) =
        items.[indexPath.Row]

    override this.GetHeightForRow (tableView, indexPath) =
        items.[indexPath.Row].Frame.Height

    override this.RowSelected (tableView, indexPath) =
        match items.[indexPath.Row] with
        | :? StringSelectionCell -> (items.[indexPath.Row] :?> StringSelectionCell).Tap()
        | _ -> ()

        tableView.DeselectRow(indexPath, true)

type ProductDetailViewController(product: Product, createBasketButton:unit->UIBarButtonItem) as this =
    inherit UITableViewController()

    let mutable CurrentProduct = product
    let tshirtIcon = UIImage.FromBundle("t-shirt")
    let mutable sizeOptions:ProductSize array = [||]
    let mutable colorOptions: ProductColor array = [||]
    let mutable colorCell : StringSelectionCell = null
    let mutable sizeCell : StringSelectionCell = null
    let mutable imageView : JBKenBurnsView = null
    let bottomView = new BottomButtonView ( ButtonText = "Add to Basket" )
    let mutable imageUrls:string array = [||]

    let addToBasket() = async {
        let center = bottomView.Button.ConvertPointToView (bottomView.Button.ImageView.Center, this.NavigationController.View)
        let imageView = new UIImageView (tshirtIcon, Center = center, ContentMode = UIViewContentMode.ScaleAspectFill )
        let backgroundView = new UIImageView (UIImage.FromBundle("circle"), Center = center)

        this.NavigationController.View.AddSubview backgroundView
        this.NavigationController.View.AddSubview imageView
        this.animateView (imageView) |> Async.StartImmediate
        this.animateView (backgroundView) |> Async.StartImmediate

        this.NavigationItem.RightBarButtonItem <- createBasketButton()

        this.AddToBasket CurrentProduct } |> Async.StartImmediate

    let loadImages() = async {
        for i = 0 to imageUrls.Length - 1 do
            let! path = FileCache.Download (Product.ImageForSize (imageUrls.[i], 320.0f * UIScreen.MainScreen.Scale))
            imageView.Images.[i] <- UIImage.FromFile path
    }

    let GetOptionsCells () =
        sizeCell <- new StringSelectionCell(this.View,
                                            Text = "Size",
                                            Items = sizeOptions.Select(fun (x:ProductSize) -> x.Description),
                                            DetailText = CurrentProduct.Size.Description,
                                            SelectionChanged = fun () ->let size = sizeOptions.[sizeCell.SelectedIndex]
                                                                        CurrentProduct <-  { CurrentProduct with Size = size } )

        colorCell <- new StringSelectionCell(this.View,
                                             Text = "Color",
                                             Items = colorOptions.Select(fun (x:ProductColor) -> x.Name),
                                             DetailText = CurrentProduct.Color.Name,
                                             SelectionChanged = fun () -> let color = colorOptions.[colorCell.SelectedIndex]
                                                                          CurrentProduct <- {CurrentProduct with Color = color } )
        [sizeCell :> UITableViewCell; colorCell :> UITableViewCell]

    let loadProductData () =
        // Add spinner while loading data.
        this.TableView.Source <- new ProductDetailPageSource ([| new SpinnerCell()|])

        colorOptions <- CurrentProduct.Colors
        sizeOptions <- CurrentProduct.Sizes
        imageUrls <-  CurrentProduct.GetImageUrls().RandomArrayPermutation()

        imageView <- new JBKenBurnsView ( Frame = new RectangleF (0.0f, -60.0f, 320.0f, 400.0f),
                                         Images = Enumerable.Range(0,imageUrls.Length).Select(fun x -> new UIImage()).ToList(),
                                         UserInteractionEnabled = false)
        loadImages () |> Async.StartImmediate
        let productDescriptionView = new ProductDescriptionView (CurrentProduct,
                                                                 Frame = new RectangleF (0.0f, 0.0f, 320.0f, 120.0f))
        let headerView = new UIView(new RectangleF(0.0f,0.0f,imageView.Frame.Width,imageView.Frame.Bottom))
        headerView.AddSubview(imageView)
        this.TableView.TableHeaderView <- headerView

        let tableItems = [|yield new CustomViewCell (productDescriptionView) :> UITableViewCell; yield! GetOptionsCells ()|]

        this.TableView.Source <- new ProductDetailPageSource(tableItems)
        this.TableView.ReloadData ()

    do this.Title <- product.Name
       loadProductData()
       this.TableView.TableFooterView <- new UIView (new RectangleF (0.0f, 0.0f, 0.0f, BottomButtonView.Height))

       bottomView.Button.Image <- tshirtIcon
       bottomView.ButtonTapped <- addToBasket
       this.View.AddSubview bottomView

    member val AddToBasket = (fun (x:Product)->()) with get,set

    member this.animateView (view:UIView) = async {
        let size = view.Frame.Size
        let grow = new SizeF(size.Width * 1.7f, size.Height * 1.7f)
        let shrink = new SizeF(size.Width * 0.4f, size.Height * 0.4f)
        let tcs = new TaskCompletionSource<bool> ()
        //Set the animation path
        let pathAnimation = CAKeyFrameAnimation.GetFromKeyPath("position")
        pathAnimation.CalculationMode <- CAAnimation.AnimationPaced.ToString()
        pathAnimation.FillMode <- CAFillMode.Forwards.ToString()
        pathAnimation.RemovedOnCompletion <- false
        pathAnimation.Duration <- 0.5

        let path = new UIBezierPath ()
        path.MoveTo (view.Center)
        path.AddQuadCurveToPoint (new PointF (290.0f, 34.0f), new PointF(view.Center.X,this.View.Center.Y))
        pathAnimation.Path <- path.CGPath

        //Set size change
        let growAnimation = CABasicAnimation.FromKeyPath("bounds.size")
        growAnimation.To <- NSValue.FromSizeF (grow)
        growAnimation.FillMode <- CAFillMode.Forwards.ToString()
        growAnimation.Duration <- 0.1
        growAnimation.RemovedOnCompletion <- false

        let shrinkAnimation = CABasicAnimation.FromKeyPath("bounds.size")
        shrinkAnimation.To <- NSValue.FromSizeF (shrink)
        shrinkAnimation.FillMode <- CAFillMode.Forwards.ToString()
        shrinkAnimation.Duration <- 0.4
        shrinkAnimation.RemovedOnCompletion <- false
        shrinkAnimation.BeginTime <- 0.1


        let animations = new CAAnimationGroup ()
        animations.FillMode <- CAFillMode.Forwards.ToString()
        animations.RemovedOnCompletion <- false
        animations.Animations <- [| pathAnimation; growAnimation; shrinkAnimation|]
        animations.Duration <- 0.5
        animations.AnimationStopped.Add(fun _ -> tcs.TrySetResult(true) |> ignore )
        view.Layer.AddAnimation (animations,"movetocart")
        NSTimer.CreateScheduledTimer (0.5, fun () -> view.RemoveFromSuperview ()) |> ignore
        do! tcs.Task |> Async.AwaitTask |> Async.Ignore }

    override this.ViewWillAppear animated =
        base.ViewWillAppear (animated)
        this.NavigationItem.RightBarButtonItem <- createBasketButton()
        imageView.Animate()
        let bottomRow = NSIndexPath.FromRowSection (this.TableView.NumberOfRowsInSection (0) - 1, 0)
        this.TableView.ScrollToRow (bottomRow,UITableViewScrollPosition.Top, false)

    override this.ViewDidLayoutSubviews () =
        base.ViewDidLayoutSubviews ()

        let mutable bound = this.View.Bounds
        bound.Y <- bound.Bottom - BottomButtonView.Height
        bound.Height <- BottomButtonView.Height
        bottomView.Frame <- bound
