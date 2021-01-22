// Learn more about F# at http://fsharp.org

open System
open System.IO
open FSharp.Data
open FSharp.Data.JsonExtensions

// 0. load all texts.
// 1. shuffle current set.
// 1.1. if shuffeled 10 times, save text and exit.
// 2. split on 2 parts.
// 3. calculate score for both parts.
// 4. if both bad, go to 1.
// 5. get best part.
// 6. go to 2.

type private Stats = JsonProvider<"./statistics.json">
let private statistics = Stats.GetSample()

let r = Random()
let shuffle xs = xs |> Seq.sortBy (fun _ -> r.Next())

[<Literal>]
let THRESHOLD = 0.95f

let getScore lines =
    1.0f

let rec fullIteration counter lines =
    let rec iteration counter (lines: string[]) =
        let left = lines.[..lines.Length / 2]
        let right = lines.[lines.Length / 2..]
        match (getScore left, getScore right) with
        | (a, b) when a < THRESHOLD && b < THRESHOLD -> fullIteration (counter + 1) lines
        | (a, b) when a > b -> iteration 0 left
        | _ -> iteration 0 right

    match counter with
    | c when c < 10 -> 
        lines
        |> shuffle
        |> Seq.toArray
        |> iteration c
    | _ -> File.WriteAllLines("result.txt", lines)

let filter lines =
    lines

[<EntryPoint>]
let main argv =
    argv.[0]
    |> (fun path -> Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
    |> Seq.collect File.ReadAllLines
    |> filter
    |> Seq.toArray
    |> fullIteration 0
    0

