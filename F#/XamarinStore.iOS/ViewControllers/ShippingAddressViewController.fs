namespace XamarinStore

open System
open MonoTouch.UIKit
open BigTed
open MonoTouch.Foundation
open System.Collections.Generic
open System.Drawing
open System.Linq
open User
open Helpers
open WebService

type ShippingAddressPageSource() =
    inherit UITableViewSource()

    member val Cells = new List<UITableViewCell> () with get,set

    override this.RowsInSection (tableview, section) =
        this.Cells.Count

    override this.GetCell (tableView, indexPath) =
        this.Cells.[indexPath.Row]

    override this.GetHeightForRow (tableView, indexPath) =
        this.Cells.[indexPath.Row].Frame.Height

    override this.RowSelected (tableView, indexPath) =
        match this.Cells.[indexPath.Row] with
        | :? StringSelectionCell as cell ->cell.Tap()
        | _ -> ()

        tableView.DeselectRow (indexPath, true)

type ShippingAddressViewController(user:User) as this =
    inherit UITableViewController()

    let Cells = new List<UITableViewCell>()

    let BottomView = new BottomButtonView ( ButtonText = "Place Order",
                                            ButtonTapped = fun _ ->this.PlaceOrder() |> Async.StartImmediate)

    let FirstNameField = new TextEntryView (PlaceHolder = "First Name", 
                                            Value = user.FirstName)

    let LastNameField = new TextEntryView ( PlaceHolder = "Last Name", 
                                            Value = user.LastName)

    let PhoneNumberField = new TextEntryView ( PlaceHolder = "Phone Number", 
                                               Value = user.Phone, 
                                               KeyboardType = UIKeyboardType.NumberPad)

    let AddressField = new TextEntryView (PlaceHolder = "Address", 
                                          Value = user.Address, 
                                          AutocapitalizationType = UITextAutocapitalizationType.Words )

    let Address2Field = new TextEntryView ( PlaceHolder = "Address", 
                                            Value = user.Address2, 
                                            AutocapitalizationType = UITextAutocapitalizationType.Words)

    let CityField = new TextEntryView ( PlaceHolder = "City", 
                                        Value = user.City, 
                                        AutocapitalizationType = UITextAutocapitalizationType.Words)

    let PostalField = new TextEntryView ( PlaceHolder = "Postal Code", 
                                         Value = user.ZipCode, 
                                         KeyboardType = UIKeyboardType.NumbersAndPunctuation )

    let CountryField = new AutoCompleteTextEntry ( PlaceHolder = "Country",
                                                  Title = "Select your Country",
                                                  Value = user.Country,
                                                  PresenterView = this)

    let StateField = new AutoCompleteTextEntry ( PlaceHolder = "State",
                                                 Value = user.State,
                                                 Title = "Select your state",
                                                 PresenterView = this)

    let GetCountries () = async {
        let! countries = WebService.Shared.GetCountries()
        let countryNames = countries.Select(fun x -> x.Name)
        CountryField.Items <- countryNames }

    let GetStates () =
        let states = WebService.Shared.GetStates (CountryField.Value)
        StateField.Items <- states

    do CountryField.ValueChanged <- fun v -> GetStates ()
       this.Title <- "Shipping"
       //This hides the back button text when you leave this View Controller
       this.NavigationItem.BackBarButtonItem <- new UIBarButtonItem ("", UIBarButtonItemStyle.Plain, handler = null)
       this.TableView.SeparatorStyle <- UITableViewCellSeparatorStyle.None

       Cells.Add (new CustomViewCell (FirstNameField ))
       Cells.Add (new CustomViewCell (LastNameField ))
       Cells.Add (new CustomViewCell (PhoneNumberField ))
       Cells.Add (new CustomViewCell (AddressField ))
       Cells.Add (new CustomViewCell (Address2Field ))
       Cells.Add (new CustomViewCell (CityField ))
       Cells.Add (new CustomViewCell (PostalField ))
       Cells.Add (new CustomViewCell (CountryField ))
       Cells.Add (new CustomViewCell (StateField ))

       GetCountries () |> Async.StartImmediate
       GetStates ()

       this.TableView.Source <- new ShippingAddressPageSource ( Cells = Cells )
       this.TableView.TableFooterView <- new UIView(new RectangleF (0.0f, 0.0f, 0.0f, BottomButtonView.Height))
       this.TableView.ReloadData()

       this.View.AddSubview BottomView

    member val ShippingComplete = fun ()->() with get,set

    member this.PlaceOrder() = async {
        let! country = WebService.Shared.GetCountryCodeFromName(CountryField.Value)
        let user = { user with FirstName = FirstNameField.Value
                               LastName = LastNameField.Value
                               Address = AddressField.Value
                               Address2 = Address2Field.Value
                               City = CityField.Value
                               Country = country 
                               Phone = PhoneNumberField.Value
                               State = StateField.Value
                               ZipCode = PostalField.Value }
        let! isValid = WebService.Shared.ValidateUser user
        match isValid with
        | Success msg -> WebService.Shared.CurrentUser <- user
                         this.ShippingComplete()
        | Failure msg -> (new UIAlertView("Error", msg, null, "Ok")).Show() }

    override this.ViewWillAppear animated =
        base.ViewWillAppear animated

    override this.ViewDidLayoutSubviews () =
        base.ViewDidLayoutSubviews ()

        let mutable bound = this.View.Bounds
        bound.Y <- bound.Bottom - BottomButtonView.Height
        bound.Height <- BottomButtonView.Height
        BottomView.Frame <- bound