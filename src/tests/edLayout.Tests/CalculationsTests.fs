module CalculationsTests

open System.Collections.Concurrent
open Xunit
open FsUnit
open Calculations
open System.Collections.Generic

let toKeyValuePairs<'TKey, 'TValue> pairs =
    pairs 
    |> Array.map (fun (a, b) -> KeyValuePair<'TKey, 'TValue>(a, b))

[<Fact>]
let ``Should consider to finish when has enough characters`` () =
    let state = ConcurrentDictionary<string, int>(toKeyValuePairs [|("th", 8)|])
    let stats = Map<string, float> [("th", 10.)]
    let total = 100
    let precision = 2.

    let actual = isFinished state stats total precision
    actual |> should be True

