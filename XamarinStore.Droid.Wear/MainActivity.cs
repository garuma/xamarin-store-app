using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics;

using Android.Support.Wearable.Activity;
using Android.Support.Wearable.Views;

using Android.Gms.Common.Apis;
using Android.Gms.Wearable;

namespace XamarinStore.Droid.Wear
{
	[Activity (Label = "Shopping", MainLauncher = true, Icon = "@drawable/icon", Theme = "@android:style/Theme.DeviceDefault.Light")]
	public class MainActivity : InsetActivity, IDataApiDataListener, IGoogleApiClientConnectionCallbacks, IGoogleApiClientOnConnectionFailedListener
	{
		const string RefreshStorePath = "/xamarin/store/Refresh";
		const string ChangeCartPath = "/xamarin/store/Cart/";
		const string StoreResultPath = "/xamarin/store/Result";

		IGoogleApiClient client;
		INode phoneNode;

		GridViewPager pager;
		TextView label;

		Handler handler;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			handler = new Handler ();
			client = new GoogleApiClientBuilder (this, this, this)
				.AddApi (WearableClass.Api)
				.Build ();
		}

		public override void OnReadyForContent ()
		{
			SetContentView (Resource.Layout.Main);
			pager = FindViewById<GridViewPager> (Resource.Id.pager);
			label = FindViewById<TextView> (Resource.Id.loading);
		}

		protected override void OnStart ()
		{
			base.OnStart ();
			client.Connect ();
		}

		public void OnConnected (Bundle p0)
		{
			WearableClass.DataApi.AddListener (client, this);
			RefreshStore ();
		}

		void DisplayError ()
		{
			Finish ();
			var intent = new Intent (this, typeof(ConfirmationActivity));
			intent.PutExtra (ConfirmationActivity.ExtraAnimationType, ConfirmationActivity.FailureAnimation);
			intent.PutExtra (ConfirmationActivity.ExtraMessage, "Can't find phone");
			StartActivity (intent);
		}

		protected override void OnStop ()
		{
			base.OnStop ();
			client.Disconnect ();
		}

		public void OnConnectionSuspended (int reason)
		{
			Android.Util.Log.Error ("GMS", "Connection suspended " + reason);
			WearableClass.DataApi.RemoveListener (client, this);
		}

		public void OnConnectionFailed (Android.Gms.Common.ConnectionResult result)
		{
			Android.Util.Log.Error ("GMS", "Connection failed " + result.ErrorCode);
		}

		public void OnDataChanged (DataEventBuffer dataEvents)
		{
			var dataEvent = Enumerable.Range (0, dataEvents.Count)
				.Select (i => dataEvents.Get (i).JavaCast<IDataEvent> ())
				.FirstOrDefault (de => de.Type == DataEvent.TypeChanged && de.DataItem.Uri.Path == StoreResultPath);
			if (dataEvent == null)
				return;
			SetProductListFromData (dataEvent.DataItem);
		}

		void SetProductListFromData (IDataItem dataItem)
		{
			var dataMapItem = DataMapItem.FromDataItem (dataItem);
			var map = dataMapItem.DataMap;

			var products = new List<SimpleProduct> (map.Size ());
			var data = map.GetDataMapArrayList ("Products");
			foreach (var d in data) {
				products.Add (new SimpleProduct {
					Id = d.GetInt ("Id", 0),
					Name = d.GetString ("Name", "<no name>"),
					Description = d.GetString ("Description", "<no desc>"),
					Image = GetBitmapForAsset (d.GetAsset ("Image"))
				});
			}

			handler.Post (() => {
				var adapter = new StoreAdapter (this, SetProductCartStatus, FragmentManager);
				adapter.SetProducts (products.ToArray ());
				pager.Adapter = adapter;
				pager.OffscreenPageCount = 2;
				pager.Visibility = ViewStates.Visible;
				label.Visibility = ViewStates.Gone;
			});
		}

		void SetProductCartStatus (SimpleProduct product, bool inCart)
		{
			var path = ChangeCartPath + product.Id + (inCart ? "?add" : "?remove");
			WearableClass.MessageApi.SendMessage (client, phoneNode.Id,
			                                      path,
			                                      new byte[0]);
		}

		void RefreshStore ()
		{
			Task.Run (() => {
				var apiResult = WearableClass.NodeApi.GetConnectedNodes (client)
					.Await ().JavaCast<INodeApiGetConnectedNodesResult> ();
				var nodes = apiResult.Nodes;
				phoneNode = nodes.FirstOrDefault ();
				if (phoneNode == null) {
					DisplayError ();
					return;
				}

				// Try to see if we have the data already
				var uri = new Android.Net.Uri.Builder ()
					.Scheme (PutDataRequest.WearUriScheme)
					.Authority (phoneNode.Id)
					.Path (StoreResultPath)
					.Build ();
				var dataResult = WearableClass.DataApi.GetDataItem (client, uri).Await ().JavaCast<IDataApiDataItemResult> ();
				if (dataResult != null && dataResult.DataItem != null)
					SetProductListFromData (dataResult.DataItem);
				else
					WearableClass.MessageApi.SendMessage (client, phoneNode.Id,
					                                      RefreshStorePath,
					                                      new byte[0]);
			});
		}

		Bitmap GetBitmapForAsset (Asset asset)
		{
			var result = WearableClass.DataApi.GetFdForAsset (client, asset)
				.Await ()
				.JavaCast<IDataApiGetFdForAssetResult> ();
			var stream = result.InputStream;
			return BitmapFactory.DecodeStream (stream);
		}
	}
}


