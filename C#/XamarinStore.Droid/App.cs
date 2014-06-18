using System;
using Android.App;
using Android.Runtime;


namespace XamarinStore
{
	[Application]
	public class MyApp : Application
	{
		public MyApp (IntPtr javaReference, JniHandleOwnership transfer)
			: base (javaReference, transfer)
		{
		}

		public override void OnCreate ()
		{
			base.OnCreate ();
		}

	}
}