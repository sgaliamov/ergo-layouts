module Configs

open FSharp.Data
open FSharp.Data.JsonExtensions
open System.IO
open System.Linq
open Calculations

type private Config = JsonProvider<"./data/config.json">
type private Stats = JsonProvider<"./data/statistics.json">

let config = Config.GetSample()
let statistics = Stats.GetSample()

let digraphsMap =
    statistics.Digraphs
        .JsonValue.Properties
        .Select(fun (a: string, b: JsonValue) -> (a, b.AsFloat()))
        .ToDictionary(fun (a: string, _: float) -> a, fun (_: string, b: float) -> b)

let lettersMap =
    statistics.Letters
        .JsonValue.Properties
        .ToDictionary(fun (a: string, _: JsonValue) -> a, fun (_: string, b: JsonValue) -> b.AsFloat())

//let isEnough (state: State) set =
//    state.digraphs.Keys
//    |> Seq.map (fun key ->
//        let delta = int statistics.Precision * state.totalDigraphs
//        let goal = digraphsMap.[key] * state.totalDigraphs
//        let current = state.digraphs.[key]
//        Math.Abs(goal - current)
//    )

let loadLayout path =
    File.ReadAllText path