module Keyboard

open System
open FSharp.Data
open FSharp.Data.JsonExtensions
open StateModels
open Utilities
open System.Collections.Generic
open KeyboardModelds
open KeyboardModelds.Hands
open System.Collections.Concurrent

type KeyboardInfo = JsonProvider<"./keyboard.json">
type private Efforts = JsonProvider<"./efforts.json">
type private Coordinates = JsonProvider<"./coordinates.json">

let private coordinates = Coordinates.GetSample()
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
        |> parseHand Hand.Left

    let rightKeys =
        layout.Right.JsonValue.Properties
        |> parseHand Hand.Right

    let keys =
        leftKeys
        |> Seq.append rightKeys
        |> Map.ofSeq

    let mapShifted (shifted, from) =
        match keys.TryGetValue from with
        | (true, key) -> (Some key, Some shifted)
        | (false, _) -> (None, None)

    let shifted =
        keyboard.Shifted.JsonValue.Properties
        |> toPairs Character.fromString (JsonExtensions.AsString >> Character.fromString)
        |> filterValuebles
        |> Seq.map mapShifted
        |> filterValuebles
        |> Seq.map flipTuple

    let efforts =
        Efforts.GetSample().JsonValue.Properties
        |> toPairs (Keys.StringKey >> Keys.create) JsonExtensions.AsFloat
        |> filterValuebleKeys
        |> Map.ofSeq

    let createCoordinate (string: string) =
        match string.Split(',', StringSplitOptions.RemoveEmptyEntries) with
        | [| y; x |] -> Double.Parse x, Double.Parse y
        | _ -> failwithf "Failed to parse coordinate %s" string

    let coordinates =
        coordinates.JsonValue.Properties
        |> toPairs (Keys.StringKey >> Keys.create) (JsonExtensions.AsString >> createCoordinate)
        |> filterValuebleKeys
        |> Map.ofSeq

    let toSet numbers =
        numbers
        |> Seq.map (fun number -> [ Keys.Key(Hand.Left, byte number); Keys.Key(Hand.Right, byte number) ])
        |> Seq.collect id

    let getKeys numbers =
        numbers
        |> Seq.map (fun (_, key) -> key)
        |> HashSet<Keys.Key>

    let getFinger numbers finger =
        numbers
        |> toSet
        |> Seq.map (fun key -> (key, finger))

    let fingers =
        getFinger keyboard.Fingers.Thumb Finger.Thumb
        |> Seq.append (getFinger keyboard.Fingers.Index Finger.Index)
        |> Seq.append (getFinger keyboard.Fingers.Middle Finger.Middle)
        |> Seq.append (getFinger keyboard.Fingers.Ring Finger.Ring)
        |> Seq.append (getFinger keyboard.Fingers.Pinky Finger.Pinky)
        |> Map.ofSeq
        |> ConcurrentDictionary<Keys.Key, Finger>

    let allKeys = keys |> Map.toSeq |> Seq.append shifted

    { Keys = Map(allKeys)
      Shifted = shifted |> Seq.map (fun (char, _) -> Character.value char) |> HashSet<char>
      Efforts = efforts
      Coordinates = coordinates
      TopKeys = keyboard.Top |> toSet |> HashSet<Keys.Key>
      HomeKeys = keyboard.Home |> toSet |> HashSet<Keys.Key>
      BottomKeys = keyboard.Bottom |> toSet |> HashSet<Keys.Key>
      RightKeys = rightKeys |> getKeys
      LeftKeys = leftKeys |> getKeys
      FingersMap = fingers }
