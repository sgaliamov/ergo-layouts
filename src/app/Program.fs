open System
open System.IO

// load csv file
// construct layout structure
// render csv file

let readFile name =
    let stream = File.OpenText name
    seq {
        while not stream.EndOfStream do yield stream.Read()
    }

let loadCsv path =
    File.ReadAllText path

[<EntryPoint>]
let main argv =
    let chars = readFile "C:\\Users\\sgaliamov\\projects\\personal\\git.md"
    for c in chars do
        Console.Write c
    Console.ReadKey()
    |> ignore
    0 // return an integer exit code
