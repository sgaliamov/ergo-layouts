module Configs

open FSharp.Data
open System.IO

type private Config = JsonProvider<"./data/config.json">

let config = Config.GetSample()

let private loadCsv path =
    File.ReadAllText path