module Main

open System
open System.IO
open System.Threading
open System.Collections.Concurrent
open Utilities

    module private Private =
        let toString (a, b) = String([| a; b |])
        
        let countLetters line =
            line 
            |> Seq.countBy id

        let countDigraphs line =
            line 
            |> Seq.pairwise
            |> Seq.map toString
            |> Seq.countBy id

        let yieldLines (stream: StreamReader) (token: CancellationToken) = seq {
            while not stream.EndOfStream && not token.IsCancellationRequested do
                yield stream.ReadLine() } // todo: use async

        let collect line =
            let folder result (key, count) = addOrUpdate result key count (+)
            let letters =
                line
                |> countLetters
                |> Seq.fold folder (ConcurrentDictionary<char, int>())
            let digraphs =
                line
                |> countDigraphs
                |> Seq.fold folder (ConcurrentDictionary<string, int>())
            (digraphs, letters)

        let aggregator (digraphs, letters) (fromDigraphs, fromLetters) =
            let resultDigraphs = sumValues fromDigraphs digraphs
            let resultLetters = sumValues fromLetters letters
            (resultDigraphs, resultLetters)

        let calculateFile filePath token =
            use stream = File.OpenText filePath
            let seed = (ConcurrentDictionary<string, int>(), ConcurrentDictionary<char, int>())
            yieldLines stream token
            |> Seq.map collect
            |> Seq.fold aggregator seed

        let div x y =
            match y with
            | 0 -> 0.0
            | _ -> (float x) / (float y)

        let print (digraphs: ConcurrentDictionary<string, int>, letters: ConcurrentDictionary<char, int>) =
            let total = Seq.sum letters.Values
            let printDigraphs key value total = printfn "%s: %.3f" key (div value total)
            let printLetters key value total = printfn "%c: %.3f" key (div value total)
            printfn "Letters:"
            letters
            |> Seq.iter (fun pair -> printLetters pair.Key pair.Value total)
            printfn "Digraphs:"
            digraphs
            |> Seq.iter (fun pair -> printDigraphs pair.Key pair.Value total)

    open Private

    let calculate path search token = async {
        let seed = (ConcurrentDictionary<string, int>(), ConcurrentDictionary<char, int>())
        Directory.EnumerateFiles(path, search, SearchOption.AllDirectories)
        |> Seq.map calculateFile // todo: can run in pararllel
        |> Seq.map (fun m -> m token)
        |> Seq.fold aggregator seed
        |> print }