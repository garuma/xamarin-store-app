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
using Android.Animation;
using Android.Graphics;
using Android.Graphics.Drawables;

using Runnable = Java.Lang.Runnable;

namespace XamarinStore
{
	public class ViewSwipeTouchListener : GestureDetector.SimpleOnGestureListener, View.IOnTouchListener
	{
		GestureDetector detector;
		View targetView;
		ViewConfiguration config;
		int subviewID;

		public event EventHandler SwipeGestureBegin;
		public event EventHandler SwipeGestureEnd;
		public event EventHandler ItemSwipped;

		public ViewSwipeTouchListener (Context context, int subviewID)
		{
			this.detector = new GestureDetector (context, this);
			this.config = ViewConfiguration.Get (context);
			this.subviewID = subviewID;
		}

		public bool OnTouch (View v, MotionEvent e)
		{
			if (targetView == null)
				targetView = subviewID == 0 ? v : v.FindViewById (subviewID);
			if (e.Action == MotionEventActions.Up || e.Action == MotionEventActions.Cancel) {
				if (SwipeGestureEnd != null)
					SwipeGestureEnd (this, EventArgs.Empty);
				var dismiss = e.Action != MotionEventActions.Cancel
				              && targetView.TranslationX > targetView.Width / 2;
				SnapView (dismiss);
			}
			detector.OnTouchEvent (e);
			return true;
		}

		public void ResetSwipe ()
		{
			if (targetView != null) {
				targetView.Alpha = 1;
				targetView.TranslationX = 0;
			}
		}

		void SnapView (bool dismiss = true)
		{
			if (targetView == null)
				return;
			var targetAlpha = dismiss ? 0 : 1;
			var targetTranslation = dismiss ? targetView.Width : 0;
			var a = targetView.Animate ()
				.Alpha (targetAlpha)
				.TranslationX (targetTranslation);
			if (dismiss) {
				a.WithEndAction (new Runnable (() => {
					if (ItemSwipped != null)
						ItemSwipped (this, EventArgs.Empty);
				}));
			}
			a.Start ();
		}

		public override bool OnFling (MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
		{
			// We are only interested in an horizontal right-side fling
			if (velocityY > velocityX || velocityX < 0)
				return base.OnFling (e1, e2, velocityX, velocityY);

			SnapView (dismiss: true);
			return true;
		}

		public override bool OnScroll (MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
		{
			if (SwipeGestureBegin != null)
				SwipeGestureBegin (this, EventArgs.Empty);
			distanceX = -distanceX;
			if (Math.Abs (distanceY) > Math.Abs (distanceX) + config.ScaledTouchSlop
				|| (distanceX < 0 && targetView.TranslationX <= 0))
				return base.OnScroll (e1, e2, distanceX, distanceY);
			targetView.TranslationX = Math.Max (0, targetView.TranslationX + distanceX);
			targetView.Alpha = (targetView.Width - targetView.TranslationX) / ((float)targetView.Width);
			return true;
		}
	}
}

