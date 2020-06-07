module MainModule

open System
open System.Threading
open System.IO
open FSharp.Data

    type private Config = JsonProvider<"./data/config.json">
    let private config = Config.GetSample()

    let private streamFile path =
        let stream = File.OpenText path
        seq {
            while not stream.EndOfStream do yield stream.Read()
        }

    let private loadCsv path =
        File.ReadAllText path

    let private processFolder path =
        Directory.GetFiles path
        ()

    let processFolders path token =
        Directory.GetDirectories path
        |> Array.iter processFolder
