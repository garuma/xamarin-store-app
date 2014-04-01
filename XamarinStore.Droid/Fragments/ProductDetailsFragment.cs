using System;
using System.Linq;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Content;
using System.Timers;
using System.Linq.Expressions;
using Android.Util;
using Android.Animation;
using Android.Graphics;

namespace XamarinStore
{
	class ProductDetailsFragment : Fragment, ViewTreeObserver.IOnGlobalLayoutListener
	{
		ImageView productImage;
		int currentIndex = 0;
		Product currentProduct;
		bool shouldAnimatePop;
		BadgeDrawable basketBadge;
		public Action<Product> AddToBasket = delegate {};
		string[] images = new string[0];
		bool cached;
		int slidingDelta;
		Spinner sizeSpinner;
		Spinner colorSpinner;

		KenBurnsDrawable productDrawable;
		ValueAnimator kenBurnsMovement;
		ValueAnimator kenBurnsAlpha;

		public ProductDetailsFragment ()
		{

		}

		public ProductDetailsFragment (Product product,int slidingDelta )
		{
			this.slidingDelta = slidingDelta;
			currentProduct = product;
			images = product.ImageUrls.ToArray().Shuffle() ?? new string[0];
		}

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			RetainInstance = true;
			SetHasOptionsMenu (true);
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate (Resource.Layout.ProductDetail, null, true);
		}

		public override void OnViewCreated (View view, Bundle savedInstanceState)
		{
			base.OnViewCreated (view, savedInstanceState);

			productImage = View.FindViewById<ImageView> (Resource.Id.productImage);
			
			sizeSpinner = View.FindViewById<Spinner> (Resource.Id.productSize);

			colorSpinner = View.FindViewById<Spinner> (Resource.Id.productColor);

			var addToBasket = View.FindViewById<Button> (Resource.Id.addToBasket);
			addToBasket.Click += delegate {
				currentProduct.Size = currentProduct.Sizes [sizeSpinner.SelectedItemPosition];
				currentProduct.Color = currentProduct.Colors [colorSpinner.SelectedItemPosition];
				shouldAnimatePop = true;
				Activity.FragmentManager.PopBackStack();
				AddToBasket (currentProduct);
			};

			View.FindViewById<TextView> (Resource.Id.productTitle).Text = currentProduct.Name ?? string.Empty;
			View.FindViewById<TextView> (Resource.Id.productPrice).Text = currentProduct.PriceDescription ?? string.Empty;
			View.FindViewById<TextView> (Resource.Id.productDescription).Text = currentProduct.Description ?? string.Empty;

			((SlidingLayout)View).InitialMainViewDelta = slidingDelta;

			LoadOptions ();
		}

		void LoadOptions()
		{
			var sizeAdapter = new ArrayAdapter<ProductSize> (Activity, Android.Resource.Layout.SimpleSpinnerDropDownItem, currentProduct.Sizes);
			sizeAdapter.SetDropDownViewResource (Android.Resource.Layout.SimpleSpinnerDropDownItem);

			sizeSpinner.Adapter = sizeAdapter;
			sizeSpinner.SetSelection (currentProduct.Sizes.IndexOf (currentProduct.Size));

			var colorAdapter = new ArrayAdapter<ProductColor> (Activity, Android.Resource.Layout.SimpleSpinnerDropDownItem, currentProduct.Colors);
			colorAdapter.SetDropDownViewResource (Android.Resource.Layout.SimpleSpinnerDropDownItem);

			colorSpinner.Adapter = colorAdapter;
		}

		public override void OnStart ()
		{
			base.OnStart ();
			AnimateImages ();
		}

		public override void OnStop ()
		{
			base.OnStop ();
			if (kenBurnsAlpha != null)
				kenBurnsAlpha.Cancel ();
			if (kenBurnsMovement != null)
				kenBurnsMovement.Cancel ();
		}

