module Main

open System.IO
open System.Threading

let private calculateLine line =
    Thread.Sleep 100
    printfn "%s" line

let private calculateFile filePath (token :CancellationToken) =
    use stream = File.OpenText filePath
    while not stream.EndOfStream && not token.IsCancellationRequested do
        stream.ReadLine() |> calculateLine // todo: use async version

let calculate path token =
    Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
    |> Seq.map calculateFile // can run in pararllel
    |> Seq.iter (fun m -> m token)
