module Tests

open System.Linq
open System
open Xunit
open Configs

open FSharp.Data
open FSharp.Data.JsonExtensions

type private Stats = JsonProvider<"./statistics.json">

let statistics = Stats.GetSample()

let get = seq {
    for (a: string, b: JsonValue)  in statistics.Digraphs.JsonValue.Properties do
        yield (a.ToString(), b.AsFloat())
}


[<Fact>]
let ``My test`` () =
    let digraphsMap =  get|> Map.ofSeq
    let r = digraphsMap.["th"]
    Assert.True(true)
