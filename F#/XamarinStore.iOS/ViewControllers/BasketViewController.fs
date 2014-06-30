namespace XamarinStore

open System
open MonoTouch.UIKit
open MonoTouch.CoreGraphics
open System.Drawing
open MonoTouch.CoreAnimation
open System.Linq
open System.Threading.Tasks
open Order
open Product

type ProductCell() as this =
    inherit UITableViewCell(UITableViewCellStyle.Default, ProductCell.Key.ToString())

    let NameLabel = new UILabel ( Text = "Name",
                                  Font = UIFont.BoldSystemFontOfSize (17.0f),
                                  BackgroundColor = UIColor.Clear,
                                  TextColor = UIColor.Black )
    let SizeLabel = new UILabel ( Text = "Size",
                                  Font = UIFont.BoldSystemFontOfSize (12.0f),
                                  BackgroundColor = UIColor.Clear,
                                  TextColor = UIColor.LightGray )
    let ColorLabel = new UILabel ( Text = "Color",
                                   Font = UIFont.BoldSystemFontOfSize (12.0f),
                                   BackgroundColor = UIColor.Clear,
                                   TextColor = UIColor.LightGray )
    let PriceLabel = new UILabel ( Text = "Price",
                                   Font = UIFont.BoldSystemFontOfSize (15.0f),
                                   BackgroundColor = UIColor.Clear,
                                   TextAlignment = UITextAlignment.Right,
                                   TextColor = Color.Blue.ToUIColor() )
    let LineView = new UIView ( BackgroundColor = UIColor.LightGray )

    let leftPadding = 15.0f
    let topPadding = 5.0f
    let image = lazy( UIImage.FromBundle ("shirt_image"))
    let imageSize = new SizeF (55.0f, 55.0f)

    do this.SelectionStyle <- UITableViewCellSelectionStyle.None
       this.ContentView.BackgroundColor <- UIColor.Clear
       this.ImageView.Image <- image.Value
       this.ImageView.ContentMode <- UIViewContentMode.ScaleAspectFill
       this.ImageView.Frame <- new RectangleF (PointF.Empty, imageSize)
       this.ImageView.BackgroundColor <- UIColor.Clear
       this.ImageView.Layer.CornerRadius <- 5.0f
       this.ImageView.Layer.MasksToBounds <- true

       NameLabel.SizeToFit()
       this.ContentView.AddSubview NameLabel

       SizeLabel.SizeToFit()
       this.ContentView.Add SizeLabel

       ColorLabel.SizeToFit ()
       this.ContentView.AddSubview (ColorLabel)

       PriceLabel.SizeToFit ()
       this.AccessoryView <- new UIView (new RectangleF (0.0f, 0.0f, PriceLabel.Frame.Width + 10.0f, 54.0f), BackgroundColor = UIColor.Clear)
       this.AccessoryView.AddSubview (PriceLabel)

       this.ContentView.AddSubview (LineView)

    static member val Key = "productCell" with get

    override this.LayoutSubviews () =
        base.LayoutSubviews ()

        let bounds = this.ContentView.Bounds
        let midY = bounds.GetMidY ()

        let center = new PointF(imageSize.Width / 2.0f + leftPadding, midY)
        this.ImageView.Frame <- new RectangleF(PointF.Empty, imageSize)
        this.ImageView.Center <- center


        let mutable x = this.ImageView.Frame.Right + leftPadding
        let mutable y = this.ImageView.Frame.Top
        let labelWidth = bounds.Width - (x + (leftPadding * 2.0f))


        NameLabel.Frame <- new RectangleF (x, y, labelWidth, NameLabel.Frame.Height)
        y <- NameLabel.Frame.Bottom

        SizeLabel.Frame <- new RectangleF (x, y, labelWidth, SizeLabel.Frame.Height)
        y <- SizeLabel.Frame.Bottom

        ColorLabel.Frame <- new RectangleF (x, y, labelWidth, ColorLabel.Frame.Height)
        y <- ColorLabel.Frame.Bottom + topPadding
        LineView.Frame <- new RectangleF (0.0f, this.Bounds.Height - 0.5f, this.Bounds.Width, 0.5f)

    member this.Update (product:Product) = async {
        NameLabel.Text <- product.Name
        SizeLabel.Text <- product.Size.Description
        ColorLabel.Text <- product.Color.Name
        PriceLabel.Text <- product.GetPriceDescription()

        this.ImageView.Image <- image.Value
        let! image = FileCache.Download(product.ImageForSize(320.0f))
        this.ImageView.Image <- UIImage.FromFile (image) }

