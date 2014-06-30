namespace XamarinStore

open Android.Graphics
open Android.Graphics.Drawables

type KenBurnsDrawable (defaultColor: Color) =
    inherit Drawable()

    let mutable alpha = 0
    let mutable matrix : Matrix = null
    let paint = new Paint(AntiAlias = false, FilterBitmap = false)
    let mutable secondSlot = false

    let mutable bmp1:Bitmap = null
    let mutable bmp2:Bitmap = null
    let mutable shader1:BitmapShader = null
    let mutable shader2:BitmapShader = null

    interface IBitmapHolder with
        member this.SetImageBitmap bmp =
            if secondSlot 
            then this.SecondBitmap <- bmp
            else this.FirstBitmap <- bmp
            secondSlot <- not secondSlot
            

    member this.FirstBitmap 
        with get () = bmp1
        and set value = bmp1 <- value
                        shader1 <- null
                        this.InvalidateSelf()    

    member this.SecondBitmap 
        with get () = bmp2
        and set value = bmp2 <- value
                        shader2 <- null
                        this.InvalidateSelf()
   
    override this.Draw (canvas:Canvas) =
        let bounds = this.Bounds

        if alpha <> 255 then
            paint.Alpha <- 255
            if this.SecondBitmap <> null then
                if shader1 = null then
                    shader1 <- new BitmapShader(this.FirstBitmap, Shader.TileMode.Clamp, Shader.TileMode.Clamp)
                shader1.SetLocalMatrix matrix
                paint.SetShader shader1 |> ignore
                canvas.DrawRect(bounds, paint)
            else
                canvas.DrawColor defaultColor
        if alpha <> 0 then
            paint.Alpha <- alpha
            if this.FirstBitmap <> null then
                if shader2 = null then
                    shader2 <- new BitmapShader (this.SecondBitmap, Shader.TileMode.Clamp, Shader.TileMode.Clamp)
                shader2.SetLocalMatrix (matrix)
                paint.SetShader shader2 |> ignore
                canvas.DrawRect (bounds, paint)
            else
                canvas.DrawColor (defaultColor)

    member this.SetMatrix matrixParam =
        matrix <- matrixParam
        this.InvalidateSelf()

    override this.SetAlpha alphaParam =
        alpha <- alpha
        this.InvalidateSelf ()

    override this.SetColorFilter cf =
        ()
    override this.Opacity = int Format.Opaque
