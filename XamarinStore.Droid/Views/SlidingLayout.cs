using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Util;

namespace XamarinStore
{
	public class SlidingLayout : LinearLayout
	{
		const int PrimaryViewId = Resource.Id.productImage;
		const int SecondaryViewId = Resource.Id.descriptionLayout;

		View primaryView, secondaryView;

		public SlidingLayout (Context context) : base (context)
		{
			Initialize ();
		}

		public SlidingLayout (Context context, IAttributeSet attrs)
			: base (context, attrs)
		{
			Initialize ();
		}

		public SlidingLayout (Context context, IAttributeSet attrs, int defStyle)
			: base (context, attrs, defStyle)
		{
			Initialize ();
		}

		void Initialize ()
		{
			Orientation = Orientation.Vertical;
		}

		public int InitialMainViewDelta {
			get;
			set;
		}

		View PrimaryView {
			get {
				return primaryView ?? (primaryView = FindViewById (PrimaryViewId));
			}
		}

		View SecondaryView {
			get {
				return secondaryView ?? (secondaryView = FindViewById (SecondaryViewId));
			}
		}

		// Inverts children drawing order so that our main item (top) is drawn last
		protected override int GetChildDrawingOrder (int childCount, int i)
		{
			return childCount - 1 - i;
		}

		// Slight hijack of the existing property so that we don't need to define our own
		// which would require Mono.Android.Export
		public override float TranslationY {
			get {
				return PrimaryView.TranslationY / InitialMainViewDelta;
			}
			set {
				PrimaryView.TranslationY = value * InitialMainViewDelta;
			}
		}

		public override float Alpha {
			get {
				return SecondaryView.Alpha;
			}
			set {
				SecondaryView.Alpha = value;
			}
		}

		public override float TranslationX {
			get {
				return base.TranslationX;
			}
			set {
				var power = (float)Math.Pow (value, 5);
				base.TranslationX = power * (Width / 2);
				base.TranslationY = -1 * power * (Height / 2);
				base.Alpha = 1 - value;
				base.ScaleX = base.ScaleY = 1 - .8f * value;
			}
		}
	}
}

