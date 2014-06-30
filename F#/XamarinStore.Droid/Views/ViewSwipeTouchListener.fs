namespace XamarinStore

open System
open Android.Content
open Android.Views
open Android.Animation

type ViewSwipeTouchListener(context:Context, subviewID:int) as this =
    inherit GestureDetector.SimpleOnGestureListener()

    let mutable detector: GestureDetector = new GestureDetector (context, this)
    let mutable targetView:View = null
    let mutable config:ViewConfiguration = ViewConfiguration.Get (context)
    let mutable subViewId = subviewID

    member this.ResetSwipe () =
        if not (targetView = null) then
            targetView.Alpha <- 1.0f
            targetView.TranslationX <- 0.0f


    member val SwipeGestureBegin = fun ()->() with get,set
    member val SwipeGestureEnd = fun ()->() with get,set
    member val ItemSwipped = fun ()->() with get,set

    member this.SnapView dismiss =
        if not (targetView = null) then
            let targetAlpha = if dismiss then 0.0f else 1.0f
            let targetTranslation = if dismiss then float32 targetView.Width else 0.0f
            let a = ObjectAnimator.OfPropertyValuesHolder (
                        targetView,
                        PropertyValuesHolder.OfFloat ("alpha", targetView.Alpha, targetAlpha),
                        PropertyValuesHolder.OfFloat ("translationX", targetView.TranslationX, targetTranslation))
            if dismiss then
                a.AnimationEnd.AddHandler(new EventHandler(fun sender eventargs ->(sender :?> ValueAnimator).RemoveAllListeners ()
                                                                                  this.ItemSwipped ()))
            a.Start () 

    interface View.IOnTouchListener with
        member this.OnTouch(v, e) =
            if (targetView = null) then
                targetView <- if subviewID = 0 then v else v.FindViewById subviewID
            if (e.Action = MotionEventActions.Up || e.Action = MotionEventActions.Cancel) then
                this.SwipeGestureEnd ()
                let dismiss = (not (e.Action = MotionEventActions.Cancel))
                              && (targetView.TranslationX > float32 targetView.Width / 2.0f)
                this.SnapView (dismiss)
            detector.OnTouchEvent (e) |> ignore
            true

    override this.OnFling(e1, e2, velocityX, velocityY) =
        // We are only interested in an horizontal right-side fling
        if (velocityY > velocityX || velocityX < 0.0f) then
            base.OnFling (e1, e2, velocityX, velocityY)
        else
            this.SnapView true
            true

    override this.OnScroll (e1, e2, distanceX, distanceY) =
        this.SwipeGestureBegin ()
        let distanceX = -distanceX
        if Math.Abs(distanceY) > (Math.Abs(distanceX) + float32 config.ScaledTouchSlop)
            || (distanceX < 0.0f && targetView.TranslationX <= 0.0f) then
            base.OnScroll (e1, e2, distanceX, distanceY)
        else targetView.TranslationX <- Math.Max (0.0f, targetView.TranslationX + distanceX)
             targetView.Alpha <- (float32 targetView.Width - targetView.TranslationX) / (float32 targetView.Width)
             true


