using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Xamarin.SSO.Client;

namespace XamarinStore
{
	public class WebService
	{
		public static readonly WebService Shared = new WebService ();

		public User CurrentUser { get; set; }
		public Order CurrentOrder { get; set; }

		XamarinSSOClient client = new XamarinSSOClient ("https://auth.xamarin.com", "0c833t3w37jq58dj249dt675a465k6b0rz090zl3jpoa9jw8vz7y6awpj5ox0qmb");

		public WebService ()
		{
			CurrentOrder = new Order ();
		}

		public async Task<bool> Login (string username, string password)
		{
			AccountResponse response;
			try {
				var request = Task.Run (() => response = client.CreateToken (username, password));
				response = await request;
				if (response.Success) {
					var user = response.User;
					CurrentUser = new  User {
						LastName = user.LastName,
						FirstName = user.FirstName,
						Email = username,
						Token = response.Token
					};
					return true;
				} else {
					Console.WriteLine ("Login failed: {0}", response.Error);
				}
			} catch (Exception ex) {
				Console.WriteLine ("Login failed for some reason...: {0}", ex.Message);
			}
			return false;
		}

		List<Product> products;
		public async Task<List<Product>> GetProducts()
		{
			if (products == null) {
				products = await Task.Factory.StartNew (() => {
					try {
						string extraParams = "";

						//TODO: Get a Monkey!!!
						//extraParams = "?includeMonkeys=true";

						var request = CreateRequest ("products" + extraParams);

						string response = ReadResponseText (request);
						return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Product>> (response);
					} catch (Exception ex) {
						Console.WriteLine (ex);
						return new List<Product> ();
					}
				});
			}
			return products;
		}
		bool hasPreloadedImages;
		public async Task PreloadImages(float imageWidth)
		{
			if (hasPreloadedImages)
				return;
			hasPreloadedImages = true;
			//Lets precach the countries too
			#pragma warning disable 4014
			GetCountries ();
			#pragma warning restore 4014
			await Task.Factory.StartNew (() => {
				var imagUrls = products.SelectMany (x => x.ImageUrls.Select (y => Product.ImageForSize (y, imageWidth))).ToList ();
				imagUrls.ForEach( async (x) => await FileCache.Download(x));

			});


		}
		List<Country> countries = new List<Country>();
		public Task<List<Country>> GetCountries()
		{
			return Task.Factory.StartNew (() => {
				try {

					if(countries.Count > 0)
						return countries;

					var request = CreateRequest ("Countries");
					string response = ReadResponseText (request);
					countries = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Country>> (response);
					return countries;
				} catch (Exception ex) {
					Console.WriteLine (ex);
					return new List<Country> ();
				}
			});
		}

		public async Task<string> GetCountryCode(string country)
		{
			var c = (await GetCountries ()).FirstOrDefault (x => x.Name == country) ?? new Country();

			return c.Code;
		}
		//No need to await anything, and no need to spawn a task to return a list.
		#pragma warning disable 1998
		public  async Task<List<string>> GetStates(string country)
		{
			if (country.ToLower () == "united states")
				return new List<string> {
					"Alabama",
					"Alaska", 
					"Arizona",
					"Arkansas",
					"California",
					"Colorado",
					"Connecticut",
					"Delaware",
					"District of Columbia",
					"Florida",
					"Georgia",
					"Hawaii",
					"Idaho",
					"Illinois",
					"Indiana",
					"Iowa",
					"Kansas",
					"Kentucky",
					"Louisiana",
					"Maine",
					"Maryland",
					"Massachusetts",
					"Michigan",
					"Minnesota",
					"Mississippi",
					"Missouri",
					"Montana",
					"Nebraska",
					"Nevada",
					"New Hampshire",
					"New Jersey",
					"New Mexico",
					"New York",
					"North Carolina",
					"North Dakota",
					"Ohio",
					"Oklahoma",
					"Oregon",
					"Pennsylvania",
					"Rhode Island",
					"South Carolina",
					"South Dakota",
					"Tennessee",
					"Texas",
					"Utah",
					"Vermont",
					"Virginia",
					"Washington",
					"West Virginia",
					"Wisconsin",
					"Wyoming",
				};
			return new List<string> ();
		}
		#pragma warning restore 1998

		static HttpWebRequest CreateRequest(string location)
		{
			var request = (HttpWebRequest)WebRequest.Create ("https://xamarin-store-app.xamarin.com/api/"+ location);
			request.Method = "GET";
			request.ContentType = "application/json";
			request.Accept = "application/json";
			return request;
		}
			

		public Task<OrderResult> PlaceOrder (User user, bool verify = false) {
			return Task.Factory.StartNew (() => {
				try {
					var content = Encoding.UTF8.GetBytes (CurrentOrder.GetJson (user));

					var request = CreateRequest ("order" + (verify ? "?verify=1" : ""));
					request.Method = "POST";
					request.ContentLength = content.Length;

					using (Stream s = request.GetRequestStream ()) {
						s.Write (content, 0, content.Length);
					}
					string response = ReadResponseText (request);
					var result = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderResult> (response);
					if(!verify && result.Success)
						CurrentOrder = new Order();
					return result;
				} catch (Exception ex) {
					return new OrderResult {
						Success = false,
						Message = ex.Message,
					};
				}
			});
		}
			

		protected static string ReadResponseText (HttpWebRequest req) {
			using (WebResponse resp = req.GetResponse ()) {
				using (Stream s = (resp).GetResponseStream ()) {
					using (var r = new StreamReader (s, Encoding.UTF8)) {
						return r.ReadToEnd ();
					}
				}
			}
		}
	}
}

