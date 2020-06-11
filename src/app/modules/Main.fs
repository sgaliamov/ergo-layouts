module Main

open System
open System.IO
open System.Collections.Concurrent
open System.Collections.Generic
open System.Text
open Calculations
open System.Threading

type StringBuilder with
    member sb.appendLine (key, value, total) =
        let div x y =
            match y with
            | 0 -> 0.0
            | _ -> (float x) / (float y)
        let getValue value total = 100.0 * div value total
        sb.AppendFormat("{0} : {1:0.####}\n", key, (getValue value total))

    member sb.appendLines<'T> (pairs: seq<KeyValuePair<'T, int>>) total =
        pairs
        |> Seq.sortByDescending (fun pair -> pair.Value)
        |> Seq.fold (fun (sb: StringBuilder) pair -> sb.appendLine(pair.Key, pair.Value, total)) sb

    member sb.Append (pairs, total) = sb.appendLines pairs total

let private print (digraphs: ConcurrentDictionary<string, int>, letters: ConcurrentDictionary<char, int>, allChars: ConcurrentDictionary<char, int>) =
    let totalLetters = Seq.sum letters.Values
    let totalChars = Seq.sum allChars.Values
    let symbolsOnly = allChars |> Seq.filter (fun x -> Char.IsPunctuation(x.Key))
    let totalDigraphs = Seq.sum digraphs.Values
    StringBuilder()
        .AppendFormat("Letters: {0}\n", totalLetters)
        .Append(letters, totalLetters)
        .AppendFormat("\nSymbols from total: {0}\n", totalChars)
        .Append(symbolsOnly, totalChars)
        .AppendFormat("\nDigraphs {0}:\n", totalDigraphs)
        .Append(digraphs, totalDigraphs)
    |> Console.WriteLine

let calculate path search (token: CancellationToken) = async {
    let seed = (ConcurrentDictionary<string, int>(), ConcurrentDictionary<char, int>(), ConcurrentDictionary<char, int>())
    Directory.EnumerateFiles(path, search, SearchOption.AllDirectories)
    |> Seq.filter (fun _ -> not token.IsCancellationRequested)
    |> Seq.map (fun filePath -> calculateFile filePath token) // todo: can run in pararllel
    |> Seq.fold aggregator seed
    |> print }