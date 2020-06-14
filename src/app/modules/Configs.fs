module Configs

open FSharp.Data
open FSharp.Data.JsonExtensions
open System.IO
open Calculations
open System
open System.Linq

type private Config = JsonProvider<"./data/config.json">
type private Stats = JsonProvider<"./data/statistics.json">

let config = Config.GetSample()
let statistics = Stats.GetSample()

//let map =
//    statistics.JsonValue
//        .GetProperty("digraphs")
//        .AsArray()
//    |> Seq.map (fun x -> x.as)


//for i in map do  
//    i.



//let isEnough (state: State) set =
//    state.digraphs.Keys
//    |> Seq.map (fun key ->
//        let delta = int statistics.Precision * state.totalDigraphs
//        let goal = statistics.Digraphs..[key] * state.totalDigraphs
//        let current = state.digraphs.[key]
//        Math.Abs(goal - current)
//    )

let loadLayout path =
    File.ReadAllText path