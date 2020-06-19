module Keyboard

open FSharp.Data
open FSharp.Data.JsonExtensions
open StateModels
open Utilities
open System.Collections.Generic
open KeyboardModelds

let load (layout: Layout.Root) =
    let parseHand hand jsonValue =
        let createKey value = Keys.HandStringKey(hand, value) |> Keys.create
        jsonValue
        |> jsonValueToPairs createKey (JsonExtensions.AsString >> Character.fromString)
        |> filterValuebleKeys
        |> Seq.map flipTuple
        |> filterValuebleKeys

    let keys =
        parseHand Hands.Hand.Left layout.Left.JsonValue.Properties
        |> Seq.append (parseHand Hands.Hand.Right layout.Right.JsonValue.Properties)
        |> Map.ofSeq

    let efforts =
        Efforts.GetSample().JsonValue.Properties
        |> jsonValueToPairs (fun string -> Keys.create <| Keys.StringKey(string)) JsonExtensions.AsFloat
        |> filterValuebleKeys
        |> Map.ofSeq

    let keyboard = KeyboardInfo.GetSample()

    let toSet items =
        items
        |> Seq.map (fun number -> Keys.Key(Hands.Hand.Left, byte number))
        |> Seq.append (keyboard.Top |> Seq.map (fun number -> Keys.Key(Hands.Hand.Right, byte number)))
        |> HashSet<Keys.Key>

    { Keys = keys
      Efforts = efforts
      TopKeys = keyboard.Top |> toSet
      HomeKeys = keyboard.Home |> toSet
      BottomKeys = keyboard.Bottom |> toSet
      ThumbKeys = keyboard.Fingers.Thumb |> toSet
      IndexKeys = keyboard.Fingers.Index |> toSet
      MiddleKeys = keyboard.Fingers.Middle |> toSet
      RingKeys = keyboard.Fingers.Ring |> toSet
      PinkyKeys = keyboard.Fingers.Pinky |> toSet }