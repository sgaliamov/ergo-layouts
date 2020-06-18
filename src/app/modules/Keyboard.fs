module Keyboard

open System
open FSharp.Data
open FSharp.Data.JsonExtensions
open Option
open Models
open Utilities
open System.Collections.Generic

type private Efforts = JsonProvider<"./data/efforts.json">
type private KeyboardJson = JsonProvider<"./data/qwerty.json">

type Hand =
    | Left = 'L'
    | Right = 'R'

type Key = Hand * byte

type Keyboard =
    { Keys: Map<Character.Char, Key>
      Efforts: Map<Key, float>
      TopKeys: HashSet<Key>
      HomeKeys: HashSet<Key>
      BottomKeys: HashSet<Key>
      ThumbKeys: HashSet<Key>
      IndexKeys: HashSet<Key>
      MiddleKeys: HashSet<Key>
      RingKeys: HashSet<Key>
      PinkyKeys: HashSet<Key> }

let load (path: string) =
    let keyboard = KeyboardJson.Load(path)
    let createKey hand number = Key(hand, Byte.Parse(number))
    let parseHand hand jsonValue =
        jsonValue
        |> jsonValueToPairs Character.fromString (JsonExtensions.AsString >> createKey hand)
        |> filterValuebleKeys

    let keys =
        parseHand Hand.Left keyboard.Left.JsonValue.Properties
        |> Seq.append (parseHand Hand.Right keyboard.Right.JsonValue.Properties)
        |> Map.ofSeq

    let createKey (string: string) =
        let createHand char =
            match char with
            | 'L' -> Some Hand.Left
            | 'R' -> Some Hand.Right
            | _ -> None
        match string with
        | HeadTail (h, t) -> createHand h |> map (fun hand -> createKey hand t)
        | _ -> None

    let efforts =
        Efforts.GetSample().JsonValue.Properties
        |> jsonValueToPairs createKey JsonExtensions.AsFloat
        |> filterValuebleKeys
        |> Map.ofSeq

    { Keys = keys
      Efforts = efforts
      TopKeys = HashSet<Key>
      HomeKeys = HashSet<Key>
      BottomKeys = HashSet<Key>
      ThumbKeys = HashSet<Key>
      IndexKeys = HashSet<Key>
      MiddleKeys = HashSet<Key>
      RingKeys = HashSet<Key>
      PinkyKeys = HashSet<Key> }