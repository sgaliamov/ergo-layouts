module Calculations

open System.Collections.Concurrent
open System.Collections.Generic
open Utilities

type Char = char
type Letter = char
type Chars = ConcurrentDictionary<Char, int>
type Letters = ConcurrentDictionary<Letter, int>
type Digraph = string
type Digraphs = ConcurrentDictionary<Digraph, int>

type State =
    { Letters: Letters
      Digraphs: Digraphs
      Chars: Chars
      TotalLetters: int
      TotalDigraphs: int
      TotalChars: int }

type Counter<'TIn, 'TOut> = seq<'TIn> -> seq<'TOut * int>

let private characters =
    HashSet<char>([ ' ' .. '~' ] |> List.except [ 'A' .. 'Z' ])

let initialState =
    { Letters = Letters()
      Digraphs = Digraphs()
      Chars = Chars()
      TotalLetters = 0
      TotalDigraphs = 0
      TotalChars = 0 }

let private calculate<'TIn, 'TOut> line (counter: Counter<'TIn, 'TOut>) =
    let folder result (key, count) = addOrUpdate result key count (+)
    line
    |> counter
    |> Seq.fold folder (ConcurrentDictionary<'TOut, int>())

let isFinished<'TKey> (state: ConcurrentDictionary<'TKey, int>) (stats: Map<string, float>) total precision =
    let isEnough count keyStatistics =
        let delta = int (precision * float total)
        let goal = int (keyStatistics * float total)
        count + delta >= goal

    state.Keys
    |> Seq.map (fun key ->
        let str = key.ToString()
        match stats.ContainsKey str with
        | true ->
            let keyStatistics = stats.[str]
            let count = state.[key]
            isEnough count keyStatistics
        | _ -> true)
    |> Seq.filter id
    |> Seq.length = state.Keys.Count

let collect line =
    let countLetters line =
        line |> Seq.filter Char.IsLetter |> Seq.countBy id

    let countChars line = line |> Seq.countBy id

    let countDigraphs line =
        line
        |> Seq.filter Char.IsLetter
        |> Seq.pairwise
        |> Seq.map toString
        |> Seq.countBy id

    let letters = calculate line countLetters
    let chars = calculate line countChars
    let digraphs = calculate line countDigraphs
    { Letters = letters
      Digraphs = digraphs
      Chars = chars
      TotalLetters = letters.Values |> Seq.sum
      TotalDigraphs = digraphs.Values |> Seq.sum
      TotalChars = chars.Values |> Seq.sum }

let aggregator state from =
    { Letters = sumValues from.Letters state.Letters
      Digraphs = sumValues from.Digraphs state.Digraphs
      Chars = sumValues from.Chars state.Chars
      TotalLetters = from.TotalLetters + state.TotalLetters
      TotalDigraphs = from.TotalDigraphs + state.TotalDigraphs
      TotalChars = from.TotalChars + state.TotalChars }

let calculateLines lines =
    let filtered (line: string) =
        line.ToLowerInvariant()
        |> Seq.filter characters.Contains

    lines
    |> Seq.map (filtered >> collect)
    |> Seq.fold aggregator initialState
