namespace XamarinStore

open System
open System.Collections.Generic
open System.Linq
open System.Text
open Android.App
open Android.Content
open Android.OS
open Android.Runtime
open Android.Util
open Android.Views
open Android.Widget
open Android.Text
open Android.Graphics
open Helpers

type LoginFragment() =
    inherit Fragment()

    let mutable password:EditText = null
    let mutable login:Button = null
    let mutable imageView:ImageView = null

    // TODO: Enter your Xamarin account email address here
    // If you do not have a Xamarin Account please sign up here: https://store.xamarin.com/account/register
    let XamarinAccountEmail = ""

    member val LoginSucceeded = fun ()->() with get,set

    member this.Login usernameParam passwordParam = async {
        let progressDialog = ProgressDialog.Show (this.Activity, "Please wait...", "Logging in", true)
        login.Enabled <- false
        password.Enabled <- false
        let! success = WebService.Shared.Login usernameParam passwordParam
        if success then
            let! canContinue = WebService.Shared.PlaceOrder (WebService.Shared.CurrentUser, true)
            if not canContinue.Success 
            then Toast.MakeText(this.Activity,"Sorry, only one shirt per person. Edit your cart and try again.", ToastLength.Long).Show()
            else this.LoginSucceeded ()
        else Toast.MakeText(this.Activity, "Please verify your Xamarin account credentials and try again", ToastLength.Long).Show()

        login.Enabled <- true
        password.Enabled <- true
        progressDialog.Hide ()
        progressDialog.Dismiss () }

    member private this.CreateInstructions (inflater:LayoutInflater) container savedInstanceState =
        let view = inflater.Inflate (Resource_Layout.PrefillXamarinAccountInstructions, null)
        let textView = view.FindViewById<TextView> (Resource_Id.codeTextView)
        let coloredText = Html.FromHtml ("<font color='#48D1CC'>public readonly</font> <font color='#1E90FF'>string</font> XamarinAccountEmail = <font color='Red'>\"...\"</font>;")
        textView.SetText (coloredText, TextView.BufferType.Spannable)
        view

    member private this.LoadUserImage () = async {
        //Get the correct size in pixels
        let px = int (TypedValue.ApplyDimension(ComplexUnitType.Dip, 85.0f, this.Activity.Resources.DisplayMetrics))
        let! data = Gravatar.GetImageBytes XamarinAccountEmail px Gravatar.G
        let! image = BitmapFactory.DecodeByteArrayAsync (data, 0, data.Length) |> Async.AwaitTask
        imageView.SetImageDrawable (new CircleDrawable (image)) }

    member private this.CreateLoginView (inflater:LayoutInflater) container savedInstanceState =
        let view = inflater.Inflate (Resource_Layout.LoginScreen, null)

        imageView <- view.FindViewById<ImageView> (Resource_Id.imageView1)
        this.LoadUserImage () |> Async.StartImmediate

        let textView = view.FindViewById<EditText> (Resource_Id.email)
        textView.Enabled <- false
        textView.Text <- XamarinAccountEmail

        password <- view.FindViewById<EditText> (Resource_Id.password)
        login <- view.FindViewById<Button> (Resource_Id.signInBtn)
        login.Click.Add(fun x-> this.Login XamarinAccountEmail password.Text |> Async.StartImmediate)
        view

    override this.OnCreate savedInstanceState =
        base.OnCreate savedInstanceState
        this.RetainInstance <- true

    override this.OnCreateView (inflater, container, savedInstanceState) =
        if XamarinAccountEmail = "" then
            this.CreateInstructions inflater container savedInstanceState
        else
            this.CreateLoginView inflater container savedInstanceState