using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Text;
using Android.Graphics;

namespace XamarinStore
{
	public class LoginFragment : Fragment
	{

		public event Action LoginSucceeded = delegate {};

		EditText password;
		Button login;
		ImageView imageView;

		/// <summary>
		/// Initializes a new instance of the <see cref="XamarinStore.iOS.LoginViewController"/> class.
		/// </summary>
		/// <param name="xamarinAccountEmail">Xamarin account email provided by the user. If it's null or empty, mini-hack
		/// instructions are displayed instead of the login screen.</param>
		public LoginFragment ()
		{

		}

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			RetainInstance = true;
			// Create your fragment here
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			if (string.IsNullOrEmpty (XamarinAccountEmail))
				return CreateInstructions (inflater, container, savedInstanceState);
			return CreateLoginView (inflater, container, savedInstanceState);
		}

		View CreateInstructions (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var view = inflater.Inflate (Resource.Layout.PrefillXamarinAccountInstructions, null);
			var textView = view.FindViewById<TextView> (Resource.Id.codeTextView);
			var coloredText = Html.FromHtml ("<font color='#48D1CC'>public readonly</font> <font color='#1E90FF'>string</font> XamarinAccountEmail = <font color='Red'>\"...\"</font>;");
			textView.SetText (coloredText, TextView.BufferType.Spannable);

			return view;
		}

		View CreateLoginView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var view = inflater.Inflate (Resource.Layout.LoginScreen, null);

			imageView = view.FindViewById<ImageView> (Resource.Id.imageView1);
			LoadUserImage ();

			var textView = view.FindViewById<EditText> (Resource.Id.email);
			textView.Enabled = false;
			textView.Text = XamarinAccountEmail;

			password = view.FindViewById<EditText> (Resource.Id.password);
			login = view.FindViewById<Button> (Resource.Id.signInBtn);
			login.Click += (object sender, EventArgs e) => {
				Login(XamarinAccountEmail,password.Text);
			};
			return view;
		}

		async void LoadUserImage()
		{
			//Get the correct size in pixels
			var px = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 85, Activity.Resources.DisplayMetrics);
			var data = await Gravatar.GetImageBytes (XamarinAccountEmail, px);
			var image = await BitmapFactory.DecodeByteArrayAsync (data, 0, data.Length);
			imageView.SetImageDrawable (new CircleDrawable (image));
		}

		// TODO: Enter your Xamarin account email address here
		// If you do not have a Xamarin Account please sign up here: https://store.xamarin.com/account/register
		readonly string XamarinAccountEmail = "";
		async void Login (string username, string password)
		{
			var progressDialog = ProgressDialog.Show (this.Activity, "Please wait...", "Logging in", true);
			this.login.Enabled = false;
			this.password.Enabled = false;
			var success = await WebService.Shared.Login (username, password);
			if (success) {
				var canContinue = await WebService.Shared.PlaceOrder (WebService.Shared.CurrentUser, true);
				if (!canContinue.Success) {
					Toast.MakeText (this.Activity,"Sorry, only one shirt per person. Edit your cart and try again.", ToastLength.Long).Show();
				}
				else
					LoginSucceeded ();
			}
			else 
				Toast.MakeText (this.Activity, "Please verify your Xamarin account credentials and try again", ToastLength.Long).Show();

			this.login.Enabled = true;
			this.password.Enabled = true;
			progressDialog.Hide ();
			progressDialog.Dismiss ();

		}
	}
}

