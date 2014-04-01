//  Created by Javier Berlana on 9/23/11.
//  Copyright (c) 2011, Javier Berlana
//  Ported to C# by James Clancey, Xamarin
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

using System;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace XamarinStore
{
	public class JBKenBurnsView : UIView
	{
		const float enlargeRatio = 1.1f;
		const int imageBuffer = 3;
		readonly Queue<UIView> Views = new Queue<UIView>();
		readonly Random random = new Random();
		public List<UIImage> Images = new List<UIImage>();
		int currentIndex;
		NSTimer timer;

		public JBKenBurnsView()
		{
			ImageDuration = 12;
			ShouldLoop = true;
			IsLandscape = true;
			BackgroundColor = UIColor.Clear;
			Layer.MasksToBounds = true;
		}

		public int CurrentIndex
		{
			get { return currentIndex; }
			set
			{
				currentIndex = value;
				if (Images.Count < currentIndex)
					Animate();
			}
		}

		public double ImageDuration { get; set; }

		public bool ShouldLoop { get; set; }

		public bool IsLandscape { get; set; }
		public event Action<int> ImageIndexChanged;
		public event EventHandler Finished;

		public void Animate()
		{
			if (timer != null)
				timer.Invalidate();
			timer = NSTimer.CreateRepeatingScheduledTimer(ImageDuration, nextImage);
			timer.Fire();
		}

		void nextImage()
		{
			if (Images.Count == 0 || currentIndex >= Images.Count && !ShouldLoop)
			{
				if (Finished != null)
					Finished(this, EventArgs.Empty);
				return;
			}
			if (currentIndex >= Images.Count)
				currentIndex = 0;

			UIImage image = Images[currentIndex];
			currentIndex++;
			if (image == null)
			if (image == null || image.Size == Size.Empty)
				return;
			float resizeRatio = -1;
			float widthDiff = -1;
			float heightDiff = -1;
			float originX = -1;
			float originY = -1;
			float zoomInX = -1;
			float zoomInY = -1;
			float moveX = -1;
			float moveY = -1;
			float frameWidth = IsLandscape ? Bounds.Width : Bounds.Height;
			float frameHeight = IsLandscape ? Bounds.Height : Bounds.Width;

			// Wider than screen 
			float imageWidth = image.Size.Width == 0 ? 100 : image.Size.Width;
			float imageHeight = image.Size.Height == 0 ? 100 : image.Size.Height;

			if (imageWidth > frameWidth)
			{
				widthDiff = imageWidth - frameWidth;

				// Higher than screen
				if (imageHeight > frameHeight)
				{
					heightDiff = imageHeight - frameHeight;

					if (widthDiff > heightDiff)
						resizeRatio = frameHeight/imageHeight;
					else
						resizeRatio = frameWidth/imageWidth;

					// No higher than screen [OK]
				}
				else
				{
					heightDiff = frameHeight - imageHeight;

					if (widthDiff > heightDiff)
						resizeRatio = frameWidth/imageWidth;
					else
						resizeRatio = Bounds.Height/imageHeight;
				}

				// No wider than screen
			}
			else
			{
				widthDiff = frameWidth - imageWidth;

				// Higher than screen [OK]
				if (imageHeight > frameHeight)
				{
					heightDiff = imageHeight - frameHeight;

					if (widthDiff > heightDiff)
						resizeRatio = imageHeight/frameHeight;
					else
						resizeRatio = frameWidth/imageWidth;

					// No higher than screen [OK]
				}
				else
				{
					heightDiff = frameHeight - imageHeight;

					if (widthDiff > heightDiff)
						resizeRatio = frameWidth/imageWidth;
					else
						resizeRatio = frameHeight/imageHeight;
				}
			}

			// Resize the image.
			var optimusWidth = (imageWidth * resizeRatio) * enlargeRatio;
			var optimusHeight = (imageHeight * resizeRatio) * enlargeRatio ;
			var imageView = new UIView
				{
					Frame = new RectangleF(0, 0, optimusWidth, optimusHeight),
					BackgroundColor = UIColor.Clear,
				};

			float maxMoveX = Math.Min(optimusWidth - frameWidth,50f);
			float maxMoveY = Math.Min(optimusHeight - frameHeight, 50f) * 2/3;

			float rotation = (random.Next(9))/100;

			switch (random.Next(3))
			{
				case 0:
					originX = 0;
					originY = 0;
					zoomInX = 1.25f;
					zoomInY = 1.25f;
					moveX = -maxMoveX;
					moveY = -maxMoveY;
					break;

			case 1:
				originX = 0;
				originY = 0;// Math.Max(frameHeight - (optimusHeight),frameHeight * 1/3);
					zoomInX = 1.1f;
					zoomInY = 1.1f;
					moveX = -maxMoveX;
					moveY = maxMoveY;
					break;

				case 2:
					originX = frameWidth - optimusWidth;
					originY = 0;
					zoomInX = 1.3f;
					zoomInY = 1.3f;
					moveX = maxMoveX;
					moveY = -maxMoveY;
					break;

			default:
				originX = frameWidth - optimusWidth;
				originY = 0;//Math.Max(frameHeight - (optimusHeight),frameHeight * 1/3);
					zoomInX = 1.2f;
					zoomInY = 1.2f;
					moveX = maxMoveX;
					moveY = maxMoveY;
					break;
			}

			var picLayer = new CALayer
				{
					Contents = image.CGImage,
					AnchorPoint = PointF.Empty,
					Bounds = imageView.Bounds,
					Position = new PointF(originX, originY)
				};
			imageView.Layer.AddSublayer(picLayer);

			var animation = new CATransition
				{
					Duration = 1,
					Type = CAAnimation.TransitionFade,
				};
			Layer.AddAnimation(animation, null);

			Views.Enqueue(imageView);
			while (Views.Count > imageBuffer)
			{
				Views.Dequeue().RemoveFromSuperview();
			}
			AddSubview(imageView);

			Animate(ImageDuration + 2, 0, UIViewAnimationOptions.CurveEaseIn, () =>
				{
					CGAffineTransform t = CGAffineTransform.MakeRotation(rotation);
					t.Translate(moveX, moveY);
					t.Scale(zoomInX, zoomInY);
					imageView.Transform = t;
				}, null);

			if (ImageIndexChanged != null)
				ImageIndexChanged(currentIndex);
		}
	}
}