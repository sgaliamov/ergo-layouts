module Keyboard

open System.IO
open FSharp.Data
open FSharp.Data.JsonExtensions
open Configs
open Models
open Utilities

type private Efforts = JsonProvider<"./data/efforts.json">
let efforts = Efforts.GetSample()

type private KeyboardJson = JsonProvider<"./data/qwerty.json">

type Hand =
    | Left = 'L'
    | Right = 'R'

type Key = Hand * byte

type Keyboard =
    { symbols: Map<Character.Char, Key option>
      efforts: Map<Character.Char, float>}

let load (path: string) =
    let keyboard = KeyboardJson.Load(path)

    //let left = keyboard.Left.JsonValue.Properties
    //|> jsonValueToPairs JsonExtensions.AsString
    //|> Seq.map 
    ()