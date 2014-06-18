module UserModels

    open Newtonsoft.Json
    open System.Runtime.Serialization
    
    [<DataContract;CLIMutable>]
    [<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type User = 
        { Email : string 
          FirstName : string
          LastName : string }
        static member CreateUser(?Email, ?FirstName, ?LastName) = 
            let initMember x ``default`` = x |> Option.fold (fun state param->param) ``default``
            { Email = initMember Email ""
              FirstName = initMember FirstName ""
              LastName = initMember LastName "" }
        
    [<DataContract;CLIMutable>]
    [<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
    type AccountResponse =
        {
            [<DataMember(Name="success")>]
            Success : bool
            [<DataMember(Name="error")>]
            Error : string
            [<DataMember(Name="user")>]
            User : User
            [<DataMember(Name="token")>]
            Token : string
        }
        static member CreateAccountResponse(?Success, ?Error, ?UserParam, ?Token) = 
            let initMember x ``default`` = x |> Option.fold (fun state param->param) ``default``
            { Success = initMember Success false
              Error = initMember Error ""
              User = initMember UserParam (User.CreateUser())
              Token = initMember Token ""}