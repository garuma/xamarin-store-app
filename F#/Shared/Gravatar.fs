module Gravatar

open System
open Helpers

#if __IOS__
open MonoTouch.Foundation
#endif

type Rating =
| G | PG | R | X

let private ratingToString = function
    | G -> "g"
    | PG -> "pg"
    | R -> "r"
    | X -> "x"

let private url = "http://www.gravatar.com/avatar.php?gravatar_id="

let private MD5Hash (input : string) =
    System.Security.Cryptography.MD5.Create().ComputeHash (System.Text.Encoding.Default.GetBytes (input))
    |> Array.map (fun x->x.ToString("x2"))
    |> String.concat ""

let private GetUrl (email:string) size rating =
    let hash = MD5Hash (email.ToLower ())

    if size < 1 || size > 600 
    then Failure "The image size should be between 20 and 80"
    else Success (url + hash + "&s=" + size.ToString () + "&r=" + (ratingToString rating))
        
let GetImageBytes email size rating = async {
    match GetUrl email size rating with
    | Failure message -> return [||]
    | Success url -> let client = new System.Net.WebClient ()
                     return! (client.DownloadDataTaskAsync url |> Async.AwaitTask) }

#if __IOS__
let GetImageData email size rating = async {
    let! data = GetImageBytes email size rating
    return NSData.FromStream(new System.IO.MemoryStream(data)) }
#endif