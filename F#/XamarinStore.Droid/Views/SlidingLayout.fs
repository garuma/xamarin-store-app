namespace XamarinStore

open System
open Android.Content
open Android.Views
open Android.Widget
open Android.Util

type SlidingLayout(context, attrs:IAttributeSet) as this = 
    inherit LinearLayout(context, attrs)

    let PrimaryViewId = Resource_Id.productImage
    let SecondaryViewId = Resource_Id.descriptionLayout

    let mutable primaryView = null
    let mutable secondaryView = null

    do this.Orientation <- Orientation.Vertical

    member val InitialMainViewDelta = 0.0f with get,set
    member this.PrimaryView with get () = if primaryView = null then this.FindViewById PrimaryViewId else primaryView
    member this.SecondaryView with get () = if secondaryView = null then this.FindViewById SecondaryViewId else secondaryView

    // Inverts children drawing order so that our main item (top) is drawn last
    override this.GetChildDrawingOrder(childCount, i) = childCount - 1 - i

    // Slight hijack of the existing property so that we don't need to define our own
    // which would require Mono.Android.Export
    override this.TranslationY
        with get () = this.PrimaryView.TranslationY / this.InitialMainViewDelta
        and set value = this.PrimaryView.TranslationY <- value * this.InitialMainViewDelta

    override this.Alpha
        with get () = this.SecondaryView.Alpha
        and set value = this.SecondaryView.Alpha <- value

    override this.TranslationX
            with get () = base.TranslationX
            and set value = let power = float32 (Math.Pow(float value, 5.0))
                            base.TranslationX <- power * (float32 this.Width / 2.0f)
                            base.TranslationY <- -1.0f * power * (float32 this.Height / 2.0f)
                            base.Alpha <- 1.0f - value
                            base.ScaleX <- 1.0f - 0.8f * value
                            base.ScaleY <- base.ScaleX
