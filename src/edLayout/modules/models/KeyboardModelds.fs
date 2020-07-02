module KeyboardModelds

open System
open System.Collections.Generic
open Option
open FSharp.Data
open StateModels
open Utilities
open System.Collections.Concurrent

type Layout =
    JsonProvider<"""{
        "left": {
            "1": "a"
        },
        "right": {
            "2": "b"
        }
    }""">

module Hands =
    type Hand =
        | Left = 'L'
        | Right = 'R'

    let create char =
        match char with
        | 'L' -> Some Hand.Left
        | 'R' -> Some Hand.Right
        | _ -> None

module Keys =
    type Key =
        | StringKey of string
        | HandStringKey of Hands.Hand * string
        | Key of Hands.Hand * byte

    let rec create union =
        match union with
        | StringKey (string) ->
            match string with
            | HeadTail (h, t) ->
                Hands.create h
                |> map (fun hand -> HandStringKey(hand, t) |> create)
                |> flatten
            | _ -> None
        | HandStringKey (hand, number) -> Some <| Key(hand, Byte.Parse(number))
        | Key (hand, number) -> Some <| Key(hand, number)

type FingersKeyMap = ConcurrentDictionary<Keys.Key, Finger>

type Keyboard =
    { Keys: Map<Character.Char, Keys.Key>
      Chars: Map<Keys.Key, Character.Char>
      Shifted: HashSet<char>
      Efforts: Map<Keys.Key, float>
      Coordinates: Map<Keys.Key, float * float>
      TopKeys: HashSet<Keys.Key>
      HomeKeys: HashSet<Keys.Key>
      BottomKeys: HashSet<Keys.Key>
      LeftKeys: HashSet<Keys.Key>
      RightKeys: HashSet<Keys.Key>
      FingersMap: FingersKeyMap }

let getFinger (fingers: FingersKeyMap) key = fingers.[key]

let getHand keyboard key =
    let left, _ = keyboard.LeftKeys.TryGetValue key
    if left then Hands.Hand.Left else Hands.Hand.Right

let isSameHand keyboard a b =
    let aHand = getHand keyboard a
    let bHand = getHand keyboard b
    aHand = bHand

let isSameFinger keyboard a b =
    isSameHand keyboard a b
    && getFinger keyboard.FingersMap a = getFinger keyboard.FingersMap b
