using System;
using System.Collections.Generic;
using System.Globalization;

using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace XamarinStore
{
	public class BasketFragment : ListFragment
	{
		Order _order;
		Button checkoutButton;

		public BasketFragment ()
		{

		}

		public BasketFragment(Order order)
		{
			_order = order;
		}

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			RetainInstance = true;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var shoppingCartView = inflater.Inflate (Resource.Layout.Basket, container, false);

			checkoutButton = shoppingCartView.FindViewById<Button> (Resource.Id.checkoutBtn);
			checkoutButton.Click += (sender, e) => {
				if (CheckoutClicked != null)
					CheckoutClicked ();
			};

			shoppingCartView.PivotY = 0;
			shoppingCartView.PivotX = container.Width;

			return shoppingCartView;
		}

		public override void OnViewCreated (View view, Bundle savedInstanceState)
		{
			base.OnViewCreated (view, savedInstanceState);
			ListView.DividerHeight = 0;
			ListView.Divider = null;
			ListAdapter = new GroceryListAdapter (view.Context, _order.Products);
			if (ListAdapter.Count == 0)
				checkoutButton.Visibility = ViewStates.Invisible;
			_order.ProductsChanged += (sender, e) => {
				checkoutButton.Visibility = _order.Products.Any () ? ViewStates.Visible : ViewStates.Invisible;
			};
		}

		public Action CheckoutClicked { get; set; }
	}

	public class GroceryListAdapter : BaseAdapter
	{
		Context context;
		IReadOnlyList<Product> items;

		public GroceryListAdapter(Context context, IReadOnlyList<Product> items) : base() 
		{
			this.context = context;
			this.items = items;
		}

		public override long GetItemId (int position)
		{
			return position;
		}

		public override Java.Lang.Object GetItem (int position)
		{
			return new Java.Lang.String (items [position].ToString ());
		}

		public override int Count {
			get { return items.Count; }
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			Product product = items [position];

			View view = convertView; // re-use an existing view, if one is available
			if (view == null) { // otherwise create a new one
				view = context
					.GetSystemService (Context.LayoutInflaterService)
					.JavaCast<LayoutInflater> ()
					.Inflate (Resource.Layout.BasketItem, parent, false);
				var swipper = ((SwipableListItem)view).SwipeListener;
				swipper.SwipeGestureBegin += (sender, e) => ((ListView)parent).RequestDisallowInterceptTouchEvent (true);
				swipper.SwipeGestureEnd += (sender, e) => ((ListView)parent).RequestDisallowInterceptTouchEvent (false);
				swipper.ItemSwipped += (sender, e) => {
					var p = ((ListView)parent).GetPositionForView (view);
					var order = WebService.Shared.CurrentOrder;
					order.Remove (order.Products [p]);
					NotifyDataSetChanged ();
				};
			}

			view.FindViewById<TextView> (Resource.Id.productTitle).Text = product.Name;
			view.FindViewById<TextView> (Resource.Id.productPrice).Text = product.PriceDescription;
			view.FindViewById<TextView> (Resource.Id.productColor).Text = product.Color.ToString();
			view.FindViewById<TextView> (Resource.Id.productSize).Text = product.Size.Description;

			var orderImage = view.FindViewById<ImageView> (Resource.Id.productImage);

			orderImage.SetImageResource (Resource.Drawable.blue_shirt);
			//No need to wait for the async download to return the view
			#pragma warning disable 4014
			orderImage.SetImageFromUrlAsync (product.ImageForSize (Images.ScreenWidth));
			#pragma warning restore 4014
			return view;
		}
	}
}