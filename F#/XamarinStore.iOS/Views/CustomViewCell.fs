namespace XamarinStore

open System
open MonoTouch.UIKit

type CustomViewCell(child:UIView) as this =
    inherit UITableViewCell(UITableViewCellStyle.Default, "CustomViewCell")

    do this.SelectionStyle <- UITableViewCellSelectionStyle.None
       let frame = child.Frame
       this.Frame <- new Drawing.RectangleF(frame.X, 0.0f, frame.Width, frame.Bottom)
       this.ContentView.AddSubview child

    member val Padding = new UIEdgeInsets() with get, set
    member val ResizeChild = true with get, set

    override this.LayoutSubviews () =
        base.LayoutSubviews ();
        if not this.ResizeChild then
            child.Center <- this.ContentView.Center
        else
            let bounds = this.ContentView.Bounds;
            child.Frame <- new Drawing.RectangleF(bounds.X + this.Padding.Left,
                                                  bounds.Y + this.Padding.Top,
                                                  bounds.Width - (this.Padding.Left + this.Padding.Right),
                                                  bounds.Height - (this.Padding.Bottom + this.Padding.Top));

