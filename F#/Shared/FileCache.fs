module FileCache

    open System.IO
    open Helpers

    let mutable saveLocation = ""
    let downloadTasks = new System.Collections.Generic.Dictionary<string, System.Threading.Tasks.Task> ()
    let locker = System.Object ()

    let hex v = if v < 10 
                then (int '0') + v
                else (int 'a') + (v-10)

    let md5 (input:string) =
        let bytes = System.Text.Encoding.UTF8.GetBytes input
                    |> (new System.Security.Cryptography.MD5CryptoServiceProvider ()).ComputeHash 
                    |> Array.map int
        Array.init 32 (fun i->if i % 2 = 0 
                              then char (hex (bytes.[i/2] >>> 4))
                              else char (hex (bytes.[i/2] &&& 0xf)))
        |> (fun x->new string(x))

    let GetDownload (url:string) fileName =
        lock locker (fun ()-> match downloadTasks.TryGetValue fileName with
                               | true, task -> task
                               | _          -> let task = (new System.Net.WebClient()).DownloadFileTaskAsync (url, fileName)
                                               downloadTasks.[fileName] <- task
                                               task)
        

    let DownloadToLocation url filename = async {
        try
            let path = Path.Combine(saveLocation, filename)
            if File.Exists path then
                return path
            else                
                do! GetDownload url path |> Async.awaitPlainTask
                return path
        with | e -> System.Console.WriteLine e
                    return "" }

    let Download url = async {
        match saveLocation with
        | "" -> return failwith "Save Location is required"
        | _ -> return! DownloadToLocation url (md5 url) }

    let RemoveTask fileName =
        lock locker (fun ()->downloadTasks.Remove fileName)