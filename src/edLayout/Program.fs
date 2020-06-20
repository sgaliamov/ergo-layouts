open System
open System.Threading
open Main
open System.Threading.Tasks

let private handler path search layout =
    use cts = new CancellationTokenSource()
    Task.Run((fun () ->
        printf "Press any key to finish..."
        Console.ReadKey true |> ignore
        cts.Cancel true) , cts.Token) |> ignore

    let result = calculate path search layout cts
    match result with
    | Ok ok ->
        Console.WriteLine ok
        0
    | Error error ->
        printf "\n\n%s" error
        -1

[<EntryPoint>]
let main argv =
    match argv with
    | [| first; second; third |] -> handler first second third
    | _ -> printf "Wrong input."; -1
