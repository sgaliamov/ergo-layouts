module TextTests

open Xunit
open FsUnit

open Program.Text

[<Fact>]
let ``Should prepare text to processing.`` () =
    let actual = convert ["123 !@#       asd      ZXC"]
    actual |> should equivalent ["asd zxc"]

[<Fact>]
let ``Should filter non letters.`` () =
    let actual = convert ["123 !@#  "; " "; ""]
    actual |> should equivalent Seq.empty

[<Fact>]
let ``Should split on punctuation.`` () =
    let actual = convert ["asdf.asdf"]
    actual |> should equivalent ["asdf asdf"]