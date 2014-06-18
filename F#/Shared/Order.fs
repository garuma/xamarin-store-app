module Order
    open User
    open Product
    open System.Linq
    open Helpers

    open Newtonsoft.Json

    open System.Runtime.Serialization
        
    [<DataContract;CLIMutable>]
    [<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type Order = 
        { Products: Product list
          SsoToken : string
          ShippingName : string
          Email : string
          Address1 : string
          Address2 : string
          City : string
          State : string
          ZipCode : string
          Phone : string
          Country : string
          [<IgnoreDataMemberAttribute>]
          Notification: (Order->unit) option }

        //Initializes an order with all unspecified fields set to empty string
        static member CreateOrder(?Products, ?SsoToken, ?ShippingName, ?Email, ?Address1, ?Address2, ?City, ?State, ?ZipCode, ?Phone, ?Country, ?Notification) = 
            let initMember x = fun y-> Option.fold (fun state param->param) y x
            { Products = initMember Products []
              SsoToken = initMember SsoToken ""
              ShippingName = initMember ShippingName ""
              Email = initMember Email ""
              Address1 = initMember Address1 ""
              Address2 = initMember Address2 ""
              City = initMember City ""
              State = initMember State ""
              ZipCode = initMember ZipCode ""
              Phone = initMember Phone ""
              Country = initMember Country ""
              Notification = Notification}

        member this.SendNotification () =
            match this.Notification with
            | Some(notification)->notification(this)
            | None -> ()

        member this.GetJson (user:User) =
            { this with
                ShippingName = sprintf "%s %s" user.FirstName user.LastName
                SsoToken = user.Token
                Email = user.Email
                Address1 = user.Address
                Address2 = user.Address2
                City = user.City
                State = user.State
                ZipCode = user.ZipCode
                Phone = user.Phone
                Country = user.Country }
            |> JsonConvert.SerializeObject

        member this.WithProduct product =
            let temp = { this with Products = product::this.Products }
            temp.SendNotification()
            temp

        member this.WithoutProduct product =
            if this.Products.Contains(product) then
                let newOrder = { this with Products = this.Products 
                                                      |> List.filter (fun x->(not (x = product))) }
                newOrder.SendNotification()
                newOrder
            else
                this
                
    [<DataContract;CLIMutable>]
    [<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type OrderResult =
        { Success: bool
          OrderNumber:string
          Message:string }