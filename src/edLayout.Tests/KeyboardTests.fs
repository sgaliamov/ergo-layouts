﻿module KeyboardTests

open Xunit
open FsUnit
open KeyboardModelds
open KeyboardModelds.Keys
open StateModels.Character

[<Fact>]
let ``Should load given layout.`` () =
    let layout =
        Layout.Parse("""
        {
            "left": {
                "1": "a"
            },
            "right": {
                "2": "b",
                "14": "\""
            },
            "shifted": {
                "`": "\""
            }
        }
        """)

    let keyboard = Keyboard.load layout

    keyboard.Keys.[Char('a')] |> should equal (StringKey("L1") |> create).Value
    keyboard.Keys.[Char('b')] |> should equal (StringKey("R2") |> create).Value
    keyboard.Keys.[Char('`')] |> should equal (StringKey("R14") |> create).Value
    keyboard.Keys.[Char('\"')] |> should equal (StringKey("R14") |> create).Value