		public override void OnCreateOptionsMenu (IMenu menu, MenuInflater inflater)
		{
			inflater.Inflate (Resource.Menu.menu, menu);
			var cartItem = menu.FindItem (Resource.Id.cart_menu_item);
			cartItem.SetIcon ((basketBadge = new BadgeDrawable (cartItem.Icon)));

			var order = WebService.Shared.CurrentOrder;
			basketBadge.Count = order.Products.Count;
			order.ProductsChanged += (sender, e) => {
				basketBadge.SetCountAnimated (order.Products.Count);
			};
			base.OnCreateOptionsMenu (menu, inflater);
		}

		public override Android.Animation.Animator OnCreateAnimator (FragmentTransit transit, bool enter, int nextAnim)
		{
			if (!enter && shouldAnimatePop)
				return AnimatorInflater.LoadAnimator (View.Context, Resource.Animation.add_to_basket_in);
			return base.OnCreateAnimator (transit, enter, nextAnim);
		}

		void AnimateImages ()
		{
			if (images.Length < 1)
				return;
			if (images.Length == 1) {
				//No need to await the change
				#pragma warning disable 4014
				Images.SetImageFromUrlAsync (productImage, Product.ImageForSize (images [0], Images.ScreenWidth));
				#pragma warning restore 4014
				return;
			}

			productImage.ViewTreeObserver.AddOnGlobalLayoutListener (this);
		}

		public async void OnGlobalLayout ()
		{
			productImage.ViewTreeObserver.RemoveGlobalOnLayoutListener (this);

			const int DeltaX = 100;

			var img1 = Images.FromUrl (Product.ImageForSize (images [0], Images.ScreenWidth));
			var img2 = Images.FromUrl (Product.ImageForSize (images [1], Images.ScreenWidth));

			productDrawable = new KenBurnsDrawable (Color.DarkBlue);
			productDrawable.FirstBitmap = await img1;
			productDrawable.SecondBitmap = await img2;
			productImage.SetImageDrawable (productDrawable);
			currentIndex++;

			var evaluator = new MatrixEvaluator ();
			var finalMatrix = new Matrix ();
			finalMatrix.SetTranslate (-DeltaX, -(float)productDrawable.FirstBitmap.Height / 1.3f + (float)productImage.Height);
			finalMatrix.PostScale (1.27f, 1.27f);
			kenBurnsMovement = ValueAnimator.OfObject (evaluator, new Matrix (), finalMatrix);
			kenBurnsMovement.Update += (sender, e) => productDrawable.SetMatrix ((Matrix)e.Animation.AnimatedValue);
			kenBurnsMovement.SetDuration (14000);
			kenBurnsMovement.RepeatMode = ValueAnimatorRepeatMode.Reverse;
			kenBurnsMovement.RepeatCount = ValueAnimator.Infinite;
			kenBurnsMovement.Start ();

			kenBurnsAlpha = ObjectAnimator.OfInt (productDrawable, "alpha", 0, 0, 0, 255, 255, 255);
			kenBurnsAlpha.SetDuration (kenBurnsMovement.Duration);
			kenBurnsAlpha.RepeatMode = ValueAnimatorRepeatMode.Reverse;
			kenBurnsAlpha.RepeatCount = ValueAnimator.Infinite;
			kenBurnsAlpha.AnimationRepeat += (sender, e) => NextImage ();
			kenBurnsAlpha.Start ();
		}

		async void NextImage ()
		{
			currentIndex = (currentIndex + 1) % images.Length;
			var image = images [currentIndex];
			await Images.SetImageFromUrlAsync (productDrawable, Product.ImageForSize (image, Images.ScreenWidth));
			PrecacheNextImage ();
		}

		void PrecacheNextImage()
		{
			if (currentIndex + 1 >= images.Length)
				cached = true;
			if (cached)
				return;
			var next = currentIndex + 1;
			var image = images [next];
			//No need to await the precache to finish
			#pragma warning disable 4014
			FileCache.Download (Product.ImageForSize (image, Images.ScreenWidth));
			#pragma warning restore 4014
		}
	}
}