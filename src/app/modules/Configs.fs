module Configs

open FSharp.Data
open System.IO

type private Config = JsonProvider<"./data/config.json">
type private Stats = JsonProvider<"./data/statistics.json">

let config = Config.GetSample()
let statistics = Stats.GetSample()

let loadLayout path =
    File.ReadAllText path