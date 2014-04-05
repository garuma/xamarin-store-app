using System;
using MonoTouch.UIKit;
using BigTed;
using MonoTouch.Foundation;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace XamarinStore
{
	public class ShippingAddressViewController : UITableViewController
	{
		User user;

		public event EventHandler ShippingComplete;

		public readonly TextEntryView FirstNameField;
		public readonly TextEntryView LastNameField;
		public readonly TextEntryView PhoneNumberField;
		public readonly TextEntryView AddressField;
		public readonly TextEntryView Address2Field;
		public readonly TextEntryView CityField;
		public readonly AutoCompleteTextEntry StateField;
		public readonly TextEntryView PostalField;
		public readonly AutoCompleteTextEntry CountryField;
		BottomButtonView BottomView;
		List<UITableViewCell> Cells = new List<UITableViewCell> ();

		public ShippingAddressViewController (User user)
		{
			this.Title = "Shipping";
			//This hides the back button text when you leave this View Controller
			this.NavigationItem.BackBarButtonItem = new UIBarButtonItem ("", UIBarButtonItemStyle.Plain, handler: null);
			this.user = user;
			TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;


			Cells.Add (new CustomViewCell (FirstNameField = new TextEntryView {
				PlaceHolder = "First Name",
				Value = user.FirstName,
			}));

			Cells.Add (new CustomViewCell (LastNameField = new TextEntryView {
				PlaceHolder = "Last Name",
				Value = user.LastName,
			}));

			Cells.Add (new CustomViewCell (PhoneNumberField = new TextEntryView {
				PlaceHolder = "Phone Number",
				Value = user.Phone,
				KeyboardType = UIKeyboardType.NumberPad,
			}));

			Cells.Add (new CustomViewCell (AddressField = new TextEntryView {
				PlaceHolder = "Address",
				Value = user.Address,
				AutocapitalizationType = UITextAutocapitalizationType.Words,
			}));

			Cells.Add (new CustomViewCell (Address2Field = new TextEntryView {
				PlaceHolder = "Address",
				Value = user.Address2,
				AutocapitalizationType = UITextAutocapitalizationType.Words,
			}));
			Cells.Add (new CustomViewCell (CityField = new TextEntryView {
				PlaceHolder = "City",
				Value = user.City,
				AutocapitalizationType = UITextAutocapitalizationType.Words,
			}));

			Cells.Add (new CustomViewCell (PostalField = new TextEntryView {
				PlaceHolder = "Postal Code",
				Value = user.ZipCode,
				KeyboardType = UIKeyboardType.NumbersAndPunctuation,
			}));

			Cells.Add (new CustomViewCell (CountryField = new AutoCompleteTextEntry {
				PlaceHolder = "Country",
				Title = "Select your Country",
				Value = user.Country,
				ValueChanged = (v) => GetStates (),
				PresenterView = this,
			}));

			Cells.Add (new CustomViewCell (StateField = new AutoCompleteTextEntry {
				PlaceHolder = "State",
				Value = user.State,
				Title = "Select your state",
				PresenterView = this,
			}));


			GetCountries ();
			GetStates ();

			TableView.Source = new ShippingAddressPageSource { Cells = Cells };
			TableView.TableFooterView = new UIView (new RectangleF (0, 0, 0, BottomButtonView.Height));
			TableView.ReloadData ();

			View.AddSubview (BottomView = new BottomButtonView () {
				ButtonText = "Place Order",
				ButtonTapped = PlaceOrder,
			});

		}

		public async void PlaceOrder()
		{
			user.FirstName = FirstNameField.Value;
			user.LastName = LastNameField.Value;
			user.Address = AddressField.Value;
			user.Address2 = Address2Field.Value;
			user.City = CityField.Value;
			user.Country = await WebService.Shared.GetCountryCode(CountryField.Value);
			user.Phone = PhoneNumberField.Value;
			user.State = StateField.Value;
			user.ZipCode = PostalField.Value;
			var isValid = await user.IsInformationValid ();
			if (!isValid.Item1) {

				new UIAlertView ("Error", isValid.Item2, null, "Ok").Show ();
				return;
			}
			if (ShippingComplete != null)
				ShippingComplete (this, EventArgs.Empty);
		}
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
		}

		async void GetCountries ()
		{
			var countries = await WebService.Shared.GetCountries ();
			CountryField.Items = countries.Select (x => x.Name);
		}

		async void GetStates ()
		{
			var states = await WebService.Shared.GetStates (CountryField.Value);
			StateField.Items = states;
		}

		public override void ViewDidLayoutSubviews ()
		{
			base.ViewDidLayoutSubviews ();

			var bound = View.Bounds;
			bound.Y = bound.Bottom - BottomButtonView.Height;
			bound.Height = BottomButtonView.Height;
			BottomView.Frame = bound;
		}

		public class ShippingAddressPageSource : UITableViewSource
		{
			public List<UITableViewCell> Cells = new List<UITableViewCell> ();

			public ShippingAddressPageSource ()
			{
			}

			public override int RowsInSection (UITableView tableview, int section)
			{
				return Cells.Count;
			}

			public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				return Cells [indexPath.Row];
			}

			public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
			{
				return Cells [indexPath.Row].Frame.Height;
			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				if (Cells [indexPath.Row] is StringSelectionCell)
					((StringSelectionCell)Cells [indexPath.Row]).Tap ();

				tableView.DeselectRow (indexPath, true);
			}
		}
	}
}

