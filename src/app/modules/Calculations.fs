module Calculations

open System
open System.Collections.Concurrent
open System.Collections.Generic
open Keyboard
open Models
open Probability
open Utilities

type private Counter<'TIn, 'TOut> = seq<'TIn> -> seq<'TOut * int>

let private characters = HashSet<char>([ ' ' .. '~' ] |> List.except [ 'A' .. 'Z' ])

let initialState =
    { Letters = Letter.Letters()
      Digraphs = Digraph.Digraphs()
      Chars = Character.Chars()
      TotalLetters = 0
      TotalDigraphs = 0
      TotalChars = 0
      Efforts = 0.
      TopKeys = 0
      HomeKeys = 0
      BottomKeys = 0
      SameFinger = 0
      InwardRolls = 0
      OutwardRolls = 0
      RightFinders = Fingers()
      LeftFinders = Fingers()
      RightHandTotal = 0
      LeftHandTotal = 0
      RightHandContinuous = 0
      LeftHandContinuous = 0 }

let private calculate<'TIn, 'TOut> line (counter: Counter<'TIn, 'TOut>) =
    let folder result (key, count) = addOrUpdate result key count (+)
    line
    |> counter
    |> Seq.fold folder (ConcurrentDictionary<'TOut, int>())

let isFinished<'TKey> 
    (state: ConcurrentDictionary<'TKey, int>)
    (stats: Map<string, Probability>)
    (total: int)
    (precision: Probability) =
    let isEnough count keyStatistics =
        let delta = Probability.calculate (float total) precision
        let goal = Probability.calculate (float total) keyStatistics
        count + delta >= goal

    state.Keys
    |> Seq.map (fun key ->
        let str = key.ToString()
        match stats.ContainsKey str with
        | true ->
            let keyStatistics = stats.[str]
            let count = state.[key]
            isEnough (float count) keyStatistics
        | _ -> true)
    |> Seq.filter id
    |> Seq.length = state.Keys.Count

let collect (keyboard: Keyboard) line =
    let countLetters line =
        line
        |> Seq.filter Char.IsLetter
        |> Seq.map Letter.create
        |> Seq.countBy id

    let countChars line =
        line
        |> Seq.map Character.create
        |> Seq.countBy id

    let countDigraphs line =
        line
        |> Seq.filter Char.IsLetter
        |> Seq.pairwise
        |> Seq.map (toString >> Digraph.create)
        |> Seq.countBy id

    let keys =
        line
        |> Seq.map Character.create
        |> Seq.map (fun char -> keyboard.Keys.[char])
        |> List.ofSeq

    let countKeys by =
        keys
        |> Seq.filter by
        |> Seq.length

    let letters = calculate line countLetters
    let chars = calculate line countChars
    let digraphs = calculate line countDigraphs

    { Letters = letters
      Digraphs = digraphs
      Chars = chars
      TotalLetters = letters.Values |> Seq.sum
      TotalDigraphs = digraphs.Values |> Seq.sum
      TotalChars = chars.Values |> Seq.sum
      Efforts = 0.
      TopKeys = countKeys keyboard.TopKeys.Contains
      HomeKeys = countKeys keyboard.HomeKeys.Contains
      BottomKeys = countKeys keyboard.BottomKeys.Contains
      SameFinger = 0
      InwardRolls = 0
      OutwardRolls = 0
      RightFinders = Fingers()
      LeftFinders = Fingers()
      RightHandTotal = 0
      LeftHandTotal = 0
      RightHandContinuous = 0
      LeftHandContinuous = 0 }

let aggregator state from =
    { Letters = sumValues from.Letters state.Letters
      Digraphs = sumValues from.Digraphs state.Digraphs
      Chars = sumValues from.Chars state.Chars
      TotalLetters = from.TotalLetters + state.TotalLetters
      TotalDigraphs = from.TotalDigraphs + state.TotalDigraphs
      TotalChars = from.TotalChars + state.TotalChars
      Efforts = from.Efforts + state.Efforts
      TopKeys = from.TopKeys + state.TopKeys
      HomeKeys = from.HomeKeys + state.HomeKeys
      BottomKeys = from.BottomKeys + state.BottomKeys
      SameFinger = from.SameFinger + state.SameFinger
      InwardRolls = from.InwardRolls + state.InwardRolls
      OutwardRolls = from.OutwardRolls + state.OutwardRolls
      RightFinders = sumValues from.RightFinders state.RightFinders
      LeftFinders = sumValues from.LeftFinders state.LeftFinders
      RightHandTotal = from.RightHandTotal + state.RightHandTotal
      LeftHandTotal = from.LeftHandTotal + state.LeftHandTotal
      RightHandContinuous = from.RightHandContinuous + state.RightHandContinuous
      LeftHandContinuous = from.LeftHandContinuous + state.LeftHandContinuous }

let calculateLines keyboard lines =
    let filtered (line: string) =
        line.ToLowerInvariant()
        |> Seq.filter characters.Contains
    lines
    |> Seq.map (filtered >> collect keyboard)
    |> Seq.fold aggregator initialState
