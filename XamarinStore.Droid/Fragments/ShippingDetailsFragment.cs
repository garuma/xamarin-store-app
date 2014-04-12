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
using Android.Views.InputMethods;
using System.Threading.Tasks;

namespace XamarinStore
{
	public class ShippingDetailsFragment : Fragment
	{
		User user;

		public ShippingDetailsFragment() : this(new User()) {}

		public ShippingDetailsFragment(User user)
		{
			this.user = user;
		}

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			RetainInstance = true;
		}

		AutoCompleteTextView state;
		AutoCompleteTextView country;
		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var shippingDetailsView = inflater.Inflate(Resource.Layout.ShippingDetails, container, false);

			var placeOrder = shippingDetailsView.FindViewById<Button> (Resource.Id.PlaceOrder);
			var phone = shippingDetailsView.FindViewById<EditText> (Resource.Id.Phone);
			phone.Text = user.Phone;

			var firstName = shippingDetailsView.FindViewById<EditText> (Resource.Id.FirstName);
			firstName.Text = user.FirstName;

			var lastName = shippingDetailsView.FindViewById<EditText> (Resource.Id.LastName);
			lastName.Text = user.LastName;

			var streetAddress1 = shippingDetailsView.FindViewById<EditText> (Resource.Id.StreetAddress1);
			streetAddress1.Text = user.Address;

			var streetAddress2 = shippingDetailsView.FindViewById<EditText> (Resource.Id.StreetAddress2);
			streetAddress2.Text = user.Address2;

			var city = shippingDetailsView.FindViewById<EditText> (Resource.Id.City);
			city.Text = user.City;

			state = shippingDetailsView.FindViewById<AutoCompleteTextView> (Resource.Id.State);
			state.Text = user.State;

			var postalCode = shippingDetailsView.FindViewById<EditText> (Resource.Id.PostalCode);
			postalCode.Text = user.ZipCode;

			country = shippingDetailsView.FindViewById<AutoCompleteTextView> (Resource.Id.Country);
			user.Country = string.IsNullOrEmpty (user.Country) ? "United States" : user.Country;
			country.Text = user.Country;
			country.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) => {
				LoadStates();
			};

			placeOrder.Click += async (sender, e) => {
				var entries = new EditText[] {
					phone, streetAddress1, streetAddress2, city, state, postalCode, country
				};
				foreach (var entry in entries)
					entry.Enabled = false;
				user.FirstName = firstName.Text;
				user.LastName = lastName.Text;
				user.Phone = phone.Text;
				user.Address = streetAddress1.Text;
				user.Address2 = streetAddress2.Text;
				user.City = city.Text;
				user.State = state.Text;
				user.ZipCode = postalCode.Text;
				user.Country = await WebService.Shared.GetCountryCode(country.Text);
				await ProcessOrder();
				foreach (var entry in entries)
					entry.Enabled = true;
			};
			LoadCountries ();
			LoadStates ();
			return 	shippingDetailsView;
		}

		async void LoadCountries()
		{
			var countries = await WebService.Shared.GetCountries ();
			country.Adapter = new ArrayAdapter(this.Activity, Android.Resource.Layout.SimpleDropDownItem1Line, countries.Select(x=> x.Name).ToList());
		}

		async void LoadStates()
		{
			var states = await WebService.Shared.GetStates (country.Text);
			state.Adapter = new ArrayAdapter(this.Activity, Android.Resource.Layout.SimpleDropDownItem1Line, states);
		}

		async Task ProcessOrder ()
		{	
			var isValid = await user.IsInformationValid ();
			if (!isValid.Item1) {
				Toast.MakeText (Activity, isValid.Item2, ToastLength.Long).Show ();
				return;
			}

			var progressDialog = ProgressDialog.Show(this.Activity, "Please wait...", "Placing Order", true);
			var result = await WebService.Shared.PlaceOrder (user);
			progressDialog.Hide ();
			progressDialog.Dismiss ();
			string message = result.Success ? "Your order has been placed!" : "Error: " + result.Message;
			Toast.MakeText (Activity, message, ToastLength.Long).Show ();
			if (!result.Success)
				return;
			var op = OrderPlaced;
			if (op != null)
				op ();
		}

		public Action OrderPlaced {get;set;}
	}

}
