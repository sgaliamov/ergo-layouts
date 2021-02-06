module Configs

open FSharp.Data

type private Settings = JsonProvider<"./settings.json">
let private appSettings = Settings.GetSample()

let settings =
    {| columns = appSettings.Columns
       handSwitchPenalty = float appSettings.HandSwitchPenalty
       sameColumnPenalty = float appSettings.SameColumnPenalty
       minDigraphs = float appSettings.MinDigraphs |}
