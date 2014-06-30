using System;
using System.Linq;
using System.Collections.Generic;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views.Animations;

namespace XamarinStore
{
	public class ProductListFragment : ListFragment
	{
		public event Action<Product,int> ProductSelected = delegate {};
		BadgeDrawable basketBadge;
		int badgeCount;

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			RetainInstance = true;
			SetHasOptionsMenu (true);
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate (Resource.Layout.XamarinListLayout, container, false);
		}

		public override void OnViewCreated (View view, Bundle savedInstanceState)
		{

			base.OnViewCreated (view, savedInstanceState);
			ListView.SetDrawSelectorOnTop (true);
			ListView.Selector = new ColorDrawable (new Android.Graphics.Color (0x30ffffff));
			if (ListAdapter == null) {
				ListAdapter = new ProductListViewAdapter (view.Context);
				GetData ();
			}
		}

		async void GetData ()
		{
			var adapter = (ProductListViewAdapter)ListAdapter;
			adapter.Products = await WebService.Shared.GetProducts ();
			//No need to await the precache, we just need to kick it off
			#pragma warning disable 4014
			WebService.Shared.PreloadImages (Images.ScreenWidth);
			#pragma warning restore 4014
			adapter.NotifyDataSetChanged ();
		}

		public override void OnListItemClick (ListView l, View v, int position, long id)
		{
			base.OnListItemClick (l, v, position, id);
			var adapter = (ProductListViewAdapter) ListView.Adapter;
			ProductSelected (adapter.Products [position], v.Top);
		}

		void AdjustParallax (object sender, View.TouchEventArgs e)
		{
			e.Handled = false;
		}

		public override void OnCreateOptionsMenu (IMenu menu, MenuInflater inflater)
		{
			inflater.Inflate (Resource.Menu.menu, menu);
			var cartItem = menu.FindItem (Resource.Id.cart_menu_item);
			cartItem.SetIcon ((basketBadge = new BadgeDrawable (cartItem.Icon)));

			var order = WebService.Shared.CurrentOrder;
			if (badgeCount != order.Products.Count)
				basketBadge.SetCountAnimated (order.Products.Count);
			else
				basketBadge.Count = order.Products.Count;
			badgeCount = order.Products.Count;
			order.ProductsChanged += (sender, e) => {
				basketBadge.SetCountAnimated (order.Products.Count);
			};
			base.OnCreateOptionsMenu (menu, inflater);
		}

		class ProductListViewAdapter : BaseAdapter
		{
			Context context;
			IInterpolator appearInterpolator = new DecelerateInterpolator ();

			public IReadOnlyList<Product> Products;
			long newItems;

			public override int Count {
				get { return Products == null ? 0 : Products.Count; }
			}

			public override Java.Lang.Object GetItem (int position)
			{
				return new Java.Lang.String (Products [position].ToString ());
			}

			public ProductListViewAdapter (Context context)
			{
				this.context = context;
			}

			public override long GetItemId (int position)
			{
				return Products[position].GetHashCode ();
			}

			public override View GetView (int position, View convertView, ViewGroup parent)
			{
				if (convertView == null) {
					var inflater = LayoutInflater.FromContext (context);
					convertView = inflater.Inflate (Resource.Layout.ProductListItem, parent, false);
					convertView.Id = 0x60000000;
				}
				convertView.Id++;
				var imageView = convertView.FindViewById<ImageView> (Resource.Id.productImage);
				var nameLabel = convertView.FindViewById<TextView> (Resource.Id.productTitle);
				var priceLabel = convertView.FindViewById<TextView> (Resource.Id.productPrice);
				var progressView = convertView.FindViewById<ProgressBar> (Resource.Id.productImageSpinner);

				var product = Products [position];
				nameLabel.Text = product.Name;
				priceLabel.Text = product.PriceDescription;

				LoadProductImage (convertView, progressView, imageView, product);

				if (((newItems >> position) & 1) == 0) {
					newItems |= 1L << position;
					var density = context.Resources.DisplayMetrics.Density;
					convertView.TranslationY = 60 * density;
					convertView.RotationX = 12;
					convertView.ScaleX = 1.1f;
					convertView.PivotY = 180 * density;
					convertView.PivotX = parent.Width / 2;
					convertView.Animate ()
						.TranslationY (0)
						.RotationX (0)
						.ScaleX (1)
						.SetDuration (450)
						.SetInterpolator (appearInterpolator)
						.Start ();
				}

				return convertView;
			}

			async void LoadProductImage (View mainView, ProgressBar progressView, ImageView imageView, Product product)
			{
				var currentId = mainView.Id;
				progressView.Visibility = ViewStates.Visible;
				imageView.SetImageResource (Android.Resource.Color.Transparent);
				await Images.SetImageFromUrlAsync (imageView,product.ImageForSize (Images.ScreenWidth));
				progressView.Visibility = ViewStates.Invisible;
			}
		}
	}
}

