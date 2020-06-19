module KeyboardModelds

open System
open System.Collections.Generic
open Option
open FSharp.Data
open StateModels
open Utilities

type Keyboard = JsonProvider<"./data/keyboard.json">
type Efforts = JsonProvider<"./data/efforts.json">
type Layout = JsonProvider<"./data/qwerty.json">

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
        | StringKey(string) ->
            match string with
            | HeadTail (h, t) -> Hands.create h |> map (fun hand -> HandStringKey(hand, t) |> create) |> flatten
            | _ -> None
        | HandStringKey(hand, number) -> Some <| Key(hand, Byte.Parse(number))
        | Key(hand, number) -> Some <| Key(hand, number)

type KeyboardConfiguration =
    { Keys: Map<Character.Char, Keys.Key>
      Efforts: Map<Keys.Key, float>
      TopKeys: HashSet<Keys.Key>
      HomeKeys: HashSet<Keys.Key>
      BottomKeys: HashSet<Keys.Key>
      ThumbKeys: HashSet<Keys.Key>
      IndexKeys: HashSet<Keys.Key>
      MiddleKeys: HashSet<Keys.Key>
      RingKeys: HashSet<Keys.Key>
      PinkyKeys: HashSet<Keys.Key> }