﻿module Configs

open System.Linq
open FSharp.Data
open FSharp.Data.JsonExtensions
open Utilities
open StateModels

type private Stats = JsonProvider<"./statistics.json">
type private Settings = JsonProvider<"./settings.json">
let private statistics = Stats.GetSample()
let private appSettings = Settings.GetSample()

let private jsonToMap mapKey json =
    json
    |> toPairs mapKey (JsonExtensions.AsFloat >> Probability.create)
    |> Map.ofSeq

let private digraphsStatistics =
    statistics.Digraphs.JsonValue.Properties
    |> jsonToMap Digraph.create

let private lettersStatistics =
    statistics.Letters.JsonValue.Properties
    |> jsonToMap (fun x -> Letter.create(x.Single()))

let settings =
    {| precision = Probability.create (float appSettings.Precision)
       digraphs = digraphsStatistics
       columns = appSettings.Columns
       letters = lettersStatistics
       handSwitchPenalty = float appSettings.HandSwitchPenalty
       doublePressPenalty = float appSettings.DoublePressPenalty
       sameFingerPenalty = float appSettings.SameFingerPenalty
       minDigraphs = float appSettings.MinDigraphs |}
