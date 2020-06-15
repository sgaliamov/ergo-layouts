﻿module Utilities

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

let jsonValueToPairs jsonValues =
    jsonValues
    |> Seq.map (fun (a: string, b: JsonValue) -> a.ToString(), b.AsFloat())
