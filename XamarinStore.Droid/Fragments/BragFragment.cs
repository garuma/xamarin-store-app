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

namespace XamarinStore
{
	public class BragFragment : Fragment
	{
		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			RetainInstance = true;
			// Create your fragment here
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var view = inflater.Inflate (Resource.Layout.BragScreen,null);
			var btn = view.FindViewById<Button> (Resource.Id.bragButton);
			btn.Click += (object sender, EventArgs e) => {
				BragOnTwitter();
			};
			return view;
		}

		void BragOnTwitter()
		{
			var message = "";
			try{
				var intent = new Intent(Intent.ActionSend);
				intent.PutExtra(Intent.ExtraText,message);
				intent.SetType("text/plain");
				StartActivity(Intent.CreateChooser(intent, Resources.GetString(Resource.String.brag_on)));
			} catch(Exception e) {
				Console.WriteLine (e);
			}
		}
	}
}

