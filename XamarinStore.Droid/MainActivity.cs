using System;
using System.Linq;
using System.Collections.Generic;
using Android.OS;
using Android.App;
using Android.Views;
using Android.Util;
using Android.Content.PM;
using Android.Content;

namespace XamarinStore
{
	[Activity (Label = "Xamarin Store", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
	public class MainActivity : Activity
	{
		int baseFragment;

		protected override void OnCreate (Bundle bundle)
		{
			var metrics = new DisplayMetrics (); 
			WindowManager.DefaultDisplay.GetMetrics (metrics);
			Images.ScreenWidth = metrics.WidthPixels;
			FileCache.SaveLocation = CacheDir.AbsolutePath;
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Main);

			//Retain fragments so don't set home if state is stored.
			if (FragmentManager.BackStackEntryCount == 0) {
				var productFragment = new ProductListFragment ();
				productFragment.ProductSelected += ShowProductDetail;

				baseFragment = productFragment.Id;
				SwitchScreens (productFragment, false, true);
			}
			ProcessInstructionFromIntent (Intent);
		}

		protected override void OnSaveInstanceState (Bundle outState)
		{
			base.OnSaveInstanceState (outState);
			outState.PutInt ("baseFragment", baseFragment);
		}

		protected override void OnRestoreInstanceState (Bundle savedInstanceState)
		{
			base.OnRestoreInstanceState (savedInstanceState);
			baseFragment = savedInstanceState.GetInt ("baseFragment");
		}

		public override bool OnMenuItemSelected (int featureId, IMenuItem item)
		{
			switch (item.ItemId) {
			case Resource.Id.cart_menu_item:
				ShowBasket ();
				return true;
			case Android.Resource.Id.Home:
				//pop full backstack when going home.	
				FragmentManager.PopBackStack (baseFragment, PopBackStackFlags.Inclusive);
				SetupActionBar ();
				return true;
			}

			return base.OnMenuItemSelected (featureId, item);
		}

		public override void OnBackPressed ()
		{
			base.OnBackPressed ();
			SetupActionBar (FragmentManager.BackStackEntryCount != 0);
		}

		protected override void OnNewIntent (Intent intent)
		{
			base.OnNewIntent (intent);
			ProcessInstructionFromIntent (intent);
		}

		public int SwitchScreens (Fragment fragment, bool animated = true, bool isRoot = false)
		{
			var transaction = FragmentManager.BeginTransaction ();

			if (animated) {
				int animIn, animOut;
				GetAnimationsForFragment (fragment, out animIn, out animOut);
				transaction.SetCustomAnimations (animIn, animOut);
			}
			transaction.Replace (Resource.Id.contentArea, fragment);
			if (!isRoot)
				transaction.AddToBackStack (null);

			SetupActionBar (!isRoot);

			return transaction.Commit ();
		}

		void GetAnimationsForFragment (Fragment fragment, out int animIn, out int animOut)
		{
			animIn = Resource.Animation.enter;
			animOut = Resource.Animation.exit;

			switch (fragment.GetType ().Name) {
			case "ProductDetailsFragment":
				animIn = Resource.Animation.product_detail_in;
				animOut = Resource.Animation.product_detail_out;
				break;
			case "BasketFragment":
				animIn = Resource.Animation.basket_in;
				break;
			}
		}

		public void ShowProductDetail (Product product, int itemVerticalOffset)
		{
			var productDetails = new ProductDetailsFragment (product, itemVerticalOffset);

			productDetails.AddToBasket += p => {
				WebService.Shared.CurrentOrder.Add (p);

				SetupActionBar ();
			};

			SwitchScreens (productDetails);
		}

		/// <summary>
		/// Setups the action bar if we want to show up arrow or not
		/// </summary>
		/// <param name="showUp">If set to <c>true</c> show up.</param>
		public void SetupActionBar (bool showUp = false)
		{
			this.ActionBar.SetDisplayHomeAsUpEnabled (showUp);
			//this.ActionBar.SetDisplayShowHomeEnabled (showUp);
		}

		public void ShowBasket ()
		{
			SwitchScreens (new BasketFragment (WebService.Shared.CurrentOrder) {
				CheckoutClicked = ShowLogin, 
			});
		}

		public void ShowLogin ()
		{
			var loginVc = new LoginFragment ();
			loginVc.LoginSucceeded += () => ShowAddress ();
			SwitchScreens (loginVc);
		}

		public void ShowAddress ()
		{
			var addressFragment = new ShippingDetailsFragment (WebService.Shared.CurrentUser) {
				OrderPlaced = OrderCompleted,
			};
			SwitchScreens (addressFragment);
		}

		public void OrderCompleted ()
		{

			FragmentManager.PopBackStack (baseFragment, PopBackStackFlags.Inclusive);
			SetupActionBar ();

			SwitchScreens (new BragFragment (), true, true);
		}

		async void ProcessInstructionFromIntent (Intent intent)
		{
			if (intent == null || !intent.HasExtra ("OrderAction"))
				return;
			var query = intent.GetStringExtra ("OrderAction");
			var id = intent.GetIntExtra ("ProductId", -1);

			try {
				var products = await WebService.Shared.GetProducts ();
				var product = products.FirstOrDefault (p => p.Name.GetHashCode () == id);
				if (product == null)
					return;

				var order = WebService.Shared.CurrentOrder;
				if (query == "add")
					order.Add (product);
				else {
					var realProduct = order.Products.FirstOrDefault (p => p.Name == product.Name);
					if (realProduct != null)
						WebService.Shared.CurrentOrder.Remove (realProduct);
				}
			} catch (Exception e) {
				Android.Util.Log.Error ("ProcessIntent", e.ToString ());
			}
		}
	}
}