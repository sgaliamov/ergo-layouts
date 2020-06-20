module Calculations

open System
open System.Collections.Concurrent
open System.Collections.Generic
open KeyboardModelds
open StateModels
open Probability
open Utilities

type private Counter<'TIn, 'TOut> = seq<'TIn> -> seq<'TOut * int>

let private characters = HashSet<char>([' '..'~'])

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
      LeftHandContinuous = 0
      Shifts = 0 }

let private calculate<'TIn, 'TOut> line (counter: Counter<'TIn, 'TOut>) =
    let folder result (key, count) = addOrUpdate result key count (+)
    line
    |> counter
    |> Seq.fold folder (ConcurrentDictionary<'TOut, int>())

let isFinished<'TKey when 'TKey : comparison>
    (state: ConcurrentDictionary<'TKey, int>)
    (stats: Map<'TKey, Probability>)
    (total: int)
    (precision: Probability) =
    let isEnough count keyStatistics =
        let delta = Probability.calculate (float total) precision
        let goal = Probability.calculate (float total) keyStatistics
        count + delta >= goal

    state.Keys
    |> Seq.filter stats.ContainsKey
    |> Seq.map (fun key ->
        let keyStatistics = stats.[key]
        let count = state.[key]
        isEnough (float count) keyStatistics)
    |> Seq.filter id
    |> Seq.length = state.Keys.Count

let private countLetters line =
    line
    |> Seq.filter Char.IsLetter
    |> Seq.map Letter.create
    |> Seq.countBy id

let private countChars line =
    line
    |> Seq.map Character.fromChar
    |> Seq.countBy id

let private countDigraphs line =
    line
    |> Seq.filter Char.IsLetter
    |> Seq.pairwise
    |> Seq.map (toString >> Digraph.create)
    |> Seq.countBy id

let private count keys by =
    keys
    |> Seq.filter by
    |> Seq.length

let collect keyboard line =
    let lowerLine =
        line 
        |> Seq.map Char.ToLowerInvariant
        |> List.ofSeq
    let keysInLine =
        lowerLine
        |> Seq.map (Character.fromChar >> (fun char ->
            // todo: apply RoP
            match keyboard.Keys.TryGetValue char with
            | (true, key) -> key
            | (false, _) -> failwithf "Can't find key for '%c'" (Character.value char)))
        |> List.ofSeq
    let letters = calculate lowerLine countLetters
    let chars = calculate lowerLine countChars
    let digraphs = calculate lowerLine countDigraphs
    let efforts =
        keysInLine
        |> Seq.sumBy (fun key ->
            match keyboard.Efforts.TryGetValue key with
            | (true, effort) -> effort
            | (false, _) -> failwithf "Can't find effort for '%s'" (key.ToString()))
    let sameFinger =
        keysInLine
        |> Seq.pairwise
        |> Seq.filter (fun (a, b) -> a = b)
        |> Seq.length

    { Letters = letters
      Digraphs = digraphs
      Chars = chars
      TotalLetters = letters.Values |> Seq.sum
      TotalDigraphs = digraphs.Values |> Seq.sum
      TotalChars = chars.Values |> Seq.sum
      Efforts = efforts
      TopKeys = count keysInLine keyboard.TopKeys.Contains
      HomeKeys = count keysInLine keyboard.HomeKeys.Contains
      BottomKeys = count keysInLine keyboard.BottomKeys.Contains
      SameFinger = sameFinger
      InwardRolls = 0
      OutwardRolls = 0
      RightFinders = Fingers()
      LeftFinders = Fingers()
      RightHandTotal = count keysInLine keyboard.RightKeys.Contains
      LeftHandTotal = count keysInLine keyboard.LeftKeys.Contains
      RightHandContinuous = 0
      LeftHandContinuous = 0
      Shifts = 0 }

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
      LeftHandContinuous = from.LeftHandContinuous + state.LeftHandContinuous
      Shifts = from.Shifts + state.Shifts }

let calculateLines keyboard lines =
    let filtered line =
        line
        |> Seq.filter characters.Contains
    lines
    |> Seq.map (filtered >> collect keyboard)
    |> Seq.fold aggregator initialState
