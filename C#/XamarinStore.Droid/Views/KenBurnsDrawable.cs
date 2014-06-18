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
using Android.Animation;
using Android.Graphics.Drawables;

namespace XamarinStore
{
	class KenBurnsDrawable : Drawable, IBitmapHolder
	{
		Color defaultColor;
		int alpha;
		Matrix matrix;
		Paint paint;
		bool secondSlot;

		Bitmap bmp1, bmp2;
		BitmapShader shader1, shader2;

		public KenBurnsDrawable (Color defaultColor)
		{
			this.defaultColor = defaultColor;
			this.paint = new Paint {
				AntiAlias = false,
				FilterBitmap = false
			};
		}

		public Bitmap FirstBitmap {
			get { return bmp1; }
			set {
				bmp1 = value;
				shader1 = null;
				InvalidateSelf ();
			}
		}

		public Bitmap SecondBitmap {
			get { return bmp2; }
			set {
				bmp2 = value;
				shader2 = null;
				InvalidateSelf ();
			}
		}

		public void SetImageBitmap (Bitmap bmp)
		{
			if (secondSlot)
				SecondBitmap = bmp;
			else
				FirstBitmap = bmp;
			secondSlot = !secondSlot;
		}

		public override void Draw (Canvas canvas)
		{
			var bounds = Bounds;

			if (alpha != 255) {
				paint.Alpha = 255;
				if (SecondBitmap != null) {
					if (shader1 == null)
						shader1 = new BitmapShader (FirstBitmap, Shader.TileMode.Clamp, Shader.TileMode.Clamp);
					shader1.SetLocalMatrix (matrix);
					paint.SetShader (shader1);
					canvas.DrawRect (bounds, paint);
				} else
					canvas.DrawColor (defaultColor);
			}
			if (alpha != 0) {
				paint.Alpha = alpha;
				if (FirstBitmap != null) {
					if (shader2 == null)
						shader2 = new BitmapShader (SecondBitmap, Shader.TileMode.Clamp, Shader.TileMode.Clamp);
					shader2.SetLocalMatrix (matrix);
					paint.SetShader (shader2);
					canvas.DrawRect (bounds, paint);
				} else
					canvas.DrawColor (defaultColor);
			}
		}

		public void SetMatrix (Matrix matrix)
		{
			this.matrix = matrix;
			InvalidateSelf ();
		}

		public override void SetAlpha (int alpha)
		{
			this.alpha = alpha;
			InvalidateSelf ();
		}

		public override void SetColorFilter (ColorFilter cf)
		{
		}

		public override int Opacity {
			get {
				return (int)Format.Opaque;
			}
		}
	}
}

