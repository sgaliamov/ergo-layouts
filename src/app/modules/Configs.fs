module Configs

open FSharp.Data
open FSharp.Data.JsonExtensions
open System.IO
open System.Linq
open Calculations
open System
open System.Collections.Concurrent

type private Config = JsonProvider<"./data/config.json">
type private Stats = JsonProvider<"./data/statistics.json">

let config = Config.GetSample()
let statistics = Stats.GetSample()

let jsonValueToMap jsonValues =
    jsonValues
    |> Seq.map (fun (a: string, b: JsonValue) -> (a.ToString(), b.AsFloat()))
    |> Map.ofSeq

let digraphsStatistics = 
    statistics.Digraphs.JsonValue.Properties
    |> jsonValueToMap

let lettersStatistics =
    statistics.Letters.JsonValue.Properties
    |> jsonValueToMap

let check state stats total precision =
    let delta = int(precision * float total)
    let goal = int(stats * float total)
    let current = state
    current + delta >= goal

let isEnough (state: State) set =
    state.digraphs.Keys
    |> Seq.map (fun key ->
        let stats = digraphsStatistics.[key]
        let current = state.digraphs.[key]
        (key, check current stats state.totalDigraphs (float statistics.Precision))
    )

let loadLayout path =
    File.ReadAllText path