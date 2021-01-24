open System
open System.IO
open System.Collections.Generic
open System.Collections.Concurrent
open FSharp.Collections.ParallelSeq

// 0. load all texts.
// 1. shuffle current set.
// 1.1. if shuffeled 10 times, save text and exit.
// 2. split on 2 parts.
// 3. calculate score for both parts.
// 4. if both bad, go to 1.
// 5. get best part.
// 6. go to 2.

[<Literal>]
let RETRYES = 50

// all letters
let alpha = ['a'..'z'] |> HashSet<char>

type DigraphsMap<'t> = ConcurrentDictionary<char * char, 't>

// calculating statistics
module Statictics =
    type DigraphsCounter = DigraphsMap<int>

    // fold collection into dictionary
    let private fold dict collection =
        // sum the value to key
        let sum (dict: ConcurrentDictionary<_, int>) key value =
            dict.AddOrUpdate(key, (fun _ v -> v), (fun _ acc v -> acc + v), value) |> ignore
            dict
        collection
        |> Seq.fold (fun acc (char, count) -> sum acc char count) dict

    // number of all pairs in a line
    let private countDigraphs (line: string) =
        line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        |> Seq.map (Seq.pairwise >> Seq.countBy id)
        |> Seq.collect id

    // calculate stats
    let calculate lines =
        let convertToStats (dict: ConcurrentDictionary<_, int>) =
            let total = dict |> Seq.sumBy (fun x -> x.Value) |> float
            dict 
            |> Seq.map (fun pair -> KeyValuePair(pair.Key, float pair.Value / total))
            |> ConcurrentDictionary<_, float>
        lines
        |> PSeq.map countDigraphs
        |> PSeq.fold fold (DigraphsCounter())
        |> convertToStats

module Text =
    // to shuffle a sequence
    let shuffle xs = xs |> PSeq.sortBy (fun _ -> Guid.NewGuid())
    
    // calculate score
    let getDiviation (digraphsStats: ConcurrentDictionary<_, float>) lines =
        lines
        |> Statictics.calculate
        |> PSeq.map (fun pair -> pair.Value / digraphsStats.[pair.Key])
        |> PSeq.average
        |> fun x -> Math.Abs(1.0 - x)
    
    // the main logic
    let rec proces threshold counter getDiviation lines =
        let rec iteration counter (lines: string[]) =
            Console.WriteLine $"Processing {lines.Length} lines, attempt {counter + 1}, {lines.[0].Substring(0, Math.Min(20, lines.[0].Length - 1))}"
            let left = lines.[..lines.Length / 2]
            let right = lines.[lines.Length / 2..]
            match (getDiviation left, getDiviation right) with
            | (a, b) when a > threshold && b > threshold -> proces threshold (counter + 1) getDiviation lines
            | (a, b) when a < b -> iteration 0 left
            | _ -> iteration 0 right
        match counter with
        | c when c < RETRYES ->
            lines
            |> shuffle
            |> PSeq.toArray
            |> iteration c
        | _ -> lines            
    
    // unpairwise sequence of tuples
    let unpairwise s = seq {
        let collection = s |> Seq.toArray
        if collection.Length > 0 then
            yield  (fst (Seq.head collection))
            yield! (collection |> Seq.map snd)
    }
    
    // keep only relevant characters in lines
    let convert lines =
        let isSpace char = char.Equals(' ')
        let mapLine (line: string) =
            line.ToLowerInvariant()
            |> Seq.map (fun x -> if alpha.Contains x then x else ' ')
            |> Seq.pairwise
            |> Seq.filter (fun (a, b) -> not (isSpace a && isSpace b))
            |> unpairwise
            |> Seq.toArray
            |> String
            |> fun x -> x.Trim()
        lines
        |> PSeq.map mapLine
        |> PSeq.filter (String.IsNullOrEmpty >> not)
        |> PSeq.toArray

// entry point
[<EntryPoint>]
let main argv =
    let threshold = Double.Parse(argv.[1])
    let lines =
        argv.[0]
        |> fun path -> Directory.EnumerateFiles(path, "*.txt", SearchOption.AllDirectories)
        |> PSeq.collect File.ReadAllLines
        |> Text.convert
        |> PSeq.toArray
    Console.WriteLine $"{lines.Length} lines loaded."
    let stats = Statictics.calculate lines
    let lines = Text.proces threshold 0 (Text.getDiviation stats) lines
    Console.WriteLine $"Done with {lines.Length} lines."

    let path = $"{argv.[1]}-{lines.Length}.result.txt"
    let content = String.Join(' ', lines)
    if not (File.Exists path) || int64 content.Length < FileInfo(path).Length then
        File.WriteAllText(Path.Combine(argv.[2], path), content)
    0
