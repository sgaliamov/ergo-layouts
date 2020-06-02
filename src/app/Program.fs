open System
open System.IO
open FSharp.Data
open System.CommandLine
open System.CommandLine.Invocation

type Config = JsonProvider<"./data/config.json">

let readFile name =
    let stream = File.OpenText name
    seq {
        while not stream.EndOfStream do yield stream.Read()
    }


let config = Config.GetSample()

let loadCsv path =
    File.ReadAllText path

let handler(flag :bool) =
    Console.WriteLine config
    //[1..5]
    //|> List.map (fun x -> x)
    //|> List.iter (printfn "%d")
    //0

[<EntryPoint>]
let main argv =
    let root = RootCommand()
    Option("--flag") |> root.AddOption
    root.Handler <- CommandHandler.Create handler
    root.Invoke argv