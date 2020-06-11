module Calculations

open System
open System.IO
open System.Threading
open System.Collections.Concurrent

open Utilities
open System.Collections.Generic

let toString (a, b) = String([| a; b |])

type Counter<'TIn, 'TOut> = seq<'TIn> -> seq<'TOut * int>

let countLetters line =
    line 
    |> Seq.filter Char.IsLetter 
    |> Seq.countBy id

let countChars line = line |> Seq.countBy id

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

let calculate<'TIn, 'TOut> line (counter: Counter<'TIn, 'TOut>) =
    let folder result (key, count) = addOrUpdate result key count (+)
    line
    |> counter
    |> Seq.fold folder (ConcurrentDictionary<'TOut, int>())

let collect line =
    let letters = calculate line countLetters
    let chars = calculate line countChars
    let digraphs = calculate line countDigraphs
    (digraphs, letters, chars)

let aggregator (digraphs, letters, chars) (fromDigraphs, fromLetters, fromChars) =
    let resultDigraphs = sumValues fromDigraphs digraphs
    let resultLetters = sumValues fromLetters letters
    let resultChars = sumValues fromChars chars
    (resultDigraphs, resultLetters, resultChars)

let characters = HashSet<char>([' '..'~'] |> List.except ['A'..'Z'])

let calculateFile filePath token =
    use stream = File.OpenText filePath
    let seed = (ConcurrentDictionary<string, int>(), ConcurrentDictionary<char, int>(), ConcurrentDictionary<char, int>())
    yieldLines stream token
    |> Seq.map (fun line -> line |> Seq.filter characters.Contains) 
    |> Seq.map collect
    |> Seq.fold aggregator seed