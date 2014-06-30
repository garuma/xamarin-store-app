namespace XamarinStore

open System
open Android.App
open Android.Content
open Android.OS
open Android.Views
open Android.Widget

type BragFragment() as this =
    inherit Fragment()

    let BragOnTwitter() =
        let message = ""
        try
            let intent = new Intent(Intent.ActionSend)
            intent.PutExtra(Intent.ExtraText,message) |> ignore
            intent.SetType("text/plain") |> ignore
            this.StartActivity(Intent.CreateChooser(intent, this.Resources.GetString(Resource_String.brag_on)))
        with e-> Console.WriteLine e

    override this.OnCreate savedInstanceState =
        base.OnCreate savedInstanceState
        this.RetainInstance <- true

    override this.OnCreateView (inflater, container, savedInstanceState) =
        let view = inflater.Inflate (Resource_Layout.BragScreen, null)
        let btn = view.FindViewById<Button> Resource_Id.bragButton
        btn.Click.Add(fun _ -> BragOnTwitter() )
        view
