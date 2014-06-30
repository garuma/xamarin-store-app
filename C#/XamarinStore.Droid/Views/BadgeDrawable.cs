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
using Android.Graphics.Drawables;
using Android.Animation;

namespace XamarinStore
{
	public class BadgeDrawable : Drawable
	{
		Drawable child;
		Paint badgePaint, textPaint;
		RectF badgeBounds = new RectF ();
		Rect txtBounds = new Rect ();
		int count = 0;
		int alpha = 0xFF;

		ValueAnimator alphaAnimator;

		public BadgeDrawable (Drawable child)
		{
			this.child = child;
			badgePaint = new Paint {
				AntiAlias = true,
				Color = Color.Blue,
			};
			textPaint = new Paint {
				AntiAlias = true,
				Color = Android.Graphics.Color.White,
				TextSize = 16,
				TextAlign = Paint.Align.Center
			};
		}

		public int Count {
			get { return count; }
			set {
				count = value;
				InvalidateSelf ();
			}
		}

		public void SetCountAnimated (int count)
		{
			if (alphaAnimator != null) {
				alphaAnimator.Cancel ();
				alphaAnimator = null;
			}
			const int Duration = 300;

			alphaAnimator = ObjectAnimator.OfInt (this, "alpha", 0xFF, 0);
			alphaAnimator.SetDuration (Duration);
			alphaAnimator.RepeatMode = ValueAnimatorRepeatMode.Reverse;
			alphaAnimator.RepeatCount = 1;
			alphaAnimator.AnimationRepeat += (sender, e) => {
				((Animator)sender).RemoveAllListeners ();
				this.count = count;
			};
			alphaAnimator.Start ();
		}

		public override void Draw (Canvas canvas)
		{
			child.Draw (canvas);
			if (count <= 0)
				return;
			badgePaint.Alpha = textPaint.Alpha = alpha;
			badgeBounds.Set (0, 0, Bounds.Width () / 2, Bounds.Height () / 2);
			canvas.DrawRoundRect (badgeBounds, 8, 8, badgePaint);
			textPaint.TextSize = (8 * badgeBounds.Height ()) / 10;
			var text = count.ToString ();
			textPaint.GetTextBounds (text, 0, text.Length, txtBounds);
			canvas.DrawText (
				text,
				badgeBounds.CenterX (),
				badgeBounds.Bottom - (badgeBounds.Height () - txtBounds.Height ()) / 2 - 1,
				textPaint
			);
		}

		protected override void OnBoundsChange (Rect bounds)
		{
			base.OnBoundsChange (bounds);
			child.SetBounds (bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
		}

		public override int IntrinsicWidth {
			get {
				return child.IntrinsicWidth;
			}
		}

		public override int IntrinsicHeight {
			get {
				return child.IntrinsicHeight;
			}
		}

		public override void SetAlpha (int alpha)
		{
			this.alpha = alpha;
			InvalidateSelf ();
		}

		public override void SetColorFilter (ColorFilter cf)
		{
			child.SetColorFilter (cf);
		}

		public override int Opacity {
			get {
				return child.Opacity;
			}
		}
	}
}

