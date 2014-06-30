namespace XamarinStore

open System
open System.Linq

open Android.App
open Android.OS
open Android.Views
open Android.Widget
open System.Threading.Tasks
open Helpers

type ShippingDetailsFragment(userParam: User.User) as this =
    inherit Fragment()
    let mutable user = userParam
    let mutable state:AutoCompleteTextView = null
    let mutable country:AutoCompleteTextView = null

    let LoadCountries() = async {
        let! countries = WebService.Shared.GetCountries ()
        country.Adapter <- new ArrayAdapter(this.Activity, Android.Resource.Layout.SimpleDropDownItem1Line, countries.Select(fun (x:WebService.Country) -> x.Name).ToList()) :> IListAdapter }

    let LoadStates() =
        let states = WebService.Shared.GetStates(country.Text).ToArray()
        state.Adapter <- (new ArrayAdapter(this.Activity, Android.Resource.Layout.SimpleDropDownItem1Line, states) :> IListAdapter)

    let ProcessOrder () = async {   
        let! result = WebService.Shared.ValidateUser user
        match result with
        | Failure msg -> Toast.MakeText(this.Activity, msg, ToastLength.Long).Show()
        | Success _ ->
            let progressDialog = ProgressDialog.Show(this.Activity, "Please wait...", "Placing Order", true)
            let! result = WebService.Shared.PlaceOrder user
            progressDialog.Hide ()
            progressDialog.Dismiss ()
            let message = if result.Success 
                          then "Your order has been placed!"
                          else "Error: " + result.Message
            Toast.MakeText(this.Activity, message, ToastLength.Long).Show()
            if result.Success then
                this.OrderPlaced() }

    override this.OnCreate savedInstanceState =
        base.OnCreate savedInstanceState
        this.RetainInstance <- true

    override this.OnCreateView(inflater, container, savedInstanceState) =
        let shippingDetailsView = inflater.Inflate(Resource_Layout.ShippingDetails, container, false)

        let placeOrder = shippingDetailsView.FindViewById<Button> (Resource_Id.PlaceOrder)
        let phone = shippingDetailsView.FindViewById<EditText> (Resource_Id.Phone)
        phone.Text <- user.Phone

        let firstName = shippingDetailsView.FindViewById<EditText> (Resource_Id.FirstName)
        firstName.Text <- user.FirstName

        let lastName = shippingDetailsView.FindViewById<EditText> (Resource_Id.LastName)
        lastName.Text <- user.LastName

        let streetAddress1 = shippingDetailsView.FindViewById<EditText> (Resource_Id.StreetAddress1)
        streetAddress1.Text <- user.Address

        let streetAddress2 = shippingDetailsView.FindViewById<EditText> (Resource_Id.StreetAddress2)
        streetAddress2.Text <- user.Address2
       
        let city = shippingDetailsView.FindViewById<EditText> (Resource_Id.City)
        city.Text <- user.City

        state <- shippingDetailsView.FindViewById<AutoCompleteTextView> (Resource_Id.State)
        state.Text <- user.State

        let postalCode = shippingDetailsView.FindViewById<EditText> (Resource_Id.PostalCode)
        postalCode.Text <- user.ZipCode

        country <- shippingDetailsView.FindViewById<AutoCompleteTextView> (Resource_Id.Country)
        user <- { user with Country = if user.Country = null then "United States" else user.Country }
        country.Text <- user.Country
        country.ItemSelected.Add(fun _-> LoadStates())

        placeOrder.Click.Add(fun _ -> async {
                                        try
                                            let entries = [| phone
                                                             streetAddress1
                                                             streetAddress2
                                                             city
                                                             state:> EditText
                                                             postalCode
                                                             country :> EditText |]
                                            for entry in entries do
                                                entry.Enabled <- false
                                            
                                            let! countryCode = WebService.Shared.GetCountryCodeFromName(country.Text)

                                            user <- { user with 
                                                        FirstName = firstName.Text
                                                        LastName = lastName.Text
                                                        Phone = phone.Text
                                                        Address = streetAddress1.Text
                                                        Address2 = streetAddress2.Text
                                                        City = city.Text
                                                        State = state.Text
                                                        ZipCode = postalCode.Text
                                                        Country = countryCode }
                                            do! ProcessOrder()
                                            for entry in entries do
                                                entry.Enabled <- true
                                        with e -> ()
                                            } |> Async.StartImmediate)
        LoadCountries () |> Async.StartImmediate
        LoadStates ()
        shippingDetailsView

    member val OrderPlaced = fun ()->() with get,set
