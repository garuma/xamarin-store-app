namespace XamarinStore

open System
open System.Collections.Generic
open System.Linq
open System.Text
open MonoTouch.UIKit
open Product

type ProductDescriptionView(product:Product) as this =
    inherit UIView()

    let padding = 20.0f
    let priceWidth = 50.0f

    let name = new UILabel( Text = "Name",
                                   BackgroundColor = UIColor.Clear,
                                   TextColor = UIColor.DarkGray,
                                   Font = UIFont.SystemFontOfSize(25.0f),
                                   TranslatesAutoresizingMaskIntoConstraints = false)

    let descriptionLabel = new UILabel( BackgroundColor = UIColor.Clear,
                                        TextColor = UIColor.DarkGray,
                                        TranslatesAutoresizingMaskIntoConstraints = false,
                                        Font = UIFont.SystemFontOfSize(12.0f),
                                        LineBreakMode = UILineBreakMode.WordWrap,
                                        Lines = 0)
    let price = new UILabel( BackgroundColor = UIColor.Clear,
                             Text = "Price",
                             TextColor = Color.Blue.ToUIColor(),
                             TranslatesAutoresizingMaskIntoConstraints = false)

    do name.SizeToFit()
       this.AddSubview(name)

       this.AddSubview(descriptionLabel)

       price.SizeToFit()
       this.AddSubview(price)

       name.Text <- product.Name
       descriptionLabel.Text <- product.Description
       price.Text <- product.GetPriceDescription()
    
    member val Name = name with get,set
    member val DescriptionLabel = descriptionLabel with get,set
    member val Price = price with get,set

    override this.LayoutSubviews() =
        base.LayoutSubviews()
        let bounds = this.Bounds
        let mutable frame = this.Name.Frame

        frame.Width <- bounds.Width - (priceWidth + padding * 2.0f)
        frame.Y <- padding
        frame.X <- padding
        this.Name.Frame <- frame

        frame <- this.Price.Frame
        frame.Y <- padding + (this.Name.Frame.Height - frame.Height)/2.0f
        frame.X <- this.Name.Frame.Right + padding
        frame.Width <- priceWidth
        this.Price.Frame <- frame

        let mutable frame = bounds
        frame.Y <- this.Name.Frame.Bottom
        frame.X <- padding
        frame.Width <- frame.Width - padding*2.0f
        frame.Height <- frame.Height - frame.Y
        this.DescriptionLabel.Frame <- frame
