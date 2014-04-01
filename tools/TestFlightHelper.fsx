module Fake.TestFlightHelper

#I "FAKE/tools"
#r "FakeLib.dll"

#nowarn "20"

open System
open System.IO

open Fake
open Fake.FileUtils
open Fake.ProcessHelper

let private sh cmd args =
    let result = Shell.Exec (cmd, args)
    if result <> 0 then
        failwithf "%s exited with error (%d)" cmd result

type TestFlightArgs =
    {
        ApiToken: string
        TeamToken: string
        File: string
        Notes: string option
        DSym: string option
        DistributionLists: string list
        Notify: bool
        Replace: bool
    }

    static member Default = {
        ApiToken = ""
        TeamToken = ""
        File = ""
        Notes = None
        DSym = None
        DistributionLists = []
        Notify = false
        Replace = false
    }

    member this.ToCurlArgs () = seq {
        yield sprintf "-F file=@%s" this.File
        yield sprintf "-F api_token=%s" this.ApiToken
        yield sprintf "-F team_token=%s" this.TeamToken
        yield sprintf "-F notes='%s'" (defaultArg this.Notes "")
        yield sprintf "-F distribution_lists='%s'" (String.concat "," this.DistributionLists)
        yield sprintf "-F notify=%b" this.Notify
        yield sprintf "-F replace=%b" this.Replace

        match this.DSym with
            | None -> ()
            | Some dsym ->
                // Zip up the .dSYM
                let zipped = dsym + ".zip"
                sh "zip" <| sprintf "-r %s %s" zipped dsym
                yield sprintf "-F dsym=@%s" zipped
    }

let TestFlight (args: TestFlightArgs) =
    String.concat " " (args.ToCurlArgs ())
    |> sprintf "http://testflightapp.com/api/builds.json %s"
    |> sh "curl"