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
	public class CircleDrawable : Drawable
	{
		Bitmap bmp;
		BitmapShader bmpShader;
		Paint paint;
		RectF oval;

		public CircleDrawable (Bitmap bmp)
		{
			this.bmp = bmp;
			this.bmpShader = new BitmapShader (bmp, Shader.TileMode.Clamp, Shader.TileMode.Clamp);
			this.paint = new Paint () { AntiAlias = true };
			this.paint.SetShader (bmpShader);
			this.oval = new RectF ();
		}

		public override void Draw (Canvas canvas)
		{
			canvas.DrawOval (oval, paint);
		}

		protected override void OnBoundsChange (Rect bounds)
		{
			base.OnBoundsChange (bounds);
			oval.Set (0, 0, bounds.Width (), bounds.Height ());
		}

		public override int IntrinsicWidth {
			get {
				return bmp.Width;
			}
		}

		public override int IntrinsicHeight {
			get {
				return bmp.Height;
			}
		}

		public override void SetAlpha (int alpha)
		{

		}

		public override int Opacity {
			get {
				return (int)Format.Opaque;
			}
		}

		public override void SetColorFilter (ColorFilter cf)
		{

		}
	}
}

