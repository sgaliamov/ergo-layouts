module GenericTests

open Xunit
open FsUnit
open FSharp.Data
open FSharp.Data.JsonExtensions

type private Stats = JsonProvider<"""{
    "digraphs": {
        "th": 1.23
    }
}""">

[<Fact>]
let ``Test map creation`` () =
    let statistics = Stats.GetSample()
    let digraphsMap = 
        statistics.Digraphs.JsonValue.Properties
        |> Seq.map (fun (a: string, b: JsonValue) -> (a.ToString(), b.AsFloat()))
        |> Map.ofSeq

    digraphsMap.["th"]
    |> should equal 1.23
