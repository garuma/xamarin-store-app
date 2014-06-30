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
using Android.Graphics;

using Runnable = Java.Lang.Runnable;

namespace XamarinStore
{
	public class SwipableListItem : FrameLayout
	{
		View mainContent;
		View secondaryContent;
		ViewSwipeTouchListener listener;

		Paint shadow;
		const int DarkTone = 0xd3;
		const int LightTone = 0xdd;
		static readonly int[] Colors = new int[] {
			Android.Graphics.Color.Rgb (DarkTone, DarkTone, DarkTone).ToArgb (),
			Android.Graphics.Color.Rgb (LightTone, LightTone, LightTone).ToArgb (),
			Android.Graphics.Color.Rgb (LightTone, LightTone, LightTone).ToArgb (),
			Android.Graphics.Color.Rgb (DarkTone, DarkTone, DarkTone).ToArgb (),
		};
		static readonly float[] Positions = new float[] { 0, .15f, .85f, 1 };

		public SwipableListItem (Context context) :
			base (context)
		{
			Initialize ();
		}

		public SwipableListItem (Context context, IAttributeSet attrs) :
			base (context, attrs)
		{
			Initialize ();
		}

		public SwipableListItem (Context context, IAttributeSet attrs, int defStyle) :
			base (context, attrs, defStyle)
		{
			Initialize ();
		}

		void Initialize ()
		{
			shadow = new Paint { AntiAlias = true };
			listener = new ViewSwipeTouchListener (Context, Resource.Id.SwipeContent);
			SetOnTouchListener (listener);
			listener.ItemSwipped += (sender, e) => {
				listener.ResetSwipe ();
			};
		}

		public ViewSwipeTouchListener SwipeListener {
			get {
				return listener;
			}
		}

		View MainContent {
			get { return mainContent ?? (mainContent = FindViewById (Resource.Id.SwipeContent)); }
		}

		View SecondaryContent {
			get { return secondaryContent ?? (secondaryContent = FindViewById (Resource.Id.SwipeAfter)); }
		}

		protected override void DispatchDraw (Android.Graphics.Canvas canvas)
		{
			// Draw interior shadow
			canvas.Save ();
			canvas.ClipRect (0, 0, Width, Height);
			canvas.DrawPaint (shadow);
			canvas.Restore ();

			base.DispatchDraw (canvas);

			// Draw custom list separator
			canvas.Save ();
			canvas.ClipRect (0, Height - 2, Width, Height);
			canvas.DrawColor (Android.Graphics.Color.Rgb (LightTone, LightTone, LightTone));
			canvas.Restore ();
		}

		protected override void OnLayout (bool changed, int left, int top, int right, int bottom)
		{
			base.OnLayout (changed, left, top, right, bottom);
			shadow.SetShader (
				new LinearGradient (0, 0, 0, bottom - top, Colors, Positions, Shader.TileMode.Repeat)
			);
		}
	}
}

