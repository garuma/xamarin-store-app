namespace XamarinStore

open System
open Android.Content
open Android.Util
open Android.Views
open Android.Graphics
open Android.Animation

type XamarinSpinnerView(context:Context, attrs: IAttributeSet) as this =
    inherit View(context,attrs)

    let mutable rotation = 0.0f
    let mutable scaleX = 1.0f
    let mutable scaleY = 1.0f
    let animation = new AnimatorSet ()
    do let baseDuration = 1000L
       let rotation = ObjectAnimator.OfFloat (this, "rotation", 0.0f, 60.0f)
       rotation.SetDuration(baseDuration) |> ignore
       rotation.RepeatCount <- ValueAnimator.Infinite
       let scale = ObjectAnimator.OfPropertyValuesHolder (
                        this,
                        PropertyValuesHolder.OfFloat ("scaleX", 1.0f, 0.9f),
                        PropertyValuesHolder.OfFloat ("scaleY", 1.0f, 0.9f))
       scale.RepeatMode <- ValueAnimatorRepeatMode.Reverse
       scale.SetDuration (baseDuration / 2L) |> ignore
       scale.RepeatCount <- ValueAnimator.Infinite
       animation.PlayTogether (rotation, scale)

    let hexagon = new Path()
    let cross = new Path()

    let hexagonPaint = new Paint ( Color = new Android.Graphics.Color (0x22, 0x76, 0xB9), AntiAlias = true)
    do hexagonPaint.SetPathEffect (new CornerPathEffect (30.0f)) |> ignore

    let crossPaint = new Paint( Color = Android.Graphics.Color.White, AntiAlias = true)
    let transformationMatrix = new Matrix()

    let DrawCross (canvas: Canvas) =
        let smallSegment = float32 this.Width / 6.0f
        let width = float32 this.Width
        let height = float32 this.Height

        let path = cross
        cross.Reset ()

        path.MoveTo (0.0f, 0.0f)
        path.RLineTo (smallSegment, 0.0f)
        path.LineTo (width / 2.0f, height / 2.0f)
        path.LineTo (width - smallSegment, 0.0f)
        path.RLineTo (smallSegment, 0.0f)
        path.LineTo (width / 2.0f + smallSegment, height / 2.0f)
        path.LineTo (width, height)
        path.RLineTo (-smallSegment, 0.0f)
        path.LineTo (width / 2.0f, height / 2.0f)
        path.LineTo (smallSegment, height)
        path.RLineTo (-smallSegment, 0.0f)
        path.LineTo (width / 2.0f - smallSegment, height / 2.0f)
        path.Close ()

        canvas.DrawPath (path, crossPaint)

    member this.StartAnimation() =
        animation.Start ()

    member this.DrawHexagon (canvas : Canvas ) =
        // The extra padding is to avoid edges being clipped
        let padding = float32 (TypedValue.ApplyDimension (ComplexUnitType.Dip, 8.0f, this.Resources.DisplayMetrics))
        let halfHeight = (float32 this.Height - padding) / 2.0f
        let side = (float32 this.Width - padding) / 2.0f
        let foo = float32 (Math.Sqrt ((float side) * (float side) - (float(halfHeight * halfHeight))))

        let path = hexagon
        hexagon.Reset ()
        path.MoveTo (float32 this.Width / 2.0f, padding / 2.0f)
        path.RLineTo (-side / 2.0f, 0.0f)
        path.RLineTo (-foo, halfHeight)
        path.RLineTo (foo, halfHeight)
        path.RLineTo (side, 0.0f)
        path.RLineTo (foo, -halfHeight)
        path.RLineTo (-foo, -halfHeight)
        path.Close ()

        let m = transformationMatrix
        m.Reset ()
        let centerX = float32 this.Width / 2.0f
        let centerY = float32 this.Height / 2.0f
        m.PostRotate (this.Rotation, centerX, centerY) |> ignore
        m.PostScale (this.ScaleX, this.ScaleY, centerX, centerY) |> ignore
        path.Transform (m)

        canvas.DrawPath (path, hexagonPaint)

    override this.Rotation
        with get () = rotation
        and set value =
            rotation <- value
            this.Invalidate ()

    override this.ScaleX
        with get () = scaleX
        and set value =
            scaleX <- value
            this.Invalidate ()

    override this.ScaleY
        with get () = scaleY
        and set value =
            scaleY <- value
            this.Invalidate ()

    override this.OnDraw canvas =
        base.OnDraw (canvas)
        this.DrawHexagon (canvas)
        canvas.Save () |> ignore
        canvas.Scale (0.4f, 0.5f, float32 this.Width / 2.0f, float32 this.Height / 2.0f)
        DrawCross (canvas)
        canvas.Restore ()
