module Configs

open System
open System.IO
open FSharp.Data
open FSharp.Data.JsonExtensions
open Utilities

type private Config = JsonProvider<"./data/config.json">
type private Stats = JsonProvider<"./data/statistics.json">
type private Settings = JsonProvider<"./data/settings.json">

let private config = Config.GetSample()
let private statistics = Stats.GetSample()
let private appSettings = Settings.GetSample()

let private digraphsStatistics =
    statistics.Digraphs.JsonValue.Properties
    |> jsonValueToMap

let private lettersStatistics =
    statistics.Letters.JsonValue.Properties
    |> jsonValueToMap

type Probability = Probability of float

let createProbability value =
    if value < 0. || value > 100.
    then raise (ArgumentOutOfRangeException("value"))
    value / 100.

let settings =
    {| precision = createProbability (float appSettings.Precision)
       digraphs = digraphsStatistics
       columns = appSettings.Columns
       letters = lettersStatistics |}

let loadLayout path = File.ReadAllText path
