open System
open System.IO
open System.Collections.Generic
open System.Collections.Concurrent

// 0. load all texts.
// 1. shuffle current set.
// 1.1. if shuffeled 10 times, save text and exit.
// 2. split on 2 parts.
// 3. calculate score for both parts.
// 4. if both bad, go to 1.
// 5. get best part.
// 6. go to 2.

// allowed diviations from the ideal score
[<Literal>]
let THRESHOLD = 0.95f

// allowed diviations from the ideal score
[<Literal>]
let RETRYES = 10

// all letters
let alpha = ['a'..'z'] |> HashSet<char>

// calculating statistics
module Statictics =
    type LettersCounter = ConcurrentDictionary<char, int>
    type DigraphsCounter = ConcurrentDictionary<char * char, int>

    // fold collection into dictionary
    let private fold dict collection =
        // sum the value to key
        let sum (dict: ConcurrentDictionary<_, int>) key value =
            dict.AddOrUpdate(key, (fun _ v -> v), (fun _ acc v -> acc + v), value) |> ignore
            dict
        collection
        |> Seq.fold (fun acc (char, count) -> sum acc char count) dict

    // number of all letters in a line
    let private countLetters line =
        line
        |> Seq.filter alpha.Contains
        |> Seq.countBy id

    // number of all pairs in a line
    let private countDigraphs (line: string) =
        line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        |> Seq.map (Seq.pairwise >> Seq.countBy id)
        |> Seq.collect id

    // calculate stats
    let calculate lines =
        let folder 
            (lettersCounts: LettersCounter, digraphsCounts: DigraphsCounter)
            (lettersSeq: seq<char * int>, digraphsSeq: seq<(char * char) * int>) =
            (fold lettersCounts lettersSeq, fold digraphsCounts digraphsSeq)

        let convertToStats (dict: ConcurrentDictionary<_, int>) =
            let total = dict |> Seq.sumBy (fun x -> x.Value)|> float
            dict 
            |> Seq.map (fun pair -> KeyValuePair(pair.Key, float pair.Value / total))
            |> ConcurrentDictionary<_, float>

        let (letters, digraphs) = 
            lines
            |> Seq.map (fun line -> 
                let letters = countLetters line
                let digraphs = countDigraphs line
                (letters, digraphs))
            |> Seq.fold folder ((LettersCounter(), DigraphsCounter()))
        (convertToStats letters, convertToStats digraphs)

// to shuffle a sequence
let r = Random()
let shuffle xs = xs |> Seq.sortBy (fun _ -> r.Next())

// calculate score
let getScore stats lines =
    1.0f

// the main logic
let rec proces counter stats lines =
    let rec iteration counter stats (lines: string[]) =
        let left = lines.[..lines.Length / 2]
        let right = lines.[lines.Length / 2..]
        match (getScore stats left, getScore stats right) with
        | (a, b) when a < THRESHOLD && b < THRESHOLD -> proces (counter + 1) stats lines
        | (a, b) when a > b -> iteration 0 stats left
        | _ -> iteration 0 stats right
    match counter with
    | c when c < RETRYES ->
        lines
        |> shuffle
        |> Seq.toArray
        |> iteration c stats
    | _ -> File.WriteAllLines("result.txt", lines)

// unpairwise sequence of tuples
let unpairwise (s: seq<'a * 'a>) : seq<'a> = seq {
    yield  (fst (Seq.head s))
    yield! (s |> Seq.map snd)
}

// keep only relevant characters in the line
let convert lines =
    let isSpace char = char.Equals(' ')
    let isAllowed char = alpha.Contains char || isSpace char
    let mapLine (line: string) =
        line.ToLowerInvariant().Replace('\t', ' ')
        |> Seq.filter isAllowed
        |> Seq.pairwise
        |> Seq.filter (fun (a, b) -> not (isSpace a && isSpace b))
        |> unpairwise
        |> string
    lines
    |> Seq.map mapLine
    |> Seq.toArray

// entry point
[<EntryPoint>]
let main argv =
    argv.[0]
    |> fun path -> Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
    |> Seq.collect File.ReadAllLines
    |> Seq.toArray
    |> (convert >> proces 0 Statictics.calculate)
    0
