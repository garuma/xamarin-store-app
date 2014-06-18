namespace XamarinStore

open Android.Graphics
open Android.Graphics.Drawables
open Android.Animation

type BadgeDrawable (child:Drawable) =
    inherit Drawable()

    let badgePaint = new Paint(AntiAlias = true, 
                               Color = Color.Blue.ToAndroidColor())
    let textPaint = new Paint(AntiAlias = true, 
                              Color = Android.Graphics.Color.White,
                              TextSize = 16.0f,
                              TextAlign = Paint.Align.Center)
    let txtBounds = new Rect()
    let mutable badgeBounds = new RectF()
    let mutable count = 0
    let mutable alpha = 0xFF

    let mutable alphaAnimator:ValueAnimator = null                                                   

    member this.Count 
        with get () = count 
        and set value = count <- value
                        this.InvalidateSelf()

    member this.SetCountAnimated countParam =
        this.Count <- countParam
        if not (alphaAnimator = null) then
            alphaAnimator.Cancel ()
            alphaAnimator <- null
        
        let Duration = 300L
        
        alphaAnimator <- ObjectAnimator.OfInt (this, "alpha", 0xFF, 0)
        alphaAnimator.SetDuration Duration |> ignore
        alphaAnimator.RepeatMode <- ValueAnimatorRepeatMode.Reverse
        alphaAnimator.RepeatCount <- 1
        alphaAnimator.AnimationRepeat.AddHandler(fun sender e -> (sender :?> Animator).RemoveAllListeners()
                                                                 count <- countParam)
        alphaAnimator.Start ()

    override this.Draw canvas =
        child.Draw canvas
        if count > 0 then
            badgePaint.Alpha <- alpha
            textPaint.Alpha <- alpha
            badgeBounds.Set (0.0f, 0.0f, (float32 (this.Bounds.Width ())) / 2.0f, (float32 (this.Bounds.Height ())) / 2.0f)
            canvas.DrawRoundRect (badgeBounds, 8.0f, 8.0f, badgePaint)
            textPaint.TextSize <- (8.0f * badgeBounds.Height ()) / 10.0f
            let text = count.ToString ()
            textPaint.GetTextBounds (text, 0, text.Length, txtBounds)
            canvas.DrawText (
                text,
                badgeBounds.CenterX (),
                badgeBounds.Bottom - (badgeBounds.Height () - (float32 (txtBounds.Height ()))) / 2.0f - 1.0f,
                textPaint)

    override this.OnBoundsChange bounds =
        base.OnBoundsChange bounds
        child.SetBounds(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom)
    
    override this.IntrinsicWidth = child.IntrinsicWidth
    override this.IntrinsicHeight = child.IntrinsicHeight
    override this.Opacity = child.Opacity

    override this.SetAlpha alphaParam =
        alpha <- alphaParam
        this.InvalidateSelf()
    
    override this.SetColorFilter filter =
        child.SetColorFilter filter
