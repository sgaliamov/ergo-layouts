module StateModels

open System
open System.Collections.Concurrent
open Utilities

module Character =
    type Char = Char of char
    type Chars = ConcurrentDictionary<Char, int>
    let fromChar value = Char value
    let fromString (string: string) =
        match string with
        | HeadTail (h, t) when t.Length = 0 -> Some (fromChar h)
        | _ -> None
    let value (Char char) = char

module Digraph =
    type Digraph = Digraph of string
    type Digraphs = ConcurrentDictionary<Digraph, int>
    let create string = Digraph (string |> Seq.sort |> Array.ofSeq |> String)
    let value (Digraph digraph) = digraph

module Letter =
    type Letter = Letter of char
    type Letters = ConcurrentDictionary<Letter, int>
    let create char = Letter char
    let value (Letter letter) = letter

module Probability =
    type Probability = Probability of float
    let calculate value (Probability propability) = value * propability
    let create value = // todo: use Option
        if value < 0. || value > 100. then raise (ArgumentOutOfRangeException("value"))
        Probability (value / 100.)
    let value (Probability probability) = probability

type Finger =
    | Thumb = 'T'
    | Index = 'I'
    | Middle = 'M'
    | Ring = 'R'
    | Pinky = 'P'

type FingersCounter = ConcurrentDictionary<Finger, int>

type State =
    { Letters: Letter.Letters
      Digraphs: Digraph.Digraphs
      Chars: Character.Chars
      TotalLetters: int
      TotalDigraphs: int
      TotalChars: int
      Result: float
      Efforts: float
      Distance: float
      TopKeys: int
      HomeKeys: int
      BottomKeys: int
      HeatMap: ConcurrentDictionary<Character.Char, float>
      SameFinger: int
      InwardRolls: int
      OutwardRolls: int
      LeftFingers: FingersCounter
      RightFingers: FingersCounter
      LeftFingersContinuous: FingersCounter
      RightFingersContinuous: FingersCounter
      LeftHandTotal: int
      RightHandTotal: int
      HandSwitch: int
      LeftHandContinuous: int
      RightHandContinuous: int
      Shifts: int }

let initialState =
    { Letters = Letter.Letters()
      Digraphs = Digraph.Digraphs()
      Chars = Character.Chars()
      TotalLetters = 0
      TotalDigraphs = 0
      TotalChars = 0
      Result = 0.
      Efforts = 0.
      Distance = 0.
      TopKeys = 0
      HomeKeys = 0
      BottomKeys = 0
      HeatMap = ConcurrentDictionary<Character.Char, float>()
      SameFinger = 0
      InwardRolls = 0
      OutwardRolls = 0
      LeftFingers = FingersCounter()
      RightFingers = FingersCounter()
      LeftFingersContinuous = FingersCounter()
      RightFingersContinuous = FingersCounter()
      LeftHandTotal = 0
      RightHandTotal = 0
      HandSwitch = 0
      LeftHandContinuous = 0
      RightHandContinuous = 0
      Shifts = 0 }
