module KeyboardModelds

open System
open System.Collections.Generic
open Option
open FSharp.Data
open StateModels
open Utilities
open System.Collections.Concurrent

type KeyboardInfo = JsonProvider<"../../configs/keyboard.json">
type Efforts = JsonProvider<"../../configs/efforts.json">
type Layout = JsonProvider<"""
{
    "left": {
        "1": "a"
    },
    "right": {
        "2": "b"
    }
}
""">

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

type Keyboard =
    { Keys: Map<Character.Char, Keys.Key>
      Shifted: HashSet<char>
      Efforts: Map<Keys.Key, float>
      TopKeys: HashSet<Keys.Key>
      HomeKeys: HashSet<Keys.Key>
      BottomKeys: HashSet<Keys.Key>
      LeftKeys: HashSet<Keys.Key>
      RightKeys: HashSet<Keys.Key>
      Fingers: ConcurrentDictionary<Keys.Key, Finger> }
