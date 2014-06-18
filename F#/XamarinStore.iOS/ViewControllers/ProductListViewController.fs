namespace XamarinStore

open System
open System.Linq
open System.Drawing
open System.Collections.Generic

open MonoTouch.UIKit
open MonoTouch.Foundation
open Product

[<AllowNullLiteral>]
type ProductListCell() as this =
    inherit UITableViewCell()

    let ImageWidth = UIScreen.MainScreen.Bounds.Width * UIScreen.MainScreen.Scale
    let PriceLabelPadding = new SizeF (16.0f, 6.0f)
    let mutable product = Product.CreateProduct()
    let imageView = new TopAlignedImageView ( ClipsToBounds = true)
    let nameLabel = new UILabel ( TextColor = UIColor.White, 
                                  TextAlignment = UITextAlignment.Left, 
                                  Font = UIFont.FromName ("HelveticaNeue-Light", 22.0f))
    do nameLabel.Layer.ShadowRadius <- 3.0f
       nameLabel.Layer.ShadowColor <- UIColor.Black.CGColor
       nameLabel.Layer.ShadowOffset <- new System.Drawing.SizeF(0.0f,1.0f)
       nameLabel.Layer.ShadowOpacity <- 0.5f   
                                
    let priceLabel = new UILabel ( Alpha = 0.95f,
                                   TextColor = UIColor.White,
                                   BackgroundColor = Color.Green.ToUIColor(),
                                   TextAlignment = UITextAlignment.Center,
                                   Font = UIFont.FromName ("HelveticaNeue", 16.0f),
                                   ShadowColor = UIColor.LightGray,
                                   ShadowOffset = new SizeF(0.5f, 0.5f) )

    let updateImage() =
        imageView.LoadUrl (product.ImageForSize ImageWidth)


    do this.SelectionStyle <- UITableViewCellSelectionStyle.None
       this.ContentView.BackgroundColor <- UIColor.LightGray

       priceLabel.Layer.CornerRadius <- 3.0f

       this.ContentView.AddSubviews (imageView, nameLabel, priceLabel)

    static member val CellId = "ProductListCell" with get

    member this.Product
        with get () = product
        and set value =
            product <- value

            nameLabel.Text <- product.Name
            priceLabel.Text <- product.GetPriceDescription().ToLower()
            updateImage()|> Async.StartImmediate

    override this.LayoutSubviews () =
        base.LayoutSubviews ()
        let bounds = this.ContentView.Bounds

        imageView.Frame <- bounds
        nameLabel.Frame <- new RectangleF ( bounds.X + 12.0f,
                                            bounds.Bottom - 58.0f,
                                            bounds.Width,
                                            55.0f )
        let priceSize = (NSObject.FromObject(this.Product.GetPriceDescription()):?> NSString).StringSize(priceLabel.Font)
        priceLabel.Frame <- new RectangleF (
            bounds.Width - priceSize.Width - 2.0f * PriceLabelPadding.Width - 12.0f,
            bounds.Bottom - priceSize.Height - 2.0f * PriceLabelPadding.Height - 14.0f,
            priceSize.Width + 2.0f * PriceLabelPadding.Width,
            priceSize.Height + 2.0f * PriceLabelPadding.Height
        )

type ProductListViewSource(productSelected:Product->unit) =
    inherit UITableViewSource()

    member val Products = [||] with get,set

    override this.RowsInSection (tableview, section) =
        if this.Products = [||] then 1 else this.Products.Length

    override this.RowSelected (tableView, indexPath) =
        if not (this.Products = [||]) then
            productSelected (this.Products.[indexPath.Row])

    override this.GetCell (tableView, indexPath) =
        if this.Products = [||] then
            new SpinnerCell() :> UITableViewCell
        else
            let cell = match tableView.DequeueReusableCell(ProductListCell.CellId) with
                        | :? ProductListCell as cell when not (cell = null) -> cell
                        | _ -> new ProductListCell()

            cell.Product <- this.Products.[indexPath.Row]
            cell :> UITableViewCell

type ProductListViewController(createBasketButton : unit->UIBarButtonItem) as this =
    inherit UITableViewController()

    let title = "Xamarin Store"
    let ProductCellRowHeight = 300.0f

    let source = new ProductListViewSource(fun x-> this.ProductTapped(x))

    let GetData () = async {
        let! products = WebService.Shared.GetProducts ()
        source.Products <- products
        WebService.Shared.PreloadImages(320.0f * UIScreen.MainScreen.Scale) |> Async.StartImmediate
        this.TableView.ReloadData () }

    do // Hide the back button text when you leave this View Controller.
       this.NavigationItem.BackBarButtonItem <- new UIBarButtonItem ("", UIBarButtonItemStyle.Plain, handler=null)
       this.TableView.SeparatorStyle <- UITableViewCellSeparatorStyle.None
       this.TableView.RowHeight <- ProductCellRowHeight
       this.TableView.Source <- source :> UITableViewSource

       GetData () |> Async.StartImmediate

    member val ImageWidth = UIScreen.MainScreen.Bounds.Width * UIScreen.MainScreen.Scale with get
    member val ProductTapped = (fun (x:Product)->()) with get,set

    override this.ViewWillAppear animated =
        base.ViewWillAppear (animated)
        this.NavigationItem.RightBarButtonItem <- createBasketButton()
