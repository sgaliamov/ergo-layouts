module Main

open System.IO
open System.Threading

let private streamFile path =
    let stream = File.OpenText path
    seq {
        while not stream.EndOfStream do yield stream.Read()
    }

let private iterateFiles path =
    seq { yield! Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories) }

let private calculateFile (filePath: string) (token: CancellationToken): string =
    ""

let private isCancelled (token: CancellationToken) = token.IsCancellationRequested

let calculate path (token: CancellationToken) =
    let files = 
        iterateFiles path
        |> Seq.where (fun _ -> isCancelled token)
        |> Seq.map calculateFile
    for m in files do
        m token
    ()
