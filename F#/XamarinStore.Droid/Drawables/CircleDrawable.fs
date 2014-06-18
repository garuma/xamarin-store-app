namespace XamarinStore

open Android.Graphics
open Android.Graphics.Drawables

type CircleDrawable(bmp:Bitmap) =
    inherit Drawable()

    let bmpShader = new BitmapShader (bmp, Shader.TileMode.Clamp, Shader.TileMode.Clamp)
    let paint = new Paint (AntiAlias = true )
    do paint.SetShader bmpShader |> ignore
    let oval = new RectF()

    override this.Draw canvas =
        canvas.DrawOval (oval, paint)

    override this.OnBoundsChange bounds =
        base.OnBoundsChange bounds
        oval.Set(0.0f, 0.0f, float32 (bounds.Width()), float32 (bounds.Height()))

    override this.IntrinsicWidth = bmp.Width

    override this.IntrinsicHeight = bmp.Height

    override this.SetAlpha alpha =
        ()

    override this.Opacity = int Format.Opaque

    override this.SetColorFilter cf =
        ()


