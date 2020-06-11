module Main

open System.IO
open System.Collections.Concurrent
open Calculations

let private print (digraphs: ConcurrentDictionary<string, int>, letters: ConcurrentDictionary<char, int>) =
    let total = Seq.sum letters.Values
    let div x y =
        match y with
        | 0 -> 0.0
        | _ -> (float x) / (float y)
    let getValue value total = 100.0 * div value total
    printfn "Total: %d\n" total
            
    printfn "Letters:"
    let printLetters key value = printfn "%c: %.3f" key value
    letters
    |> Seq.sortByDescending (fun pair -> pair.Value)
    |> Seq.iter (fun pair -> printLetters pair.Key (getValue pair.Value total))
            
    printfn "\nDigraphs:"
    let printDigraphs key value = printfn "%s: %.3f" key value
    digraphs
    |> Seq.sortByDescending (fun pair -> pair.Value)
    |> Seq.iter (fun pair -> printDigraphs pair.Key (getValue pair.Value total))

let calculate path search token = async {
    let seed = (ConcurrentDictionary<string, int>(), ConcurrentDictionary<char, int>())
    Directory.EnumerateFiles(path, search, SearchOption.AllDirectories)
    |> Seq.map calculateFile // todo: can run in pararllel
    |> Seq.map (fun m -> m token)
    |> Seq.fold aggregator seed
    |> print }