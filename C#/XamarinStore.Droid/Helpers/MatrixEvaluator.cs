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

namespace XamarinStore
{
	class MatrixEvaluator : Java.Lang.Object, ITypeEvaluator
	{
		float[] startComponents = new float[9];
		float[] endComponents = new float[9];
		float[] currentComponents = new float[9];
		Matrix result = new Matrix ();

		public Java.Lang.Object Evaluate (float fraction, Java.Lang.Object startValue, Java.Lang.Object endValue)
		{
			((Matrix)startValue).GetValues (startComponents);
			((Matrix)endValue).GetValues (endComponents);

			for (int i = 0; i < 9; i++)
				currentComponents [i] = startComponents [i] + (endComponents [i] - startComponents [i]) * fraction;

			result.SetValues (currentComponents);
			return result;
		}
	}
}

