
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

using Android.Gms.Wearable;
using Android.Gms.Common.Apis;

namespace XamarinStore
{
	[Service]
	[IntentFilter (new[] { "com.google.android.gms.wearable.BIND_LISTENER" })]
	public class WearService : WearableListenerService
	{
		const string RefreshStorePath = "/xamarin/store/Refresh";
		const string ChangeCartPath = "/xamarin/store/Cart/";
		const string StoreResultPath = "/xamarin/store/Result";

		public override void OnMessageReceived (IMessageEvent messageEvent)
		{
			base.OnMessageReceived (messageEvent);
			if (messageEvent.Path != RefreshStorePath
			    && !messageEvent.Path.StartsWith (ChangeCartPath))
				return;

			HandleMessage (messageEvent);
		}

		async void HandleMessage (IMessageEvent message)
		{
			try {
				Android.Util.Log.Info ("WearIntegration", "Received Message");
				var client = new GoogleApiClientBuilder (this)
					.AddApi (WearableClass.Api)
					.Build ();

				var result = client.BlockingConnect (30, Java.Util.Concurrent.TimeUnit.Seconds);
				if (!result.IsSuccess)
					return;
				var path = message.Path;

				try {
					if (path == RefreshStorePath) {
						var products = await WebService.Shared.GetProducts ();
						var mapReq = PutDataMapRequest.Create (StoreResultPath);
						var map = mapReq.DataMap;

						var children = new List<DataMap> (products.Count);
						foreach (var p in products ) {
							var obj = new DataMap ();
							var id = p.Name.GetHashCode ();
							obj.PutInt ("Id", id);
							obj.PutString ("Name", p.Name);
							obj.PutString ("Description", p.PriceDescription);
							obj.PutAsset ("Image", await CreateWearAssetFrom (p.ImageForSize (360)));
							children.Add (obj);
						}
						map.PutDataMapArrayList ("Products", children.ToList ());
						WearableClass.DataApi.PutDataItem (client, mapReq.AsPutDataRequest ());
					} else if (path.StartsWith (ChangeCartPath)) {
						var uri = new Uri ("wear://watch" + path);
						var query = uri.GetComponents (UriComponents.Query, UriFormat.Unescaped);
						var lastPath = uri.GetComponents (UriComponents.Path, UriFormat.Unescaped);
						lastPath = lastPath.Substring (lastPath.LastIndexOf ('/') + 1);
						int id;
						if (!int.TryParse (lastPath, out id))
							return;

						var intent = new Intent (this, typeof (MainActivity))
							.AddFlags (ActivityFlags.NewTask)
							.AddFlags (ActivityFlags.SingleTop)
							.PutExtra ("OrderAction", query)
							.PutExtra ("ProductId", id);
						StartActivity (intent);
					}
				} finally {
					client.Disconnect ();
				}
			} catch (Exception e) {
				Android.Util.Log.Error ("WearIntegration", e.ToString ());
			}
		}

		async Task<Asset> CreateWearAssetFrom (string url)
		{
			if (string.IsNullOrEmpty (FileCache.SaveLocation))
				FileCache.SaveLocation = CacheDir.AbsolutePath;
			var filePath = await FileCache.Download (url);
			return Asset.CreateFromBytes (System.IO.File.ReadAllBytes (filePath));
		}
	}
}

