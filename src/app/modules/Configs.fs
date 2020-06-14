module Configs

open System.IO
open FSharp.Data
open FSharp.Data.JsonExtensions
open Utilities

type private Config = JsonProvider<"./data/config.json">
type private Stats = JsonProvider<"./data/statistics.json">
type private Settings = JsonProvider<"./data/settings.json">

let private config = Config.GetSample()
let private statistics = Stats.GetSample()

let private digraphsStatistics = 
    statistics.Digraphs.JsonValue.Properties
    |> jsonValueToMap

let private lettersStatistics =
    statistics.Letters.JsonValue.Properties
    |> jsonValueToMap

let stats = {|
    precision = float statistics.Precision;
    digraphs = digraphsStatistics;
    letters = lettersStatistics |}

let loadLayout path = File.ReadAllText path

let settings = Settings.GetSample()
