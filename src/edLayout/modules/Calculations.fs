module Calculations

open System
open System.Collections.Concurrent
open System.Collections.Generic
open Configs
open KeyboardModelds
open StateModels
open Probability
open Utilities

type private Counter<'TIn, 'TOut> = seq<'TIn> -> seq<'TOut * int>
let private characters = [' '..'~'] |> HashSet<char>
let private lettersOnly = ['a'..'z'] |> HashSet<char>
let private START_TOKEN = Keys.StringKey "START"

let private calculate line (counter: Counter<'TIn, 'TOut>) =
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
    |> Seq.length = state.Keys.Count && state.Keys.Count <> 0

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

let private countCountinuous keys (hand: HashSet<Keys.Key>) =
    keys
    |> Seq.pairwise
    |> Seq.fold (fun count (a, b) ->
        if hand.Contains(a) && hand.Contains(b) then count + 1
        else count) 0

let private countFingers (fingers: FingersKeyMap) keys (hand: HashSet<Keys.Key>) =
    keys
    |> Seq.filter hand.Contains
    |> Seq.groupBy (getFinger fingers)
    |> Seq.map (fun (finger, keys) -> (finger, keys |> Seq.length))
    |> Map.ofSeq
    |> FingersCounter

let private getFactor keyboard prev key =
    if prev = START_TOKEN then 0.
    else if key = prev then settings.doublePressPenalty
    else if isSameFinger keyboard key prev then settings.sameFingerPenalty
    else if not (isSameHand keyboard key prev) then settings.handSwitchPenalty
    else 1.0

let private getDistance keyboard prev key =
    let calcluateDistance (x1, y1) (x2, y2) = sqrt ((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2))
    if prev = START_TOKEN then 1.
    else if key = prev then 1.
    else if not (isSameHand keyboard key prev) then 1.
    else calcluateDistance keyboard.Coordinates.[key] keyboard.Coordinates.[prev]

let private toKeys keyboard line =
    line
    |> Seq.map (Character.fromChar >> (fun char ->
        // todo: apply RoP
        match keyboard.Keys.TryGetValue char with
        | (true, key) -> key
        | (false, _) -> failwithf "Can't find key for '%c'" (Character.value char)))
    |> List.ofSeq

