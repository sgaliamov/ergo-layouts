﻿module Configs

open FSharp.Data
open FSharp.Data.JsonExtensions
open Utilities
open Models

type private Stats = JsonProvider<"./data/statistics.json">
type private Settings = JsonProvider<"./data/settings.json">
let private statistics = Stats.GetSample()
let private appSettings = Settings.GetSample()

let private jsonToMap json =
    json
    |> jsonValueToPairs JsonExtensions.AsFloat
    |> Seq.map (fun (key, value) -> (key, Probability.create value))
    |> Map.ofSeq

let private digraphsStatistics =
    statistics.Digraphs.JsonValue.Properties
    |> jsonToMap

let private lettersStatistics =
    statistics.Letters.JsonValue.Properties
    |> jsonToMap

let settings =
    {| precision = Probability.create (float appSettings.Precision)
       digraphs = digraphsStatistics
       columns = appSettings.Columns
       letters = lettersStatistics
       minDigraphs = float appSettings.MinDigraphs / 100. |}
