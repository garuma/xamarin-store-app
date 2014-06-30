#r "System.Runtime.Serialization.dll"
#r "Newtonsoft.Json"
#r "System.Web.Services"
#r "System.Xml"

#load "User.fs"
#load "Helpers.fs"
#load "Product.fs"
#load "Order.fs"
#load "UserModels.fs"
#load "XamarinSSOClient.fs"
#load "FileCache.fs"
#load "WebService.fs"

open User
open Newtonsoft.Json

open WebService
open Helpers
open Order
open User
open FileCache
open Product

WebService.Shared.GetProducts() |> Async.RunSynchronously

let product = Product.CreateProduct()
let productRef = ref product