module Main

open System
open System.IO
open System.Collections.Concurrent
open System.Collections.Generic
open System.Text
open Calculations

let private div x y =
    match y with
    | 0 -> 0.0
    | _ -> (float x) / (float y)

let private getValue value total = 100.0 * div value total

let private appendLine (sb: StringBuilder, key, value, total) = 
    sb.AppendFormat("{0} : {1:0.###}\n", key, (getValue value total))
    |> ignore

let private appendLines<'T> (sb: StringBuilder) (pairs: seq<KeyValuePair<'T, int>>) total =
    pairs
    |> Seq.sortByDescending (fun pair -> pair.Value)
    |> Seq.map (fun pair -> (sb, pair.Key, pair.Value, total))
    |> Seq.iter appendLine

let private print (digraphs: ConcurrentDictionary<string, int>, letters: ConcurrentDictionary<char, int>) =
    let totalLetters = Seq.sum letters.Values
    let totalDigraphs = Seq.sum digraphs.Values
    let sb = StringBuilder().AppendFormat("Letters: {0}\n", totalLetters)

    appendLines sb letters totalLetters
    sb.AppendFormat("\nDigraphs {0}:\n", totalDigraphs) |> ignore
    appendLines sb digraphs totalDigraphs

    Console.WriteLine(sb)

let calculate path search token = async {
    let seed = (ConcurrentDictionary<string, int>(), ConcurrentDictionary<char, int>())
    Directory.EnumerateFiles(path, search, SearchOption.AllDirectories)
    |> Seq.map calculateFile // todo: can run in pararllel
    |> Seq.map (fun m -> m token)
    |> Seq.fold aggregator seed
    |> print }