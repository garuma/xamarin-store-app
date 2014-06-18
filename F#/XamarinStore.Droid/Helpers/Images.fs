namespace XamarinStore

open System.Collections.Generic
open System.Threading.Tasks
open Android.Graphics
open Android.Widget
open Helpers

type IBitmapHolder =
    abstract member SetImageBitmap: Bitmap->unit

module Images =
    let mutable ScreenWidth = 320.0f
    let bmpCache = new Dictionary<string, Bitmap> ()

    let FromUrl url : Async<Bitmap> = async {
        match bmpCache.TryGetValue url with
        | true, bmp -> return bmp
        | _ -> let! path = FileCache.Download(url)
               match path with
               | "" -> return null
               | path -> let! bmp = BitmapFactory.DecodeFileAsync (path) |> Async.AwaitTask
                         bmpCache.[url] <- bmp
                         return bmp }

    type Android.Widget.ImageView with
        member this.SetImageFromUrlAsync url = async {
            try
                let! bmp = FromUrl url
                this.SetImageBitmap bmp
            with e->()}
    
    let SetBitmapImageFromUrlAsync (imageView:IBitmapHolder) url = async {
        try
            let! bmp = FromUrl url
            imageView.SetImageBitmap bmp
        with e->()}
    
    let SetImageViewFromUrlAsync (imageView:ImageView) url = async {
        try
            let! bmp = FromUrl url
            imageView.SetImageBitmap bmp
        with e->()}
