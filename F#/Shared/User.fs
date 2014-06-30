module User

    open System
    open System.Runtime.Serialization
    open Newtonsoft.Json

    [<DataContract;CLIMutable>]
    [<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type User = 
        { FirstName: string
          LastName: string
          Token: string
          Email: string
          Address : string
          Address2 : string
          City: string
          State: string
          ZipCode: string
          Phone: string
          Country: string }

        //Initializes a user with all unspecified fields set to empty string
        static member CreateUser(?FirstName, ?LastName, ?Token, ?Email, ?Address, ?Address2, ?City, ?State, ?ZipCode, ?Phone, ?Country) = 
            let initMember x = x |> Option.fold (fun state param->param) ""
            { FirstName = initMember FirstName
              LastName = initMember LastName
              Token = initMember Token
              Email = initMember Email
              Address = initMember Address
              Address2 = initMember Address2
              City = initMember City
              State = initMember State
              ZipCode = initMember ZipCode
              Phone = initMember Phone
              Country = initMember Country }