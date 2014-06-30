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
using Android.Animation;

namespace XamarinStore
{
	public class XamarinSpinnerView : View
	{
		AnimatorSet animation;
		float rotation, scaleX, scaleY;

		Path hexagon, cross;
		Paint hexagonPaint, crossPaint;
		Matrix transformationMatrix;

		public XamarinSpinnerView (Context context) :
			base (context)
		{
			Initialize ();
		}

		public XamarinSpinnerView (Context context, IAttributeSet attrs) :
			base (context, attrs)
		{
			Initialize ();
		}

		public XamarinSpinnerView (Context context, IAttributeSet attrs, int defStyle) :
			base (context, attrs, defStyle)
		{
			Initialize ();
		}

		void Initialize ()
		{
			ScaleX = ScaleY = 1;
			var baseDuration = 1000;
			animation = new AnimatorSet ();
			var rotation = ObjectAnimator.OfFloat (this, "rotation", 0, 60);
			rotation.SetDuration (baseDuration);
			rotation.RepeatCount = ValueAnimator.Infinite;
			var scale = ObjectAnimator.OfPropertyValuesHolder (
				this,
				PropertyValuesHolder.OfFloat ("scaleX", 1, .9f),
				PropertyValuesHolder.OfFloat ("scaleY", 1, .9f)
			);
			scale.RepeatMode = ValueAnimatorRepeatMode.Reverse;
			scale.SetDuration (baseDuration / 2);
			scale.RepeatCount = ValueAnimator.Infinite;
			animation.PlayTogether (rotation, scale);
			animation.Start ();
		}

		public override float Rotation {
			get { return rotation; }
			set {
				rotation = value;
				Invalidate ();
			}
		}

		public override float ScaleX {
			get { return scaleX; }
			set {
				scaleX = value;
				Invalidate ();
			}
		}

		public override float ScaleY {
			get { return scaleY; }
			set {
				scaleY = value;
				Invalidate ();
			}
		}

		protected override void OnDraw (Canvas canvas)
		{
			base.OnDraw (canvas);
			DrawHexagon (canvas);

			canvas.Save ();
			canvas.Scale (.4f, .5f, Width / 2, Height / 2);
			DrawCross (canvas);
			canvas.Restore ();
		}

		void DrawHexagon (Canvas canvas)
		{
			// The extra padding is to avoid edges being clipped
			var padding = (int)TypedValue.ApplyDimension (ComplexUnitType.Dip, 8, Resources.DisplayMetrics);
			var halfHeight = (Height - padding) / 2;
			var side = (Width - padding) / 2;
			var foo = (int)Math.Sqrt (side * side - halfHeight * halfHeight);

			var path = hexagon ?? (hexagon = new Path ());
			hexagon.Reset ();
			path.MoveTo (Width / 2, padding / 2);
			path.RLineTo (-side / 2, 0);
			path.RLineTo (-foo, halfHeight);
			path.RLineTo (foo, halfHeight);
			path.RLineTo (side, 0);
			path.RLineTo (foo, -halfHeight);
			path.RLineTo (-foo, -halfHeight);
			path.Close ();

			var m = transformationMatrix ?? (transformationMatrix = new Matrix ());
			m.Reset ();
			var centerX = Width / 2;
			var centerY = Height / 2;
			m.PostRotate (Rotation, centerX, centerY);
			m.PostScale (ScaleX, ScaleY, centerX, centerY);
			path.Transform (m);

			if (hexagonPaint == null) {
				hexagonPaint = new Paint {
					Color = new Android.Graphics.Color (0x22, 0x76, 0xB9),
					AntiAlias = true,
				};
				hexagonPaint.SetPathEffect (new CornerPathEffect (30));
			}

			canvas.DrawPath (path, hexagonPaint);
		}

		void DrawCross (Canvas canvas)
		{
			var smallSegment = Width / 6;

			var path = cross ?? (cross = new Path ());
			cross.Reset ();

			path.MoveTo (0, 0);
			path.RLineTo (smallSegment, 0);
			path.LineTo (Width / 2, Height / 2);
			path.LineTo (Width - smallSegment, 0);
			path.RLineTo (smallSegment, 0);
			path.LineTo (Width / 2 + smallSegment, Height / 2);
			path.LineTo (Width, Height);
			path.RLineTo (-smallSegment, 0);
			path.LineTo (Width / 2, Height / 2);
			path.LineTo (smallSegment, Height);
			path.RLineTo (-smallSegment, 0);
			path.LineTo (Width / 2 - smallSegment, Height / 2);
			path.Close ();

			if (crossPaint == null) {
				crossPaint = new Paint {
					AntiAlias = true,
					Color = Android.Graphics.Color.White
				};
			}

			canvas.DrawPath (path, crossPaint);
		}
	}
}

