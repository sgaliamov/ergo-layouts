module Utilities

open System
open System.Collections.Concurrent
open System.Collections.Generic
open FSharp.Data

let addOrUpdate<'T1, 'T2> (dict: ConcurrentDictionary<'T1, 'T2>) key value onUpdate =
    dict.AddOrUpdate(key, (fun _ v -> v), (fun _ acc v -> onUpdate acc v), value)
    |> ignore
    dict

let sumValues<'T> source (seed: ConcurrentDictionary<'T, int>) =
    let folder acc (pair: KeyValuePair<'T, int>) = addOrUpdate acc pair.Key pair.Value (+)
    source
    |> Seq.fold folder (ConcurrentDictionary<'T, int>(seed))

let toString (a, b) = String ([| a; b |])

let toPairs mapKey mapValue jsonValues =
    jsonValues
    |> Seq.map (fun (a: string, b: JsonValue) -> mapKey a, mapValue b)           

let tryParseDouble (value: string) =
    match Double.TryParse value with
    | true, out -> Some out
    | false, _ -> None

let (|HeadTail|_|) (string: string) =
    match string.Length with
    | 0 -> None
    | _ -> Some(string.[0], string.Substring(1))

let filterValuebleKeys<'TKey, 'TValue> seq =
     seq
     |> Seq.filter (fun (key: 'TKey option, _: 'TValue) -> key.IsSome)
     |> Seq.map (fun (key, value) -> key.Value, value)

let filterValuebles<'TKey, 'TValue> seq =
    seq
    |> Seq.filter (fun (key: 'TKey option, value: 'TValue option) -> key.IsSome && value.IsSome)
    |> Seq.map (fun (key, value) -> key.Value, value.Value)

let flipTuple (a, b) = (b, a)
