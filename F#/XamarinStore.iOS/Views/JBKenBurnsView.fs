//  Created by Javier Berlana on 9/23/11.
//  Copyright (c) 2011, Javier Berlana
//  Ported to C# by James Clancey, Xamarin
//  Ported to F# by Michael Ciccotti
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy of this 
//  software and associated documentation files (the "Software"), to deal in the Software 
//  without restriction, including without limitation the rights to use, copy, modify, merge, 
//  publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons 
//  to whom the Software is furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in all copies 
//  or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//  INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
//  PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
//  FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
//  ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS 
//  IN THE SOFTWARE.
//
namespace XamarinStore

open System
open System.Collections.Generic
open System.Drawing
open MonoTouch.CoreAnimation
open MonoTouch.CoreGraphics
open MonoTouch.Foundation
open MonoTouch.UIKit

[<AllowNullLiteralAttribute>]
type JBKenBurnsView() as this =
    inherit UIView()

    let enlargeRatio = 1.1f
    let imageBuffer = 3
    let Views = new Queue<UIView>()
    let random = new Random()
    let mutable timer : NSTimer = null
    let mutable currentIndex = 0

    do this.BackgroundColor <- UIColor.Clear
       this.Layer.MasksToBounds <- true

    member val ImageDuration = 12.0 with get,set
    member val ShouldLoop = true with get,set
    member val Images = new List<UIImage>() with get,set
    member val IsLandscape = true with get,set

    member this.CurrentIndex
        with get () = currentIndex
        and set value = currentIndex <- value
                        if this.Images.Count < currentIndex then
                            this.Animate()

    member val ImageIndexChanged = fun (x:int)->() with get,set
    member val Finished = fun ()->() with get,set

    member this.Animate() =
        if timer <> null then
            timer.Invalidate()
        timer <- NSTimer.CreateRepeatingScheduledTimer(this.ImageDuration, fun () ->this.NextImage())
        timer.Fire()

    member this.NextImage() =
        if this.Images.Count = 0 || currentIndex >= this.Images.Count && (not this.ShouldLoop) then
            this.Finished()
        else
            if currentIndex >= this.Images.Count then
                currentIndex <- 0

            let image = this.Images.[currentIndex]
            currentIndex <- currentIndex + 1
            if image <> null && image.Size <> SizeF.Empty then
                let mutable resizeRatio = -1.0f
                let mutable widthDiff = -1.0f
                let mutable heightDiff = -1.0f
                let mutable originX = -1.0f
                let mutable originY = -1.0f
                let mutable zoomInX = -1.0f
                let mutable zoomInY = -1.0f
                let mutable moveX = -1.0f
                let mutable moveY = -1.0f
                let frameWidth = if this.IsLandscape then this.Bounds.Width else this.Bounds.Height
                let frameHeight = if this.IsLandscape then this.Bounds.Height else this.Bounds.Width

                // Wider than screen 
                let imageWidth = if image.Size.Width = 0.0f then 100.0f else image.Size.Width
                let imageHeight = if image.Size.Height = 0.0f then 100.0f else image.Size.Height

                if imageWidth > frameWidth then
                    widthDiff <- imageWidth - frameWidth;

                    // Higher than screen
                    if imageHeight > frameHeight then
                        heightDiff <- imageHeight - frameHeight;

                        if widthDiff > heightDiff then
                            resizeRatio <- frameHeight/imageHeight
                        else
                            resizeRatio <- frameWidth/imageWidth

                        // No higher than screen [OK]
                    else
                        heightDiff <- frameHeight - imageHeight

                        if widthDiff > heightDiff then
                            resizeRatio <- frameWidth/imageWidth
                        else
                            resizeRatio <- this.Bounds.Height/imageHeight

                    // No wider than screen
                else
                    widthDiff <- frameWidth - imageWidth;

                    // Higher than screen [OK]
                    if imageHeight > frameHeight then
                        heightDiff <- imageHeight - frameHeight

                        if widthDiff > heightDiff then
                            resizeRatio <- imageHeight/frameHeight
                        else
                            resizeRatio <- frameWidth/imageWidth

                        // No higher than screen [OK]
                    else
                        heightDiff <- frameHeight - imageHeight

                        if widthDiff > heightDiff then
                            resizeRatio <- frameWidth/imageWidth
                        else
                            resizeRatio <- frameHeight/imageHeight

                // Resize the image.
                let optimusWidth = (imageWidth * resizeRatio) * enlargeRatio
                let optimusHeight = (imageHeight * resizeRatio) * enlargeRatio
                let imageView = new UIView(
                                    Frame = new RectangleF(0.0f, 0.0f, optimusWidth, optimusHeight),
                                    BackgroundColor = UIColor.Clear)

                let maxMoveX = Math.Min(optimusWidth - frameWidth,50.0f);
                let maxMoveY = Math.Min(optimusHeight - frameHeight, 50.0f) * 2.0f/3.0f;

                let rotation = (float32 (random.Next 9))/100.0f;

                match random.Next(3) with
                | 0 -> originX <- 0.0f
                       originY <- 0.0f
                       zoomInX <- 1.25f
                       zoomInY <- 1.25f
                       moveX <- -maxMoveX
                       moveY <- -maxMoveY
                | 1 -> originX <- 0.0f
                       originY <- 0.0f
                       zoomInX <- 1.1f
                       zoomInY <- 1.1f
                       moveX <- -maxMoveX
                       moveY <- maxMoveY
                | 2 -> originX <- frameWidth - optimusWidth
                       originY <- 0.0f
                       zoomInX <- 1.3f
                       zoomInY <- 1.3f
                       moveX <- maxMoveX
                       moveY <- -maxMoveY
                | _ -> originX <- frameWidth - optimusWidth;
                       originY <- 0.0f
                       zoomInX <- 1.2f
                       zoomInY <- 1.2f
                       moveX <- maxMoveX
                       moveY <- maxMoveY

                let picLayer = new CALayer( Contents = image.CGImage,
                                            AnchorPoint = PointF.Empty,
                                            Bounds = imageView.Bounds,
                                            Position = new PointF(originX, originY))

                imageView.Layer.AddSublayer(picLayer)

                let animation = new CATransition( Duration = 1.0,
                                                  Type = CAAnimation.TransitionFade.ToString())

                this.Layer.AddAnimation(animation, null)

                Views.Enqueue(imageView)
                while Views.Count > imageBuffer do
                    Views.Dequeue().RemoveFromSuperview()

                this.AddSubview imageView

                let zoomInXImmutable = zoomInX
                let zoomInYImmutable = zoomInY
                let moveXImmutable = moveX
                let moveYImmutable = moveY

                UIView.Animate(this.ImageDuration + 2.0, 
                                 0.0, 
                                 UIViewAnimationOptions.CurveEaseIn, 
                                 (fun () -> let t = CGAffineTransform.MakeRotation(rotation)
                                            t.Translate(moveXImmutable, moveYImmutable)
                                            t.Scale(zoomInXImmutable, zoomInYImmutable)
                                            imageView.Transform <- t), 
                                 null)

                this.ImageIndexChanged(currentIndex)
