namespace XamarinStore

open System
open MonoTouch.UIKit
open MonoTouch.CoreGraphics
open System.Drawing
open System.Threading.Tasks
open MonoTouch.Twitter
open System.Linq
open MonoTouch.Foundation
open User
open Helpers

[<AllowNullLiteralAttribute>]
type ProcessingView() as this=
    inherit UIView()

    let gear = new UIImageView(UIImage.FromBundle("gear"))
    let status = new UILabel( BackgroundColor =  UIColor.Clear,
                              TextAlignment = UITextAlignment.Center,
                              TextColor =  UIColor.White,
                              Lines =  0,
                              LineBreakMode = UILineBreakMode.WordWrap,
                              ContentMode =  UIViewContentMode.Top)
    let tryAgain = new ImageButton( TintColor = UIColor.White,
                                    Text =  "Try Again" )
    let mutable lastFrame = RectangleF.Empty
    let mutable isSpinning = false
    let mutable currentRotation = 0
    let mutable tcs : TaskCompletionSource<bool> = null

    do this.AddSubview gear
       this.AddSubview status
       tryAgain.TouchUpInside.Add( fun _ ->UIView.Animate(0.3, fun () ->tryAgain.RemoveFromSuperview())
                                           this.TryAgain() )



    let GetNextRotation increment =
        currentRotation <- currentRotation + increment;
        if currentRotation >= 360 then
            currentRotation <- currentRotation - 360

        let rad = float32 (Math.PI * (float currentRotation) / 180.0)
        CGAffineTransform.MakeRotation (rad)

    let animationEnded() =
        if tcs <> null then
            tcs.TrySetResult true |> ignore
        tcs <- null

    let rec startGear() =
        UIView.Animate (0.6, 
                        (fun () ->  UIView.SetAnimationCurve (UIViewAnimationCurve.EaseIn)
                                    gear.Transform <- GetNextRotation(30)),
                        fun () ->spin())

    and stopGear() =
        UIView.Animate (0.6, 
                        (fun () ->  UIView.SetAnimationCurve (UIViewAnimationCurve.EaseOut);
                                    gear.Transform <- GetNextRotation(30)),
                        fun () ->animationEnded())

    and spin() =
        if not isSpinning then
            stopGear ()
        else
            UIView.Animate(0.2, 
                           (fun () -> UIView.SetAnimationCurve (UIViewAnimationCurve.Linear)
                                      gear.Transform <- GetNextRotation(10)),
                           fun () ->spin())

    member val TryAgain = fun ()->() with get,set

    override this.LayoutSubviews () =
        base.LayoutSubviews ()

        //Only re-layout if the bounds changed. If not the gear bounces around during rotation.
        if this.Bounds <> lastFrame then
            lastFrame <- this.Bounds
            gear.Center <- new PointF (this.Bounds.GetMidX (), this.Bounds.GetMidY () - (gear.Frame.Height/2.0f))

    member this.SpinGear() =
        this.Status <- "Processing Order..."
        if not isSpinning then
            isSpinning <- true
            startGear ()
    
    member this.StopGear() =
        tcs <- new TaskCompletionSource<bool>()
        isSpinning <- false
        tcs.Task

    member this.Status 
        with get () = status.Text
        and set value = status.Text <- if value = null then "" else value
                        let mutable statusFrame = new RectangleF (10.0f, gear.Frame.Bottom + 10.0f, this.Bounds.Width - 20.0f, this.Bounds.Height - gear.Frame.Bottom)
                        statusFrame.Height <- status.SizeThatFits(statusFrame.Size).Height
                        status.Frame <- statusFrame

    member this.ShowTryAgain() =
        let center = new PointF (this.Bounds.GetMidX (), this.Bounds.Height - tryAgain.Frame.Height / 2.0f - 10.0f)
        tryAgain.Center <- center
        tryAgain.Alpha <- 0.0f
        this.AddSubview tryAgain
        UIView.Animate (0.3, fun () -> tryAgain.Alpha <- 1.0f)

