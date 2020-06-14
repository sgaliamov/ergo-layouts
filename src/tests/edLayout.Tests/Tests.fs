module Tests

open System.Linq
open System
open Xunit
open Configs

open FSharp.Data
open FSharp.Data.JsonExtensions

type private Stats = JsonProvider<"./statistics.json">

let statistics = Stats.GetSample()

[<Fact>]
let ``My test`` () =
    let a = statistics.Digraphs.JsonValue.Properties
    let b = a.ToDictionary(new Func<(string*JsonValue),string>(fun (a,b) -> a))
    let c = a.Select(fun (a, b: JsonValue) -> (a, b.AsFloat())).ToArray()
    Assert.True(true)
