module Utilities

open System
open System.Collections.Concurrent
open System.Collections.Generic
open FSharp.Data

let addOrUpdate<'T1, 'T2> (dict: ConcurrentDictionary<'T1, 'T2>) key value onUpdate =
    dict.AddOrUpdate (
        key,
        new Func<'T1, 'T2, 'T2>(fun _ value -> value),
        new Func<'T1, 'T2, 'T2, 'T2>(fun _ acc value -> onUpdate acc value),
        value)
        |> ignore
    dict

let sumValues<'T> source destination =
    let folder acc (pair: KeyValuePair<'T, int>) = addOrUpdate acc pair.Key pair.Value (+)
    source
    |> Seq.fold folder destination

let toString (a, b) = String([| a; b |])

let jsonValueToMap jsonValues =
    jsonValues
    |> Seq.map (fun (a: string, b: JsonValue) -> (a.ToString(), b.AsFloat()))
    |> Map.ofSeq