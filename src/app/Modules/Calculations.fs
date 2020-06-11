module Calculations

open System
open System.IO
open System.Threading
open System.Collections.Concurrent

open Utilities
open System.Collections.Generic

let toString (a, b) = String([| a; b |])
       
let countLetters line =
    line 
    |> Seq.countBy id

let countDigraphs line =
    line
    |> Seq.filter Char.IsLetter
    |> Seq.pairwise
    |> Seq.map toString
    |> Seq.countBy id

let yieldLines (stream: StreamReader) (token: CancellationToken) = seq {
    while not stream.EndOfStream && not token.IsCancellationRequested do
        yield stream
            .ReadLine()
            .ToLowerInvariant() } // todo: use async

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

let set = HashSet<char>([' '..'~'])
set.ExceptWith(['A'..'Z'])

let calculateFile filePath token =
    use stream = File.OpenText filePath
    let seed = (ConcurrentDictionary<string, int>(), ConcurrentDictionary<char, int>())
    yieldLines stream token
    |> Seq.map (fun line -> line |> Seq.filter set.Contains) 
    |> Seq.map collect
    |> Seq.fold aggregator seed