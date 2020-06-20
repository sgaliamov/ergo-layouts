module GenericTests

open Xunit
open FsUnit
open FSharp.Data
open FSharp.Data.JsonExtensions

type private Stats =
    JsonProvider<"""{
    "digraphs": {
        "th": 1.23
    }
}""">

[<Fact>]
let ``Test map creation.`` () =
    let statistics = Stats.GetSample()

    let digraphsMap =
        statistics.Digraphs.JsonValue.Properties
        |> Seq.map (fun (a: string, b: JsonValue) -> (a.ToString(), b.AsFloat()))
        |> Map.ofSeq

    digraphsMap.["th"] |> should equal 1.23

[<Fact>]
let ``Map override dublicates on creation.`` () =
    let actual = Map([(1, "a"); (2, "b"); (1, "c")])
    let expected = Map([(1, "c"); (2, "b")])

    actual |> should equal expected