type BasketViewTableViewSource() =
    inherit UITableViewSource()

    member val RowDeleted = fun ()->() with get,set

    //region implemented abstract members of UITableViewSource

    override this.RowsInSection(tableview, section) =
        WebService.CurrentOrder.Products.Length

    override this.GetCell (tableView, indexPath) =
        let cell = match tableView.DequeueReusableCell(ProductCell.Key.ToString()) with
                   | :? ProductCell as cell ->cell
                   | _ -> new ProductCell ()
        //No need to wait to return the cell It will update when the data is ready
        cell.Update(WebService.CurrentOrder.Products.[indexPath.Row]) |> Async.StartImmediate
        cell :> UITableViewCell

    //endregion

    override this.EditingStyleForRow (tableView, indexPath) =
        UITableViewCellEditingStyle.Delete

    override this.CommitEditingStyle (tableView, editingStyle, indexPath) =
        if editingStyle = UITableViewCellEditingStyle.Delete then
            WebService.CurrentOrder <- WebService.CurrentOrder.WithoutProduct (WebService.CurrentOrder.Products.[indexPath.Row])
            tableView.DeleteRows ([| indexPath |], UITableViewRowAnimation.Fade)
            this.RowDeleted ()

type BasketViewController() as this =
    inherit UITableViewController()

    let mutable EmptyCartImageView = null

    let totalAmount = new UILabel ( Text = "$1,000",
                                    TextColor = UIColor.White,
                                    TextAlignment = UITextAlignment.Center,
                                    Font = UIFont.BoldSystemFontOfSize(17.0f))

    let UpdateTotals () =
        if WebService.CurrentOrder.Products.Length = 0 then
           totalAmount.Text <- ""
        else
            let total = WebService.CurrentOrder.Products.Sum(fun x -> x.Price)
            totalAmount.Text <- total.ToString("C")

    let CheckEmpty animate =
        EmptyCartImageView <- new EmptyBasketView ( Alpha = if animate then 0.0f else 1.0f)
        if WebService.CurrentOrder.Products.Length = 0 then
            this.View.AddSubview (EmptyCartImageView)
            this.View.BringSubviewToFront (EmptyCartImageView)
            if animate then
                UIView.Animate (0.25, fun () -> EmptyCartImageView.Alpha <- 1.0f)

        else if EmptyCartImageView <> null then
            EmptyCartImageView.RemoveFromSuperview ()
            EmptyCartImageView <- null

    let bottomView = new BottomButtonView ( ButtonText = "Checkout",
                                            ButtonTapped = fun () -> this.Checkout() )

    do this.Title <- "Your Basket"
       //This hides the back button text when you leave this View Controller
       this.NavigationItem.BackBarButtonItem <- new UIBarButtonItem ("", UIBarButtonItemStyle.Plain, handler = null)
       this.TableView.Source <- new BasketViewTableViewSource ( RowDeleted = fun ()->UpdateTotals()
                                                                                     CheckEmpty(true))
       this.TableView.SeparatorStyle <- UITableViewCellSeparatorStyle.None
       this.TableView.RowHeight <- 75.0f
       this.TableView.TableFooterView <- new UIView (new RectangleF (0.0f, 0.0f, 0.0f, BottomButtonView.Height))
       this.View.AddSubview bottomView
       CheckEmpty(false)
       totalAmount.SizeToFit ()
       this.NavigationItem.RightBarButtonItem <- new UIBarButtonItem (totalAmount)
       UpdateTotals ()

    
    member val Checkout = fun ()->() with get,set

    override this.ViewDidLayoutSubviews () =
        base.ViewDidLayoutSubviews ()
        let mutable bound = this.View.Bounds
        bound.Y <- bound.Bottom - BottomButtonView.Height
        bound.Height <- BottomButtonView.Height
        bottomView.Frame <- bound

        if EmptyCartImageView <> null then
            EmptyCartImageView.Frame <- this.View.Bounds