using System;
using Android.App;
using Android.Runtime;

using TestFlight;

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

			TestFlight.TestFlight.TakeOff (this, "dfc77da6-f29c-4fff-acd6-59dc5ad774ab");
			AndroidEnvironment.UnhandledExceptionRaiser += HandleUnhandledException;
		}

		void HandleUnhandledException (object sender, RaiseThrowableEventArgs e)
		{
			TestFlight.TestFlight.SendCrash (e.Exception);
		}

		protected override void Dispose (bool disposing)
		{
			AndroidEnvironment.UnhandledExceptionRaiser -= HandleUnhandledException;
			base.Dispose (disposing);
		}
	}
}