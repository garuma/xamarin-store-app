module WebService

    open System
    open System.IO
    open System.Net
    open System.Runtime.Serialization
    open Newtonsoft.Json

    open User
    open Order
    open Helpers
    open Product
    open XamarinSSOClient
    
    [<DataContract;CLIMutable>]
    [<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type Country = 
        { 
            [<DataMember(Name="code")>]
            Code: string
            [<DataMember(Name="name")>]
            Name: string 
        }
    
    let ReadResponseText (req : HttpWebRequest) =
        use response = req.GetResponse ()
        use s = response.GetResponseStream()
        use r = new StreamReader (s, System.Text.Encoding.UTF8)
        r.ReadToEnd()
    
    let CreateRequest location =
        let request = WebRequest.Create ("https://xamarin-store-app.xamarin.com/api/"+ location) :?> HttpWebRequest
        request.Method <- "GET"
        request.ContentType <- "application/json"
        request.Accept <- "application/json"
        request

    let mutable CurrentOrder : Order = Order.CreateOrder()

    type WebService() =
        let client = new XamarinSSOClient ("https://auth.xamarin.com", "0c833t3w37jq58dj249dt675a465k6b0rz090zl3jpoa9jw8vz7y6awpj5ox0qmb")
        let mutable products = [||]
        let mutable hasPreloadedImages = false
        let mutable countries = [||]

        member val CurrentUser = User.CreateUser() with get,set

        member this.Login username password = async {
            try
                let! token = client.CreateToken username password
                match token with
                | Failure msg ->return false
                | Success accountResponse -> if not accountResponse.Success then
                                                 Console.WriteLine ("Login failed: {0}", accountResponse.Error)
                                                 return false
                                             else
                                                 let user = accountResponse.User
                                                 this.CurrentUser <- { FirstName = user.FirstName
                                                                       LastName = user.LastName
                                                                       Email = user.Email
                                                                       Token = accountResponse.Token 
                                                                       Address = ""; Address2 = ""; City = ""; State = ""; ZipCode = ""; Phone = ""; Country = ""}
                                                 return true
            with | e-> Console.WriteLine ("Login failed for some reason...: {0}", e.Message)
                       return false }

        member this.GetProducts ():Async<Product []> = async {
            match products with
            | [||] -> try
                          return! System.Threading.Tasks.Task.Run(fun _->
                              //let extraParams = "&includeMonkeys=true"
                              let extraParams = ""
                              let response = ReadResponseText (CreateRequest ("products?shirttype=fsharp"+extraParams))
                              products <- JsonConvert.DeserializeObject<Product array>(response)
                              products ) |> Async.AwaitTask
                      with | e-> Console.WriteLine e
                                 return [||]
            | _ -> return products }

        member this.GetCountries() = async {
            match countries with
            | [||] -> try
                          return! System.Threading.Tasks.Task.Run(fun _->
                              let response = ReadResponseText (CreateRequest "Countries")
                              countries <- JsonConvert.DeserializeObject<Country array>(response)
                              countries ) |> Async.AwaitTask
                      with | e-> Console.WriteLine e
                                 return [||]
            | _ -> return countries }

        member this.PreloadImages imageWidth = async {
            if hasPreloadedImages then
                ()
            else hasPreloadedImages <- true
                 this.GetCountries() |> Async.Ignore |> Async.StartImmediate
                 do! products |> Array.collect (fun x->x.GetImageUrls() )
                              |> Array.map (fun y->Product.ImageForSize(y, imageWidth) 
                                                   |> FileCache.Download )
                              |> Async.Parallel |> Async.Ignore }

        member this.GetCountryCodeFromName name = async {
            let! countries = this.GetCountries() 
            match countries |> Array.tryFind (fun x-> x.Name = name) with
            | Some(country) -> return country.Code
            | None -> return "" }

        member this.GetCountryNameFromCode code = async {
            let! countries = this.GetCountries() 
            match countries |> Array.tryFind (fun x-> x.Code = code) with
            | Some(country) -> return country.Name
            | None -> return "" }
            
        member this.GetStates (country:String) =
            if country.ToLower() = "united states" then
                [ "Alabama"
                  "Alaska" 
                  "Arizona"
                  "Arkansas"
                  "California"
                  "Colorado"
                  "Connecticut"
                  "Delaware"
                  "District of Columbia"
                  "Florida"
                  "Georgia"
                  "Hawaii"
                  "Idaho"
                  "Illinois"
                  "Indiana"
                  "Iowa"
                  "Kansas"
                  "Kentucky"
                  "Louisiana"
                  "Maine"
                  "Maryland"
                  "Massachusetts"
                  "Michigan"
                  "Minnesota"
                  "Mississippi"
                  "Missouri"
                  "Montana"
                  "Nebraska"
                  "Nevada"
                  "New Hampshire"
                  "New Jersey"
                  "New Mexico"
                  "New York"
                  "North Carolina"
                  "North Dakota"
                  "Ohio"
                  "Oklahoma"
                  "Oregon"
                  "Pennsylvania"
                  "Rhode Island"
                  "South Carolina"
                  "South Dakota"
                  "Tennessee"
                  "Texas"
                  "Utah"
                  "Vermont"
                  "Virginia"
                  "Washington"
                  "West Virginia"
                  "Wisconsin"
                  "Wyoming" ]
            else
                []
        member this.PlaceOrder(user, ?verify) = async {
            try 
                
                return! System.Threading.Tasks.Task.Run(fun _->
                    let verify = if Some(true) = verify then true else false
                    let json = { CurrentOrder with Products = CurrentOrder.Products |> List.map (fun x->x.CreateCopy()) }.GetJson user
                    let content = encoding.GetBytes json
                    let request = CreateRequest ("fsharporder" + if verify then "?verify=1" else "")
                    request.Method <- "POST"
                    request.ContentLength <- int64 content.Length
                     
                    use s = request.GetRequestStream()
                    s.Write(content, 0, content.Length)

                    let response = ReadResponseText request
                    let result = JsonConvert.DeserializeObject<OrderResult> response

                    if (not verify) && result.Success then
                        CurrentOrder <- Order.CreateOrder()

                    result ) |> Async.AwaitTask
            with | e-> return { Success=false; OrderNumber=""; Message=e.Message } }

        member this.ValidateUser user = async {
            if      user.FirstName = "" then return Failure "First name is required"
            else if user.LastName  = "" then return Failure "Last name is required"
            else if user.Phone     = "" then return Failure "Phone number is required"
            else if user.Address   = "" then return Failure "Address is required"
            else if user.City      = "" then return Failure "City is required"
            else if user.Country   = "" then return Failure "Country is required"
            else if user.ZipCode   = "" then return Failure "Zip Code is required"
            else if user.Country.ToLower () = "usa" then 
                let! countryCode = this.GetCountryNameFromCode user.Country
                let states = this.GetStates countryCode
                if states |> List.exists ((=) user.State) then
                    return Success "User is valid"
                else return Failure ("\"" + user.State + "\"" +  " is not a valid state")
            else return Success "User is valid" }
                
    let Shared = new WebService()