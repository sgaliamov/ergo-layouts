module Utilities

open System
open System.Collections.Concurrent
open System.Collections.Generic
open FSharp.Data

let addOrUpdate (dict: ConcurrentDictionary<'T1, 'T2>) key value onUpdate =
    dict.AddOrUpdate(key, (fun _ v -> v), (fun _ acc v -> onUpdate acc v), value)
    |> ignore
    dict

let sumValues source (seed: ConcurrentDictionary<'TKey, 'TValue>) add =
    let folder acc (pair: KeyValuePair<'TKey, 'TValue>) = addOrUpdate acc pair.Key pair.Value add
    source
    |> Seq.fold folder (ConcurrentDictionary<'TKey, 'TValue>(seed))

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

let filterValuebleKeys seq =
     seq
     |> Seq.filter (fun (key: 'TKey option, _: 'TValue) -> key.IsSome)
     |> Seq.map (fun (key, value) -> key.Value, value)

let filterValuebles seq =
    seq
    |> Seq.filter (fun (key: 'TKey option, value: 'TValue option) -> key.IsSome && value.IsSome)
    |> Seq.map (fun (key, value) -> key.Value, value.Value)

let flipTuple (a, b) = (b, a)
