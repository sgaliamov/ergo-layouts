module CalculationsTests

open System.Collections.Concurrent
open Xunit
open FsUnit
open Calculations
open Probability

[<Fact>]
let ``Should consider to finish when has enough characters`` () =
    let stats = Map<string, Probability> [ ("th", Probability.create 10.) ]
    let total = 100
    let precision = Probability.create 2.
    let state = ConcurrentDictionary<string, int>(Map<string, int> [ ("th", 8) ])

    let actual = isFinished state stats total precision
    actual |> should be True
