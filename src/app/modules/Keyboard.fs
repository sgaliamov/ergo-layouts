module Keyboard

open FSharp.Data
open FSharp.Data.JsonExtensions
open StateModels
open Utilities
open System.Collections.Generic
open KeyboardModelds

let private keyboard = KeyboardInfo.GetSample()

let load (layout: Layout.Root) =
    let parseHand hand jsonValue =
        let createKey value = Keys.HandStringKey(hand, value) |> Keys.create
        jsonValue
        |> toPairs createKey (JsonExtensions.AsString >> Character.fromString)
        |> filterValuebles
        |> Seq.map flipTuple
        |> List.ofSeq

    let leftKeys =
        layout.Left.JsonValue.Properties
        |> parseHand Hands.Hand.Left

    let rightKeys =
        layout.Right.JsonValue.Properties
        |> parseHand Hands.Hand.Right

    let keys =
        leftKeys
        |> Seq.append rightKeys
        |> Map.ofSeq

    let mapShifted (from, shifted) = (shifted, keys.[from])

    let shifted =
        keyboard.Shifted.JsonValue.Properties
        |> Seq.append layout.Shifted.JsonValue.Properties
        |> toPairs Character.fromString (JsonExtensions.AsString >> Character.fromString)
        |> filterValuebles
        |> Seq.map flipTuple
        |> Seq.map mapShifted

    let efforts =
        Efforts.GetSample().JsonValue.Properties
        |> toPairs (fun string -> Keys.create <| Keys.StringKey(string)) JsonExtensions.AsFloat
        |> filterValuebleKeys
        |> Map.ofSeq

    let toSet items =
        items
        |> Seq.map (fun number -> Keys.Key(Hands.Hand.Left, byte number))
        |> Seq.append (keyboard.Top |> Seq.map (fun number -> Keys.Key(Hands.Hand.Right, byte number)))
        |> HashSet<Keys.Key>

    let getKeys items =
        items
        |> Seq.map (fun (_, key) -> key)
        |> HashSet<Keys.Key>

    { Keys = Map(keys |> Map.toSeq |> Seq.append shifted)
      Efforts = efforts
      TopKeys = keyboard.Top |> toSet
      HomeKeys = keyboard.Home |> toSet
      BottomKeys = keyboard.Bottom |> toSet
      RightKeys = rightKeys |> getKeys
      LeftKeys = leftKeys |> getKeys
      ThumbKeys = keyboard.Fingers.Thumb |> toSet
      IndexKeys = keyboard.Fingers.Index |> toSet
      MiddleKeys = keyboard.Fingers.Middle |> toSet
      RingKeys = keyboard.Fingers.Ring |> toSet
      PinkyKeys = keyboard.Fingers.Pinky |> toSet }
