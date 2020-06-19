module Models

open System
open System.Collections.Concurrent
open FSharp.Data
open Utilities

type Keyboard = JsonProvider<"./data/keyboard.json">
type Efforts = JsonProvider<"./data/efforts.json">
type Layout = JsonProvider<"./data/qwerty.json">

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
    let create string = Digraph string
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

type Finger =
    | Thumb = 'T'
    | Index = 'I'
    | Middle = 'M'
    | Ring = 'R'
    | Pinky = 'P'

type Fingers = ConcurrentDictionary<Finger, int>

type State =
    { Letters: Letter.Letters
      Digraphs: Digraph.Digraphs
      Chars: Character.Chars
      TotalLetters: int
      TotalDigraphs: int
      TotalChars: int
      Efforts: float
      TopKeys: int
      HomeKeys: int
      BottomKeys: int
      SameFinger: int
      InwardRolls: int
      OutwardRolls: int
      RightFinders: Fingers
      LeftFinders: Fingers
      RightHandTotal: int
      LeftHandTotal: int
      RightHandContinuous: int
      LeftHandContinuous: int
      Shifts: int }