let collect (keyboard: Keyboard) line =
    let (topKeys, homeKeys, bottomKeys, leftKeys, rightKeys, fingersMap) =
        keyboard.TopKeys,
        keyboard.HomeKeys,
        keyboard.BottomKeys,
        keyboard.LeftKeys,
        keyboard.RightKeys,
        keyboard.FingersMap

    let line = line |> Seq.cache
    let isSameHand (a, b) = isSameHand keyboard a b
    let isSameFinger (a, b) = isSameFinger keyboard a b

    let lowerLine =
        line 
        |> Seq.map Char.ToLowerInvariant
        |> List.ofSeq

    let keysInLine = toKeys keyboard lowerLine
    let letters = calculate lowerLine countLetters
    let chars = calculate lowerLine countChars
    let digraphs = calculate lowerLine countDigraphs

    let factorsMap =
        START_TOKEN::keysInLine
        |> Seq.pairwise
        |> Seq.map (fun (prev, key) -> key, getFactor keyboard prev key)
        |> Map

    let distanceMap =
        START_TOKEN::keysInLine
        |> Seq.pairwise
        |> Seq.map (fun (prev, key) -> key, getDistance keyboard prev key)
        |> Map

    let efforts =
        keysInLine
        |> Seq.sumBy (fun key -> keyboard.Efforts.[key] * factorsMap.[key])

    let distance =
        keysInLine
        |> Seq.sumBy (fun key -> distanceMap.[key])

    let effortsMap =
        keysInLine
        |> Seq.map (fun key -> keyboard.Chars.[key], keyboard.Efforts.[key] * factorsMap.[key] * distanceMap.[key])
        |> Map.ofSeq
        |> ConcurrentDictionary

    let result = effortsMap |> Seq.sumBy (fun x -> x.Value)

    let sameFinger =
        keysInLine
        |> Seq.pairwise
        |> Seq.filter isSameFinger
        |> Seq.length

    let handSwitch =
        keysInLine
        |> Seq.pairwise
        |> Seq.filter (fun x -> not (isSameHand x))
        |> Seq.length

    let inwards =
        keysInLine
        |> Seq.pairwise
        |> Seq.filter isSameHand
        |> Seq.fold (fun count (a, b) ->
            if topKeys.Contains(a) && not (topKeys.Contains(b)) then count + 1
            else if homeKeys.Contains(a) && bottomKeys.Contains(b) then count + 1
            else count) 0

    let outwards =
        keysInLine
        |> Seq.pairwise
        |> Seq.filter isSameHand
        |> Seq.fold (fun count (a, b) ->
            if bottomKeys.Contains(a) && not (bottomKeys.Contains(b)) then count + 1
            else if homeKeys.Contains(a) && topKeys.Contains(b) then count + 1
            else count) 0

    let shifts =
        line
        |> Seq.zip lowerLine
        |> Seq.filter (fun (a, b) -> a <> b || keyboard.Shifted.Contains a)
        |> Seq.length

    let letterKeys =
        lowerLine
        |> Seq.filter lettersOnly.Contains
        |> toKeys keyboard
        |> List.ofSeq

    { Letters = letters
      Digraphs = digraphs
      Chars = chars
      TotalLetters = letters.Values |> Seq.sum
      TotalDigraphs = digraphs.Values |> Seq.sum
      TotalChars = chars.Values |> Seq.sum
      Result = result
      Efforts = efforts
      Distance = distance
      TopKeys = count keysInLine topKeys.Contains
      HomeKeys = count keysInLine homeKeys.Contains
      BottomKeys = count keysInLine bottomKeys.Contains
      SameFinger = sameFinger
      InwardRolls = inwards
      OutwardRolls = outwards
      LeftFingers = countFingers fingersMap keysInLine leftKeys
      RightFingers = countFingers fingersMap keysInLine rightKeys
      LeftHandTotal = count keysInLine rightKeys.Contains
      RightHandTotal = count keysInLine leftKeys.Contains
      LeftHandContinuous = countCountinuous letterKeys leftKeys
      RightHandContinuous = countCountinuous letterKeys rightKeys
      Shifts = shifts
      CharEfforts = effortsMap
      HandSwitch = handSwitch }

let aggregator state from =
    { Letters = sumValues from.Letters state.Letters (+)
      Digraphs = sumValues from.Digraphs state.Digraphs (+)
      Chars = sumValues from.Chars state.Chars (+)
      TotalLetters = from.TotalLetters + state.TotalLetters
      TotalDigraphs = from.TotalDigraphs + state.TotalDigraphs
      TotalChars = from.TotalChars + state.TotalChars
      Result = from.Result + state.Result
      Efforts = from.Efforts + state.Efforts
      Distance = from.Distance + state.Distance
      TopKeys = from.TopKeys + state.TopKeys
      HomeKeys = from.HomeKeys + state.HomeKeys
      BottomKeys = from.BottomKeys + state.BottomKeys
      SameFinger = from.SameFinger + state.SameFinger
      InwardRolls = from.InwardRolls + state.InwardRolls
      OutwardRolls = from.OutwardRolls + state.OutwardRolls
      LeftFingers = sumValues from.LeftFingers state.LeftFingers (+)
      RightFingers = sumValues from.RightFingers state.RightFingers (+)
      LeftHandTotal = from.LeftHandTotal + state.LeftHandTotal
      RightHandTotal = from.RightHandTotal + state.RightHandTotal
      LeftHandContinuous = from.LeftHandContinuous + state.LeftHandContinuous
      RightHandContinuous = from.RightHandContinuous + state.RightHandContinuous
      Shifts = from.Shifts + state.Shifts
      HandSwitch = from.HandSwitch + state.HandSwitch
      CharEfforts = sumValues from.CharEfforts state.CharEfforts (+) }

let calculateLines keyboard lines =
    let filtered line = line |> Seq.filter characters.Contains
    lines
    |> Seq.map (filtered >> collect keyboard)
    |> Seq.fold aggregator initialState
