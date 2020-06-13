module Main

open System
open System.IO
open System.Collections.Generic
open System.Text
open Calculations
open System.Threading

type StringBuilder with
    member sb.appendPair (key, value, total) =
        let div x y =
            match y with
            | 0 -> 0.0
            | _ -> (float x) / (float y)
        let getValue value total = 100.0 * div value total
        sb.AppendFormat("{0} : {1:0.####}\t", key, (getValue value total))

    member sb.appendLines<'T> (pairs: seq<KeyValuePair<'T, int>>) total =
        pairs
        |> Seq.sortByDescending (fun pair -> pair.Value)
        |> Seq.fold (fun (sb: StringBuilder) pair -> sb.appendPair(pair.Key, pair.Value, total)) sb

    member sb.Append (pairs, total) = sb.appendLines pairs total

let yieldLines filePath (token: CancellationToken) = seq {
    use stream = File.OpenText filePath
    while not stream.EndOfStream && not token.IsCancellationRequested do
        yield stream
            .ReadLine()
            .ToLowerInvariant() } // todo: use async

let private print state =
    let symbolsOnly = state.chars |> Seq.filter (fun x -> Char.IsPunctuation(x.Key))
    StringBuilder()
        .AppendFormat("Letters: {0}\n", state.totalLetters)
        .Append(state.letters, state.totalLetters)
        .AppendFormat("\nSymbols from total: {0}\n", state.totalChars)
        .Append(symbolsOnly, state.totalChars)
        .AppendFormat("\nDigraphs {0}:\n", state.totalDigraphs)
        .Append(state.digraphs, state.totalDigraphs)
    |> Console.WriteLine

let calculate path search (token: CancellationToken) = async {
    let seed = {
        letters = Letters()
        digraphs = Digraphs()
        chars = Chars()
        totalLetters = 0
        totalDigraphs = 0
        totalChars = 0 }
    Directory.EnumerateFiles(path, search, SearchOption.AllDirectories)
    |> Seq.filter (fun _ -> not token.IsCancellationRequested)
    |> Seq.map (fun filePath -> yieldLines filePath token)
    |> Seq.map calculateLines // todo: can run in pararllel
    |> Seq.fold aggregator seed
    |> print }