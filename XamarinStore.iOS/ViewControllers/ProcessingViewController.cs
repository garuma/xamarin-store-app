using System;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using System.Drawing;
using System.Threading.Tasks;
using MonoTouch.Twitter;
using System.Linq;
using MonoTouch.Foundation;

namespace XamarinStore.iOS
{
	public class ProcessingViewController : UIViewController
	{
		User user;

		public event EventHandler OrderPlaced;

		public ProcessingViewController (User user)
		{
			Title = "Processing";
			this.user = user;
			this.NavigationItem.RightBarButtonItem = new UIBarButtonItem (UIBarButtonSystemItem.Cancel, (sender,args) => {
				this.DismissViewControllerAsync(true);
			});
		}
		ProcessingView proccessView;
		public override void LoadView ()
		{
			base.LoadView ();
			View.BackgroundColor = UIColor.Gray;
			View.AddSubview(proccessView = new ProcessingView {
				TryAgain = ProcessOrder
			});
		}
		public override void ViewDidLayoutSubviews ()
		{
			base.ViewDidLayoutSubviews ();
			proccessView.Frame = View.Bounds;
		}
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			ProcessOrder ();
		}
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			if (NavigationController == null)
				return;

			NavigationController.NavigationBar.BarStyle = UIBarStyle.Black;
		}

		async void ProcessOrder ()
		{
			proccessView.SpinGear ();
			var result = await WebService.Shared.PlaceOrder (user);
			proccessView.Status = result.Success ? "Your order has been placed!" : result.Message;;
			await proccessView.StopGear ();
			if (!result.Success) {
				proccessView.ShowTryAgain ();
				return;	
			}
			showSuccess ();
			if (OrderPlaced != null)
				OrderPlaced (this, EventArgs.Empty);
		}

		void showSuccess()
		{
			var view = new SuccessView {
				Frame = View.Bounds,
				Close = () => DismissViewController(true,null),
				Tweet = tweet,
			};
			View.AddSubview(view);
			UIView.Animate (.3, () => {
				proccessView.Alpha = 0f;
			});
			view.AnimateIn ();
		}
		async void tweet()
		{
			DismissViewController (true, null);

			var tvc = new MonoTouch.Twitter.TWTweetComposeViewController();
			var products = await WebService.Shared.GetProducts();
			if(products.Count > 0){
				products.Shuffle ();

				var imageUrl = products.First ().ImageUrl;
				var imagePath = await FileCache.Download (imageUrl);
				tvc.AddImage (UIImage.FromFile (imagePath));
			}
			tvc.AddUrl (NSUrl.FromString("http://xamarin.com/sharp-shirt"));
			tvc.SetInitialText("I just built a native iOS app with C# using #Xamarin and all I got was this free C# t-shirt!");
			PresentViewController(tvc, true,null);
		}

		class ProcessingView : UIView
		{
			UIImageView gear;
			UILabel status;
			ImageButton tryAgain;
			public Action TryAgain;
			public ProcessingView()
			{
				gear = new UIImageView(UIImage.FromBundle("gear"));
				AddSubview(gear);

				status = new UILabel(){
					BackgroundColor =  UIColor.Clear,
					TextAlignment = UITextAlignment.Center,
					TextColor =  UIColor.White,
					Lines =  0,
					LineBreakMode = UILineBreakMode.WordWrap,
					ContentMode =  UIViewContentMode.Top,

				};
				AddSubview(status);

				tryAgain = new ImageButton(){
					TintColor = UIColor.White,
					Text =  "Try Again"
				};
				tryAgain.TouchUpInside += (object sender, EventArgs e) => {
					Animate(.3,tryAgain.RemoveFromSuperview);
					if(TryAgain != null)
						TryAgain();
				};
			}

			RectangleF lastFrame;
			public override void LayoutSubviews ()
			{
				base.LayoutSubviews ();

				//Only re-layout if the bounds changed. If not the gear bounces around during rotation.
				if (Bounds == lastFrame)
					return;
				lastFrame = Bounds;
				gear.Center = new PointF (Bounds.GetMidX (), Bounds.GetMidY () - (gear.Frame.Height/2));
			}

			bool isSpinning;
			int currentRotation = 0;
			public void SpinGear()
			{
				Status = "Processing Order...";
				if (isSpinning)
					return;
				isSpinning = true;
				startGear ();
			}
			void startGear()
			{
				Animate (.6, () => {
					UIView.SetAnimationCurve (UIViewAnimationCurve.EaseIn);
					gear.Transform = GetNextRotation(30);
				}, spin);
			}
			void spin()
			{
				if (!isSpinning) {
					stopGear ();
					return;
				}
				Animate (.2, () => {
					UIView.SetAnimationCurve (UIViewAnimationCurve.Linear);
					gear.Transform = GetNextRotation(10);
				}, spin);
			}
			void stopGear()
			{
				Animate (.6, () => {
					UIView.SetAnimationCurve (UIViewAnimationCurve.EaseOut);
					gear.Transform = GetNextRotation(30);
				},animationEnded);
			}
			CGAffineTransform GetNextRotation(int increment)
			{
				currentRotation += increment;
				if (currentRotation >= 360)
					currentRotation -=  360;

				var rad = (float)(Math.PI * currentRotation / 180f);
				return CGAffineTransform.MakeRotation (rad);
			}
			TaskCompletionSource<bool> tcs;
			public Task StopGear()
			{
				tcs = new TaskCompletionSource<bool> ();
				isSpinning = false;
				return tcs.Task;
			}
			public string Status 
			{
				get{ return status.Text; }
				set { 
					status.Text = value ?? ""; 

					var statusFrame = new RectangleF (10, gear.Frame.Bottom + 10f, Bounds.Width - 20f, Bounds.Height - gear.Frame.Bottom);
					statusFrame.Height = status.SizeThatFits (statusFrame.Size).Height;
					status.Frame = statusFrame;
				}
			}

			void animationEnded()
			{
				if (tcs != null)
					tcs.TrySetResult (true);
				tcs = null;
			}

			public void ShowTryAgain()
			{
				var center = new PointF (Bounds.GetMidX (), Bounds.Height - tryAgain.Frame.Height / 2 - 10);
				tryAgain.Center = center;
				tryAgain.Alpha = 0;
				AddSubview (tryAgain);
				Animate (.3, () => {tryAgain.Alpha = 1;});
			}

		}

		class SuccessView : UIView
		{
			UIImageView Check;
			UILabel label1;
			UILabel label2;
			ImageButton twitter;
			ImageButton done;
			public Action Close { get; set; }
			public Action Tweet { get; set; }
			public SuccessView()
			{
				AddSubview(Check = new UIImageView(UIImage.FromBundle("success")){
					Alpha = 0,
				});

				AddSubview(label1 = new UILabel{
					Text = "Order Complete",
					TextAlignment = UITextAlignment.Center,
					Font = UIFont.BoldSystemFontOfSize(25),
					TextColor = UIColor.White,
					Alpha = 0,
				});
				label1.SizeToFit();

				AddSubview(label2 = new UILabel{
					Text = "We've received your order and we'll email you as soon as your items ship." ,
					TextAlignment = UITextAlignment.Center,
					Font = UIFont.SystemFontOfSize(17),
					Lines = 0,
					LineBreakMode = UILineBreakMode.WordWrap,
					TextColor = UIColor.White,
					Alpha = 0,
				});
				label2.SizeToFit();

				twitter = new ImageButton{
					Text = "Brag on Twitter",
					Image = UIImage.FromBundle("twitter").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
					TintColor = UIColor.White,
					Font = UIFont.SystemFontOfSize(20),
					Alpha = 0,
				};
				twitter.TouchUpInside += (object sender, EventArgs e) => {
					if(Tweet != null)
						Tweet();
				};
				if(TWTweetComposeViewController.CanSendTweet)
					AddSubview(twitter);

				AddSubview(done = new ImageButton{
					Text = "Done",
					TintColor = UIColor.White,
					Font = UIFont.SystemFontOfSize(20),
					Alpha = 0,
				});
				done.TouchUpInside += (object sender, EventArgs e) => {
					if(Close != null)
						Close();
				};

			}
			float yOffset = 20;
			public override void LayoutSubviews ()
			{
				base.LayoutSubviews ();
				const float padding = 10;
				var y = Bounds.Height / 3;
				Check.Center = new PointF (Bounds.GetMidX (), y - Check.Frame.Height/2 );

				var frame = label1.Frame;
				frame.X = padding;
				frame.Y = Check.Frame.Bottom + padding + yOffset;
				frame.Width =  Bounds.Width - (padding * 2);
				label1.Frame = frame;

				frame.Y = frame.Bottom + padding;
				frame.Height = label2.SizeThatFits (new SizeF(frame.Width,Bounds.Height)).Height;
				label2.Frame = frame;

				frame = done.Frame;
				frame.Y = Bounds.Height - padding - frame.Height;
				frame.X = (Bounds.Width - frame.Width) / 2;
				done.Frame = frame;

				frame.Y -= (frame.Height + padding);
				twitter.Frame = frame;

			}
			public async void AnimateIn()
			{
				yOffset = 20;
				LayoutSubviews ();
				await AnimateAsync (.1, () => {
					UIView.SetAnimationCurve(UIViewAnimationCurve.EaseOut);
					Check.Alpha = twitter.Alpha = done.Alpha = 1;
				});
				Animate(.2,()=>{
					UIView.SetAnimationCurve(UIViewAnimationCurve.EaseInOut);
					yOffset = 0;
					LayoutSubviews ();
					label1.Alpha = label2.Alpha = 1;
				});

			}

		}
	}
}