type SuccessView() as this =
    inherit UIView()

    let Check = new UIImageView(UIImage.FromBundle("success"), Alpha = 0.0f )
    let label1 = new UILabel( Text = "Order Complete",
                              TextAlignment = UITextAlignment.Center,
                              Font = UIFont.BoldSystemFontOfSize(25.0f),
                              TextColor = UIColor.White,
                              Alpha = 0.0f)
    let label2 = new UILabel( Text = "We've received your order and we'll email you as soon as your items ship." ,
                              TextAlignment = UITextAlignment.Center,
                              Font = UIFont.SystemFontOfSize(17.0f),
                              Lines = 0,
                              LineBreakMode = UILineBreakMode.WordWrap,
                              TextColor = UIColor.White,
                              Alpha = 0.0f )

    let twitter = new ImageButton( Text = "Brag on Twitter",
                                   Image = UIImage.FromBundle("twitter").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                                   TintColor = UIColor.White,
                                   Font = UIFont.SystemFontOfSize(20.0f),
                                   Alpha = 0.0f )
    let doneButton = new ImageButton( Text = "Done",
                                      TintColor = UIColor.White,
                                      Font = UIFont.SystemFontOfSize(20.0f),
                                      Alpha = 0.0f )
    let mutable yOffset = 20.0f

    do this.AddSubview Check
       this.AddSubview label1
       label1.SizeToFit()
       this.AddSubview label2
       label2.SizeToFit()
       twitter.TouchUpInside.Add(fun _ ->  this.Tweet())
       if TWTweetComposeViewController.CanSendTweet then
           this.AddSubview twitter
       this.AddSubview doneButton
       doneButton.TouchUpInside.Add(fun _ -> this.Close() )

    member val Close = fun ()->() with get,set
    member val Tweet = fun ()->() with get,set

    override this.LayoutSubviews () =
        base.LayoutSubviews ()
        let padding = 10.0f
        let y = this.Bounds.Height / 3.0f
        Check.Center <- new PointF (this.Bounds.GetMidX (), y - Check.Frame.Height/2.0f )

        let mutable frame = label1.Frame
        frame.X <- padding
        frame.Y <- Check.Frame.Bottom + padding + yOffset
        frame.Width <-  this.Bounds.Width - (padding * 2.0f)
        label1.Frame <- frame

        frame.Y <- frame.Bottom + padding
        frame.Height <- label2.SizeThatFits(new SizeF(frame.Width,this.Bounds.Height)).Height
        label2.Frame <- frame

        let mutable frame = doneButton.Frame
        frame.Y <- this.Bounds.Height - padding - frame.Height
        frame.X <- (this.Bounds.Width - frame.Width) / 2.0f
        doneButton.Frame <- frame

        frame.Y <- frame.Y - (frame.Height + padding)
        twitter.Frame <- frame

    member this.AnimateIn() = async {
        yOffset <- 20.0f
        this.LayoutSubviews ()
        do! UIView.AnimateAsync (0.1, fun () -> UIView.SetAnimationCurve(UIViewAnimationCurve.EaseOut)
                                                Check.Alpha <- 1.0f
                                                twitter.Alpha <- 1.0f
                                                doneButton.Alpha <- 1.0f) |> Async.AwaitTask |> Async.Ignore
        UIView.Animate(0.2, fun () -> UIView.SetAnimationCurve(UIViewAnimationCurve.EaseInOut)
                                      yOffset <- 0.0f
                                      this.LayoutSubviews ()
                                      label1.Alpha <- 1.0f
                                      label2.Alpha <- 1.0f ) } |> Async.StartImmediate

type ProcessingViewController(user: User) as this=
    inherit UIViewController()

    let dismissViewController = new EventHandler(fun unit _ -> this.DismissViewControllerAsync(true) |> ignore);

    let mutable processView:ProcessingView = null

    do this.Title <- "Processing"
       this.NavigationItem.RightBarButtonItem <- new UIBarButtonItem (UIBarButtonSystemItem.Cancel, 
                                                                      dismissViewController)

    let tweet() = async {
        this.DismissViewController (true, null)

        let tvc = new MonoTouch.Twitter.TWTweetComposeViewController()
        let! products = WebService.Shared.GetProducts()
        if products.Length > 0 then
            
            let imageUrl = products.RandomArrayPermutation().First().GetImageUrl()()
            let! imagePath = FileCache.Download imageUrl
            tvc.AddImage( UIImage.FromFile imagePath ) |> ignore

        tvc.AddUrl (NSUrl.FromString("http://xamarin.com/sharp-shirt")) |> ignore
        tvc.SetInitialText("I just built a native iOS app with C# using #Xamarin and all I got was this free C# t-shirt!") |> ignore
        this.PresentViewController(tvc, true,null) }

    let showSuccess() =
        let view = new SuccessView ( Frame = this.View.Bounds,
                                     Close = (fun () -> this.DismissViewController(true,null)),
                                     Tweet = fun () ->tweet() |> Async.StartImmediate )
        this.View.AddSubview view
        UIView.Animate (0.3, fun () -> processView.Alpha <- 0.0f )
        view.AnimateIn ()

    let ProcessOrder () = async {
        processView.SpinGear ()
        let! result = WebService.Shared.PlaceOrder (user)
        processView.Status <- if result.Success then "Your order has been placed!" else result.Message
        do! processView.StopGear() |> Async.AwaitTask |> Async.Ignore
        if not result.Success then
            processView.ShowTryAgain ()
        else
            showSuccess ()
            this.OrderPlaced() }

    member val OrderPlaced = fun ()->() with get,set
                                                                     
    override this.LoadView () =
        base.LoadView ()
        this.View.BackgroundColor <- UIColor.Gray
        processView <- new ProcessingView ( TryAgain = fun ()->ProcessOrder()|> Async.StartImmediate )
        this.View.AddSubview processView

    override this.ViewDidLayoutSubviews () =
        base.ViewDidLayoutSubviews ()
        processView.Frame <- this.View.Bounds

    override this.ViewDidAppear animated =
        base.ViewDidAppear animated
        ProcessOrder () |> Async.StartImmediate

    override this.ViewWillAppear animated =
        base.ViewWillAppear animated
        if this.NavigationController <> null then
            this.NavigationController.NavigationBar.BarStyle <- UIBarStyle.Black