﻿module Calculations

open System.Collections.Concurrent
open System.Collections.Generic
open Utilities

type Char = char
type Letter = char
type Chars = ConcurrentDictionary<Char, int>
type Letters = ConcurrentDictionary<Letter, int>
type Digraph = string
type Digraphs = ConcurrentDictionary<Digraph, int>
type State = {
    letters: Letters
    digraphs: Digraphs
    chars: Chars
    totalLetters: int
    totalDigraphs: int
    totalChars: int }
type Counter<'TIn, 'TOut> = seq<'TIn> -> seq<'TOut * int>

let private characters = HashSet<char>([' '..'~'] |> List.except ['A'..'Z'])

let stateSeed = {
    letters = Letters()
    digraphs = Digraphs()
    chars = Chars()
    totalLetters = 0
    totalDigraphs = 0
    totalChars = 0 }

let private calculate<'TIn, 'TOut> line (counter: Counter<'TIn, 'TOut>) =
    let folder result (key, count) = addOrUpdate result key count (+)
    line
    |> counter
    |> Seq.fold folder (ConcurrentDictionary<'TOut, int>())

let isFinished<'TKey> (state: ConcurrentDictionary<'TKey, int>) (stats: Map<string, float>) total precision =
    let isEnough count keyStatistics =
        let delta = int(precision * float total)
        let goal = int(keyStatistics * float total)
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
    |> Seq.length
    = state.Keys.Count

let collect line =
    let countLetters line =
        line 
        |> Seq.filter Char.IsLetter 
        |> Seq.countBy id
    
    let countChars line =
        line
        |> Seq.countBy id
    
    let countDigraphs line =
        line
        |> Seq.filter Char.IsLetter
        |> Seq.pairwise
        |> Seq.map toString
        |> Seq.countBy id

    let letters = calculate line countLetters
    let chars = calculate line countChars
    let digraphs = calculate line countDigraphs
    { 
        letters = letters
        digraphs = digraphs
        chars = chars
        totalLetters = letters.Values |> Seq.sum
        totalDigraphs = digraphs.Values |> Seq.sum
        totalChars = chars.Values |> Seq.sum
    }

let aggregator state from = {
    letters = sumValues from.letters state.letters
    digraphs = sumValues from.digraphs state.digraphs
    chars = sumValues from.chars state.chars
    totalLetters = from.totalLetters + state.totalLetters
    totalDigraphs = from.totalDigraphs + state.totalDigraphs
    totalChars = from.totalChars + state.totalChars }

let calculateLines lines =
    lines
    |> Seq.map (fun (line: string) -> line.ToLowerInvariant() |> Seq.filter characters.Contains) 
    |> Seq.map collect
    |> Seq.fold aggregator stateSeed
