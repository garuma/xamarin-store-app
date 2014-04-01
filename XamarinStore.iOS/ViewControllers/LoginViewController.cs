using System;
using MonoTouch.UIKit;
using BigTed;
using MonoTouch.Foundation;

namespace XamarinStore.iOS
{
	public class LoginViewController : UIViewController
	{
		UIView ContentView;
		LoginView LoginView;
		UIScrollView scrollView;
		float keyboardOffset = 0;
		public event Action LoginSucceeded = delegate {};

		/// <summary>
		/// Initializes a new instance of the <see cref="XamarinStore.iOS.LoginViewController"/> class.
		/// </summary>
		/// <param name="xamarinAccountEmail">Xamarin account email provided by the user. If it's null or empty, mini-hack
		/// instructions are displayed instead of the login screen.</param>
		public LoginViewController ()
		{
			this.Title = "Log in";
			//This hides the back button text when you leave this View Controller
			this.NavigationItem.BackBarButtonItem = new UIBarButtonItem ("", UIBarButtonItemStyle.Plain, handler: null);
			AutomaticallyAdjustsScrollViewInsets = false;
		}

		bool ShouldShowInstructions {
			get { return string.IsNullOrEmpty (XamarinAccountEmail); }
		}

		public override void ViewDidLayoutSubviews ()
		{
			var bounds = View.Bounds;
			ContentView.Frame = bounds;
			scrollView.ContentSize = bounds.Size;
			//Resize Scroller for keyboard;
			bounds.Height -= keyboardOffset;
			scrollView.Frame = bounds;
		}

		public override void LoadView ()
		{
			base.LoadView ();
			View.AddSubview (scrollView = new UIScrollView (View.Bounds));
			if (ShouldShowInstructions) {
				scrollView.Add (ContentView = new PrefillXamarinAccountInstructionsView ());
			} else {
				LoginView = new LoginView (XamarinAccountEmail);
				LoginView.UserDidLogin += _ => Login (XamarinAccountEmail, LoginView.PasswordField.Text);
				scrollView.Add (ContentView = LoginView);
			}
		}
		private void OnKeyboardNotification (NSNotification notification)
		{
			if (IsViewLoaded) {

				//Check if the keyboard is becoming visible
				bool visible = notification.Name == UIKeyboard.WillShowNotification;
				UIView.Animate (UIKeyboard.AnimationDurationFromNotification (notification), () => {
					UIView.SetAnimationCurve ((UIViewAnimationCurve)UIKeyboard.AnimationCurveFromNotification (notification));
					var frame = UIKeyboard.FrameEndFromNotification (notification);
					keyboardOffset = visible ? frame.Height : 0; 
					ViewDidLayoutSubviews ();
				});
			}
		}


		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillHideNotification, OnKeyboardNotification);
			NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillShowNotification, OnKeyboardNotification);
		}
		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
			NSNotificationCenter.DefaultCenter.RemoveObservers (new []{UIKeyboard.WillHideNotification,UIKeyboard.WillShowNotification});
		}

		// TODO: Enter your Xamarin account email address here
		// If you do not have a Xamarin Account please sign up here: https://store.xamarin.com/account/register
		readonly string XamarinAccountEmail = "";
		async void Login (string username, string password)
		{

			BTProgressHUD.Show ("Logging in...");

			var success = await WebService.Shared.Login (username, password);
			if (success) {
				var canContinue = await WebService.Shared.PlaceOrder (WebService.Shared.CurrentUser, true);
				if (!canContinue.Success) {
					new UIAlertView ("Sorry", "Only one shirt per person. Edit your cart and try again.", null, "OK").Show();
					BTProgressHUD.Dismiss ();
					return;
				}
			}

			BTProgressHUD.Dismiss ();

			if (success) {
				LoginSucceeded ();
			} else {
				var alert = new UIAlertView ("Could Not Log In", "Please verify your Xamarin account credentials and try again", null, "OK");
				alert.Show ();
				alert.Clicked += delegate {
					LoginView.PasswordField.SelectAll (this);
					LoginView.PasswordField.BecomeFirstResponder ();
				};
			}
		}
	}
}

