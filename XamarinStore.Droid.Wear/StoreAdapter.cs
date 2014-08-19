
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

using Android.Support.Wearable.Activity;
using Android.Support.Wearable.Views;

using Android.Gms.Wearable;

using XamarinStore;

namespace XamarinStore.Droid.Wear
{
	public class StoreAdapter : FragmentGridPagerAdapter
	{
		SimpleProduct[] products = new SimpleProduct[0];
		Context context;
		Action<SimpleProduct, bool> clickHandler;
		HashSet<int> enabledRows = new HashSet<int> ();

		public StoreAdapter (Context context, Action<SimpleProduct, bool> clickHandler, FragmentManager fm)
			: base (fm)
		{
			this.context = context;
			this.clickHandler = clickHandler;
		}

		public void SetProducts (SimpleProduct[] products)
		{
			this.products = products;
			NotifyDataSetChanged ();
		}

		public override int GetColumnCount (int row)
		{
			return 2;
		}

		public override int RowCount {
			get {
				return products.Length;
			}
		}

		public override Fragment GetFragment (int row, int column)
		{
			var product = products [row];
			if (column == 0) {
				var card = CardFragment.Create (product.Name, product.Description);
				card.SetCardGravity ((int)GravityFlags.Bottom);
				card.SetExpansionEnabled (false);
				return card;
			} else {
				var res = context.Resources;
				var btn = ToggleActionButtonFragment.WithAction (
					Tuple.Create (res.GetString (Resource.String.add_to_cart), res.GetString (Resource.String.remove_from_cart)),
					Resource.Drawable.cart_button,
					initiallyEnabled: enabledRows.Contains (row)
				);
				btn.Toggled += cked => {
					if (cked)
						enabledRows.Add (row);
					else
						enabledRows.Remove (row);
					clickHandler (product, cked);
				};
				return btn;
			}
		}

		protected override long GetFragmentId (int row, int column)
		{
			// Default action fragment ID generation is good enough
			if (column > 0)
				return base.GetFragmentId (row, column);
			return products [row].Id;
		}

		// The current GridPagerView implementation lack support for ImageRefence coming from
		// an asset so we have to do the conversion ourselves
		public override ImageReference GetBackground (int row, int column)
		{
			var p = products [row];
			return ImageReference.ForBitmap (p.Image);
		}
	}
}

