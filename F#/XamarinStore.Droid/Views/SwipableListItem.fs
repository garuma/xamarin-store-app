namespace XamarinStore

open Android.Content
open Android.Util
open Android.Views
open Android.Widget
open Android.Graphics

type SwipableListItem(context: Context, attrs:IAttributeSet) as this =
    inherit FrameLayout(context, attrs)

    let mutable mainContent:View = null
    let mutable secondaryContent:View = null
    let listener: ViewSwipeTouchListener = new ViewSwipeTouchListener (context, Resource_Id.SwipeContent)

    let shadow:Paint = new Paint ( AntiAlias = true )
    static let DarkTone = 0xd3
    static let LightTone = 0xdd

    do this.SetOnTouchListener listener
       listener.ItemSwipped <- fun _ -> listener.ResetSwipe()

    static member Colors = [| Android.Graphics.Color.Rgb(DarkTone, DarkTone, DarkTone).ToArgb()
                              Android.Graphics.Color.Rgb(LightTone, LightTone, LightTone).ToArgb()
                              Android.Graphics.Color.Rgb(LightTone, LightTone, LightTone).ToArgb()
                              Android.Graphics.Color.Rgb(DarkTone, DarkTone, DarkTone).ToArgb ()|]
    static member Positions = [| 0.0f; 0.15f; 0.85f; 1.0f |]
                                             
    member this.SwipeListener = listener

    member this.MainContent = if mainContent = null then 
                                  mainContent <- this.FindViewById Resource_Id.SwipeContent
                              mainContent

    member this.SecondaryContent = if secondaryContent = null then
                                       secondaryContent <- this.FindViewById Resource_Id.SwipeAfter
                                   secondaryContent

    override this.DispatchDraw canvas =
        // Draw interior shadow
        canvas.Save () |> ignore
        canvas.ClipRect (0, 0, this.Width, this.Height) |> ignore
        canvas.DrawPaint shadow
        canvas.Restore ()

        base.DispatchDraw canvas
        // Draw custom list separator
        canvas.Save () |> ignore
        canvas.ClipRect (0, this.Height - 2, this.Width, this.Height) |> ignore
        canvas.DrawColor (Android.Graphics.Color.Rgb(LightTone, LightTone, LightTone))
        canvas.Restore ()

    override this.OnLayout (changed, left, top, right, bottom) =
        base.OnLayout (changed, left, top, right, bottom)
        shadow.SetShader (new LinearGradient (0.0f, 0.0f, 0.0f, float32 (bottom - top), SwipableListItem.Colors, SwipableListItem.Positions, Shader.TileMode.Repeat)) |> ignore

